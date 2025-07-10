using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DEPRECRMessageProcessor : CMRMessageResponseProcessor
	{
		public DEPRECRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.DEPREC, "Depot Export Receival Notice Response (DEPRECR)")
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
