using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.Testing
{
	class InboundInterchangeProcessorForTesting : InboundInterchangeProcessor
	{
		public InboundInterchangeProcessorForTesting(LoggingInformation logger)
			: base(logger)
		{
		}

		public const string AppCode = "~#@";

		protected override string[] ApplicationCodes
		{
			get { return new string[] { AppCode }; }
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new InboundMessageCreatorForTesting();
		}
	}
}
