using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class AddAttachmentResponseProcessor : COLSMessageProcessor<CusStorageDocPivot, AddAttachmentResponse>
	{
		public AddAttachmentResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.AddAttachment, "Add Attachment")
		{
		}

		protected override string ProcessCore(CusStorageDocPivot attachment, AddAttachmentResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			var result = EDIMessage.Status.Received;
			var colsHeader = (QuarantineColsHeader)attachment.Parent;
			if (messageData.result == ResponseSuccess)
			{
				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulAddAttachment;
				if (attachment.CSD_MessageStatus == COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse)
				{
					attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.SuccessfulDocumentSent;
				}
				else if (attachment.CSD_MessageStatus == COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse)
				{
					attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.SucessfulLastdocSent;

					var entryNumber = colsHeader.LRNCusEntryNumber;
					if (entryNumber != null)
					{
						entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
						QueueLodgementStatusRequest(colsHeader);
					}
					else
					{
						result = EDIMessage.Status.Error;
						Logger.LogError($"Unable to get LRN CusEntryNumber from QuarantineColsHeader for PK [{colsHeader.PK}]");
					}
				}
			}
			else
			{
				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedAddAttachment;

				if (attachment.CSD_MessageStatus == COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse)
				{
					attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.FailedDocumentSentResponse;
				}
				else if (attachment.CSD_MessageStatus == COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse)
				{
					attachment.CSD_MessageStatus = COLSDocumentStatusList.Codes.FailedLastdocSentResponse;
				}

				if (messageData.validationMessages != null)
				{
					foreach (var responseMessageError in messageData.validationMessages)
					{
						Logger.LogError("Custom add attachment validate message code : " + responseMessageError.messageCode + " message Text: " + responseMessageError.messageText);
					}
				}
				else
				{
					Logger.LogError("Custom add attachment result: " + messageData.result);
				}
			}

			QueueNextAttachmentMessage(colsHeader);
			return result;
		}

		void QueueLodgementStatusRequest(QuarantineColsHeader header)
		{
			var senderProvider = new COLSMessageSender(header, "");
			senderProvider.SendLodgementStatusMessage();
		}
	}
}
