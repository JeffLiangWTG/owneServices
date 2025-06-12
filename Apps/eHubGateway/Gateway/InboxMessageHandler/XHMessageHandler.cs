using CargoWise.eHub.Common;
using CargoWise.eHub.Gateway.OpenAPIs.XHGateway;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.eHub.Gateway
{
    public class XHMessageHandler : InboxMessageHandler
    {
        private readonly Func<string, IClient> clientFactory;
        public XHMessageHandler() : this((url) => new Client(url, new HttpClient()))
        {

        }
        public XHMessageHandler(Func<String, IClient> clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        private string GetServerUrl()
        {
            return ConfigurationManager.AppSettings["XHGatewayUrl"];
        }

        public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
        {
            Message msg = new Message();
            using (var memoryStream = new MemoryStream())
            {
                message.MessageStream.Seek(0, SeekOrigin.Begin);
                message.MessageStream.CopyTo(memoryStream);
                msg.MessageBody = memoryStream.ToArray();
            }

            msg.Properties = new Dictionary<string, string>();

            msg.Properties.Add("custom.SourceParty", senderID);
            msg.Properties.Add("custom.ApplicationCode", message.ApplicationCode);
            msg.Properties.Add("custom.DestinationParty", message.ClientID);
            msg.Properties.Add("custom.EmailSubject", message.EmailSubject);
            msg.Properties.Add("custom.FileName", message.FileName);
            msg.Properties.Add("custom.MessageTrackingID", message.MessageTrackingID.ToString());
            msg.Properties.Add("custom.MessageType", message.SchemaName);
            msg.Properties.Add("custom.SchemaType", Enum.GetName(typeof(MessageSchemaType), message.SchemaType));
            msg.Properties.Add("custom.ResolvedDestinationParty", ResolvedRecipient);
            msg.Properties.Add("custom.IsXHubMessage", "true");

            var client = clientFactory(GetServerUrl());

            try
            {
	            client.SendMessageAsync(msg).GetAwaiter().GetResult();
            }
            catch (TaskCanceledException ex)
            {
	            throw new XHGatewayException(ex.Message, 500, string.Empty, null, ex.InnerException);
			}

		}

        public string ResolvedRecipient { get; set; } = string.Empty;
    }
}
