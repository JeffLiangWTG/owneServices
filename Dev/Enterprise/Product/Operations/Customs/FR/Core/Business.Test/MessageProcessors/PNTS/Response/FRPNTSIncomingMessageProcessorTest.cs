using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class FRPNTSIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestIETS016EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS016Processor>("016");
		}

		public void TestIETS028EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS028Processor>("028");
		}

		public void TestIETS410EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS410Processor>("410");
		}

		public void TestIETS460EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS460Processor>("460");
		}

		public void TestIETS030EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS030Processor>("030");
		}

		public void TestIETS029EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS029Processor>("029");
		}

		public void TestIETS095EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS095Processor>("095");
		}

		public void TestIETS906EDIMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<IETS906Processor>("906");
		}

		void AssertEDIMessageCreatedAndProcessed<T>(string messageSubType)
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageSubType = messageSubType;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_MessageType = "STO";

			var processor = new FRPNTSIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			var applicationTypeProcessor = processor.GetApplicationTypeProcessorCore(message);
			AssertType<T>(applicationTypeProcessor);
		}
	}
}
