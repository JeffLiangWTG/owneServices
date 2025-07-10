using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CSWOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			var cswMessage = Factory.New<EDIMessage>();
			cswMessage.EM_ApplicationCode = ApplicationCodeList.Codes.CNCustomsSingleWindow;
			var nexMessage = Factory.New<EDIMessage>();
			nexMessage.EM_ApplicationCode = ApplicationCodeList.Codes.AUCustomsNEXDOC;

			var processor = new CSWOutgoingMessageProcessorForTest(new LoggingInformation());

			CombineAssertions(() =>
			{
				AssertEquals("CSW matches", true, cswMessage.MatchesFilter(processor.MessageFilter));
				AssertEquals("NEX does not match", false, nexMessage.MatchesFilter(processor.MessageFilter));
				AssertEquals("OrderBy", EDIMessage.Schema.EM_MessageNum, processor.MessageFilter.OrderBy);
			});
		}

		public void TestCreateNewInterchangeProvider()
		{
			var processor = new CSWOutgoingMessageProcessorForTest(new LoggingInformation());
			var provider = processor.CreateNewInterchangeProvider(new NonDependentEDIMessageCollection(Factory));
			AssertType<CSWInterchangeProvider>(provider);
		}
	}

	public class CSWOutgoingMessageProcessorForTest : CSWOutgoingMessageProcessor
	{
		public CSWOutgoingMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public new ZQuery MessageFilter => base.MessageFilter;

		public new InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => base.CreateNewInterchangeProvider(readyMessages);
	}
}
