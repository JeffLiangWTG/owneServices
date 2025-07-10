using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAIARRMessageProcessor : CMRMessageResponseProcessor
	{
		public SEAIARRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.SEAIAR, "CUSRES - Sea impending Arrival Report Response - (SEAIARR)")
		{
		}
	}
}
