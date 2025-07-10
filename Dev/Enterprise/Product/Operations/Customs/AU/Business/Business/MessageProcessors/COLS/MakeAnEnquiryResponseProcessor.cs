using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class MakeAnEnquiryResponseProcessor : COLSMessageProcessor<QuarantineColsHeader, MakeAnEnquiryResponse>
	{
		public MakeAnEnquiryResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.MakeAnEnquiry, "Make An Enquiry")
		{
		}

		protected override string ProcessCore(QuarantineColsHeader header, MakeAnEnquiryResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			var isAME = header.QCH_MessageStatus == COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithoutDocsResponse;
			var isAMD = header.QCH_MessageStatus == COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse;
			if (!isAME && !isAMD)
			{
				Logger.LogError("Unable to process the message as QCH_MessageStatus is neither 'AME' nor 'AMD'");
				return EDIMessage.Status.Error;
			}

			var responseResult = messageData.result;
			if ((responseResult == ResponseSuccess || responseResult == "Enquiry saved successfully.") && !string.IsNullOrEmpty(messageData.lrn))
			{
				header.AddANewLRN(messageData.lrn, COLSEntryStatusList.Codes.LrnActive);
				if (isAMD)
				{
					header.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulMakeAnEnquiryWithDocs;
					SetApplicationReferenceOfAttachmentMessages(header, messageData.lrn);
					QueueNextAttachmentMessage(header);
				}
				else
				{
					header.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulMakeAnEnquiryWithoutDocs;
					header.LRNCusEntryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
				}
			}
			else
			{
				if (isAMD)
				{
					header.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithDocs;
					header.DiscardPendingAttachmentMessages();
				}
				else
				{
					header.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedMakeAnEnquiryWithoutDocs;
				}
			}

			return EDIMessage.Status.Received;
		}
	}
}
