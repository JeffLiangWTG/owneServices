using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXPSTAMessageProcessor))]
	sealed class ExportEXPSTAMessageProcessorTest : MessageProcessorAbstractTest<ExportEXPSTAMessageProcessor, AesInboundEDIMessage<IEXPSTA>>
		, ITestEntryLinesLockedAfterProcessing
	{
		public void TestGetLinkedObjectFromOriginalMessage()
		{
			PrepareGetLinkedObjectFromMRN();
			PrepareGetLinkedObjectFromLRNAndCustomsOfficeOfExport(declaration, "dexpdf1");
			ProcessMessage(message);
			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			var expectedLinkedObject = PrepareGetLinkedObjectFromMRN();
			PrepareGetLinkedObjectFromLRNAndCustomsOfficeOfExport(declaration, "dexpdf1");
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns<string>(null);
			ProcessMessage(message);
			AssertEquals(expectedLinkedObject, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromLRNAndCustomsOffice()
		{
			var expectedLinkedObject = PrepareGetLinkedObjectFromLRNAndCustomsOfficeOfExport(declaration, "dexpdf1");
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns<string>(null);
			ProcessMessage(message);
			AssertEquals(expectedLinkedObject, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromLRNAndCustomsOffice_Logging()
		{
			PrepareGetLinkedObjectFromLRNAndCustomsOfficeOfExport(declaration, "dexpdf1");
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration2.JE_CustomsOffice = "DE005866";
			PrepareGetLinkedObjectFromLRNAndCustomsOfficeOfExport(declaration2, "dexpdf2");
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns<string>(null);

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Warning Log", expected: true, logger.UserLogStrings.Contains($"\tMatched 2 CusEntryHeaders searched by LRN '{lrn}' and CustomsOfficeOfExport 'DE005866'."));
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Reset();
			messageMock.Setup(m => m.DataProvider).Returns((IEXPSTA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestValidMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("EntryHeaderStatusDescription", "Vorankundigung entgegengenommen", entryHeader.EntryHeaderStatusDescription);
				AssertEquals("CH_EntryStatus", "10", entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", mrn, entryHeader.MovementReferenceNumber);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var subject = $"AES EXP Status Message Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Status Message Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Status Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for {reference} has a Status Message. For details please follow the Link to the Job<br /><br />";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
						+ "<tr><td>MRN:</td><td>12DE123456789012E0</td></tr>"
						+ "<tr><td>Status:</td><td>10</td></tr>"
						+ "<tr><td>Status Text:</td><td>Vorankundigung entgegengenommen</td></tr>"
						+ "</table>";
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals(mrn, message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals(lrn, message.GetLogbookLocalReferenceNumber());
		}

		public void TestAddCustomsEntryStatusLog()
		{
			ProcessMessage(message);
			AssertEquals("10", entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestCancelWarehouse_119() => AssertCancelWarehouse("119", 1);
		public void TestCancelWarehouse_191() => AssertCancelWarehouse("191", 1);
		public void TestCancelWarehouse_520() => AssertCancelWarehouse("520", 1);
		public void TestCancelWarehouse_500() => AssertCancelWarehouse("500", 0);
		void AssertCancelWarehouse(string exportStatus, int numberOfExpectedDataexportEvents)
		{
			dataProviderMock.Setup(x => x.ExportStatus).Returns(exportStatus);
			WhsDataTestHelper.CreateOutwardCusProcedureExport(Factory);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "4071";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_WarehouseTransactionStatus = "OCP";

			ProcessMessage(message, doSave: true);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
			var dataTransferEvents = entryHeader.Logs.Find(query);
			AssertEquals(numberOfExpectedDataexportEvents, dataTransferEvents.Length);
		}

		public void TestExitedStatusUpdated()
		{
			entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived;
			dataProviderMock.Setup(m => m.ExportStatus).Returns("520");

			ProcessMessage(message);

			AssertEquals(ExportExitStatus.Codes.UnknownOrNotReported, entryHeader.CH_ExitedStatus);
		}

		public void TestExitedStatusNotUpdatedWhenExitedStatusEXT()
		{
			entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;
			dataProviderMock.Setup(m => m.ExportStatus).Returns("520");

			ProcessMessage(message);

			AssertEquals(ExportExitStatus.Codes.ExitedSatisfactorily, entryHeader.CH_ExitedStatus);
		}

		public void TestExitedStatusNotUpdatedWhenExportStatusNot520()
		{
			entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived;

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertNotEquals("Precondition", "520", dataProviderMock.Object.ExportStatus);
				AssertEquals(ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived, entryHeader.CH_ExitedStatus);
			});
		}

		protected override ZString MessageFriendlyName => "Export EXPSTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXPSTA>> Processor => new ExportEXPSTAMessageProcessor(logger);

		public string DeclarationMessageType => Common.Shared.SharedJobMessageTypeList.Codes.Export;

		EDIMessage ITestEntryLinesLockedAfterProcessing.PrepareMessagesAndGetMessageToProcessForEntryLinesLockedTest(CusEntryHeader entryHeader)
		{
			var outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "1122334455");
			outgoingMessage.EM_ApplicationCode = "DEE";
			outgoingMessage.EM_MessageType = Messaging.EDIMessageTypeList.Codes.AES;
			outgoingMessage.EM_MessageSubType = MessageTypeList.Codes.Export;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(outgoingMessage);
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("1122334455");
			Factory.Save();

			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			const string de = Core.Constants.CountryCodes.Germany;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Customs Status Export AES-EXP");
			helper.CreateCusCodeList(de, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "10", "Vorankundigung entgegengenommen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";

			var groupPK = EnvProxy.Instance.Registry.PostMasterGroup;
			var group = Factory.Load<GlbGroup>(groupPK);
			group.Staff.Add(staff);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "ATB150000620520195875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "EXPSTA5875");
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IEXPSTA>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1234567890");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXPSTA5875");
			dataProviderMock.Setup(m => m.ExportStatus).Returns("10");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(mrn);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(lrn);
			dataProviderMock.Setup(m => m.CustomsOfficeOfExport).Returns("DE005866");

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPSTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}

		const string mrn = "12DE123456789012E0";
		const string lrn = "localReference123";

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		Mock<IEXPSTA> dataProviderMock;
		Mock<AesInboundEDIMessage<IEXPSTA>> messageMock;
		EDIMessage outgoingMessage;
		AesInboundEDIMessage<IEXPSTA> message;

		CusEntryHeader PrepareGetLinkedObjectFromLRNAndCustomsOfficeOfExport(JobDeclaration declarationToUpdate, string messageNum)
		{
			var entryHeaderAsParent = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeaderAsParent.CH_Status = Common.Shared.MessageStatusList.Codes.Sent;
			var dexpdf = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeaderAsParent, messageNum);

			declarationToUpdate.JE_CustomsOffice = "DE005866";
			dexpdf.EM_ApplicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDF);
			dexpdf.EM_ApplicationCode = ApplicationCodeList.Codes.DECustomsAesSystem;
			dexpdf.EM_MessageType = Messaging.EDIMessageTypeList.Codes.AES;
			dexpdf.EM_MessageSubType = MessageTypeList.Codes.Export;
			dexpdf.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			dexpdf.EM_SystemCreateTimeUtc = ZDateTime.Now;
			var entryNum = CusEntryNumber.LoadOrCreate(entryHeaderAsParent, Core.Constants.CountryCodes.Germany);
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			entryNum.CE_EntryNum = lrn;
			Factory.Save();

			return entryHeaderAsParent;
		}

		CusEntryHeader PrepareGetLinkedObjectFromMRN()
		{
			var entryHeaderAsParent = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeaderAsParent.MovementReferenceNumberSetter(mrn);
			return entryHeaderAsParent;
		}
	}
}
