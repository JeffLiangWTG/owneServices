using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.ServiceTasks.Testing
{
	public class DEMOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter() => TestHelper.AssertMessageFilter(
			Factory
			, testProcessor
			, (testProcessor) => ((DEMOutgoingMessageProcessorForTest)testProcessor).MessageFilter
			, emcsMatchesFilter: true);

		public void TestPrepareEmcs()
		{
			var message = Factory.New<EmcsEDIMessage>();
			TestHelper.AssertInterchangePrepared(Factory, processor, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LoggingInformation logger = new LoggingInformation();
			processor = new DEMOutgoingMessageProcessor(logger);
			testProcessor = new DEMOutgoingMessageProcessorForTest(logger);
		}
		DEMOutgoingMessageProcessor processor;
		DEMOutgoingMessageProcessorForTest testProcessor;

		class DEMOutgoingMessageProcessorForTest : DEMOutgoingMessageProcessor
		{
			public DEMOutgoingMessageProcessorForTest(LoggingInformation logger)
				: base(logger)
			{
			}

			public new ZQuery MessageFilter => base.MessageFilter;
		}
	}
}
