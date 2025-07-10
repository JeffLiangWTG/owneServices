using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CARLSTRMessageProcessor : CMRMessageResponseProcessor
	{
		public CARLSTRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.CARLST, "Cargo List Report Response - (CARLSTR)")
		{
		}
	}
}
