using System;
using System.Configuration;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	public class NZCustomsMessageHandler : eHub2MessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			if (!PreCheck(senderID, envelopeTrackingID, message)) return;

			base.Handle(senderID, envelopeTrackingID, message);

		}

		protected virtual bool CheckSenderLicenceType(string senderID, out string failReason)
		{
			failReason = string.Empty;

			if (IsEnterpriseLicenceProduction(senderID)) return true;

			failReason = string.Format("Only production licensed CW1/Enterprise systems are permitted to send production messages to NZ Customs.");
			
			return false;
		}

		#region Implementation

		bool PreCheck(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			var failReason = string.Empty;
			if (!IsFailNZCustomsMessageDeliveryConfigured(out failReason) && CheckSenderLicenceType(senderID, out failReason)) return true;

			FailMessage(senderID, envelopeTrackingID, message, failReason);
			return false;
		}

		bool IsFailNZCustomsMessageDeliveryConfigured(out string failReason)
		{
			var failNZCustomsMessageDelivery = GetConfigurationFailNZCustomsMessageDelivery();
			failReason = failNZCustomsMessageDelivery ? "Cannot be delivered as this system does not have a NZCustoms connection." : string.Empty;
			return failNZCustomsMessageDelivery;
		}

		internal virtual bool GetConfigurationFailNZCustomsMessageDelivery()
		{
			bool failNZCustomsMessageDelivery;
			bool.TryParse(ConfigurationManager.AppSettings["FailNZCustomsMessageDelivery"], out failNZCustomsMessageDelivery);
			return failNZCustomsMessageDelivery;
		}

		void FailMessage(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, string errorDescription)
		{
			Guid inboxPK = Guid.NewGuid();
			InsertMessageToInbox(senderID, envelopeTrackingID, message, inboxPK);
			InsertErrorAndUpdateInboxMessageStatus(Guid.NewGuid(), "GTW", "UKN", errorDescription, inboxPK, Guid.Empty);
		}

		#endregion
	}
}