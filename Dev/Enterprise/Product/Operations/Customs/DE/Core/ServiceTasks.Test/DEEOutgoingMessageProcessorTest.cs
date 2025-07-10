using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	class DEEOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter() => TestHelper.AssertMessageFilter(
			Factory
			, testProcessor
			, (testProcessor) => ((DEEOutgoingMessageProcessorForTest)testProcessor).MessageFilter
			, aesMatchesFilter: true);

		public void TestPrepareAes()
		{
			var message = Factory.New<AesEDIMessage>();
			TestHelper.AssertInterchangePrepared(Factory, processor, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LoggingInformation logger = new LoggingInformation();
			processor = new DEEOutgoingMessageProcessor(logger);
			testProcessor = new DEEOutgoingMessageProcessorForTest(logger);
		}
		DEEOutgoingMessageProcessor processor;
		DEEOutgoingMessageProcessorForTest testProcessor;
	}

	public class DEEOutgoingMessageProcessorForTest : DEEOutgoingMessageProcessor
	{
		public DEEOutgoingMessageProcessorForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		public new ZQuery MessageFilter => base.MessageFilter;
	}
}
