using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSIncomingMessageProcessor : BaseMessageProcessor<CDSEDIMessage>
	{
		public CDSIncomingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new CDSResponseMessageProcessor(Logger));
			result.Add(new CDSInventoryLinkingControlResponseMessageProcessor(Logger));
			result.Add(new CDSInventoryLinkingMovementResponseMessageProcessor(Logger));
			result.Add(new CDSInventoryLinkingMovementTotalsMessageProcessor(Logger));
			result.Add(new CDSInventoryLinkingQueryResponseMessageProcessor(Logger));
			result.Add(new CDSErrorResponseMessageProcessor(Logger));
			result.Add(new CDSSynchronousResponseMessageProcessor(Logger));
			result.Add(new CDSDeclarationInfoResponseMessageProcessor(Logger));
			result.Add(new CDSDocumentUploadConfirmationMessageProcessor(Logger));
			return result;
		}

		protected override bool MessageShouldBeProcessedInASeparateFactory => true;
	}
}
