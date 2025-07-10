using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.Testing
{
	class InboundInterchangeProcessorForTesting2 : BaseInboundInterchangeProcessor
	{
		public InboundInterchangeProcessorForTesting2(LoggingInformation logger)
			: base(logger)
		{
		}

		public const string AppCode = "~#@";

		protected override string[] ApplicationCodes
		{
			get { return new string[] { AppCode }; }
		}
		protected override bool ProcessInterchange(EDIInterchange interchange)
		{
			return interchange.EI_InterchangeType != "BOB";
		}
	}
}
