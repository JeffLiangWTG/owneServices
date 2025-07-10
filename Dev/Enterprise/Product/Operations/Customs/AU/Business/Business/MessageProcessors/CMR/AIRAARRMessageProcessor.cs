using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIRAARRMessageProcessor : BaseAirCargoMessageProcessor
	{
		public AIRAARRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.AIRAAR, "Air Actual Arrival Report Response - (AIRAARR)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType.Contains(AcceptedString); }
		}
	}
}
