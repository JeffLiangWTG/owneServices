using System;
using System.IO;
using System.ServiceModel;
using CargoWise.eHub.Common;
using CargoWise.eHub.Gateway.XHub.XHubGateway;

namespace CargoWise.eHub.Gateway
{
	public class XHubMessageHandler : InboxMessageHandler
	{
		#region Handle

		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			const string messageNamespace = "urn:uuid:A2415482-5D5B-4054-8773-DCA6FA605E11";

			var xHubMessageHeader = new XHubMessageHeader(new[]
				{
					new Property { Name = "SourceParty", Namespace = messageNamespace, Value = senderID},
					new Property { Name = "DestinationParty", Namespace = messageNamespace, Value = message.ClientID },
					new Property { Name = "MessageType", Namespace = messageNamespace, Value = message.SchemaName },
					new Property { Name = "MessageTrackingID", Namespace = messageNamespace, Value = message.MessageTrackingID.ToString() }
				});

			SendToXHubGateway(message, xHubMessageHeader);
		}

		public virtual void SendToXHubGateway(eHubGatewayMessage message, XHubMessageHeader xHubMessageHeader)
		{
			using (var client = CreateXHubGatewayClient())
			{
				using (var operationContextScope = new OperationContextScope(client.InnerChannel))
				{
					OperationContext.Current.OutgoingMessageHeaders.Add(xHubMessageHeader);
					message.MessageStream.Seek(0, SeekOrigin.Begin);

					client.SendXHubMessage(message.MessageStream);
				}
			}
		}

		#endregion

		#region CreateXHubGatewayClient

		xHubGatewayClient CreateXHubGatewayClient()
		{
			return new xHubGatewayClient();
		}

		#endregion
	}
}