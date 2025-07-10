using System.Collections.Generic;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRPNTSIncomingMessageProcessor : FRIncomingMessageProcessorBase
	{
		public FRPNTSIncomingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override IEnumerable<ApplicationTypeMessageProcessor> GetApplicationTypeMessageProcessors(LoggingInformation logger)
		{
			yield return new IETS016Processor(logger);
			yield return new IETS028Processor(logger);
			yield return new IETS410Processor(logger);
			yield return new IETS460Processor(logger);
			yield return new IETS030Processor(logger);
			yield return new IETS029Processor(logger);
			yield return new IETS095Processor(logger);
			yield return new IETS928Processor(logger);
			yield return new IETS906Processor(logger);
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			return MessageProcessors.FirstOrDefault(x => x.GetType().Name.Contains(message.EM_MessageSubType));
		}

		protected override InboundInterchangeProcessor GetInboundInterchangeProcessor(LoggingInformation logger) => new FRPNTSInboundInterchangeProcessor(logger);
	}
}
