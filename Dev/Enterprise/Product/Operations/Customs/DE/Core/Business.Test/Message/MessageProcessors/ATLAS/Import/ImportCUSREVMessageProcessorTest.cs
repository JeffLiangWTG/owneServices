using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
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
	[TestedType(typeof(ImportCUSREVMessageProcessor))]
	class ImportCUSREVMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportCUSREVMessageProcessor, AtlasInboundEDIMessage<ICUSREV>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType<CusEntryHeader>("Type is CusEntryHeader", Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestGetLinkedObject_MRN()
		{
			mrnEntryNumber.CE_EntryNum = "24DE123050554788M5";
			dataProviderMock.Setup(m => m.CancelledMRN).Returns("24DE123050554788M5");
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType<CusEntryHeader>("Type is CusEntryHeader", Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.CancelledReferenceNumber).Returns("ATB150000620920205876");
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("Message status", AtlasEDIMessage.Status.Error, Message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSREV)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestsProcessMessageSuccessfully()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AtlasEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.REV, entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", "ATC400533951120184849", entryHeader.MovementReferenceNumber);
			});
		}

		public void TestProcessMessageSuccessfully_MRN_RefNrIsEmpty()
		{
			mrnEntryNumber.CE_EntryNum = "23DE123050554788M5";

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.CancelledReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.CancelledMRN).Returns("23DE123050554788M5");
			dataProviderMock.Setup(m => m.MRN).Returns("24DE123050554788M5");

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AtlasEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.REV, entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", "24DE123050554788M5", entryHeader.MovementReferenceNumber);
			});
		}

		public void TestProcessMessageSuccessfully_MRNAndReferenceNumber_RefNrIsLinked()
		{
			dataProviderMock.Setup(m => m.MRN).Returns("24DE123050554788M5");
			dataProviderMock.Setup(m => m.CancelledMRN).Returns("23DE123050554788M5");
			Factory.Save();

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AtlasEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.REV, entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", "ATC400533951120184849", entryHeader.MovementReferenceNumber);
			});
		}

		public void TestProcessMessageSuccessfully_MRNAndReferenceNumber_MRNIsLinked()
		{
			mrnEntryNumber.CE_EntryNum = "23DE123050554788M5";

			dataProviderMock.Setup(m => m.MRN).Returns("24DE123050554788M5");
			dataProviderMock.Setup(m => m.CancelledMRN).Returns("23DE123050554788M5");

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AtlasEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.REV, entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", "24DE123050554788M5", entryHeader.MovementReferenceNumber);
			});
		}

		public void TestEventsCreated()
		{
			ProcessMessage(Message);
			var entryStatus = entryHeader.CH_EntryStatus;
			AssertEquals(entryStatus, entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestMovementReferenceNumberNotUpdatedWhenReferenceNumberIsEmpty()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			ProcessMessage(Message);
			AssertEquals("MovementReferenceNumber", "ATB150000620920205875", entryHeader.MovementReferenceNumber);
		}

		public void TestDocumentsAttached()
		{
			ProcessMessage(Message);
			AssertDocumentLinkingSubscribers(new BusinessObject[] { entryHeader });
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			mrnEntryNumber.CE_EntryNum = "23DE123050554788M5";

			dataProviderMock.Setup(m => m.MRN).Returns("24DE123050554788M5");
			dataProviderMock.Setup(m => m.CancelledMRN).Returns("23DE123050554788M5");

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"Import CUSREV – Customs Reverse Message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CUSREV – Customs Reverse Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Reverse Message. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>New MRN</td><td>24DE123050554788M5</td></tr>"
								   + "<tr><td>Canceled MRN</td><td>23DE123050554788M5</td></tr>"
								   + "<tr><td>New Registration Number</td><td>ATC400533951120184849</td></tr>"
								   + "<tr><td>Canceled Registration Number</td><td>ATB150000620920205875</td></tr>"
								   + "<tr><td>Cancellation Reason</td><td>Transmitted by default</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable("Single", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.Reason).Returns(ZString.Empty);
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;
			ProcessMessage(Message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertNotContains("No Reference Number", "<td>New Registration Number</td>", email.Body);
				AssertNotContains("No Reason", "<td>Cancellation Reason</td>", email.Body);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(Message);
			AssertEquals("ATC400533951120184849, ATB150000620920205875", Message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_MRN()
		{
			mrnEntryNumber.CE_EntryNum = "23DE123050554788M5";
			dataProviderMock.Setup(m => m.MRN).Returns("24DE123050554788M5");
			dataProviderMock.Setup(m => m.CancelledMRN).Returns("23DE123050554788M5");

			ProcessMessage(Message);
			AssertEquals("ATC400533951120184849, ATB150000620920205875, 24DE123050554788M5, 23DE123050554788M5", Message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "Import CUSREV Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSREV>> Processor => new ImportCUSREVMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<ICUSREV> Message => messageMock.Object;

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620920205875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMock = new Mock<ICUSREV>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000001");
			dataProviderMock.Setup(x => x.CancelledReferenceNumber).Returns("ATB150000620920205875");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC400533951120184849");
			dataProviderMock.Setup(m => m.Reason).Returns("Transmitted by default");
			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSREV>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		Mock<ICUSREV> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSREV>> messageMock;
		EDIMessage outgoingMessage;
		CusEntryNumber mrnEntryNumber;
	}
}
