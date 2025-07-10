using System.Collections.Generic;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors.Ncts.Response;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class TP5IncomingMessageProcessor : FRIncomingMessageProcessorBase
	{
		public TP5IncomingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override IEnumerable<ApplicationTypeMessageProcessor> GetApplicationTypeMessageProcessors(LoggingInformation logger)
		{
			yield return new CC004CProcessor(logger);
			yield return new CC009CProcessor(logger);
			yield return new CC019CProcessor(logger);
			yield return new CC022CProcessor(logger);
			yield return new CC025CProcessor(logger);
			yield return new CC028CProcessor(logger);
			yield return new CC029CProcessor(logger);
			yield return new CC035CProcessor(logger);
			yield return new CC043CProcessor(logger);
			yield return new CC045CProcessor(logger);
			yield return new CC055CProcessor(logger);
			yield return new CC056CProcessor(logger);
			yield return new CC057CProcessor(logger);
			yield return new CC140CProcessor(logger);
			yield return new CC182CProcessor(logger);
			yield return new CD906CProcessor(logger);
			yield return new CC917CProcessor(logger);
			yield return new CC928CProcessor(logger);
			yield return new CCF02CProcessor(logger);
			yield return new CCF03CProcessor(logger);
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			return MessageProcessors.FirstOrDefault(x => x.GetType().Name.Contains(message.EM_MessageSubType));
		}

		protected override InboundInterchangeProcessor GetInboundInterchangeProcessor(LoggingInformation logger)
		{
			return new TP5InboundInterchangeProcessor(logger);
		}
		protected override EDIMessageComparer EDIMessageComparer => new NctsEDIMessageComparer(System.ComponentModel.ListSortDirection.Ascending);
	}
}
