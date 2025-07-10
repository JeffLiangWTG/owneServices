using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportCUSTRAMessageProcessor))]
	sealed class ImportCUSTRAMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportCUSTRAMessageProcessor, AtlasInboundEDIMessage<ICUSTRA>>
	{
		public void TestGetLinkedObject_ReferenceNumber()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType("Type is CusEntryHeader", typeof(CusEntryHeader), Message.EM_LinkedObject);
				AssertSame(entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestGetLinkedObject_FromMRN()
		{
			mrnEntryNumber.CE_EntryNum = "23DE12345678901234";

			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			ProcessMessage(Message);

			AssertSame(entryHeader, Message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, Message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSTRA)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.TRA, entryHeader.CH_EntryStatus);
			});
		}

		public void TestEventsCreated()
		{
			ProcessMessage(Message);
			var entryStatus = entryHeader.CH_EntryStatus;
			AssertEquals(entryStatus, entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;
			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CUSTRA – Customs Transmission Message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CUSTRA – Customs Transmission Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Transmission Message. For details please follow the link to the job.";
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

			ProcessMessage(Message);

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

			ProcessMessage(Message);
			AssertContainsExactElementsInAnyOrder(new[] { "ATB150000620520195875", "23DE12345678901234" }, Message.GetLogbookRegistrationNumbers());
		}

		protected override ZString MessageFriendlyName => "Import CUSTRA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSTRA>> Processor => new ImportCUSTRAMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<ICUSTRA> Message => messageMock.Object;

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = "DEA";
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "ABC123456";

			dataProviderMock = new Mock<ICUSTRA>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.ForwardedDate).Returns(new ZDate(2020, 9, 17));
			dataProviderMock.Setup(m => m.Reason).Returns("Any Reason");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSTRA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			Factory.Save();
		}

		CusEntryHeader entryHeader;
		CusEntryNumber mrnEntryNumber;
		Mock<ICUSTRA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSTRA>> messageMock;
		EDIMessage outgoingMessage;
	}
}
