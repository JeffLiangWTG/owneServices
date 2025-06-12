using System;
using System.IO;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway
{
	class LegacyXmlE2EMessageHandler : DefaultInboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			if (!PartyAccessor().IsLegacyXmlAllowedClient(senderID) || !PartyAccessor().IsLegacyXmlAllowedClient(message.ClientID))
			{
				throw new InvalidDataException (
					@"You are using E2E with the superseded legacy application Type, XMS that is no longer supported.
Please change your E2E settings as per WiseLearning documents 'How To Set up E2E'.
The receiving CW1 system needs to also be adjusted as per the documents 'How To Set up E2E'");
			}
			else
			{
				base.Handle(senderID, envelopeTrackingID, message);
			}
		}

		internal virtual IPartyAccessor PartyAccessor()
		{
			return DataAccessFactories.NewPartyAccessorInstance();
		}
	}
}