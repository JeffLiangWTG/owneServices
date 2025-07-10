using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIRIARRMessageProcessor : BaseAirCargoMessageProcessor
	{
		public AIRIARRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.AIRIAR, "Air Impending Arrival Report Response - (AIRIARR)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType.Contains(AcceptedString); }
		}
	}
}
