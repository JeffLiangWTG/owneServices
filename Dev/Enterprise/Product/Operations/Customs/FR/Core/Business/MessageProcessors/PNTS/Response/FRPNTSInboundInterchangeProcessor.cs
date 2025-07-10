using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	class FRPNTSInboundInterchangeProcessor : FRInboundInterchangeProcessorBase
	{
		public FRPNTSInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new PNTSInbondMessageCreator());
		}
		IInboundMessageCreator messageCreator;
	}
}
