using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaGIncomingMessageProcessor : FRIncomingMessageProcessorBase
	{
		public DeltaGIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IEnumerable<ApplicationTypeMessageProcessor> GetApplicationTypeMessageProcessors(LoggingInformation logger)
		{
			yield return new ImportDeltaCResponseMessageProcessor(Logger);
			yield return new DCGResponseMessageProcessor(Logger);
			yield return new ImportDeltaDResponseMessageProcessor(Logger);
			yield return new ExportDeltaCResponseMessageProcessor(Logger);
			yield return new ExportDeltaDResponseMessageProcessor(Logger);
			yield return new ECSResponseMessageProcessor(Logger);
		}

		protected override InboundInterchangeProcessor GetInboundInterchangeProcessor(LoggingInformation logger)
		{
			return new DeltaGInboundInterchangeProcessor(Logger);
		}

		protected override EDIMessageComparer EDIMessageComparer => new DeltaGEDIMessageComparer(System.ComponentModel.ListSortDirection.Ascending);
	}
}
