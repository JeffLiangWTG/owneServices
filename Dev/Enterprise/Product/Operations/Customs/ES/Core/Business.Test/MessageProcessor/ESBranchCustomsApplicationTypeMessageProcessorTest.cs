using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class ESBranchCustomsApplicationTypeMessageProcessorTest<T, TResponseProvider> : TestCaseWithFactory
		where T : ESBranchCustomsApplicationTypeMessageProcessor<TResponseProvider>
	{
		public void TestApplicationCode()
		{
			AssertEquals("ApplicationCode", "ESC", processor.ApplicationCode);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", GetExpectedProcessorFriendlyName(), processor.MessageFriendlyName);
		}

		public void TestMessageTypesToInclude()
		{
			AssertContainsExactElementsInAnyOrder("MessageTypesToInclude", GetExpectedProcessorMessageTypesToInclude(), processor.MessageTypesToInclude);
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new LoggingInformation();
			processor = GetNewResponseMessageProcessor(logger);
		}
		protected T processor;
		protected LoggingInformation logger;
		protected const string EsCode = CountryCodes.Spain;

		protected abstract T GetNewResponseMessageProcessor(LoggingInformation logger);

		protected abstract ZString GetExpectedProcessorFriendlyName();

		protected abstract ZString[] GetExpectedProcessorMessageTypesToInclude();

		protected void ProcessMessageForTest(TestEdiMessage message, EDIMessageCollection boMessageCollection = null)
		{
			processor.PreProcessMessage(message);
			processor.ProcessMessage(message);

			if (boMessageCollection != null)
			{
				boMessageCollection.Reload(true);
			}
		}
	}
}
