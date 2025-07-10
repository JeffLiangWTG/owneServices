using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	sealed class CustomsAndExciseReportMessageProcessorTest : TestCaseWithFactory
	{
		public void TestIsBranchFilter()
		{
			var processor = new CustomsAndExciseReportMessageProcessorForTest(new LoggingInformation());
			AssertEquals(false, processor.IsBranchFilter);
		}

		public void TestMessageFilter()
		{
			var ierMessage = Factory.New<EDIMessage>();
			ierMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsAndExcise;
			var ieiMessage = Factory.New<EDIMessage>();
			ieiMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsImport;

			var processor = new CustomsAndExciseReportMessageProcessorForTest(new LoggingInformation());

			CombineAssertions(() =>
			{
				var filter = processor.MessageFilter;
				AssertEquals("IER matches", true, ierMessage.MatchesFilter(filter));
				AssertEquals("IEI does not match", false, ieiMessage.MatchesFilter(filter));
			});
		}

		public void TestCreateNewInterchangeProvider()
		{
			var processor = new CustomsAndExciseReportMessageProcessorForTest(new LoggingInformation());
			var provider = processor.CreateNewInterchangeProvider(new NonDependentEDIMessageCollection(Factory));
			AssertType<CustomsAndExciseReportInterchangeProvider>(provider);
		}

		public class CustomsAndExciseReportMessageProcessorForTest : CustomsAndExciseReportMessageProcessor
		{
			public CustomsAndExciseReportMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public new bool IsBranchFilter => base.IsBranchFilter;

			public new ZQuery MessageFilter => base.MessageFilter;

			public new InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => base.CreateNewInterchangeProvider(readyMessages);

			public new void HandleBeforeMessageProcessing(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory) => base.HandleBeforeMessageProcessing(readyMessages, factory);
		}
	}
}
