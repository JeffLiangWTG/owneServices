using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEIMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override ZString GetExpectedMessageCode() => "SEI";

		protected override ZString GetExpectedMessageName() => "Sea Cargo Establishment Information  - (SEI)";

		protected override Type IncomingMessageType => typeof(CMRSEIMessage);

		protected override void SetUp()
		{
			base.SetUp();
			var logger = new LoggingInformation();
			processor = new SEIMessageProcessor(logger);
		}
		SEIMessageProcessor processor;
	}
}
