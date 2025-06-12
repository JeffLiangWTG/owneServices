using System;
using CargoWise.eHub.Common;
using CargoWise.eHub.Products.AirMessaging.ServiceBroker.Services;

namespace CargoWise.eHub.Gateway
{
	public class AirMessageHandler : InboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			var inboxPk = Guid.NewGuid();
			EnqueueMessage(new AirMessageQueuer(), senderID, inboxPk, message);
		}
	}
}