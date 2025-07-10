using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Ncts.Response
{
	public class TP5InboundInterchangeProcessor : FRInboundInterchangeProcessorBase
	{
		public TP5InboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new TP5InboundMessageCreator();
		}
	}
}
