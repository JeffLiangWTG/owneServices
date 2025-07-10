using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXQSTAMessageProcessor))]
	class ExportEXQSTAMessageProcessorTest : MessageProcessorAbstractTest<ExportEXQSTAMessageProcessor, AesInboundEDIMessage<IEXQSTA>>
	{
		protected override ZString MessageFriendlyName => "Export EXQSTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXQSTA>> Processor => new ExportEXQSTAMessageProcessor(logger);

		protected override bool ExpectedNeedAttachDocumentsToMessage => true;

		protected override bool ExpectedNeedAttachDocumentsToLinkedObject => false;

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertType("Type is StatusRequest", typeof(StatusRequest), message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", originalMessage, message.EM_LinkedObject);
				AssertEquals("LinkedObject status", EDIMessage.Status.Acknowledged, originalMessage.EM_Status);
			});
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("50000002");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", AesEDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IEXQSTA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestDocumentsAttached()
		{
			ProcessMessage(message);
			AssertDocumentLinkingSubscribers(new BusinessObject[] { message });
		}

		public void TestProcessMessageCore()
		{
			ProcessMessage(message);
			AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("21DE123050554788M5", message.GetLogbookRegistrationNumber());
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalMessage = Factory.New<StatusRequest>();
			originalMessage.Module = ExportStatusRequestModuleCodeList.Codes.AES;
			originalMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			originalMessage.EM_MessageNum = ReferencedMessageIdentifier;

			dataProviderMock = new Mock<IEXQSTA>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("21DE123050554788M5");

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXQSTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<IEXQSTA> dataProviderMock;
		Mock<AesInboundEDIMessage<IEXQSTA>> messageMock;
		StatusRequest originalMessage;
		AesInboundEDIMessage<IEXQSTA> message;
		const string ReferencedMessageIdentifier = "50000001";
	}
}
