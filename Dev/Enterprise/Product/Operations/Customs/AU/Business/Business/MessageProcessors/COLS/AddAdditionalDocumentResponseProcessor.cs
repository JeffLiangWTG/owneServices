using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class AddAdditionalDocumentResponseProcessor : COLSMessageProcessor<QuarantineColsHeader, AddAdditionalDocumentResponse>
	{
		public AddAdditionalDocumentResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.AddAdditionalDocument, "Add Additional Document")
		{
		}

		protected override string ProcessCore(QuarantineColsHeader header, AddAdditionalDocumentResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			var result = EDIMessage.Status.Error;
			if (messageData.result == ResponseSuccess)
			{
				header.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulAddDocument;
				var entryNumber = header.LRNCusEntryNumber;
				if (entryNumber != null)
				{
					entryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
					QueueNextAttachmentMessage(header);
					result = EDIMessage.Status.Received;
				}
				else
				{
					Logger.LogError("Unable to get EntryNumber");
				}
			}
			else
			{
				header.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedAddDocument;
				header.DiscardPendingAttachmentMessages();
				result = EDIMessage.Status.Received;
			}

			return result;
		}
	}
}
