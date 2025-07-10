using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	class FRPortsInboundInterchangeProcessor : FRInboundInterchangeProcessorBase
	{
		public FRPortsInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string GenericMessageInterchangeType => FREDIMessage.ApplicationCodes.FRPortMessage;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new UCC5InboundMessageCreator();
	}
}
