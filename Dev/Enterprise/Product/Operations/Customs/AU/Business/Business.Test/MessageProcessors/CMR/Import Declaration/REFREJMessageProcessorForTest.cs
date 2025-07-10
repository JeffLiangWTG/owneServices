using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class REFREJMessageProcessorForTest : REFREJMessageProcessor
	{
		public REFREJMessageProcessorForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		public bool ShouldSendAcknowledgementReportExposed
		{
			get { return ShouldSendAcknowledgementReport; }
		}
	}
}
