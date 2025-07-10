using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class SwitchAepLodgementResponseProcessor : COLSMessageProcessor<QuarantineColsHeader, SwitchAepLodgementResponse>
	{
		public SwitchAepLodgementResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.SwitchAepLodgement, "Switch Aep Lodgement")
		{
		}

		protected override string ProcessCore(QuarantineColsHeader colsHeader, SwitchAepLodgementResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			var result = EDIMessage.Status.Error;
			if (messageData.result == ResponseSuccess && !string.IsNullOrEmpty(messageData.lrn))
			{
				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.SuccessfulSwitchAepRequest;
				colsHeader.AddANewLRN(messageData.lrn, COLSEntryStatusList.Codes.LrnInactive);
				result = EDIMessage.Status.Received;
			}
			else
			{
				colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.FailedSwitchAepRequest;
				result = EDIMessage.Status.Received;
			}

			return result;
		}
	}
}
