using System;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	class JPCustomsInboxMessageHandler : InboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			message.FileName = IPNetworking.GetRequesterIP4Address() + ";" + IPNetworking.GetLocalIP4Address();
			InsertMessageToInbox(senderID, envelopeTrackingID, message, true);
		}
	}
}
