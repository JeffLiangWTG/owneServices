using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEInboundInterchangeProcessor : FRInboundInterchangeProcessorBase
	{
		public DeltaIEInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new DeltaIEInboundMessageCreator();
		}
	}
}
