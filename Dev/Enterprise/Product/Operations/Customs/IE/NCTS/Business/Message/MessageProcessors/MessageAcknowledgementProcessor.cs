using System;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class MessageAcknowledgementProcessor : IE.Business.MessageAcknowledgementProcessor
	{
		public MessageAcknowledgementProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override void SetJobFailed(EDIMessage originalMessage)
		{
			//Do nothing
		}

		protected override void SendFailureNotification<T>(EDIMessage originalMessage, ITransaction transaction, EDIMessage targetMessage)
		{
			if (originalMessage.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				using (SetApplicationCodeForEmail(originalMessage.EM_ApplicationCode))
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup(
						factory: originalMessage.Factory,
						relatedJob: messageAttachee.RelatedJob,
						messageTypeInSubject: originalMessage.MessageTypeWithDescription,
						body: ((T)Activator.CreateInstance(typeof(T), originalMessage, transaction)).GetInterpretation(),
						isFailure: true,
						branchForEmailLogo: messageAttachee.Branch,
						sourceBusinessObject: (BusinessObject)messageAttachee,
						getEmailAddressToSendTo: () => GetEmailAddressToSendToFromQueuedUser(originalMessage)
					);
				}
			}
		}
	}
}
