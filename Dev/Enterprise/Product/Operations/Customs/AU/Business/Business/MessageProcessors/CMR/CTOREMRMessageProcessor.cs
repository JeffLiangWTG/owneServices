using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOREMRMessageProcessor : CMRMessageResponseProcessor
	{
		public CTOREMRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.CTOREM, "CTO Removal Notice Response(CTOREMR)")
		{
		}
	}
}
