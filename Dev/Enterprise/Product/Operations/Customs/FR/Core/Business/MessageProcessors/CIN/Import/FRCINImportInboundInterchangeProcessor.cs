using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	class FRCINImportInboundInterchangeProcessor : FRInboundInterchangeProcessorBase
	{
		public FRCINImportInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsCIN;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new UCC5InboundMessageCreator();
	}
}
