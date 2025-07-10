using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEIncomingMessageProcessor : FRIncomingMessageProcessorBase
	{
		public DeltaIEIncomingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override IEnumerable<ApplicationTypeMessageProcessor> GetApplicationTypeMessageProcessors(LoggingInformation logger)
		{
			yield return new IE404Processor(logger);
			yield return new IE410Processor(logger);
			yield return new IE426Processor(logger);
			yield return new IE428Processor(logger);
			yield return new IE429Processor(logger);
			yield return new IE431Processor(logger);
			yield return new IE456Processor(logger);
			yield return new FRA101Processor(logger);
			yield return new FRA102Processor(logger);
			yield return new FRA103Processor(logger);
			yield return new IE460Processor(logger);
			yield return new IE917Processor(logger);
			yield return new IE451Processor(logger);
		}

		protected override InboundInterchangeProcessor GetInboundInterchangeProcessor(LoggingInformation logger)
		{
			return new DeltaIEInboundInterchangeProcessor(logger);
		}
	}
}
