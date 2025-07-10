using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsTRQSTAMessageProcessor))]
	sealed class NctsTRQSTAMessageProcessorTest : MessageProcessorAbstractTest<NctsTRQSTAMessageProcessor, AtlasInboundEDIMessage<ITRQSTA>>
	{
		public void TestLinkedObjectNotFound()
		{
			originalMessage.EM_MessageNum = "NOTORIGINALMSG";

			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("LinkedObject", originalMessage.PK, message.EM_LinkedObject.PK);
				AssertEquals("LinkedObject status", EDIMessage.Status.Acknowledged, originalMessage.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(x => x.DataProvider).Returns((ITRQSTA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
			messageMock.Verify(x => x.DataProvider);
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestDocumentsAttachedToMessage()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { message });
				AssertCollectionNotContains("Document Linking Subscriber not contains Outgoing Message", new BusinessObject[] { originalMessage }, GetDocumentLinking());
			});
		}

		public void TestDocumentsAttachedToMessageIfIsDiscarded()
		{
			var previousMessage = Factory.New<AtlasInboundEDIMessage<ITRQSTA>>();
			previousMessage.EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
			previousMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			previousMessage.EM_MessageNum = MessageIdentifier;
			previousMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			previousMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			previousMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			CombineAssertions(() =>
			{
				ProcessMessage(message);

				AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { message });
				AssertCollectionNotContains("Document Linking Subscriber not contains Outgoing Message", new BusinessObject[] { originalMessage }, GetDocumentLinking());
			});
		}

		public void TestGetCorrectBranchPK()
		{
			CombineAssertions(() =>
			{
				message.EM_GB = Factory.New<GlbCompany>().Branches.AddNew().PK;
				AssertNotEquals("Pre: Different Branches", originalMessage.EM_GB, message.EM_GB);

				ProcessMessage(message);
				AssertEquals("After: Same Branch", originalMessage.EM_GB, message.EM_GB);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals(MovementReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "NCTS TRQSTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ITRQSTA>> Processor => new NctsTRQSTAMessageProcessor(logger);

		protected override bool ExpectedNeedAttachDocumentsToMessage => true;

		protected override bool ExpectedNeedAttachDocumentsToLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();

			originalMessage = Factory.New<StatusRequest>();
			originalMessage.Module = ExportStatusRequestModuleCodeList.Codes.NCTS;
			originalMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			originalMessage.EM_MessageNum = ReferencedMessageIdentifier;

			dataProviderMock = new Mock<ITRQSTA>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(MovementReferenceNumber);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ITRQSTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<ITRQSTA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ITRQSTA>> messageMock;
		AtlasInboundEDIMessage<ITRQSTA> message;
		StatusRequest originalMessage;

		const string MessageIdentifier = "DETQSB58750000000381119050419125839";
		const string ReferencedMessageIdentifier = "DE441715100000000000000000000477553";
		const string MovementReferenceNumber = "22DE000000001234E0";
	}
}
