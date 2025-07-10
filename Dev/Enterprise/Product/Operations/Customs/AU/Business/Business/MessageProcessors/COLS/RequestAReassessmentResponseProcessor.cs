using System;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class RequestAReassessmentResponseProcessor : COLSMessageProcessor<QuarantineColsHeader, RequestAReassessmentResponse>
	{
		public RequestAReassessmentResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.RequestAReassessment, "Request A Reassessment")
		{
		}

		protected override string ProcessCore(QuarantineColsHeader colsHeader, RequestAReassessmentResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			var isARW = colsHeader.QCH_MessageStatus == COLSHeaderStatusList.Codes.AwaitingReassessmentWithDocsResponse;
			var isARN = colsHeader.QCH_MessageStatus == COLSHeaderStatusList.Codes.AwaitingReassessmentWithoutDocsResponse;
			if (!isARN && !isARW)
			{
				Logger.LogError("Unable to process the message as QCH_MessageStatus is neither 'ARN' nor 'ARW'");
				return EDIMessage.Status.Error;
			}

			var result = EDIMessage.Status.Received;
			var responseResult = messageData.result;

			if (!string.IsNullOrEmpty(messageData.generatedLrn)
				&& (responseResult == ResponseSuccess
				|| ZArchitecture.Core.StringExtension.Contains(responseResult, "Reassessment request saved succesfully", StringComparison.OrdinalIgnoreCase)))    // Using StringExtension directly, so to resolve .NET Core confusion.
			{
				colsHeader.AddANewLRN(messageData.generatedLrn, COLSEntryStatusList.Codes.LrnActive);
				if (isARW)
				{
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulReassessmentWithDocs;
					SetApplicationReferenceOfAttachmentMessages(colsHeader, messageData.generatedLrn);
					QueueNextAttachmentMessage(colsHeader);
				}
				else
				{
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulReassessmentWithoutDocs;
					colsHeader.LRNCusEntryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
				}
			}
			else
			{
				if (isARW)
				{
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedReassessmentWithDocs;
					colsHeader.DiscardPendingAttachmentMessages();
				}
				else
				{
					colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedReassessmentWithoutDocs;
				}
			}

			return result;
		}
	}
}
