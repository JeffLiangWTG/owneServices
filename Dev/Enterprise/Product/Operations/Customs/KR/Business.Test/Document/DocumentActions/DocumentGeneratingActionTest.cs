using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(DocumentGeneratingAction))]
	sealed class DocumentGeneratingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestElements()
		{
			var action = (DocumentGeneratingAction)GetNewBusinessObject();
			AssertEquals("ABC111", action.EntryNumber);
			AssertEquals(false, action.ToBeDelivered);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new DocumentGeneratingAction(entry, "ABC111", JobDeclarationDocumentSupporter.DataContexts.EXPEntryHeaderBO);
		}

		public void TestValidateToBeDeliveredFor5BD()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.GoodsRemovalPriorToCustomsRelease;
			AssertionValidateToBeDeliveredWhenCE_EntryStatusHasMessageStatus(ElectronicDocumentTypeList.Codes._5BD, ElectronicDocumentTypeList.Codes._5BD, menuName);
		}

		public void TestValidateToBeDeliveredFor5BA()
		{
			var originalRejectedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalRejected, CustomsMessageStatusTypeList.Descriptions.OriginalRejected);
			var originalAcceptedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Descriptions.OriginalAccepted);

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AgreedRateForAllLines;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "A";

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entrynum1 = entry1.EntryNumbers.AddNew();
			entrynum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			entrynum1.CE_EntryStatus = originalRejectedPair.Code;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entrynum2 = entry2.EntryNumbers.AddNew();
			entrynum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			entrynum2.CE_EntryStatus = originalAcceptedPair.Code;

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);
			AssertEquals(0, collection.Count);

			entry1.CH_CEI_Instruction = entryInstruction.PK;
			entry2.CH_CEI_Instruction = entryInstruction.PK;

			collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			AssertEquals(2, collection.Count);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(originalRejectedPair.Description, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(originalAcceptedPair.Description, action2.StatusDescription);
		}

		public void TestValidateToBeDeliveredFor5TM()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.GoldVATDeclaration;
			AssertionValidateToBeDeliveredWhenCE_EntryStatusHasMessageStatus(ElectronicDocumentTypeList.Codes._5TM, ElectronicDocumentTypeList.Codes._5TM, menuName);
		}

		public void TestValidateToBeDeliveredFor5FN()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate;
			AssertionValidateToBeDeliveredWhenCE_EntryStatusHasMessageStatus(ElectronicDocumentTypeList.Codes._5FN, ElectronicDocumentTypeList.Codes._5FN, menuName);
		}

		public void TestValidateToBeDeliveredFor5BF()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.CancellationOfImportDeclaration;
			AssertionValidateToBeDeliveredWhenCH_EntryStatusHasEntryStatus(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BF, menuName);
		}

		public void TestValidateToBeDeliveredForD72()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate;
			AssertionValidateToBeDeliveredWhenCE_EntryStatusHasMessageStatus(ElectronicDocumentTypeList.Codes._D72, ElectronicDocumentTypeList.Codes._D72, menuName);
		}

		public void TestValidateToBeDeliveredFor830_ExportVehicleNo()
		{
			var entryDeclinedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalRejected, CustomsMessageStatusTypeList.Descriptions.OriginalRejected);
			var entryApprovedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Descriptions.OriginalAccepted);
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = entryDeclinedPair.Code;
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = "EXP";
			entryNumber1.CE_EntryNum = "AAA111";
			var entryLine1 = entry1.MergedLines.AddNew();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.VehicleNumbers.AddNew();

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = entryApprovedPair.Code;
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = "EXP";
			entryNumber2.CE_EntryNum = "BBB222";
			var entryLine2 = entry2.MergedLines.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.VehicleNumbers.AddNew();

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(entryDeclinedPair.Description, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(entryApprovedPair.Description, action2.StatusDescription);
		}

		public void TestValidateToBeDeliveredFor5DP5DQ_1()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration;
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(Common.KR.KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DP, menuName);
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(Common.KR.KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DQ, menuName);
		}

		public void TestValidateToBeDeliveredFor5DP5DQ_2()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.LocalExportGoodsInspectionResultReport;
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(Common.KR.KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DP, menuName);
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(Common.KR.KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DQ, menuName);
		}

		public void TestValidateToBeDeliveredFor5DS()
		{
			AssertValidateToBeDeliveredHeaderCoreByAmendmotOfLocalExport(ElectronicDocumentTypeList.Codes._5DQ, ElectronicDocumentTypeList.Codes._5DS, "GOVCBR5DS_Test.xml");
		}

		public void TestValidateToBeDeliveredFor5DR()
		{
			AssertValidateToBeDeliveredHeaderCoreByAmendmotOfLocalExport(ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.Codes._5DR, "GOVCBR5DR_Test.xml");
		}

		public void TestValidateToBeDeliveredFor830()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate;
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(JobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._830, menuName);
		}

		public void TestValidateToBeDeliveredFor830English()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English;
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(JobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._830, menuName);
		}

		public void TestValidateToBeDeliveredFor830_3()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.ExportGoodsInspectionResultReport;
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(JobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._830, menuName);
		}

		public void TestValidateToBeDeliveredFor830_4()
		{
			var menuName = JobDeclarationDocumentSupporter.MenuNames.InspectionPlanAndResultReport;
			AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(JobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._830, menuName);
		}

		public void TestValidateToBeDeliveredFor5UL()
		{
			var menu = Factory.New<MasterFiles.Integration.IStmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.RefundRequest;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entrynum1 = entry1.EntryNumbers.AddNew();
			entrynum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entrynum1.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry1, "11111", entrynum1);
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
			var entrynum2 = entry2.EntryNumbers.AddNew();
			entrynum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entrynum2.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry2, "22222", entrynum2);
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

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalRejected, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, action2.StatusDescription);
		}

		public void TestValidateToBeDeliveredFor929_ReImportofExportedGoods()
		{
			var entryDeclinedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalRejected, CustomsMessageStatusTypeList.Descriptions.OriginalRejected);
			var entryApprovedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Descriptions.OriginalAccepted);
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = entryDeclinedPair.Code;
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = "IMP";
			entryNumber1.CE_EntryNum = "AAA111";
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.PreviousExpDecLineCollection.AddNew();

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = entryApprovedPair.Code;
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = "IMP";
			entryNumber2.CE_EntryNum = "BBB222";
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.PreviousExpDecLineCollection.AddNew();

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(entryDeclinedPair.Description, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(entryApprovedPair.Description, action2.StatusDescription);
		}

		public void TestValidateToBeDeliveredFor929_8_VATDefermentConflict()
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges;
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "한국아이비엠(주)");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_OH_DutyPayer = payer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber.CE_EntryNum = "AAA111";

			CreateNewStatementHeader(declaration.CompanyPK, entryNumber.CE_EntryNum, "1234123456789012345", ZDateTime.Today);

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action = collection[0];
			OrgHeaderWrapper.New(payer).ZO_VATDeferment = VATDefermentCodeList.Codes.Conflict;
			action.ToBeDelivered = true;
			AssertHasErrorContaining(action.ToBeDeliveredInfo, action.VATDefermentConflictErrorMessage);
			action.ToBeDelivered = false;
			AssertNoErrorContaining(action.ToBeDeliveredInfo, action.VATDefermentConflictErrorMessage);

			OrgHeaderWrapper.New(payer).ZO_VATDeferment = VATDefermentCodeList.Codes.Y;
			action.ToBeDelivered = true;
			AssertNoErrorContaining(action.ToBeDeliveredInfo, action.VATDefermentConflictErrorMessage);
			action.ToBeDelivered = false;
			AssertNoErrorContaining(action.ToBeDeliveredInfo, action.VATDefermentConflictErrorMessage);

			OrgHeaderWrapper.New(payer).ZO_VATDeferment = "";
			action.ToBeDelivered = true;
			AssertHasErrorContaining(action.ToBeDeliveredInfo, action.VATDefermentConflictErrorMessage);
			action.ToBeDelivered = false;
			AssertNoErrorContaining(action.ToBeDeliveredInfo, action.VATDefermentConflictErrorMessage);
		}

		public void TestValidateToBeDeliveredFor929_8_MissingPayer()
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber.CE_EntryNum = "AAA111";

			CreateNewStatementHeader(declaration.CompanyPK, entryNumber.CE_EntryNum, "1234123456789012345", ZDateTime.Today);

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action = collection[0];
			action.ToBeDelivered = true;
			AssertHasErrorContaining(action.ToBeDeliveredInfo, action.MissingPayerErrorMessage);
			action.ToBeDelivered = false;
			AssertNoErrorContaining(action.ToBeDeliveredInfo, action.MissingPayerErrorMessage);

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "한국아이비엠(주)");
			declaration.JE_OH_DutyPayer = payer.PK;
			action.ToBeDelivered = true;
			AssertNoErrorContaining(action.ToBeDeliveredInfo, action.MissingPayerErrorMessage);
			action.ToBeDelivered = false;
			AssertNoErrorContaining(action.ToBeDeliveredInfo, action.MissingPayerErrorMessage);
		}

		void CreateNewStatementHeader(ZGuid companyPK, ZString entryNumber, ZString statementNumber, ZDateTime processDate)
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = companyPK;
			statement.B2_StatementNumber = statementNumber;
			statement.B2_ProcessDate = processDate;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_SequenceNumber = 1;
			Factory.Save();
		}

		public void TestValidateToBeDeliveredForDKJ()
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CreateDocumentGeratingActionDKJData(CustomsMessageStatusTypeList.Codes.CancellationRejected, "AAA111");
			CreateDocumentGeratingActionDKJData(CustomsMessageStatusTypeList.Codes.CancellationAccepted, "BBB222");

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);
			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.CancellationRejected, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, "This document has been rejected by Customs.");
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.CancellationAccepted, action2.StatusDescription);

			void CreateDocumentGeratingActionDKJData(ZString status, ZString entryNumber)
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_Status = status;
				entry.EntryNumber = entryNumber;
				entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5AF;
				entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			}
		}

		void AssertionValidateToBeDeliveredWhenCE_EntryStatusHasMessageStatus(string entryType, string messageType, string menuName)
		{
			var originalRejectedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalRejected, CustomsMessageStatusTypeList.Descriptions.OriginalRejected);
			var originalAcceptedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Descriptions.OriginalAccepted);
			AssertionValidateToBeDeliveredCore(entryType, messageType, menuName, originalRejectedPair, originalAcceptedPair, "This document has been rejected by Customs.");
		}

		void AssertionValidateToBeDeliveredWhenCH_StatusHasStatus(string entryType, string messageType, string menuName)
		{
			var entryDeclinedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalRejected, CustomsMessageStatusTypeList.Descriptions.OriginalRejected);
			var entryApprovedPair = new CodeDescriptionPair(CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Descriptions.OriginalAccepted);
			AssertionValidateToBeDeliveredHeaderCoreByCH_Status(entryType, messageType, menuName, entryDeclinedPair, entryApprovedPair, "This document has been rejected by Customs.");
		}

		void AssertionValidateToBeDeliveredWhenCH_EntryStatusHasEntryStatus(string entryType, string messageType, string menuName)
		{
			var entryDeclinedPair = new CodeDescriptionPair(CustomsEntryStatusTypeList.Codes.DMS, CustomsEntryStatusTypeList.Descriptions.DMS);
			var entryApprovedPair = new CodeDescriptionPair(CustomsEntryStatusTypeList.Codes.ANT, CustomsEntryStatusTypeList.Descriptions.ANT);
			AssertionValidateToBeDeliveredHeaderCoreByCH_EntryStatus(entryType, messageType, menuName, entryDeclinedPair, entryApprovedPair, "This document has been declined by Customs.");
		}

		void AssertionValidateToBeDeliveredCore(string entryType, string messageType, string menuName, CodeDescriptionPair status1ToBeWarned, CodeDescriptionPair status2NotToBeWarned, string expectedWarningMessage)
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = menuName;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber1.CE_EntryNum = "AAA111";
			entry1.Messages.AddNew().EM_MessageType = messageType;

			var entrynum1 = entry1.EntryNumbers.AddNew();
			entrynum1.CE_EntryType = entryType;
			entrynum1.CE_EntryStatus = status1ToBeWarned.Code;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber2.CE_EntryNum = "BBB222";
			entry2.Messages.AddNew().EM_MessageType = messageType;

			var entrynum2 = entry2.EntryNumbers.AddNew();
			entrynum2.CE_EntryType = entryType;
			entrynum2.CE_EntryStatus = status2NotToBeWarned.Code;

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(status1ToBeWarned.Description, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(status2NotToBeWarned.Description, action2.StatusDescription);
		}

		void AssertionValidateToBeDeliveredHeaderCoreByCH_EntryStatus(string entryType, string messageType, string menuName, CodeDescriptionPair status1ToBeWarned, CodeDescriptionPair status2NotToBeWarned, string expectedWarningMessage)
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = menuName;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = entryType;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryStatus = status1ToBeWarned.Code;
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = entryType;
			entryNumber1.CE_EntryNum = "AAA111";
			entry1.Messages.AddNew().EM_MessageType = messageType;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryStatus = status2NotToBeWarned.Code;
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = entryType;
			entryNumber2.CE_EntryNum = "BBB222";
			entry2.Messages.AddNew().EM_MessageType = messageType;

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(status1ToBeWarned.Description, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(status2NotToBeWarned.Description, action2.StatusDescription);
		}

		void AssertionValidateToBeDeliveredHeaderCoreByCH_Status(string entryType, string messageType, string menuName, CodeDescriptionPair status1ToBeWarned, CodeDescriptionPair status2NotToBeWarned, string expectedWarningMessage)
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = menuName;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = entryType;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = status1ToBeWarned.Code;
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = entryType;
			entryNumber1.CE_EntryNum = "AAA111";
			entry1.Messages.AddNew().EM_MessageType = messageType;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = status2NotToBeWarned.Code;
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = entryType;
			entryNumber2.CE_EntryNum = "BBB222";
			entry2.Messages.AddNew().EM_MessageType = messageType;

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(status1ToBeWarned.Description, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(status2NotToBeWarned.Description, action2.StatusDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		void AssertValidateToBeDeliveredHeaderCoreByAmendmotOfLocalExport(string entryType, string messageType, string fileName)
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration;
			var expectedWarningMessage = "This document has been rejected by Customs.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration = new TestDataSetupHelper(Factory).SetLocalExportEntryData(declaration, entryType, "3271420001710", messageType, fileName);
			declaration.CustomsEntryHeaders[0].CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			declaration = new TestDataSetupHelper(Factory).SetLocalExportEntryData(declaration, entryType, "3271420001711", messageType, fileName);
			declaration.CustomsEntryHeaders[1].CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			Factory.Save();

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			action1.ToBeDelivered = true;
			AssertHasWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			action1.ToBeDelivered = false;
			AssertNoWarningContaining(action1.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalRejected, action1.StatusDescription);

			var action2 = collection[1];
			action2.ToBeDelivered = true;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			action2.ToBeDelivered = false;
			AssertNoWarningContaining(action2.ToBeDeliveredInfo, expectedWarningMessage);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, action2.StatusDescription);
		}

		public void TestApplyingTaxExemptionOrSpecificUseDutyRateStatus()
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			CreateEntryNumAndMessage(entry1, ElectronicDocumentTypeList.Codes._929, KRJobMessageTypeList.Codes.Import, "AAA111", CustomsMessageStatusTypeList.Codes.OriginalAccepted, ZDateTime.Today);
			CreateEntryNumAndMessage(entry1, ElectronicDocumentTypeList.Codes._5FN, ElectronicDocumentTypeList.Codes._5FN, ZString.Empty, CustomsMessageStatusTypeList.Codes.OriginalAccepted, ZDateTime.Today.AddHours(2));
			CreateEntryNumAndMessage(entry1, ElectronicDocumentTypeList.Codes._5FN, ElectronicDocumentTypeList.Codes._5FN, ZString.Empty, CustomsMessageStatusTypeList.Codes.OriginalSent, ZDateTime.Today.AddHours(1));

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			CreateEntryNumAndMessage(entry2, ElectronicDocumentTypeList.Codes._929, KRJobMessageTypeList.Codes.Import, "BBB222", CustomsMessageStatusTypeList.Codes.OriginalAccepted, ZDateTime.Today);
			CreateEntryNumAndMessage(entry2, ElectronicDocumentTypeList.Codes._5FN, ElectronicDocumentTypeList.Codes._5FN, ZString.Empty, CustomsMessageStatusTypeList.Codes.OriginalSent, ZDateTime.Today.AddHours(3));
			CreateEntryNumAndMessage(entry2, ElectronicDocumentTypeList.Codes._5FN, ElectronicDocumentTypeList.Codes._5FN, ZString.Empty, CustomsMessageStatusTypeList.Codes.OriginalRejected, ZDateTime.Today.AddHours(2));
			CreateEntryNumAndMessage(entry2, ElectronicDocumentTypeList.Codes._5FN, ElectronicDocumentTypeList.Codes._5FN, ZString.Empty, CustomsMessageStatusTypeList.Codes.OriginalAccepted, ZDateTime.Today.AddHours(1));

			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			var action1 = collection[0];
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, action1.StatusDescription);
			var action2 = collection[1];
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalRejected, action2.StatusDescription);

			void CreateEntryNumAndMessage(CusEntryHeader entry, string messageType, string entryType, string entryNumber, string entryStatus, ZDateTime createTime)
			{
				entry.Messages.AddNew().EM_MessageType = messageType;

				var entryNum = entry.EntryNumbers.AddNew();
				entryNum.CE_EntryType = entryType;
				entryNum.CE_EntryNum = entryNumber;
				entryNum.CE_EntryStatus = entryStatus;
				entryNum.CE_SystemCreateTimeUtc = createTime;
			}
		}

		public void TestGridFieldsFor929_8()
		{
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges;
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "한국아이비엠(주)");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_OH_DutyPayer = payer.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber.CE_EntryNum = "AAA111";

			CreateNewStatementHeader(declaration.CompanyPK, entryNumber.CE_EntryNum, "1234123456789012345", new ZDateTime(2025, 05, 12));
			
			var collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			AssertEquals(1, collection.Count);
			AssertEquals("1234123456789012345", collection[0].PaymentInvoiceNumber);
			AssertEquals(new ZDateTime(2025, 05, 12), collection[0].ReceivedDate);

			CreateNewStatementHeader(declaration.CompanyPK, entryNumber.CE_EntryNum, "1234123456789012346", new ZDateTime(2025, 05, 11));

			collection = new DocumentGeneratingActionCollection(declaration);
			collection.InitialiseFor(menu);

			AssertEquals(1, collection.Count);
			AssertEquals("Multiple Invoices", collection[0].PaymentInvoiceNumber);
			AssertEquals(ZDateTime.Empty, collection[0].ReceivedDate);
		}
	}
}
