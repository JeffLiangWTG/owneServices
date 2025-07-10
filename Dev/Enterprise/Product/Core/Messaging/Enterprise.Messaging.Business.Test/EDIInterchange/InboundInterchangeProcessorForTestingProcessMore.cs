using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.Testing
{
	class InboundInterchangeProcessorForTestingProcessMore : InboundInterchangeProcessor
	{
		public InboundInterchangeProcessorForTestingProcessMore(LoggingInformation logger)
			: base(logger)
		{
		}

		public const string AppCode = "~#@";

		protected override string[] ApplicationCodes => new string[] { AppCode };
		protected override bool ShouldCheckForMore => true;
		int noOfInt;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			if (noOfInt++ < 2)
			{
				var factory = new BusinessObjectFactory();
				IncomingInterchangeProcessorTest.CreateAndSaveInterchange(factory, AppCode, "SND", "RCV", "header", "data", "");
			}
			return new InboundMessageCreatorForTesting();
		}
	}
}
