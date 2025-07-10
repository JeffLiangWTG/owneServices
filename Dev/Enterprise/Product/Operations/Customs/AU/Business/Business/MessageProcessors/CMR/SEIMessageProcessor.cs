using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEIMessageProcessor : BaseSeaCargoMessageProcessor
	{
		public SEIMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.SEI, "Sea Cargo Establishment Information  - (SEI)")
		{
		}

		protected override bool IsUnsolicitedMessage
		{
			get { return true; }
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return true; }
		}
	}
}
