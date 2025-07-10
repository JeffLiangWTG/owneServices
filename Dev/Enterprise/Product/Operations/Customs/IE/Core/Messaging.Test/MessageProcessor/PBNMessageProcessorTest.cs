using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	sealed class PBNMessageProcessorTest : TestCaseWithFactory
	{
		public void TestIsBranchFilter()
		{
			var processor = new PBNMessageProcessorForTest(new LoggingInformation());
			AssertEquals(false, processor.IsBranchFilter);
		}

		public void TestMessageFilter()
		{
			var pbnMessage = Factory.New<EDIMessage>();
			pbnMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsPBN;
			var ieiMessage = Factory.New<EDIMessage>();
			ieiMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsImport;

			var processor = new PBNMessageProcessorForTest(new LoggingInformation());

			CombineAssertions(() =>
			{
				var filter = processor.MessageFilter;
				AssertEquals("PBN matches", true, pbnMessage.MatchesFilter(filter));
				AssertEquals("IEI does not match", false, ieiMessage.MatchesFilter(filter));
			});
		}

		public void TestCreateNewInterchangeProvider()
		{
			var processor = new PBNMessageProcessorForTest(new LoggingInformation());
			var provider = processor.CreateNewInterchangeProvider(new NonDependentEDIMessageCollection(Factory));
			AssertType<PBNInterchangeProvider>(provider);
		}

		public class PBNMessageProcessorForTest : PBNMessageProcessor
		{
			public PBNMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public new bool IsBranchFilter => base.IsBranchFilter;

			public new ZQuery MessageFilter => base.MessageFilter;

			public new InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => base.CreateNewInterchangeProvider(readyMessages);

			public new void HandleBeforeMessageProcessing(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory) => base.HandleBeforeMessageProcessing(readyMessages, factory);
		}
	}
}
