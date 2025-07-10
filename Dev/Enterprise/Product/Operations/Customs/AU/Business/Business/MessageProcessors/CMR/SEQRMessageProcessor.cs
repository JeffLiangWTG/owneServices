using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEQRMessageProcessor : BaseSeaCargoMessageProcessor
	{
		public SEQRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.SEQ, "SEA Cargo Establishment Query Response - (SEQR)")
		{
		}
	}
}
