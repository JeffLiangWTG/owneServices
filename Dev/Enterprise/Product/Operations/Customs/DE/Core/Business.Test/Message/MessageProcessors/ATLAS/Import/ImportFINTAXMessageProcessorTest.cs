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
	[TestedType(typeof(ImportFINTAXMessageProcessor))]
	class ImportFINTAXMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportFINTAXMessageProcessor, AtlasInboundEDIMessage<IFINTAX>>
	{
		public void TestGetLinkedObject_ReferenceNumber()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType("Type is CusEntryHeader", typeof(CusEntryHeader), Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestGetLinkedObject_Mrn()
		{
			mrnEntryNumber.CE_EntryNum = "24DE12345678901234";

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType("Type is CusEntryHeader", typeof(CusEntryHeader), Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, Message.EM_Status);
				AssertEquals("CH_EntryStatus", ZString.Empty, entryHeader.CH_EntryStatus);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IFINTAX)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AtlasEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.TXF, entryHeader.CH_EntryStatus);
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
			var outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var reference = declaration.JE_DeclarationReference;

			var subject = $"Import FINTAX – Final Tax Assessment Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import FINTAX – Final Tax Assessment Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Final Tax Assessment. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>MRN</td><td>24DE12345678901234</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>LocalReferenceNumber</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(Message);
			AssertEquals("ATC996151771020016389, 24DE12345678901234", Message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "Import FINTAX Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IFINTAX>> Processor => new ImportFINTAXMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<IFINTAX> Message => messageMock.Object;

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATC996151771020016389";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			dataProviderMock = new Mock<IFINTAX>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("FINTAX20833294803129238170736212505");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016389");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("LocalReferenceNumber");
			dataProviderMock.Setup(m => m.MRN).Returns("24DE12345678901234");
			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IFINTAX>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		Mock<IFINTAX> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IFINTAX>> messageMock;
		CusEntryNumber mrnEntryNumber;
	}
}
