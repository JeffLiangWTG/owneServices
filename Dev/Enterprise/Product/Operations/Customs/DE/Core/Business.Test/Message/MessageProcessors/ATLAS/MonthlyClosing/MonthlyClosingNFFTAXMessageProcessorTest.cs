using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(MonthlyClosingNFFTAXMessageProcessor))]
	sealed class MonthlyClosingNFFTAXMessageProcessorTest : MessageProcessorAbstractTest<MonthlyClosingNFFTAXMessageProcessor, AtlasInboundEDIMessage<INFFTAX>>
	{
		public void TestGetLinkedObject_ReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_MRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.MRN).Returns(ReferenceNumber);
			ProcessMessage(message);
			AssertSame(declaration, message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195879");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestLinkedObject_ReferenceNumberMissing()
		{
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns(string.Empty);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((INFFTAX)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestGenerateEmail()
		{
			dataProviderMock.Setup(m => m.MRN).Returns("23DE586601055987B7");

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			var goodsItem1 = new Mock<INFFTAXGoodsItem>();
			goodsItem1.Setup(x => x.SequenceNumber).Returns("1");
			var goodsItem2 = new Mock<INFFTAXGoodsItem>();
			goodsItem2.Setup(x => x.SequenceNumber).Returns("2");
			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			ProcessMessage(message);

			var reference = declaration.CRD_JobReferenceNumber;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"Monthly Closing NFFTAX – Reasons for not finally fixed Import Taxes Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Monthly Closing NFFTAX – Reasons for not finally fixed Import Taxes Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DEMonthlyClosing&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Monthly Closing Declaration for Job {reference} received Reasons for not finally fixed Import Taxes. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
								   + "<tr><td>MRN</td><td>23DE586601055987B7</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>KTG/KTG1/80/6835</td></tr>"
								   + "</table>";
			var goodsItemsTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Affected Lines</td></tr>"
								   + "<tr><td>1</td></tr>"
								   + "<tr><td>2</td></tr>"
								   + "</table>";
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipient("Single", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
				AssertContains("Body-Message-Table", bodyMessageTable, email.Body);
				AssertContains("GoodsItems-Table", goodsItemsTable, email.Body);
			});
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns(string.Empty);
			ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertNotContains("No Local Reference Number", "<td>Local Reference Number</td>", email.Body);
				AssertNotContains("No MRN", "<td>MRN</td>", email.Body);
				AssertNotContains("GoodsItems Table is not shown when it has no rows", @"<td>Affected Lines</td>>", email.Body);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(m => m.MRN).Returns("23DE586601055987B7");

			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { ReferenceNumber, "23DE586601055987B7" }, message.GetLogbookRegistrationNumbers());
		}

		protected override ZString MessageFriendlyName => "Monthly Closing NFFTAX Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<INFFTAX>> Processor => new MonthlyClosingNFFTAXMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = ReferenceNumber;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, "ABC123456");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMock = new Mock<INFFTAX>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("KTG/KTG1/80/6835");
			dataProviderMock.Setup(x => x.GoodsItems).Returns(Array.Empty<INFFTAXGoodsItem>());

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<INFFTAX>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		CusReconDeclaration declaration;
		Mock<INFFTAX> dataProviderMock;
		Mock<AtlasInboundEDIMessage<INFFTAX>> messageMock;
		AtlasInboundEDIMessage<INFFTAX> message;
		EDIMessage outgoingMessage;
		const string ReferenceNumber = "ATB150000620520195875";
	}
}
