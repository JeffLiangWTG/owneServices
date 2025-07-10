using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXPRELMessageProcessor))]
	sealed class ExportEXPRELMessageProcessorTest : MessageProcessorAbstractTest<ExportEXPRELMessageProcessor, AesInboundEDIMessage<IEXPREL>>
		, ITestEntryLinesLockedAfterProcessing
	{
		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "00DE000000000000E0";
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum.Parent = entryHeader;

			messageMock.Setup(x => x.DataProvider.ReferencedMessageIdentifier).Returns("INVALID_CODE");

			ProcessMessage(message);

			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Reset();
			messageMock.Setup(m => m.DataProvider).Returns((IEXPREL)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		[TestDate(2024, 2, 20, 14, 22, 35)]
		public void TestEntryStatus131()
		{
			entryHeader.CH_EntryStatus = "131";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Status", AesEDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("EntryStatus", "501", entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", "00DE000000000000E0", entryHeader.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate", new ZDateTime(2024, 2, 20, 15, 22, 35), entryHeader.MovementReferenceNumberIssueDate);
				AssertEquals("EntryHeaderReleaseDate", new ZDateTime(2024, 2, 20, 15, 22, 35), entryHeader.CH_EntryReleaseDate);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"AES EXP Release Message Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Release Message Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Release Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for {reference} has a Release Message. For details please follow the Link to the Job.";
				const string bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
						+ "<tr><td>MRN:</td><td>00DE000000000000E0</td></tr>"
						+ "</table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == subject);
				AssertEmailForSingleRecipientWithTable("Message for EntryStatus = 501: ", email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestAddCustomsEntryStatusLog_EntryStatus131()
		{
			entryHeader.CH_EntryStatus = "131";
			ProcessMessage(message);
			AssertEquals("501", entryHeader.GetCustomsEntryStatusEventReference());
		}

		[TestDate(2024, 2, 20, 14, 22, 35)]
		public void TestEntryStatus132()
		{
			entryHeader.CH_EntryStatus = "132";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Status", AesEDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("EntryStatus", "502", entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", "00DE000000000000E0", entryHeader.MovementReferenceNumber);
				AssertEquals("MovementReferenceNumberIssueDate", new ZDateTime(2024, 2, 20, 15, 22, 35), entryHeader.MovementReferenceNumberIssueDate);
				AssertEquals("EntryHeaderReleaseDate", new ZDateTime(2024, 2, 20, 15, 22, 35), entryHeader.CH_EntryReleaseDate);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"AES EXP Release Message Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Release Message Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Release Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for {reference} has a Release Message. For details please follow the Link to the Job.";
				const string bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
						+ "<tr><td>MRN:</td><td>00DE000000000000E0</td></tr>"
						+ "</table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == subject);
				AssertEmailForSingleRecipientWithTable("Message for EntryStatus = 502: ", email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestEntryStatus141()
		{
			entryHeader.CH_EntryStatus = "141";
			ProcessMessage(message);
			AssertEquals("501", entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestEntryStatus142()
		{
			entryHeader.CH_EntryStatus = "142";
			ProcessMessage(message);
			AssertEquals("502", entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestEmailAttachments()
		{
			var reference = entryHeader.Declaration.JE_DeclarationReference;
			var lrn = entryHeader.LocalReferenceNumber;
			var subject = $"AES EXP Release Message Response for {reference} - LRN: {lrn}";
			entryHeader.CH_EntryStatus = "132";
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				},
				new AttachedDocument
				{
					FileName = "file2.pdf",
					Type = new DocumentType { Code = "BBB", Description = "BBB Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("YYY="))
				}
			});
			ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == subject);
			var emailAttachments = email.Attachments.Cast<AttachmentDef>();
			var file1 = emailAttachments.Single(x => x.DisplayName == "file1.pdf");
			var file2 = emailAttachments.Single(x => x.DisplayName == "file2.pdf");
			CombineAssertions(() =>
			{
				AssertEquals("File1 content", Convert.FromBase64String("XXX="), file1.Data);
				AssertEquals("File2 content", Convert.FromBase64String("YYY="), file2.Data);
			});
		}

		public void TestAddCustomsEntryStatusLog_EntryStatus132()
		{
			entryHeader.CH_EntryStatus = "132";
			ProcessMessage(message);
			AssertEquals("502", entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestEntryStatusNot131Or132Or141Or142()
		{
			entryHeader.CH_EntryStatus = "10";
			ProcessMessage(message);
			var stmNote = GetStmNote(message);
			CombineAssertions(() =>
			{
				AssertEquals("Status", AesEDIMessage.Status.Discarded, message.EM_Status);
				AssertEquals("EntryStatus", "10", entryHeader.CH_EntryStatus);
				AssertEquals("NoteText", "Message discarded: Entry Status '131', '132', '141' or '142' expected.", stmNote.ST_NoteText);
				AssertNull("No Email expected", Env.OutgoingCustomsMailManager.EmailsCreated.SingleOrDefault());
			});
		}

		public void TestAddCustomsEntryStatusLog_Not131Or132Or141Or142()
		{
			entryHeader.CH_EntryStatus = "10";
			ProcessMessage(message);
			AssertEquals(ZString.Empty, entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("00DE000000000000E0", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("Reference1234567abcd", message.GetLogbookLocalReferenceNumber());
		}

		[GuiTest]
		public void TestUpdateWarehouse()
		{
			ZString outOfWarehouseWarehousingProcedureCode = "4071";
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			CreateRefProcedure(outOfWarehouseWarehousingProcedureCode, "Y", "N");

			var testHelper = new WhsDataTestHelper<JobDeclaration, OrgSupplierPart, BaseCusClassification, CusClassPartPivot>(Factory);
			var jobDeclaration = testHelper.GetNewDeclaration("EXP", "DECL1234", "23DE1234567890", 5.0m);

			testHelper.WhsWarehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var whsReceive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, jobDeclaration.Importer.PK, "3-DECL1234");
			whsReceive.WD_CustomsParentReference = "3-DECL1234-EDIDATEDI";
			testHelper.GetNewWhsReceiveLine(whsReceive.PK, testHelper.Part.PK, "12345", 11, 11, 11);

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var cusEntryHeader = (CusEntryHeader)jobDeclaration.ActiveEntryHeaders[0];
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "23DE1234567890";
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;
			cusEntryHeader.CH_EntryStatus = "131";
			cusEntryHeader.CH_WarehouseTransactionStatus = "OCP";

			outgoingMessage.EM_LinkedObject = cusEntryHeader;

			CreateEntryLinesAndLinkedInvoicesForWarehouseTest(cusEntryHeader, jobDeclaration, entryInstruction, testHelper.Part, outOfWarehouseWarehousingProcedureCode);

			var msg = messageMock.Object;

			Factory.Save();
			ProcessMessage(msg, true);

			var universalShipmentMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "UDM"));
			AssertNotNull("shipment has been exported", universalShipmentMessage);
		}

		public void TestWarehouseUpdated_TransactionStatusOCP()
		{
			AssertUpdateWarehouse(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, true);
		}

		public void TestWarehouseUpdated_TransactionStatusOUA()
		{
			AssertUpdateWarehouse(WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, true);
		}

		public void TestWarehouseUpdated_TransactionStatusOCW()
		{
			AssertUpdateWarehouse(WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal, true);
		}

		public void TestWarehouseUpdated_TransactionStatusOHD()
		{
			AssertUpdateWarehouse(WarehouseTransactionStatusList.Codes.OutwardHolding, true);
		}

		public void TestWarehouseNotUpdated_TransactionStatusEmpty()
		{
			AssertUpdateWarehouse(string.Empty, false);
		}

		void CreateRefProcedure(ZString procedureCode, string isOutOfWarehouse, string isIntoWarehouse)
		{
			var procedureOutOfWarehouse = Factory.NewWithValidTestData<RefCusProcedure>();
			procedureOutOfWarehouse.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedureOutOfWarehouse.ZZ6_ShipmentType = "EXP";
			procedureOutOfWarehouse.ZZ6_ProcedureCode = procedureCode.Left(2);
			procedureOutOfWarehouse.ZZ6_Concession = procedureCode.PadRight(7).Right(3);
			procedureOutOfWarehouse.ZZ6_OutOfWarehouse = isOutOfWarehouse;
			procedureOutOfWarehouse.ZZ6_IntoWarehouse = isIntoWarehouse;
			procedureOutOfWarehouse.ZZ6_PreviousProcedureCode = procedureCode.Substring(2, 2);
		}

		(CusEntryLine, CusEntryLine) CreateEntryLinesAndLinkedInvoicesForWarehouseTest(CusEntryHeader cusEntryHeader, JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction, OrgSupplierPart part, ZString procedureCode)
		{
			var entryLine1 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var invHeader1 = jobDeclaration.Invoices.Single();
			invHeader1.JZ_InvoiceNumber = "1";

			var invoiceLine = cusEntryHeader.InvoiceLines.Cast<JobComInvoiceLine>().Single();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_Procedure = procedureCode;

			var invHeader2 = jobDeclaration.Invoices.AddNew();
			invHeader2.JZ_InvoiceAmount = 6 * 100m;
			invHeader2.JZ_InvoiceNumber = "2";

			var invoiceLine2 = invHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			invoiceLine2.JI_InvoiceQuantity = 6.0m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsQuantity = 60;
			invoiceLine2.JI_LinePrice = 600;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = procedureCode;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			return (entryLine1, entryLine2);
		}

		void AssertUpdateWarehouse(string warehouseTransactionStatus, bool shouldUpdateWarehouse)
		{
			ZString outOfWarehouseWarehousingProcedureCode = "4071";
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			CreateRefProcedure(outOfWarehouseWarehousingProcedureCode, "Y", "N");

			var testHelper = new WhsDataTestHelper<JobDeclaration, OrgSupplierPart, BaseCusClassification, CusClassPartPivot>(Factory);
			var jobDeclaration = testHelper.GetNewDeclaration("EXP", "DECL1234", "23DE1234567890", 5.0m);

			var cusEntryHeader = (CusEntryHeader)jobDeclaration.ActiveEntryHeaders[0];

			cusEntryHeader.CH_EntryStatus = "502";
			cusEntryHeader.CH_WarehouseTransactionStatus = warehouseTransactionStatus;

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine = cusEntryHeader.AllEntryLines.AddNew();

			var invoiceLine = cusEntryHeader.InvoiceLines.Cast<JobComInvoiceLine>().Single();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = outOfWarehouseWarehousingProcedureCode;

			var dataProviderMock = new Mock<IEXPREL>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1289402357");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXPREL4389");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("00DE000000000000E0");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("Reference1234567abcd");
			dataProviderMock.Setup(m => m.IssuingDateTime).Returns(ZDateTime.UtcNow.ToDateTime());

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPREL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(cusEntryHeader, "EXPREL4389");
			var processor = Processor;
			processor.PreProcessMessage(message);

			AssertEquals(shouldUpdateWarehouse, processor.UpdateWarehouse);
		}

		protected override ZString MessageFriendlyName => "Export EXPREL Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXPREL>> Processor => new ExportEXPRELMessageProcessor(logger);

		public string DeclarationMessageType => Common.Shared.SharedJobMessageTypeList.Codes.Export;

		EDIMessage ITestEntryLinesLockedAfterProcessing.PrepareMessagesAndGetMessageToProcessForEntryLinesLockedTest(CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = "131";
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

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "EXPREL4387");
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IEXPREL>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1289402357");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXPREL4387");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("00DE000000000000E0");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("Reference1234567abcd");
			dataProviderMock.Setup(m => m.IssuingDateTime).Returns(ZDateTime.UtcNow.ToDateTime());

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPREL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		EDIMessage outgoingMessage;
		Mock<IEXPREL> dataProviderMock;
		Mock<AesInboundEDIMessage<IEXPREL>> messageMock;
		AesInboundEDIMessage<IEXPREL> message;
	}
}
