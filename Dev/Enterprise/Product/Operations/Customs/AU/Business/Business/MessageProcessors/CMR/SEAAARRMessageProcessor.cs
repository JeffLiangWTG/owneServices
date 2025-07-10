using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAAARRMessageProcessor : CMRMessageResponseProcessor
	{
		public SEAAARRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.SEAAAR, "Sea Actual Arrival Report Response - (SEAAARR)")
		{
		}
	}
}
