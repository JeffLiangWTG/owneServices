using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class DeclarationDocumentEventsHandlerTest : TestCaseWithFactory
	{
		public void TestCanHandleMenuItem()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var handler = new DeclarationDocumentEventsHandler();

			menuItem.SU_MenuName = "Export Declaration Certificate (Korean)";
			AssertEquals(true, handler.CanHandleMenuItem(menuItem));

			menuItem.SU_MenuName = "Random Menu Item";
			AssertEquals(false, handler.CanHandleMenuItem(menuItem));
		}

		public void TestHandleDocumentPrintRequested()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
		}

		public void TestHandleDocumentPrintRequestedIfHasErrors()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = declaration.BrokerAddress?.Header?.PK ?? ZGuid.Empty;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "AAA111";

			CreateNewStatementHeader(declaration.CompanyPK, entry.EntryNumber, "0127020112001320507");

			OrgHeaderWrapper.New(declaration.DutyPayer).ZO_VATDeferment = VATDefermentCodeList.Codes.Conflict;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);

			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.OK);

			handler.HandleDocumentPrintRequested(this, args);
			AssertContains(supporter.DocumentGenerationActions[0].VATDefermentConflictErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void CreateNewStatementHeader(ZGuid companyPK, ZString entryNumber, ZString statementNumber)
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = companyPK;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			statement.B2_StatementNumber = statementNumber;
			statement.B2_StatementAmount = 1000000m;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_SequenceNumber = 1;
			Factory.Save();
		}

		public void TestHandleDocumentPrintRequestedIfHasNotificationWithoutError()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = Common.KR.KRJobMessageTypeList.Codes.Export;
			entryNumber.CE_EntryNum = "AAA111";
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._830;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);

			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);

			handler.HandleDocumentPrintRequested(this, args);
			AssertContains("There are notifications. Are you sure you wish to print this document?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertInitiateForDocumentGeneratingActionsByNoEntryNum(string em_MessageType, string su_MenuName)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entry3 = declaration.CustomsEntryHeaders.AddNew();

			entry1.Messages.AddNew().EM_MessageType = em_MessageType;
			entry1.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.DMS;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = su_MenuName;

			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry1, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			entry2.Messages.AddNew().EM_MessageType = em_MessageType;
			entry2.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			entry3.Messages.AddNew().EM_MessageType = em_MessageType;
			entry3.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.DMS;

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(3, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
		void AssertInitiateForDocumentGeneratingActions(string em_MessageType, string su_MenuName)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entry3 = declaration.CustomsEntryHeaders.AddNew();

			entry1.Messages.AddNew().EM_MessageType = em_MessageType;

			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = em_MessageType;
			entryNum1.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = su_MenuName;

			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry1, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			entry2.Messages.AddNew().EM_MessageType = em_MessageType;
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = em_MessageType;
			entryNum2.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			entry3.Messages.AddNew().EM_MessageType = em_MessageType;
			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = em_MessageType;
			entryNum3.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(3, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		void AssertInitiateForDocumentGeneratingActionsByEntry_CH_Staus(string su_MenuName)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = su_MenuName;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry1, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(3, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		void AssertInitiateForDocumentGeneratingActionsByAmendmentOfLocalExport(string ch_MessageType, string em_MessageType, string fileName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration = new TestDataSetupHelper(Factory).SetLocalExportEntryData(declaration, ch_MessageType, "1111111111111", em_MessageType, fileName);

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(declaration.CustomsEntryHeaders[0], supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			declaration = new TestDataSetupHelper(Factory).SetLocalExportEntryData(declaration, ch_MessageType, "2222222222222", em_MessageType, fileName);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void Test5BD()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5BD, JobDeclarationDocumentSupporter.MenuNames.GoodsRemovalPriorToCustomsRelease);
		}

		public void Test5BA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_AgreedDutyRatePreferenceCode = "A";
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_AgreedDutyRatePreferenceCode = "A";
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_AgreedDutyRatePreferenceCode = ZString.Empty;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = entryInstruction1.PK;

			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			entryNum1.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AgreedRateForAllLines;

			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry1, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_CEI_Instruction = entryInstruction2.PK;
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			entryNum2.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			handler.DocumentSupporter = supporter;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_CEI_Instruction = entryInstruction3.PK;
			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			entryNum3.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			handler.DocumentSupporter = supporter;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void Test5TM()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5TM, JobDeclarationDocumentSupporter.MenuNames.GoldVATDeclaration);
		}

		public void Test5FN()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5FN, JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate);
		}

		public void Test5BF()
		{
			AssertInitiateForDocumentGeneratingActionsByNoEntryNum(ElectronicDocumentTypeList.Codes._5BF, JobDeclarationDocumentSupporter.MenuNames.CancellationOfImportDeclaration);
		}

		public void TestD72()
		{
			AssertInitiateForDocumentGeneratingActionsByNoEntryNum(ElectronicDocumentTypeList.Codes._D72, JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate);
		}

		public void Test5GV()
		{
			AssertInitiateForDocumentGeneratingActionsByNoEntryNum(ElectronicDocumentTypeList.Codes._5GV, JobDeclarationDocumentSupporter.MenuNames.NoticeOfAmendmentOrSupplementaryActions);
		}

		public void Test830_ExportVehicleNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			var entryLine1 = entry1.MergedLines.AddNew();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.VehicleNumbers.AddNew();

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry1, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryLine2 = entry2.MergedLines.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.VehicleNumbers.AddNew();
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			var entryLine3 = entry3.MergedLines.AddNew();
			var invoiceLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.VehicleNumbers.AddNew();
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(3, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestDocumentLocalExportDeclaration()
		{
			AssertInitiateForDocumentGeneratingActionsByEntry_CH_Staus(JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration);
		}

		public void TestDocumentLocalExportGoodsInspectionResultReport()
		{
			AssertInitiateForDocumentGeneratingActionsByEntry_CH_Staus(JobDeclarationDocumentSupporter.MenuNames.LocalExportGoodsInspectionResultReport);
		}
		public void TestDocumentAmendmentOfLocalExportDeclaration_5DR()
		{
			AssertInitiateForDocumentGeneratingActionsByAmendmentOfLocalExport(ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.Codes._5DR, "GOVCBR5DR_Test.xml");
		}
		public void TestDocumentAmendmentOfLocalExportDeclaration_5DS()
		{
			AssertInitiateForDocumentGeneratingActionsByAmendmentOfLocalExport(ElectronicDocumentTypeList.Codes._5DQ, ElectronicDocumentTypeList.Codes._5DS, "GOVCBR5DS_Test.xml");
		}

		public void Test830()
		{
			AssertInitiateForDocumentGeneratingActionsByEntry_CH_Staus(JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate);
		}

		public void Test830English()
		{
			AssertInitiateForDocumentGeneratingActionsByEntry_CH_Staus(JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English);
		}

		public void Test830_3()
		{
			AssertInitiateForDocumentGeneratingActionsByEntry_CH_Staus(JobDeclarationDocumentSupporter.MenuNames.ExportGoodsInspectionResultReport);
		}

		public void Test830_4()
		{
			AssertInitiateForDocumentGeneratingActionsByEntry_CH_Staus(JobDeclarationDocumentSupporter.MenuNames.InspectionPlanAndResultReport);
		}

		public void Test5WN()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5WN, JobDeclarationDocumentSupporter.MenuNames.NoticeOfTaxAdjustment);
		}

		public void Test5GU()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5GU, JobDeclarationDocumentSupporter.MenuNames.CorrectionNoticeOfCountryOfOrigin);
		}

		public void Test5UO()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5UO, JobDeclarationDocumentSupporter.MenuNames.NoticeOfFinalizedRefund);
		}

		public void Test5TW()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5TW, JobDeclarationDocumentSupporter.MenuNames.NoticeOfCorrectionReviewResults);
		}

		public void Test5TV()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5TV, JobDeclarationDocumentSupporter.MenuNames.NoticeOfCustomsMandatedAmendment);
		}

		public void Test5FV()
		{
			AssertInitiateForDocumentGeneratingActions(ElectronicDocumentTypeList.Codes._5FV, JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase);
		}

		public void Test5FK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "AAA111";

			CreateNewStatementHeader(declaration.CompanyPK, entry1.EntryNumber, "0127020112001320507");
			declaration.Reload();
			entry1.Reload();

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges;

			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry1, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "BBB222";

			CreateNewStatementHeader(declaration.CompanyPK, entry2.EntryNumber, "0127020112001320508");
			declaration.Reload();
			entry2.Reload();

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void Test5UL_SingleSnapshot()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNum);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			using (var stream = KRXmlObjectSerializer.Serialize(new Import5ULCreator().Create(entry, refundDetails)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry.Factory.Save();
			}

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.RefundRequest;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);
		}

		public void Test5UL_MultiSnapshots()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum1.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry1, "11111", entryNum1);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			using (var stream = KRXmlObjectSerializer.Serialize(new Import5ULCreator().Create(entry1, refundDetails)))
			{
				var snapshot = entry1.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry1.Factory.Save();
			}

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum2.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry2, "22222", entryNum2);
			refundDetails = new GOVCBR5ULDetails(sendingObject);
			using (var stream = KRXmlObjectSerializer.Serialize(new Import5ULCreator().Create(entry2, refundDetails)))
			{
				var snapshot = entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				snapshot.CES_VersionNumber = (ZShort)2;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry2.Factory.Save();
			}

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum3.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry3, "33333", entryNum3);
			refundDetails = new GOVCBR5ULDetails(sendingObject);
			using (var stream = KRXmlObjectSerializer.Serialize(new Import5ULCreator().Create(entry3, refundDetails)))
			{
				var snapshot = entry3.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				snapshot.CES_VersionNumber = (ZShort)3;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry3.Factory.Save();
			}

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.RefundRequest;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(3, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void Test929_ReImportOfExportedGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.PreviousExpDecLineCollection.AddNew();

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(entry1, supporter.DocumentGenerationActions[0].Entry);
			AssertNull("Where there is only one entry, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.PreviousExpDecLineCollection.AddNew();
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			var entryLine3 = entry3.MergedLines.AddNew();
			entryLine3.PreviousExpDecLineCollection.AddNew();
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(3, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void Test5AS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "22926");

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.JZ_IncoTerm = IncotermList.Codes.FreeOnBoard;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry1 = declaration.CustomsEntryHeaders[0];
			entry1.EntryNumber = "2292620004191X";
			Factory.Save();

			var menuItem = base.Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration;
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var handler = new DeclarationDocumentEventsHandler();
			handler.DocumentSupporter = supporter;
			var args = new DocumentCancelEventArgs(menuItem);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(0, supporter.DocumentGenerationActions.Count);
			AssertNull("Where there is a entry what doesn't have 830 snapshot, then DocumentGeneratingActionForm does not need to be shown", ZFormModaliser.LastFormShownDialogForTest);

			using (var stream = KRXmlObjectSerializer.Serialize(new ExportEntryHeaderCreator().Create(entry1)))
			{
				var snapshot = entry1.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry1.Factory.Save();
			}

			handler.DocumentSupporter = supporter;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals("The form must display although it has only 1 when it is amendment", typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.JZ_IncoTerm = IncotermList.Codes.CostInsuranceAndFreight;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry2 = declaration.CustomsEntryHeaders[1];
			entry2.EntryNumber = "2292620004192X";

			using (var stream = KRXmlObjectSerializer.Serialize(new ExportEntryHeaderCreator().Create(entry2)))
			{
				var snapshot = entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry2.Factory.Save();
			}

			handler.DocumentSupporter = supporter;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals(2, supporter.DocumentGenerationActions.Count);
			AssertEquals(typeof(DocumentGeneratingActionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}
}
