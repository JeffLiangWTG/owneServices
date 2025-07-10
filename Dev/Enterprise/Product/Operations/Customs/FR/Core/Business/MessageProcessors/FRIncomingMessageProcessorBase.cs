using System.Collections.Generic;
using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class FRIncomingMessageProcessorBase : BaseMessageProcessor
	{
		protected FRIncomingMessageProcessorBase(LoggingInformation logger)
			: base(logger)
		{
		}
		protected abstract IEnumerable<ApplicationTypeMessageProcessor> GetApplicationTypeMessageProcessors(LoggingInformation logger);
		protected abstract InboundInterchangeProcessor GetInboundInterchangeProcessor(LoggingInformation logger);

		protected override sealed List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.AddRange(GetApplicationTypeMessageProcessors(Logger));
			return result;
		}

		public void ProcessInterchangesAndExecuteBatch(CancellationToken cancellationToken)
		{
			var processor = GetInboundInterchangeProcessor(Logger);
			try
			{
				((IInboundInterchangeProcessor)processor).Execute(cancellationToken);
			}
			finally
			{
				processor.Dispose();
			}

			ExecuteBatch(cancellationToken);
		}

		protected override bool MessageShouldBeProcessedInASeparateFactory => true;
	}
}
