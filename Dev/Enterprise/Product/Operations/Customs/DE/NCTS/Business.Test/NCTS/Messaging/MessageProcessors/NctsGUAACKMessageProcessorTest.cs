using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGUAACKMessageProcessor))]
	sealed class NctsGUAACKMessageProcessorTest : MessageProcessorAbstractTest<NctsGUAACKMessageProcessor, AtlasInboundEDIMessage<IGUAACK>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IGUAACK)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestLinkedObjectNotFound()
		{
			outgoingMessage.EM_MessageNum = "12345";
			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(cusGuaranteeHeader, message.EM_LinkedObject);
		}

		public void TestProcessMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FFL";
			staff.GS_FullName = "Fritze Flink";
			staff.GS_EmailAddress = "ffl@wtg.com";

			Factory.Save();
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("MainAccessCode updated", NewMainAccessCode, cusGuaranteeHeader.MainAccessCode);
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Logbook reference", GuaranteeReferenceNumber, message.GetLogbookLocalReferenceNumber());

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var subject = $"Confirmation of access/administration code change in Guarantee: {GuaranteeReferenceNumber} Response";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = ZString.Empty;
				var bodyMessageSummary = $"Your main access code for GRN {GuaranteeReferenceNumber} has been updated.";
				AssertEmailForSingleRecipient(ZString.Empty, email, "ffl@wtg.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
			});
		}

		protected override ZString MessageFriendlyName => "NCTS GUAACK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IGUAACK>> Processor => new NctsGUAACKMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_Number = GuaranteeReferenceNumber;
			cusGuaranteeHeader.MainAccessCode = "9876";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(cusGuaranteeHeader, ReferencedMessageIdentifier);
			outgoingMessage.CreateOrUpdateNote(LogbookHelper.LogbookGUAMainAccessCode, NewMainAccessCode);

			dataProviderMock = new Mock<IGUAACK>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0624123347");
			dataProviderMock.Setup(m => m.GuaranteeReferenceNumber).Returns(GuaranteeReferenceNumber);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IGUAACK>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}

		Mock<IGUAACK> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IGUAACK>> messageMock;
		AtlasInboundEDIMessage<IGUAACK> message;
		CusGuaranteeHeader cusGuaranteeHeader;
		EDIMessage outgoingMessage;

		const string ReferencedMessageIdentifier = "DE302989100000000000000000000487287";
		const string GuaranteeReferenceNumber = "GRN1234567890";
		const string NewMainAccessCode = "1234";
	}
}
