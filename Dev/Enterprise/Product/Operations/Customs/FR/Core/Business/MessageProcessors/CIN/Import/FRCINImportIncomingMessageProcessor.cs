using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRCINImportIncomingMessageProcessor : FRIncomingMessageProcessorBase
	{
		public FRCINImportIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IEnumerable<ApplicationTypeMessageProcessor> GetApplicationTypeMessageProcessors(LoggingInformation logger)
		{
			yield return new FRCINImportResponseMessageProcessor(Logger);
		}
		protected override InboundInterchangeProcessor GetInboundInterchangeProcessor(LoggingInformation logger)
		{
			return new FRCINImportInboundInterchangeProcessor(Logger);
		}
	}
}
