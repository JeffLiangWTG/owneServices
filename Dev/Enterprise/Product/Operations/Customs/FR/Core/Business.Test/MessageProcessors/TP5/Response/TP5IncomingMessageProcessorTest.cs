using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class TP5IncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEDIMessageComparer()
		{
			var processor = new NCTSIncomingMessageProcessorForTest(new LoggingInformation());
			AssertType<EdiMessages.NctsEDIMessageComparer>(processor.EDIMessageComparerExposed);
		}

		class NCTSIncomingMessageProcessorForTest(LoggingInformation logger) : TP5IncomingMessageProcessor(logger)
		{
			public EDIMessageComparer EDIMessageComparerExposed => base.EDIMessageComparer;
		}

		public void TestMessageCreatedAndProcessed()
		{
			AssertEDIMessageCreatedAndProcessed<CC004CProcessor>("004");
			AssertEDIMessageCreatedAndProcessed<CC009CProcessor>("009");
			AssertEDIMessageCreatedAndProcessed<CC019CProcessor>("019");
			AssertEDIMessageCreatedAndProcessed<CC022CProcessor>("022");
			AssertEDIMessageCreatedAndProcessed<CC025CProcessor>("025");
			AssertEDIMessageCreatedAndProcessed<CC028CProcessor>("028");
			AssertEDIMessageCreatedAndProcessed<CC029CProcessor>("029");
			AssertEDIMessageCreatedAndProcessed<CC035CProcessor>("035");
			AssertEDIMessageCreatedAndProcessed<CC043CProcessor>("043");
			AssertEDIMessageCreatedAndProcessed<CC045CProcessor>("045");
			AssertEDIMessageCreatedAndProcessed<CC055CProcessor>("055");
			AssertEDIMessageCreatedAndProcessed<CC056CProcessor>("056");
			AssertEDIMessageCreatedAndProcessed<CC057CProcessor>("057");
			AssertEDIMessageCreatedAndProcessed<CC140CProcessor>("140");
			AssertEDIMessageCreatedAndProcessed<CC182CProcessor>("182");
			AssertEDIMessageCreatedAndProcessed<CD906CProcessor>("906");
			AssertEDIMessageCreatedAndProcessed<CC917CProcessor>("917");
			AssertEDIMessageCreatedAndProcessed<CC928CProcessor>("928");
			AssertEDIMessageCreatedAndProcessed<CCF02CProcessor>("F02");
			AssertEDIMessageCreatedAndProcessed<CCF03CProcessor>("F03");
		}

		void AssertEDIMessageCreatedAndProcessed<T>(string messageSubType)
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageType = MessageTypeList.Codes.TP5;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;

			var processor = new TP5IncomingMessageProcessor(new LoggingInformation());
			var applicationTypeProcessor = processor.GetApplicationTypeProcessorCore(message);
			AssertType<T>(applicationTypeProcessor);
		}
	}
}
