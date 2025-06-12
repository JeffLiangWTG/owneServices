using System;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	class InvalidMessageHandler : MessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			// do nothing with this message because it's NOT valid.
		}
	}
}
