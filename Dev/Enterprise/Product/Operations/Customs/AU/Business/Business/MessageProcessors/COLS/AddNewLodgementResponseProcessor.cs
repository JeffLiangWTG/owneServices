using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class AddNewLodgementResponseProcessor : COLSMessageProcessor<QuarantineColsHeader, AddNewLodgementResponse>
	{
		public AddNewLodgementResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.AddNewLodgement, "Add New Lodgement")
		{
		}

		protected override string ProcessCore(QuarantineColsHeader colsHeader, AddNewLodgementResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			var result = EDIMessage.Status.Error;
			if (messageData.result == "Lodgement saved successfully" && !string.IsNullOrEmpty(messageData.lrn))
			{
				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulAddLodgement;
				colsHeader.AddANewLRN(messageData.lrn, COLSEntryStatusList.Codes.LrnActive);

				SetApplicationReferenceOfAttachmentMessages(colsHeader, messageData.lrn);
				QueueNextAttachmentMessage(colsHeader);
				result = EDIMessage.Status.Received;
			}
			else
			{
				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedAddLodgement;
				colsHeader.DiscardPendingAttachmentMessages();
				result = EDIMessage.Status.Received;
			}

			return result;
		}
	}
}
