using System;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	public class AUCustomsMessageHandler : eHub2MessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			base.Handle(senderID, envelopeTrackingID, message);
		}
	}
}