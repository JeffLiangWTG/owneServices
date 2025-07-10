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
	[TestedType(typeof(MonthlyClosingCUSNOAMessageProcessor))]
	class MonthlyClosingCUSNOAMessageProcessorTest : MessageProcessorAbstractTest<MonthlyClosingCUSNOAMessageProcessor, AtlasInboundEDIMessage<ICUSNOA>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals("Correct LinkedObject obtained", declaration, message.EM_LinkedObject);
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

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			var goodsItemMock1 = new Mock<ICUSNOAGoodsItem>();
			goodsItemMock1.Setup(x => x.SequenceNumber).Returns("2");
			goodsItemMock1.Setup(x => x.DocumentType).Returns("9BBL");
			goodsItemMock1.Setup(x => x.DocumentReference).Returns("AD357-001");
			goodsItemMock1.Setup(x => x.CancellationWriteOffFlag).Returns("N");

			var goodsItemMock2 = new Mock<ICUSNOAGoodsItem>();
			goodsItemMock2.Setup(x => x.SequenceNumber).Returns("17");
			goodsItemMock2.Setup(x => x.DocumentType).Returns("Y100");
			goodsItemMock2.Setup(x => x.DocumentReference).Returns("002/34689 - 002");
			goodsItemMock2.Setup(x => x.CancellationWriteOffFlag).Returns("Y");
			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[] { goodsItemMock1.Object, goodsItemMock2.Object });

			ProcessMessage(message);

			var reference = declaration.CRD_JobReferenceNumber;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"Monthly Closing CUSNOA – Customs Notification of Attribution Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Monthly Closing CUSNOA – Customs Notification of Attribution Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DEMonthlyClosing&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Monthly Closing Declaration for Job {reference} received a Customs Notification of Attribution. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Registration Number</td><td>ATB150000620520195875</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>KTG/KTG1/80/6835</td></tr>"
								   + "</table>";
			var goodsItemsTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Line Number</td><td>Document Type</td><td>Document Reference</td><td>Cancellation?</td></tr>"
								   + "<tr><td>2</td><td>9BBL</td><td>AD357-001</td><td>N</td></tr>"
								   + "<tr><td>17</td><td>Y100</td><td>002/34689 - 002</td><td>Y</td></tr>"
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
				AssertNotContains("GoodsItems Table is not shown when it has no rows", @"<td>Affected Lines</td>>", email.Body);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("ATB150000620520195875", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "Monthly Closing CUSNOA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSNOA>> Processor => new MonthlyClosingCUSNOAMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, "ABC123456");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMock = new Mock<ICUSNOA>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("KTG/KTG1/80/6835");
			dataProviderMock.Setup(x => x.GoodsItems).Returns(Array.Empty<ICUSNOAGoodsItem>());

			var mockMessage = Factory.NewMoq<AtlasInboundEDIMessage<ICUSNOA>>();
			mockMessage.Setup(x => x.DataProvider).Returns(dataProviderMock.Object);
			message = mockMessage.Object;
			Factory.Save();
		}
		CusReconDeclaration declaration;
		Mock<ICUSNOA> dataProviderMock;
		AtlasInboundEDIMessage<ICUSNOA> message;
		EDIMessage outgoingMessage;
	}
}
