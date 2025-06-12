using System;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	class DefaultInboxMessageHandler : InboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			InsertMessageToInbox(senderID, envelopeTrackingID, message, true);
		}
	}
}
