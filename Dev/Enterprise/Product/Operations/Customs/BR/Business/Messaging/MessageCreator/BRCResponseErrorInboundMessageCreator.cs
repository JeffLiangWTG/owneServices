using Enterprise.BatchProcessor;

namespace Enterprise.Customs.BR.Business
{
	public class BRCResponseErrorInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCResponseErrorInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ExtractResponseMessageAsMessageText => false;
	}
}
