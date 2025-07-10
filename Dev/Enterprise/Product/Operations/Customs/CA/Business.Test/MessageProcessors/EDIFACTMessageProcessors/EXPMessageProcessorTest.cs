using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Business.MessageProcessors.Testing;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class EXPMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestOverrides()
		{
			var expMessageProcessor = new EXPMessageProcessor(logger);
			AssertEquals("ApplicationCode", EDIMessage.ApplicationCodes.CAEXP, expMessageProcessor.ApplicationCode);
			AssertEquals("MessageFriendlyName", "CA Customs G7 Export", expMessageProcessor.MessageFriendlyName);
		}

		public void TestGetMessageProcessorGeneratesErrorsWhenCannotFindProcessor()
		{
			var expMessageProcessor = new EXPMessageProcessor(logger);
			var ediMessage = Factory.NewWithValidTestData<UnknownEDIMessage>();
			var edifactMessage = new Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage();

			try
			{
				expMessageProcessor.GetMessageProcessorCore(ediMessage, edifactMessage);
				Fail("CriticalMessageProcessorException must be trown");
			}
			catch (CriticalMessageProcessorException e)
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Could not find a supporting Processor Class. Message type: {0}", ediMessage.GetType()), e.Message);
			}
		}

		class UnknownEDIMessage : EDIMessageWithBatchNumber
		{
			public UnknownEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}
	}
}
