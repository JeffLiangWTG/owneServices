using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public class ErrorMessageProcessor : SendingNotificationCommonProcessor
	{
		public ErrorMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => Res.GetString("1599F14E-EA30-4B87-AEDB-3677FD3787EA", "Error Message");

		protected override string ApplicationCodeCore => throw new InvalidOperationException("ApplicationCode should not be used. Should override MessageFilterCore.");

		protected override void ProcessMessageCore(BaseEDIMessage targetMessage)
		{
			targetMessage.EM_Status = EDIMessage.Status.Received;
			if (targetMessage.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				var originalMessage = InboundEDIMessage.GetOriginalMessage(targetMessage.Factory, targetMessage.EM_ApplicationCode, targetMessage.EM_ApplicationReference);
				SendFailureNotification(targetMessage, messageAttachee, originalMessage, targetMessage.EM_MessageText);
				messageAttachee.LogicalStatus = LogicalStatusList.Codes.Failed;
			}
		}

		void SendFailureNotification(BaseEDIMessage targetMessage, IMessageAttachee messageAttachee, BaseEDIMessage originalMessage, ZString messagText)
		{
			var transactionId = ZString.Empty;
			var messageType = ZString.Empty;
			var messageNum = ZString.Empty;
			var applicationCode = ZString.Empty;
			GlbStaff userToNotify = null;
			if (originalMessage != null)
			{
				transactionId = originalMessage.EM_ApplicationReference;
				messageType = originalMessage.MessageTypeWithDescription;
				messageNum = originalMessage.EM_MessageNum;
				userToNotify = originalMessage.UserWhoQueuedThisRecord;
				applicationCode = originalMessage.EM_ApplicationCode;
			}
			else
			{
				transactionId = targetMessage.EM_ApplicationReference;
				messageType = targetMessage.MessageTypeWithDescription;
				applicationCode = targetMessage.EM_ApplicationCode;
			}

			using (SetApplicationCodeForEmail(applicationCode))
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(
				factory: targetMessage.Factory,
				relatedJob: messageAttachee.RelatedJob,
				messageTypeInSubject: messageType,
				body: GenerateMessageBodyText(transactionId, messageType, messageNum, messagText),
				isFailure: true,
				branchForEmailLogo: messageAttachee.Branch,
				sourceBusinessObject: (BusinessObject)messageAttachee,
				getEmailAddressToSendTo: () =>
				{
					var emailAddress = userToNotify?.GS_EmailAddress ?? ZString.Empty;
					if (emailAddress.IsEmpty && messageAttachee.CustomsAgent is GlbStaff customsAgent)
					{
						emailAddress = customsAgent.GS_EmailAddress;
					}
					return emailAddress;
				});
			}
		}

		string GenerateMessageBodyText(ZString transactionId, ZString messageType, ZString messageNum, ZString messagText)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("E07F7816-C17E-4280-8BC2-59E9A7AE6C46", "Submission was unsuccessful"));
			var messageInfoBuilder = new HtmlTableCreator();
			messageInfoBuilder.WriteRow(Res.GetString("9D9F7BFA-FF1A-4C25-AFEE-9F97E5CB2E16", "Transaction ID"), transactionId);
			messageInfoBuilder.WriteRow(Res.GetString("FF918D7D-A1DE-4484-A67A-D642B56B5FCB", "Message Type"), messageType);
			messageInfoBuilder.WriteRow(Res.GetString("B2BDE62C-4C1F-4AD3-997B-9EF5E798B5C1", "Message Number"), messageNum);
			htmlBuilder.Append(messageInfoBuilder.ToHtml());
			htmlBuilder.Append(messagText);
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}
	}
}
