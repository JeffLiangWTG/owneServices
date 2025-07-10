using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public abstract class MessageProcessorWithEmailNotification<T> : MessageProcessor<T>
	{
		protected MessageProcessorWithEmailNotification(LoggingInformation logger)
			: base(logger)
		{
		}

		protected sealed override void ProcessMessageCore(EDIMessage message, T messageObject)
		{
			var manifestHeader = message.EM_LinkedObject as AsycudaManifestHeader;
			if (manifestHeader != null)
			{
				ProcessMessageSendEmailNotification(message.Factory, manifestHeader, messageObject);
				SetMessageInterpretation(message, manifestHeader, messageObject);
			}

			SetMessageProcessedStatus(message);
		}

		protected virtual void SetMessageInterpretation(EDIMessage message, AsycudaManifestHeader manifestHeader, T messageObject)
		{
			message.EM_MessageInterpretation = GenerateEmailBodyText(manifestHeader, messageObject);
		}

		protected virtual void ProcessMessageSendEmailNotification(BusinessObjectFactory factory, AsycudaManifestHeader manifestHeader, T messageObject)
		{
			SendEmailNotification(factory, manifestHeader, messageObject);
			UpdateManifestHeader(manifestHeader, messageObject);
		}

		protected virtual void SetMessageProcessedStatus(EDIMessage message)
		{
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
		}

		protected virtual void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, T messageObject)
		{
		}

		protected void SendEmailNotification(BusinessObjectFactory factory, AsycudaManifestHeader manifestHeader, T messageObject)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(
				factory: factory,
				relatedJob: null,
				messageTypeInSubject: GenerateEmailSubjectText(manifestHeader, messageObject),
				body: GenerateEmailBodyText(manifestHeader, messageObject),
				isFailure: false,
				branchForEmailLogo: manifestHeader?.Branch,
				sourceBusinessObject: manifestHeader,
				getEmailAddressToSendTo: () => GetEmailAddress(manifestHeader)
			);
		}

		protected sealed override IRegistryItem GetEmailGroupRegistryItem() => GetEmailGroupRegistryItemCore();

		protected abstract string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, T messageObject);

		protected abstract string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, T messageObject);

		protected abstract IRegistryItem GetEmailGroupRegistryItemCore();

		protected virtual ZString GetEmailAddress(AsycudaManifestHeader manifestHeader)
		{
			var originalMessage = GetOriginalMessage(manifestHeader);
			var emailAddress = originalMessage?.UserWhoQueuedThisRecord?.GS_EmailAddress ?? ZString.Empty;
			if (emailAddress.IsEmpty && manifestHeader?.CustomsAgent is GlbStaff customsAgent)
			{
				emailAddress = customsAgent.GS_EmailAddress;
			}

			return emailAddress;
		}

		protected string GetISO8601DateTimeWithSecondsPrecision(DateTime? dateTime)
		{
			if (dateTime.HasValue)
			{
				var iso8601DateTime = dateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss");
				var suffix = dateTime.Value.Kind == DateTimeKind.Utc ? "Z" : string.Empty;
				return $"{iso8601DateTime}{suffix}";
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
