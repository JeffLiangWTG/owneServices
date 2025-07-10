using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WARRELRMessageProcessor : CMRMessageResponseProcessor
	{
		public WARRELRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.WARREL, "Warehouse Export Release Notice Response (WARRELR)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return (statusType.Contains(CMRDocumentStatus.Clear.Code) || statusType.Contains(CMRDocumentStatus.Withdrawn.Code)); }
		}
	}
}
