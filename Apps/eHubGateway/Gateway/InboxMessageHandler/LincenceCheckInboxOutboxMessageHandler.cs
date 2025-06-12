using System;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
    public class LicenceCheckInboxOutboxMessageHandler : InboxOutboxMessageHandler
    {
        public LicenceCheckInboxOutboxMessageHandler(string clientIdSuffix = "")
        {
            this.clientIdSuffix = clientIdSuffix;
        }

        public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
        {
            CheckLicenceAndUpdateClientID(senderID, message, clientIdSuffix);
            base.Handle(senderID, envelopeTrackingID, message);
        }

        string clientIdSuffix;
    }
}