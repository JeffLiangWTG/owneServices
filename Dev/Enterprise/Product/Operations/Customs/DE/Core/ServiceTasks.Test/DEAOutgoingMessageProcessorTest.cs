using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	class DEAOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter() => TestHelper.AssertMessageFilter(
			Factory
			, testProcessor
			, (testProcessor) => ((DEAOutgoingMessageProcessorForTest)testProcessor).MessageFilter
			, atlasMatchesFilter: true);

		public void TestPrepareAtlas()
		{
			var message = Factory.New<AtlasEDIMessage>();
			TestHelper.AssertInterchangePrepared(Factory, processor, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LoggingInformation logger = new LoggingInformation();
			processor = new DEAOutgoingMessageProcessor(logger);
			testProcessor = new DEAOutgoingMessageProcessorForTest(logger);
		}

		DEAOutgoingMessageProcessor processor;

		DEAOutgoingMessageProcessorForTest testProcessor;

		public class DEAOutgoingMessageProcessorForTest : DEAOutgoingMessageProcessor
		{
			public DEAOutgoingMessageProcessorForTest(LoggingInformation logger)
				: base(logger)
			{
			}

			public new ZQuery MessageFilter => base.MessageFilter;
		}
	}
}
