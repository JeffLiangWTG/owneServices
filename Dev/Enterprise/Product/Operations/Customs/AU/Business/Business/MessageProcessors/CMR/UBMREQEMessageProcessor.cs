using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UBMREQEMessageProcessor : BaseUnderbondMessageProcessor
	{
		public UBMREQEMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.UBMREQE, "Underbond Movement Request Error Response - (UBMREQE)")
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			if (incomingMessage.EM_LinkedObject is CusHAWB)
			{
				CusHAWB hAWB = (CusHAWB)incomingMessage.EM_LinkedObject;
				if (outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Original)
				{
					hAWB.RequestLogs.CancelAll();
				}
			}
			return true;
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType.Contains(AcceptedString); }
		}
	}
}
