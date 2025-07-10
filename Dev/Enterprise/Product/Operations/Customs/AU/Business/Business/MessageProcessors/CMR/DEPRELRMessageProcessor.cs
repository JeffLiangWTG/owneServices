using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DEPRELRMessageProcessor : CMRMessageResponseProcessor
	{
		public DEPRELRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.DEPREL, "Depot Export Release Notice Response (DEPRELR)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get
			{
				return (statusType.Contains(CMRDocumentStatus.Clear.Code) ||
		statusType.Contains(CMRDocumentStatus.Error.Code) ||
		statusType.Contains(CMRDocumentStatus.Withdrawn.Code));
			}
		}
	}
}
