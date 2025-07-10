using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WARRETRMessageProcessor : CMRMessageResponseProcessor
	{
		public WARRETRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.WARRET, "Warehouse Export Return Notice Response (WARRETR)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType.Contains(CMRDocumentStatus.Clear.Code); }
		}
	}
}
