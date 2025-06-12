using System;

namespace CargoWise.eHub.Gateway
{
	public class ZACustomsMessageHandler : InboxOutboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, Common.eHubGatewayMessage message)
		{
            CheckLicenceAndUpdateClientID(senderID, message);
			base.Handle(senderID, envelopeTrackingID, message);
		}
	}
}