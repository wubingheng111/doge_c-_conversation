using Newtonsoft.Json;
using RestSharp;
namespace Ernie
{
    public partial class AI_cahtbot : Form
    {

        public class User
        {
            public string? Name { get; set; }
            public Image? Avatar { get; set; }
            public Color MessageColor { get; set; }
        }
        

        public class ChatMessage
        {
            public User? Sender { get; set; }
            public string? Text { get; set; }
        }

        public AI_cahtbot()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 100);
        }

        const string API_KEY = "VfiOrBcUm1TQZWzZQD9A9RRW";
        const string SECRET_KEY = "4MCAqWnnJ1IHHtbW8qUJgoVHIBtbwWUX";
        const int MAX_TOKENS = 4000;
        private User currentUser;

        private void GetAccessToken_Click(object sender, EventArgs e)
        {
            client = new RestClient($"https://aip.baidubce.com/oauth/2.0/token?client_id=RAWNJTl7ml69HPXE7urd606c&client_secret=UTuXPncTrMHCIedR3dBXY7afhjJB04k2&grant_type=client_credentials");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            MessageBox.Show(response.Content);

        }

        double temperature = 0.8;
        double top_p = 0.8;
        bool search = true;



        public class Message
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }

        public class ErnieMessage
        {
            [JsonProperty("messages")]
            public Message[] Messages { get; set; }

            [JsonProperty("temperature")]
            public double Temperature { get; set; }

            [JsonProperty("top_p")]
            public double TopP { get; set; }

            [JsonProperty("penalty_score")]
            public double PenaltyScore { get; set; }

            [JsonProperty("disable_search")]
            public bool DisableSearch { get; set; }

            [JsonProperty("enable_citation")]
            public bool EnableCitation { get; set; }
        }
        ErnieMessage ernieMessage = new ErnieMessage();

        public class ChatCompletionResult
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("object")]
            public string ObjectType { get; set; } // ��������������Ը���ʵ������������������������.NET������Object���ͻ  

            [JsonProperty("created")]
            public long CreatedTimestamp { get; set; } // ͨ��ʱ�����ת��ΪDateTime�������ﱣ��Ϊlong  

            [JsonProperty("result")]
            public string Result { get; set; }

            [JsonProperty("is_truncated")]
            public bool IsTruncated { get; set; }

            [JsonProperty("need_clear_history")]
            public bool NeedClearHistory { get; set; }

            [JsonProperty("finish_reason")]
            public string FinishReason { get; set; }

            [JsonProperty("usage")]
            public Usage UsageInfo { get; set; }

            // Ƕ��������ʾUsage����  
            public class Usage
            {
                [JsonProperty("prompt_tokens")]
                public int PromptTokens { get; set; }

                [JsonProperty("completion_tokens")]
                public int CompletionTokens { get; set; }

                [JsonProperty("total_tokens")]
                public int TotalTokens { get; set; }
            }
        }

        class MessagePool
        {
            public int tokens = 0;
            public List<Message> messagePool = new List<Message>();
        }
        MessagePool messagePool = new MessagePool();

        enum eRole { User, Assistant}
        string Role(eRole role)
        {
            return role == eRole.User ? "user" : "assistant";
        }
        void AddMessageToPool(string strMessage, eRole role)
        {
            Message message = new Message()
            {
                Role = Role(role),
                Content = strMessage,
            };
            messagePool.messagePool.Add(message);
            while (messagePool.tokens >= MAX_TOKENS)
                messagePool.messagePool.Remove(messagePool.messagePool[0]);
            ernieMessage.Messages = messagePool.messagePool.ToArray();
        }
        private string Chat(string strSend, eRole role)
        {
            var client = new RestClient($"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/completions?access_token={GetAccessTokenMethod()}");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            AddMessageToPool(strSend, role);
            var body = JsonConvert.SerializeObject(ernieMessage, Formatting.None);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            AddMessageToPool(response.Content, eRole.Assistant);
            ChatCompletionResult result = JsonConvert.DeserializeObject<ChatCompletionResult>(response.Content);
            FillCompletionInfo(result);
            if (result.NeedClearHistory) ClearHistoryMethod();
            return result.Result;
        }

        void FillCompletionInfo(ChatCompletionResult result)
        {
            if(result== null) return;
            
            textId.Text = result.Id == null ? "" : result.Id;
            textObject.Text = result.ObjectType == null ? "" : result.ObjectType;
            textClearHistory.Text = result.NeedClearHistory == null ? "" : result.NeedClearHistory.ToString();
            textFinishFlag.Text = result.FinishReason == null ? "" : result.FinishReason;
            textPromptTake.Text = result.UsageInfo == null || result.UsageInfo.PromptTokens == null ? "" : result.UsageInfo.PromptTokens.ToString();
            textReturnTake.Text = result.UsageInfo == null || result.UsageInfo.CompletionTokens == null ? "" : result.UsageInfo.CompletionTokens.ToString();
            textTotalTake.Text = result.UsageInfo == null || result.UsageInfo.TotalTokens == null ? "" : result.UsageInfo.TotalTokens.ToString();
        }

        RestClient client = new RestClient();
        void Init()
        {
            var client = new RestClient($"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/completions?access_token={GetAccessTokenMethod()}");
            MessageBox.Show(GetAccessTokenMethod());
            client.Timeout = -1;

            ernieMessage = new ErnieMessage()
            {
                Temperature = 0.8,
                TopP = 0.8,
                PenaltyScore = 1.0,
                DisableSearch = false,
                Messages = new Message[0],
                EnableCitation = false
            };

            this.trackBarRandom.Maximum = 100;
            this.trackBarRandom.Value = 80;
            this.switchSearch.Active = false;
        }
        static string GetAccessTokenMethod()
        {
            var client = new RestClient($"https://aip.baidubce.com/oauth/2.0/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", API_KEY);
            request.AddParameter("client_secret", SECRET_KEY);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            var result = JsonConvert.DeserializeObject<dynamic>(response.Content);
            
            return result.access_token.ToString();
        }
        private void AddMessageToChat(ChatMessage message)
        {
            // ����RichTextBox��ѡ��λ�ú���ɫ  
            textShow.SelectionStart = textShow.TextLength;
            textShow.SelectionColor = message.Sender.MessageColor;
            Font userFont = new Font("Microsoft YaHei", 9F, FontStyle.Bold);
            if (message.Sender.Name == "User")
                textShow.SelectionFont = userFont;
            textShow.AppendText(message.Sender.Name + ": " + message.Text + Environment.NewLine + Environment.NewLine);

            // ��ѡ��λ���ƻ�ĩβ���Ա���һ�������Ϣʱ���Ḳ�������ı�  
            textShow.SelectionStart = textShow.TextLength;
            textShow.SelectionColor = textShow.ForeColor; // ������ɫ  

            // �����Ҫ����������ײ���ʾ������Ϣ  
            textShow.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            currentUser = new User { Name = "Alice", MessageColor = Color.Blue };
            Init();
        }



        private void buttonSend_Click(object sender, EventArgs e)
        {
            // ����currentUser�ǵ�ǰ��¼���û�  
            ChatMessage message = new ChatMessage
            {
                Sender = currentUser,
                Text = textSend.Text
            };

            AddMessageToChat(message);

            ChatMessage response = new ChatMessage
            {
                Sender = new User()
                {
                    Name = "Ernie",
                    MessageColor = Color.Green
                },
                Text = Chat(message.Text, eRole.User)
            };

            AddMessageToChat(response);
            // ���������Ա���������Ϣ  
            textSend.Clear();
        }
        void ClearHistoryMethod()
        {
            ernieMessage.Messages = new Message[] { };
            messagePool.messagePool.Clear();
        }
        private void ClearHistory_Click(object sender, EventArgs e)
        {
            ClearHistoryMethod();
        }

        private void uiSymbolButton1_Click(object sender, EventArgs e)
        {
            this.ernieMessage.Temperature = (double)this.trackBarRandom.Value / 100;
            this.ernieMessage.DisableSearch = this.switchSearch.Active;

        }

        private void textSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // ����Ƿ�ͬʱ������ Alt ��  
                if ((e.Modifiers & Keys.Alt) == Keys.Alt)
                {
                    // �� TextBox �в��뻻�з�  
                    textSend.AppendText(Environment.NewLine);
                    e.SuppressKeyPress = true; // ��ֹĬ�ϵ� Enter ����Ϊ���緢�� Ding �����ƶ����㣩  
                }
                else
                {
                    // ���� Button �ĵ���¼�  
                    buttonSend_Click(sender, EventArgs.Empty);
                    // ���ܻ���Ҫ��ֹĬ�ϵ� Enter ����Ϊ������ȡ�����������  
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void textShow_TextChanged(object sender, EventArgs e)
        {

        }
    }
}