using System.Linq;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	abstract class COLSMessageProcessor : CustomsMessageProcessor
	{
		public const string ResponseSuccess = "SUCCESS";

		protected COLSMessageProcessor(LoggingInformation logger, string messageType, string messageFriendlyName)
			: base(logger, messageType, messageFriendlyName)
		{
		}

		protected override ZGuid AcknowledgementEmailGroup => ZGuid.Empty;

		protected override ZString AcknowledgementEmailMode => ZString.Empty;

		protected override ZGuid ImpedimentEmailGroup => ZGuid.Empty;

		protected override ZString ImpedimentEmailMode => ZString.Empty;

		protected override ZGuid ErrorEmailGroup => ZGuid.Empty;

		protected override ZString ErrorEmailMode => ZString.Empty;
	}

	abstract class COLSMessageProcessor<TLinkedObject, TMessageData> : COLSMessageProcessor
		where TLinkedObject : BusinessObject
	{
		protected COLSMessageProcessor(LoggingInformation logger, string messageType, string messageFriendlyName)
			: base(logger, messageType, messageFriendlyName)
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			var status = EDIMessage.Status.Error;
			var originalMessage = GetOriginalMessage(message);
			if (originalMessage?.EM_LinkedObject is TLinkedObject linkedJob)
			{
				message.EM_LinkedObject = linkedJob;
				var messageData = DeserializeObjectCore(message);
				if (messageData != null)
				{
					status = ProcessCore(linkedJob, messageData, message, originalMessage);
				}
				else
				{
					Logger.LogError($"Unable to deserialize message text from '{message.EM_MessageText}'");
				}
			}
			else
			{
				Logger.LogError($"Unable to find linked COLS Header for message #{message.EM_MessageNum}");
			}
			return status;
		}

		EDIMessage GetOriginalMessage(EDIMessage message)
		{
			EDIMessage originalMessage = null;
			var sessionID = message.Interchange?.EI_SessionGUID ?? ZGuid.Empty;
			if (sessionID.IsValid)
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, sessionID);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				var outboundInterchange = message.Factory.LoadTop1<EDIInterchange>(query);
				originalMessage = outboundInterchange?.ContainedMessages.Cast<EDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			}
			return originalMessage;
		}

		protected abstract string ProcessCore(TLinkedObject header, TMessageData messageData, EDIMessage message, EDIMessage originalMessage);

		protected virtual TMessageData DeserializeObjectCore(EDIMessage message) => JsonSerializer.Deserialize<TMessageData>(message.EM_MessageText);

		protected void QueueNextAttachmentMessage(QuarantineColsHeader header)
		{
			var attachmentMessages = header.PendingAddAttachmentMessages;
			var nextAttachmentMessage = attachmentMessages.FirstOrDefault(x => x.EM_MessageSubType == AUCOLSMessageSubTypeList.Codes.NormalAttachment)
						?? attachmentMessages.FirstOrDefault(x => x.EM_MessageSubType == AUCOLSMessageSubTypeList.Codes.LastdocAttachment);

			if (nextAttachmentMessage != null)
			{
				nextAttachmentMessage.EM_Status = EDIMessage.Status.Queued;
				header.QCH_MessageStatus = COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse;
			}
		}

		protected void SetApplicationReferenceOfAttachmentMessages(QuarantineColsHeader header, ZString lrn)
		{
			foreach (var message in header.PendingAddAttachmentMessages)
			{
				message.EM_ApplicationReference = lrn;
			}
		}
	}
}
