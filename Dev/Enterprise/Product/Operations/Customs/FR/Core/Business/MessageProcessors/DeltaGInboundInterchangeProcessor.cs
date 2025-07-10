using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	class DeltaGInboundInterchangeProcessor : FRInboundInterchangeProcessorBase
	{
		public DeltaGInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new UCC5InboundMessageCreator();
	}
}
