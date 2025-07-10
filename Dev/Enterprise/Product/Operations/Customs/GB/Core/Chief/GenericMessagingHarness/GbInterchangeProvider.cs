using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.Chief
{
	public abstract class GbInterchangeProvider : InterchangeProviderBase
	{
		public GbInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		public GbInterchangeProvider(NonDependentEDIMessageCollection messages, ILogger serviceLogger)
			: base(messages)
		{
			this.iLogger = serviceLogger;
		}

		protected override string GetCollationKey(EDIMessage message)
		{
			return DoNotCollateType;
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "See United Kingdom Customs registry."; }
		}

		protected virtual bool SupportsMultipleMessagesPerInterchange
		{
			get { return false; }
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection collatedMessagesShouldBeOnlyOne, EDIInterchange interchange)
		{
			if (!SupportsMultipleMessagesPerInterchange && collatedMessagesShouldBeOnlyOne.Count > 1)
			{
				throw new Exception("Can only send one message per interchange");
			}
			foreach (EDIMessage message in collatedMessagesShouldBeOnlyOne.ToArray())
			{
				UpdateQueuedMessageToPendingAndPerformAndOtherUpdatesAfterSpoolingMessageIntoInterchange(message);
				interchange.ContainedMessages.Add(message);
				PrepareInterchangeProperties(interchange, message);
			}
		}

		protected virtual void UpdateQueuedMessageToPendingAndPerformAndOtherUpdatesAfterSpoolingMessageIntoInterchange(EDIMessage message)
		{
			message.EM_Status = EDIMessage.Status.Pending;
		}

		public virtual string GetTargetRecipient(EDIMessage message)
		{
			if (message != null && !message.EM_MessageOwner.IsEmpty)
			{
				return message.EM_MessageOwner; // Badge
			}
			else
			{
				return this.ApplicationCode;
			}
		}

		public virtual void PrepareInterchangeProperties(EDIInterchange interchange, EDIMessage message)
		{
			interchange.EI_ApplicationCode = this.ApplicationCode;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_To = GetTargetRecipient(message);
			interchange.EI_From = this.ApplicationCode;
			interchange.EI_BodyText = message.EM_MessageText;

			if (iLogger != null)
			{
				iLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Interchange is prepared for upload. Application Code: {0}, message number: {1}.", interchange.EI_ApplicationCode, message.EM_MessageNum));
			}
		}

		protected CredentialsSetting GetCredentialForMessage(EDIMessage message)
		{
			if (string.IsNullOrEmpty(GetTargetRecipient(message)))
			{
				throw new HostedServiceException("Recipient of interchange could not be found. Cannot send as don't know to whom to send it");
			}

			//we will handle a null credential error later when sending the interchange in SendInt
			return CredentialsSetting.GetCredentialsForBadge(GetTargetRecipient(message), message.Branch.Company.PK);  // Interchange.Comapny can be DIFFERENT to Message.Company. Interchange.Company will be that of the branch under which the service task runs, message's will be anything.
		}

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return GetUNZString(messageCount.ToString());
		}

		public abstract string ApplicationCode { get; }

		readonly ILogger iLogger;
	}
}
