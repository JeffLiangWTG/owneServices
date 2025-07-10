using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business
{
	public abstract class MessageAttacheeMessageProcessor<TEDIMessage, TDataProvider> : MessageProcessor<TEDIMessage, TDataProvider> where TEDIMessage : InboundEDIMessage
	{
		protected MessageAttacheeMessageProcessor(LoggingInformation logger, System.Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override ZGuid GetBranchPk(BusinessObject linkedObject) => linkedObject is IMessageAttachee messageAttachee && messageAttachee.Branch is GlbBranch branch ? branch.PK : ZGuid.Invalid;

		protected override BusinessObject GetLinkedObject(BusinessObjectFactory factory, TEDIMessage message) => FindOriginalOutgoingMessage(factory, message)?.EM_LinkedObject;

		protected void SendEmailNotification(TEDIMessage message, bool isFailure = true)
		{
			if (message.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				var originalOutgoingMessage = FindOriginalOutgoingMessage(message.Factory, message);

				GenerateHtmlEmailAndSendToOriginalOrGroup(
					factory: message.Factory,
					relatedJob: messageAttachee.RelatedJob,
					messageTypeInSubject: GetEmailSubject(message),
					body: GetEmailNotification(message, messageAttachee),
					isFailure: isFailure,
					branchForEmailLogo: messageAttachee.Branch,
					sourceBusinessObject: (BusinessObject)messageAttachee,
					getEmailAddressToSendTo: () => GetEmailAddressToSendToFromQueuedUser(originalOutgoingMessage)
				);
			}
		}

		public virtual string GetLogicalStatus(TEDIMessage message, IMessageAttachee messageAttachee, TDataProvider provider) => null;

		public virtual string GetEntryStatus(TEDIMessage message, IMessageAttachee messageAttachee, TDataProvider provider) => null;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, TEDIMessage message, TDataProvider provider)
		{
			UpdateGuaranteeTransactionsIfNeeded(message, provider);
			UpdateEntryHeader(message, provider);

			if (NeedToSendEmailNotification(message))
			{
				SendEmailNotification(message, GetIsFailure(provider));
			}
		}

		void UpdateEntryHeader(TEDIMessage message, TDataProvider provider)
		{
			if (message.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				if (GetLogicalStatus(message, messageAttachee, provider) is string status)
				{
					messageAttachee.LogicalStatus = status;
				}

				if (GetEntryStatus(message, messageAttachee, provider) is string entryStatus)
				{
					messageAttachee.EntryStatus = entryStatus;
				}
				UpdateMessageAttacheeCore(messageAttachee, provider);
			}
		}

		protected virtual void UpdateGuaranteeTransactionsIfNeeded(TEDIMessage message, TDataProvider provider) { }

		protected virtual bool GetIsFailure(TDataProvider provider) => false;

		protected virtual bool NeedToSendEmailNotification(EDIMessage message) => false;

		protected virtual void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, TDataProvider provider) { }

		protected virtual string GetEmailNotification(TEDIMessage message, IMessageAttachee messageAttachee) => message.EM_MessageInterpretation;

		protected virtual string GetEmailSubject(TEDIMessage message) => message.MessageTypeWithDescription;
	}
}
