using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class LodgementStatusResponseProcessor : COLSMessageProcessor<QuarantineColsHeader, LodgementStatusResponse>
	{
		public LodgementStatusResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.LodgementStatus, "Lodgement Status")
		{
		}

		protected override string ProcessCore(QuarantineColsHeader header, LodgementStatusResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			if (messageData.result == ResponseSuccess)
			{
				header.QCH_LodgementStatus = header.Lookups.COLSLodgementStatusList.GetCodeFromDescription(messageData.status);
				header.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulGetLodgementStatus;
			}
			else
			{
				header.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedGetLodgementStatus;
			}

			return EDIMessage.Status.Received;
		}
	}
}
