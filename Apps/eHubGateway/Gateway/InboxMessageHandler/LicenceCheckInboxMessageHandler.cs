using System;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
    public class LicenceCheckInboxMessageHandler : InboxMessageHandler
    {
        public LicenceCheckInboxMessageHandler(string clientIdSuffix = "")
        {
            this.clientIdSuffix = clientIdSuffix;
        }

        public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
        {
            CheckLicenceAndUpdateClientID(senderID, message, clientIdSuffix);
            InsertMessageToInbox(senderID, envelopeTrackingID, message, true);
        }

        string clientIdSuffix;
    }
}