using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.EU;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.EU.Testing
{
	[TestedType(typeof(Report_ISTAccounting))]
	class Report_ISTAccountingTest : CustomsReportDbCreateScriptTest
	{
		public void TestBranch()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			var storageRegLineTransactionPK2 = TestDataCreator.CreateCusTempStorageRegLineTransaction(storageRegLinePK, "2222", DateTime.Today, "CON");
			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows SRH_Reference", new string[] { "FRJ000000002", "FRJ000000002" }, filteredRows);
		}

		public void TestCreateDate()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.CreateDateFrom, DateTime.Today.AddDays(-1)), (Report_ISTAccountingParameter.CreateDateTo, DateTime.Today.AddDays(1)));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.CreateDateFrom, DateTime.Today.AddDays(1)), (Report_ISTAccountingParameter.CreateDateTo, DateTime.Today.AddDays(2)));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestJobReference()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.JobReference, "FRJ000000001"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.JobReference, "FRJ000000002"));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestDeclarationStatus()
		{
			UpdateStorageDecColumn("STH_DeclarationStatus", "OPN", SqlDbType.Char, storageDecPK);
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.DeclarationStatus, "OPN"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference when DeclarationStatus is OPN", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.DeclarationStatus, "CLS"));
			AssertEquals(0, filteredRows.Length);

			UpdateStorageDecColumn("STH_DeclarationStatus", "CLS", SqlDbType.Char, storageDecPK);
			TestDataCreator.CreateStmALog("CusTempStorageRegHeader", storageRegHeaderPK, "1", DateTime.Today, "ACT");

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.DeclarationStatus, "OPN"));
			AssertEquals(0, filteredRows.Length);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.DeclarationStatus, "CLS"));
			AssertEquals(0, filteredRows.Length);

			TestDataCreator.CreateStmALog("CusTempStorageRegHeader", storageRegHeaderPK, "2", DateTime.Today, "SCM");
			TestDataCreator.CreateStmALog("CusTempStorageRegHeader", storageRegHeaderPK, "3", DateTime.Today, "SCM");

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.DeclarationStatus, "OPN"));
			AssertEquals(0, filteredRows.Length);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.DeclarationStatus, "CLS"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference when DeclarationStatus is CLS", new string[] { "FRJ000000002" }, filteredRows);
		}

		public void TestPresenter()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.PresenterAddressPK, presenterAddressPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.PresenterAddressPK, Guid.NewGuid()));
			AssertEquals(0, filteredRows.Length);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, Guid.NewGuid()));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestCustomsProfile()
		{
			UpdateStorageHeaderColumn("SJH_CustomsProfile", "1111", SqlDbType.VarChar, storageHeaderPK);
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.CustomsProfile, "1111"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.CustomsProfile, "2222"));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestGoodsLocation()
		{
			UpdateStorageRegLineColumn("SRL_LocationOfGoods", "1111", SqlDbType.VarChar, storageRegLinePK);
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.GoodsLocation, "1111"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK), (Report_ISTAccountingParameter.GoodsLocation, "2222"));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestCustomsProcedureAndCustomsStatusIfDec()
		{
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00000001", "IMP", 1);
			var cusInstructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "10", "10", DateTime.Today, 1);
			UpdateCusEntryInstructionColumn("CEI_SubStyle", "5", SqlDbType.VarChar, cusInstructionPK);
			var entryHeaderPK = TestDataCreator.CreateCusEntryHeader(declarationPK, 1);
			UpdateEntryHeaderColumn("CH_CEI_Instruction", cusInstructionPK, SqlDbType.UniqueIdentifier, entryHeaderPK);
			UpdateEntryHeaderColumn("CH_EntryStatus", "010", SqlDbType.VarChar, entryHeaderPK);
			UpdateEntryHeaderColumn("CH_BGMReference", "1-B00169622", SqlDbType.VarChar, entryHeaderPK);
			UpdateStorageRegLineTransactionColumn("SRT_ReferenceType", "DEC", SqlDbType.VarChar, storageRegLineTransactionPK);
			UpdateStorageRegLineTransactionColumn("SRT_InternalReferenceNumber", "1-B00169622", SqlDbType.VarChar, storageRegLineTransactionPK);
			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", -1, SqlDbType.Int, storageRegLineTransactionPK);

			var filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row CustomsProcedure when SRT_ReferenceType is DEC", new string[] { "10/5" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row CustomsStatus when SRT_ReferenceType is DEC", new string[] { "AWAITINGRESPONSE" }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 0, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsProcedure when SRT_PackageQty is 0", new string[] { "" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsStatus when SRT_PackageQty is 0", new string[] { "" }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsProcedure when SRT_PackageQty is greater than 0", new string[] { "" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsStatus when SRT_PackageQty is greater than 0", new string[] { "" }, filteredRows);
		}

		public void TestCustomsProcedureAndCustomsStatusIfNCTS()
		{
			var nctsPK = TestDataCreator.CreateCusInbondHeader("NCT000000001", branchPK);
			UpdateNctsHeaderColumn("BH_HeaderType", "D", SqlDbType.VarChar, nctsPK);
			var nctsMovementPK = TestDataCreator.CreateCusInBondMoveHeader(nctsPK);
			UpdateNctsMovementHeaderColumn("BM_CustomsStatus", "AMA", SqlDbType.VarChar, nctsMovementPK);
			UpdateNctsMovementHeaderColumn("BM_InBondEntryType", "T1", SqlDbType.VarChar, nctsMovementPK);
			UpdateStorageRegLineTransactionColumn("SRT_ReferenceType", "MRN", SqlDbType.VarChar, storageRegLineTransactionPK);
			UpdateStorageRegLineTransactionColumn("SRT_InternalReferenceNumber", "NCT000000001", SqlDbType.VarChar, storageRegLineTransactionPK);
			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", -1, SqlDbType.Int, storageRegLineTransactionPK);

			var filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row CustomsProcedure when SRT_ReferenceType is MRN", new string[] { "T1" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row CustomsStatus when SRT_ReferenceType is MRN", new string[] { "AMA" }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 0, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsProcedure when SRT_PackageQty is 0", new string[] { "" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsStatus when SRT_PackageQty is 0", new string[] { "" }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsProcedure when SRT_PackageQty is greater than 0", new string[] { "" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsStatus when SRT_PackageQty is greater than 0", new string[] { "" }, filteredRows);
		}

		public void TestCustomsProcedureAndCustomsStatusIfIST()
		{
			UpdateStorageRegLineTransactionColumn("SRT_ReferenceType", "IST", SqlDbType.VarChar, storageRegLineTransactionPK);
			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", -1, SqlDbType.Int, storageRegLineTransactionPK);
			UpdateStorageDecColumn("STH_DeclarationStatus", "OPN", SqlDbType.Char, storageDecPK);

			var filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row CustomsProcedure when SRT_ReferenceType is IST", new string[] { "IST" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row CustomsStatus when SRT_ReferenceType is IST", new string[] { "OPN" }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 0, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsProcedure when SRT_PackageQty is 0", new string[] { "" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsStatus when SRT_PackageQty is 0", new string[] { "" }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "CustomsProcedure", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsProcedure when SRT_PackageQty is greater than 0", new string[] { "" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "CustomsStatus", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank CustomsStatus when SRT_PackageQty is greater than 0", new string[] { "" }, filteredRows);
		}

		public void TestSpecificHandlingDescription()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SpecificHandlingDescription", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank SpecificHandlingDescription when SJH_IsExaminationExpected and SJH_IsSameConditionExpected are false", new string[] { "" }, filteredRows);

			UpdateStorageHeaderColumn("SJH_IsExaminationExpected", 1, SqlDbType.Bit, storageHeaderPK);
			filteredRows = GetFilteredRows(selectedColumn: "SpecificHandlingDescription", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SpecificHandlingDescription when SJH_IsExaminationExpected is true", new string[] { "Examination Expected" }, filteredRows);

			UpdateStorageHeaderColumn("SJH_IsExaminationExpected", 0, SqlDbType.Bit, storageHeaderPK);
			UpdateStorageHeaderColumn("SJH_IsSameConditionExpected", 1, SqlDbType.Bit, storageHeaderPK);
			filteredRows = GetFilteredRows(selectedColumn: "SpecificHandlingDescription", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SpecificHandlingDescription when SJH_IsSameConditionExpected is true", new string[] { "Same State Expected" }, filteredRows);

			UpdateStorageHeaderColumn("SJH_IsExaminationExpected", 1, SqlDbType.Bit, storageHeaderPK);
			filteredRows = GetFilteredRows(selectedColumn: "SpecificHandlingDescription", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SpecificHandlingDescription when SJH_IsExaminationExpected and SJH_IsSameConditionExpected are true", new string[] { "Examination Expected/Same State Expected" }, filteredRows);
		}

		public void TestQtyInAndQtyOut()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "QtyIn", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyIn when SRT_PackageQty is 0", new int[] { 0 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "QtyOut", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyOut when SRT_PackageQty is 0", new int[] { 0 }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "QtyIn", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row QtyIn when SRT_PackageQty is greater than 0", new int[] { 1 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "QtyOut", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyOut when SRT_PackageQty is greater than 0", new int[] { 0 }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", -1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "QtyIn", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyIn when SRT_PackageQty is less than 0", new int[] { 0 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "QtyOut", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row QtyOut when SRT_PackageQty is less than 0", new int[] { -1 }, filteredRows);
		}

		public void TestTSI_GoodsOrigin()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "TSI_GoodsOrigin", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 rows blank TSI_GoodsOrigin when no tempStorageLineItem", new string[] { "" }, filteredRows);

			var storageLineItemPK11 = TestDataCreator.CreateCusTempStorageLineItem(storageLinePK, DateTime.Today);
			UpdateStorageLineItemColumn("TSI_GoodsOrigin", "AU", SqlDbType.VarChar, storageLineItemPK11);
			filteredRows = GetFilteredRows(selectedColumn: "TSI_GoodsOrigin", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row TSI_GoodsOrigin when 1 tempStorageLineItem", new string[] { "AU" }, filteredRows);

			var storageLineItemPK12 = TestDataCreator.CreateCusTempStorageLineItem(storageLinePK, DateTime.Today);
			UpdateStorageLineItemColumn("TSI_GoodsOrigin", "AU", SqlDbType.VarChar, storageLineItemPK12);
			filteredRows = GetFilteredRows(selectedColumn: "TSI_GoodsOrigin", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row TSI_GoodsOrigin when 2 tempStorageLineItems with same goods origin", new string[] { "AU" }, filteredRows);

			var storageLineItemPK13 = TestDataCreator.CreateCusTempStorageLineItem(storageLinePK, DateTime.Today);
			UpdateStorageLineItemColumn("TSI_GoodsOrigin", "GB", SqlDbType.VarChar, storageLineItemPK13);
			filteredRows = GetFilteredRows(selectedColumn: "TSI_GoodsOrigin", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row TSI_GoodsOrigin when some tempStorageLineItems with different goods origin", new string[] { "MLP" }, filteredRows);

			var storageLinePK2 = TestDataCreator.CreateCusTempStorageLine(storageDecPK, 2, 20m, 2, "IM", 2, 2, DateTime.Today);
			var storageLineItemPK21 = TestDataCreator.CreateCusTempStorageLineItem(storageLinePK2, DateTime.Today);
			UpdateStorageLineItemColumn("TSI_GoodsOrigin", "DE", SqlDbType.VarChar, storageLineItemPK21);
			var storageLineItemPK22 = TestDataCreator.CreateCusTempStorageLineItem(storageLinePK2, DateTime.Today);
			UpdateStorageLineItemColumn("TSI_GoodsOrigin", "DE", SqlDbType.VarChar, storageLineItemPK22);

			var storageRegLinePK2 = TestDataCreator.CreateCusTempStorageRegLine(storageRegHeaderPK, 2);
			var storageRegLineTransactionPK2 = TestDataCreator.CreateCusTempStorageRegLineTransaction(storageRegLinePK2, "2222", DateTime.Today, "CON");
			filteredRows = GetFilteredRows(selectedColumn: "TSI_GoodsOrigin", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows TSI_GoodsOrigin when more than 1 storage line exists", new string[] { "MLP", "DE" }, filteredRows);
		}

		public void TestTSL_GoodsDescription()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "TSL_GoodsDescription", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row blank TSL_GoodsDescription", new string[] { "" }, filteredRows);

			UpdateStorageLineColumn("TSL_GoodsDescription", "description", SqlDbType.VarChar, storageLinePK);
			filteredRows = GetFilteredRows(selectedColumn: "TSL_GoodsDescription", (Report_ISTAccountingParameter.BranchPK, branchPK), (Report_ISTAccountingParameter.PresenterHeaderPK, presenterPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 rows TSL_GoodsDescription", new string[] { "description" }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "FR", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "FRANCE");
			customerPK = TestDataCreator.CreateOrganisation("Customer", "Customser");
			presenterPK = TestDataCreator.CreateOrganisation("Presenter", "Presenter");
			presenterAddressPK = TestDataCreator.CreateAddress(presenterPK, "PresenterAddress", "PresenterAddress");
			storageHeaderPK = TestDataCreator.CreateCusTempStorageJobHeader(branchPK, "FRJ000000001", "1111", customerPK, DateTime.Today);
			UpdateStorageHeaderColumn("SJH_OA_Presenter", presenterAddressPK, SqlDbType.UniqueIdentifier, storageHeaderPK);
			storageDecPK = TestDataCreator.CreateCusTempStorageDec(storageHeaderPK, DateTime.Today);
			storageLinePK = TestDataCreator.CreateCusTempStorageLine(storageDecPK, 1, 10m, 1, "IM", 1, 1, DateTime.Today);
			storageRegHeaderPK = TestDataCreator.CreateCusTempStorageRegHeader("FRJ000000002", "IST", DateTime.Today);
			UpdateStorageRegHeaderColumn("SRH_InternalReference", "FRJ000000001", SqlDbType.VarChar, storageRegHeaderPK);
			storageRegLinePK = TestDataCreator.CreateCusTempStorageRegLine(storageRegHeaderPK, 1);
			storageRegLineTransactionPK = TestDataCreator.CreateCusTempStorageRegLineTransaction(storageRegLinePK, "1111", DateTime.Today, "CON");
		}
		Guid companyPK;
		Guid branchPK;
		Guid customerPK;
		Guid presenterPK;
		Guid presenterAddressPK;
		Guid storageHeaderPK;
		Guid storageDecPK;
		Guid storageLinePK;
		Guid storageRegHeaderPK;
		Guid storageRegLinePK;
		Guid storageRegLineTransactionPK;

		protected void UpdateStorageHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageJobHeader", columnName, columnValue, columnType, "SJH_PK", primaryKeyValue);
		}

		protected void UpdateStorageDecColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageDec", columnName, columnValue, columnType, "STH_PK", primaryKeyValue);
		}

		protected void UpdateStorageLineColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageLine", columnName, columnValue, columnType, "TSL_PK", primaryKeyValue);
		}

		protected void UpdateStorageLineItemColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageLineItem", columnName, columnValue, columnType, "TSI_PK", primaryKeyValue);
		}

		protected void UpdateStorageRegHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageRegHeader", columnName, columnValue, columnType, "SRH_PK", primaryKeyValue);
		}

		protected void UpdateStorageRegLineColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageRegLine", columnName, columnValue, columnType, "SRL_PK", primaryKeyValue);
		}

		protected void UpdateStorageRegLineTransactionColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageRegLineTransaction", columnName, columnValue, columnType, "SRT_PK", primaryKeyValue);
		}

		protected void UpdateCusEntryInstructionColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryInstruction", columnName, columnValue, columnType, "CEI_PK", primaryKeyValue);
		}

		protected void UpdateNctsHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondHeader", columnName, columnValue, columnType, "BH_PK", primaryKeyValue);
		}

		protected void UpdateNctsMovementHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondMoveHeader", columnName, columnValue, columnType, "BM_PK", primaryKeyValue);
		}

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_ISTAccountingParameter.BranchPK);
			yield return (SqlDbType.SmallDateTime, Report_ISTAccountingParameter.CreateDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_ISTAccountingParameter.CreateDateTo);
			yield return (SqlDbType.VarChar, Report_ISTAccountingParameter.JobReference);
			yield return (SqlDbType.Char, Report_ISTAccountingParameter.DeclarationStatus);
			yield return (SqlDbType.UniqueIdentifier, Report_ISTAccountingParameter.PresenterHeaderPK);
			yield return (SqlDbType.UniqueIdentifier, Report_ISTAccountingParameter.PresenterAddressPK);
			yield return (SqlDbType.VarChar, Report_ISTAccountingParameter.CustomsProfile);
			yield return (SqlDbType.VarChar, Report_ISTAccountingParameter.GoodsLocation);
		}

		class Report_ISTAccountingParameter
		{
			public const string BranchPK = "@BranchPK";
			public const string CreateDateFrom = "@CreateDateFrom";
			public const string CreateDateTo = "@CreateDateTo";
			public const string JobReference = "@JobReference";
			public const string DeclarationStatus = "@DeclarationStatus";
			public const string @PresenterHeaderPK = "@PresenterHeaderPK";
			public const string @PresenterAddressPK = "@PresenterAddressPK";
			public const string CustomsProfile = "@CustomsProfile";
			public const string GoodsLocation = "@GoodsLocation";
		}
	}
}
