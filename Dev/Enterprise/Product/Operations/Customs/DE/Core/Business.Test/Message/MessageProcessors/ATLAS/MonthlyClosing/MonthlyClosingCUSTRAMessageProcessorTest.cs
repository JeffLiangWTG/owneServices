using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(MonthlyClosingCUSTRAMessageProcessor))]
	sealed class MonthlyClosingCUSTRAMessageProcessorTest : MessageProcessorAbstractTest<MonthlyClosingCUSTRAMessageProcessor, AtlasInboundEDIMessage<ICUSTRA>>
	{
		public void TestGetLinkedObject_ReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("LinkedObject from ReferenceNumber", reconDeclaration, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_FromMRN()
		{
			mrnEntryNumber.CE_EntryNum = "23DE12345678901234";

			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			ProcessMessage(message);

			AssertEquals("Correct LinkedObject obtained", reconDeclaration, message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSTRA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

			(var line1, var line2) = CreateCusReconEntryLines();
			(var line3, var line4) = CreateCusReconEntryLines();
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("CRD_CustomsStatus", EntryStatus.TRA, reconDeclaration.CRD_CustomsStatus);
				AssertEquals("line1.CRL_CustomsStatus)", EntryStatus.REJ, line1.CRL_CustomsStatus);
				AssertEquals("line2.CRL_CustomsStatus)", EntryStatus.TRA, line2.CRL_CustomsStatus);
				AssertEquals("line3.CRL_CustomsStatus)", EntryStatus.REJ, line3.CRL_CustomsStatus);
				AssertEquals("line4.CRL_CustomsStatus)", EntryStatus.TRA, line4.CRL_CustomsStatus);
			});

			(CusReconEntryLine, CusReconEntryLine) CreateCusReconEntryLines()
			{
				var reconEntry = reconDeclaration.CusReconEntries.AddNew();
				reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
				reconEntry.CRE_EntryType = ImportDeclarationTypeList.Codes.AAV;
				reconEntry.CRE_OA_DeclarantAddress = orgAddress;
				var firstLine = reconEntry.CusReconEntryLines.AddNew();
				firstLine.CRL_CustomsStatus = EntryStatus.REJ;
				firstLine.CRL_LineNumber = 1;
				firstLine.CRL_OriginalEntryLineNumber = 1;
				var secondLine = reconEntry.CusReconEntryLines.AddNew();
				secondLine.CRL_LineNumber = 2;
				secondLine.CRL_OriginalEntryLineNumber = 2;
				return (firstLine, secondLine);
			}
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;
			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			ProcessMessage(message);

			var reference = reconDeclaration.CRD_JobReferenceNumber;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Monthly Closing CUSTRA – Customs Transmission Message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Monthly Closing CUSTRA – Customs Transmission Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DEMonthlyClosing&BusinessEntityPK=" + reconDeclaration.PK;
			var bodyMessageSummary = $@"Your Monthly Closing Declaration for Job {reference} received a Customs Transmission Message. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Registration Number</td><td>ATB150000620520195875</td></tr>"
								   + "<tr><td>MRN</td><td>23DE12345678901234</td></tr>"
								   + "<tr><td>Transmission Date</td><td>17.09.2020</td></tr>"
								   + "<tr><td>Transmission Reason</td><td>Any Reason</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			dataProviderMock.Setup(m => m.ForwardedDate).Returns(ZDate.Empty);
			dataProviderMock.Setup(m => m.Reason).Returns(ZString.Empty);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			CombineAssertions(() =>
			{
				AssertNotContains("No Transmission Date", "<td>Transmission Date</td>", email.Body);
				AssertNotContains("No Transmission Reason", "<td>Transmission Reason</td>", email.Body);
				AssertNotContains("No MRN", "<td>MRN</td>", email.Body);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { "ATB150000620520195875", "23DE12345678901234" }, message.GetLogbookRegistrationNumbers());
		}

		protected override ZString MessageFriendlyName => "Monthly Closing CUSTRA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSTRA>> Processor => new MonthlyClosingCUSTRAMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			reconDeclaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			mrnEntryNumber = CusEntryNumber.LoadOrCreate(reconDeclaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(reconDeclaration, "ABC123456");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMock = new Mock<ICUSTRA>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.ForwardedDate).Returns(new ZDate(2020, 9, 17));
			dataProviderMock.Setup(m => m.Reason).Returns("Any Reason");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSTRA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		CusReconDeclaration reconDeclaration;
		Mock<ICUSTRA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSTRA>> messageMock;
		AtlasInboundEDIMessage<ICUSTRA> message;
		EDIMessage outgoingMessage;
		CusEntryNumber mrnEntryNumber;
	}
}
