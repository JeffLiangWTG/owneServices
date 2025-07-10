using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.AIS;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business.Testing
{
	class BranchMessageProcessorTest : TestCaseWithFactory
	{
		public void TestGetCachedApplicationTypeProcessor()
		{
			var processor = new BranchMessageProcessor();
			var message = TestHelper.GetIM415VMessage(Factory);
			var applicationTypeProcessor = processor.GetApplicationTypeProcessorCore(message);
			AssertType<IM415VProcessor>("Found", applicationTypeProcessor);
			AssertSame("Cached", applicationTypeProcessor, processor.GetApplicationTypeProcessorCore(message));
		}

		public void TestGetApplicationTypeProcessorCore()
		{
			var processor = new BranchMessageProcessor();
			CombineAssertions(() =>
			{
				AssertType<IM415VProcessor>("Found IM415V Processor", processor.GetApplicationTypeProcessorCore(TestHelper.GetIM415VMessage(Factory)));
				AssertType<IM917Processor>("Found IM917 Processor", processor.GetApplicationTypeProcessorCore(TestHelper.GetIM917Message(Factory)));

				AssertType<PSRProcessor>("Found CustomsAndExciseReportInboundMessageProcessor", processor.GetApplicationTypeProcessorCore(TestHelper.GetPSRMessage(Factory)));

				var emcsProcessorType = ObjectFactory.Get<IEMCSResponseMessageDetails>().GetResponseDetail("801").ProcessorType;
				AssertEquals("Found EMCS Processor", emcsProcessorType, processor.GetApplicationTypeProcessorCore(TestHelper.GetIE801Message(Factory)).GetType());

				AssertNull("Not found", processor.GetApplicationTypeProcessorCore(Factory.New<BaseEDIMessage>()));

				AssertType<MessageAcknowledgementProcessor>("MessageAcknowledgementProcessor", processor.GetApplicationTypeProcessorCore(TestHelper.GetAcknowledgementMessage(Factory)));
			});
		}

		public void TestGetMessageProcessorQueryIncludesNCTS()
		{
			var message = Factory.New<BaseEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsNCTS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			AssertEquals("Should include NCTS", message.PK, NewFactory().Load<EDIMessage>(new BranchMessageProcessor().GetMessageProcessorQuery()).Single().PK);
		}

		public void TestGetMessageProcessorQueryIncludesAISUCC5()
		{
			var message = Factory.New<BaseEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsUCC5Import;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			AssertEquals("Should include AIS UCC5", message.PK, NewFactory().Load<EDIMessage>(new BranchMessageProcessor().GetMessageProcessorQuery()).Single().PK);
		}

		public void TestEDIMessageComparer()
		{
			AssertType<IEEDIMessageComparer>("EDIMessageComparer Type", new BranchMessageProcessorForTest().EDIMessageComparerExposedForTesting);
		}

		public void TestMessagesPerSave()
		{
			AssertEquals("We can get multiple messages together, e.g. 528 & 529. We want to process them separately so that all events are logged", 1, new BranchMessageProcessorForTest().MessagesPerSaveExposed);
		}

		class BranchMessageProcessorForTest : BranchMessageProcessor
		{
			public Enterprise.Messaging.Business.EDIMessageComparer EDIMessageComparerExposedForTesting => base.EDIMessageComparer;

			public int MessagesPerSaveExposed => base.MessagesPerSaveCore;
		}
	}
}
