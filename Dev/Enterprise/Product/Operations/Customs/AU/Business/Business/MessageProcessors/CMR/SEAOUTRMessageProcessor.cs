using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAOUTRMessageProcessor : BaseSeaCargoMessageProcessor
	{
		public SEAOUTRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.SEAOUT, "Sea Outturn Report Response - (SEAOUTR)")
		{
		}
	}
}
