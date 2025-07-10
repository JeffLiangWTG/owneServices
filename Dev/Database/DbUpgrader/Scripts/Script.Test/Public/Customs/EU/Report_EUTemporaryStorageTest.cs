using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.EU;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.EU.Testing
{
	[TestedType(typeof(Report_EUTemporaryStorage))]
	class Report_EUTemporaryStorageTest : CustomsReportDbCreateScriptTest
	{
		public void TestBranch()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "BalancePKG", (Report_EUTemporaryStorageParameter.BranchPK, branchPK), (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect BalancePKG is equal to SRL_PackagesRemaining when country of branch is not ES", new int[] { 2 }, filteredRows);

			UpdateGlbBranchColumn("GB_RN_NKCountryCode", "ES", SqlDbType.VarChar, branchPK);
			filteredRows = GetFilteredRows(selectedColumn: "BalancePKG", (Report_EUTemporaryStorageParameter.BranchPK, branchPK), (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect BalancePKG is equal to the sum of SRT_PackageQty that is not deleted when country of branch is ES", new int[] { 3 }, filteredRows);

			var storageRegLineTransactionPK2 = TestDataCreator.CreateCusTempStorageRegLineTransaction(storageRegLinePK, "2222", DateTime.Today, "DEL", 1);
			filteredRows = GetFilteredRows(selectedColumn: "BalancePKG", (Report_EUTemporaryStorageParameter.BranchPK, branchPK), (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect BalancePKG is equal to the sum of SRT_PackageQty that is not deleted when country of branch is ES", new int[] { 3 }, filteredRows);

			var storageRegLineTransactionPK3 = TestDataCreator.CreateCusTempStorageRegLineTransaction(storageRegLinePK, "3333", DateTime.Today, "CON", 4);
			filteredRows = GetFilteredRows(selectedColumn: "BalancePKG", (Report_EUTemporaryStorageParameter.BranchPK, branchPK), (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect BalancePKG is equal to the sum of SRT_PackageQty that is not deleted when country of branch is ES", new int[] { 7, 7 }, filteredRows);
		}

		public void TestCreateDate()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.CreateDateFrom, DateTime.Today.AddDays(-1)), (Report_EUTemporaryStorageParameter.CreateDateTo, DateTime.Today.AddDays(1)));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.CreateDateFrom, DateTime.Today.AddDays(1)), (Report_EUTemporaryStorageParameter.CreateDateTo, DateTime.Today.AddDays(2)));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestTSDNumber()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.TSDNumber, "FRJ000000002"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.TSDNumber, "FRJ000000001"));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestPremises()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, new Guid()));
			AssertEquals(0, filteredRows.Length);
		}

		public void TestTransactionShown()
		{
			UpdateCusTempStorageRegLineColumn("SRL_CustomsStatus", "OPN", SqlDbType.VarChar, storageRegLinePK);

			var filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.TransactionShown, "ALL"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.TransactionShown, "ACT"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			UpdateCusTempStorageRegLineColumn("SRL_CustomsStatus", "CLS", SqlDbType.VarChar, storageRegLinePK);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.TransactionShown, "ALL"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRH_Reference", new string[] { "FRJ000000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "SRH_Reference", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.TransactionShown, "ACT"));
			AssertEquals("Expect 0 row SRH_Reference", 0, filteredRows.Length);
		}

		public void TestQtyInAndQtyOut()
		{
			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 0, SqlDbType.Int, storageRegLineTransactionPK);
			var filteredRows = GetFilteredRows(selectedColumn: "QtyIn", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyIn when SRT_PackageQty is 0", new int[] { 0 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "QtyOut", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyOut when SRT_PackageQty is 0", new int[] { 0 }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", 1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "QtyIn", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row QtyIn when SRT_PackageQty is greater than 0", new int[] { 1 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "QtyOut", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyOut when SRT_PackageQty is greater than 0", new int[] { 0 }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_PackageQty", -1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "QtyIn", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 QtyIn when SRT_PackageQty is less than 0", new int[] { 0 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "QtyOut", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row QtyOut when SRT_PackageQty is less than 0", new int[] { -1 }, filteredRows);
		}

		public void TestWgtInAndWgtOut()
		{
			UpdateStorageRegLineTransactionColumn("SRT_GrossWeight", 0, SqlDbType.Int, storageRegLineTransactionPK);
			var filteredRows = GetFilteredRows(selectedColumn: "WgtIn", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 WgtIn when SRT_GrossWeight is 0", new decimal[] { 0 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "WgtOut", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 WgtOut when SRT_GrossWeight is 0", new decimal[] { 0 }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_GrossWeight", 1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "WgtIn", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row WgtIn when SRT_GrossWeight is greater than 0", new decimal[] { 1 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "WgtOut", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 WgtOut when SRT_GrossWeight is greater than 0", new decimal[] { 0 }, filteredRows);

			UpdateStorageRegLineTransactionColumn("SRT_GrossWeight", -1, SqlDbType.Int, storageRegLineTransactionPK);
			filteredRows = GetFilteredRows(selectedColumn: "WgtIn", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row 0 WgtIn when SRT_GrossWeight is less than 0", new decimal[] { 0 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "WgtOut", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row WgtOut when SRT_GrossWeight is less than 0", new decimal[] { -1 }, filteredRows);
		}

		public void TestSRI_GoodsItemNumber()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRI_GoodsItemNumber", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRI_GoodsItemNumber", new int[] { 1 }, filteredRows);

			storageRegLineItemPK = TestDataCreator.CreateCusTempStorageRegLineItem(2, "28571428", DateTime.Today);
			TestDataCreator.CreateCusTempStorageRegLineItemPivot(storageRegLinePK, storageRegLineItemPK, 2, DateTime.Today);
			filteredRows = GetFilteredRows(selectedColumn: "SRI_GoodsItemNumber", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows SRI_GoodsItemNumber", new int[] { 1, 2 }, filteredRows);
		}

		public void TestGrossWeight()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "SRV_GrossWeight", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRV_GrossWeight", new decimal[] { 2 }, filteredRows);

			storageRegLineItemPK = TestDataCreator.CreateCusTempStorageRegLineItem(2, "28571428", DateTime.Today);
			TestDataCreator.CreateCusTempStorageRegLineItemPivot(storageRegLinePK, storageRegLineItemPK, 4, DateTime.Today);
			filteredRows = GetFilteredRows(selectedColumn: "SRV_GrossWeight", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows SRV_GrossWeight", new decimal[] { 2, 4 }, filteredRows);

			storageRegLinePK = TestDataCreator.CreateCusTempStorageRegLine(storageRegHeaderPK, 2);
			storageRegLineTransactionPK = TestDataCreator.CreateCusTempStorageRegLineTransaction(storageRegLinePK, "1112", DateTime.Today, "CON");
			filteredRows = GetFilteredRows(selectedColumn: "SRL_LineNumber", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 3 rows SRL_LineNumber", new int[] { 1, 1, 2 }, filteredRows);

			storageRegLineItemPK = TestDataCreator.CreateCusTempStorageRegLineItem(1, "57142857", DateTime.Today);
			TestDataCreator.CreateCusTempStorageRegLineItemPivot(storageRegLinePK, storageRegLineItemPK, 3, DateTime.Today);
			filteredRows = GetFilteredRows(selectedColumn: "SRV_GrossWeight", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 3 rows SRV_GrossWeight", new decimal[] { 2, 4, 3 }, filteredRows);
		}

		public void TestSRT_InternalReferenceType()
		{
			UpdateStorageRegLineTransactionColumn("SRT_InternalReferenceType", "OTH", SqlDbType.VarChar, storageRegLineTransactionPK);
			var filteredRows = GetFilteredRows(selectedColumn: "SRT_InternalReferenceType", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row SRT_InternalReferenceType", new string[] { "OTH" }, filteredRows);
		}

		public void TestHolder()
		{
			UpdateGlbBranchColumn("GB_RN_NKCountryCode", "ES", SqlDbType.VarChar, branchPK);
			var filteredRows = GetFilteredRows(selectedColumn: "OK_CustomsRegNo", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.BranchPK, branchPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row OK_CustomsRegNo", new string[] { "DE123546" }, filteredRows);

			var orgCusCode2 = TestDataCreator.CreateOrgCusCode(orgHeader, "EOR", "777555", "ES");
			filteredRows = GetFilteredRows(selectedColumn: "OK_CustomsRegNo", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.BranchPK, branchPK));
			AssertContainsExactElementsInAnyOrder("Expect OK_CustomsRegNo with Code='EOR' For 'ES'", new string[] { "ES777555" }, filteredRows);

			UpdateOrgCusCodeColumn("OK_CodeType", "UST", SqlDbType.VarChar, orgCusCode2);
			filteredRows = GetFilteredRows(selectedColumn: "OK_CustomsRegNo", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.BranchPK, branchPK));
			AssertContainsExactElementsInAnyOrder("Expect OK_CustomsRegNo with Code='EOR' For 'DE'", new string[] { "DE123546" }, filteredRows);

			UpdateOrgCusCodeColumn("OK_CodeType", "NIF", SqlDbType.VarChar, orgCusCode2);
			UpdateOrgCusCodeColumn("OK_CustomsRegNo", "111333", SqlDbType.VarChar, orgCusCode2);
			filteredRows = GetFilteredRows(selectedColumn: "OK_CustomsRegNo", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.BranchPK, branchPK));
			AssertContainsExactElementsInAnyOrder("Expect OK_CustomsRegNo with Code='NIF'", new string[] { "ES111333" }, filteredRows);

			UpdateGlbBranchColumn("GB_RN_NKCountryCode", "DE", SqlDbType.VarChar, branchPK);
			filteredRows = GetFilteredRows(selectedColumn: "OK_CustomsRegNo", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK), (Report_EUTemporaryStorageParameter.BranchPK, branchPK));
			AssertContainsExactElementsInAnyOrder("Expect OK_CustomsRegNo with Code='EOR'", new string[] { "DE123546" }, filteredRows);
		}

		public void TestHolderName()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "OH_FullName", (Report_EUTemporaryStorageParameter.PremisesPK, premisesPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row OH_FullName", new string[] { "ES Madrid Company" }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "FR", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "FRANCE", "FR");
			customerPK = TestDataCreator.CreateOrganisation("Customer", "Customser"); 
			premisesAddressPK = TestDataCreator.CreateAddress(customerPK, "PremisesAddress", "PremisesAddress");
			premisesPK = TestDataCreator.CreateCusTempStorageRegPremises("ABC", "ABC", premisesAddressPK, DateTime.Today);
			storageRegHeaderPK = TestDataCreator.CreateCusTempStorageRegHeader("FRJ000000002", "IST", DateTime.Today, premisesPK);
			UpdateStorageRegHeaderColumn("SRH_InternalReference", "FRJ000000001", SqlDbType.VarChar, storageRegHeaderPK);
			storageRegLinePK = TestDataCreator.CreateCusTempStorageRegLine(storageRegHeaderPK, 1, 2);
			storageRegLineTransactionPK = TestDataCreator.CreateCusTempStorageRegLineTransaction(storageRegLinePK, "1111", DateTime.Today, "CON", 3);
			storageRegLineItemPK = TestDataCreator.CreateCusTempStorageRegLineItem(1, "14285714", DateTime.Today);
			TestDataCreator.CreateCusTempStorageRegLineItemPivot(storageRegLinePK, storageRegLineItemPK, 2, DateTime.Today);
			orgHeader = TestDataCreator.CreateOrganisation("DES", "ES Madrid Company");
			TestDataCreator.CreateCusAuthorizationUsage(premisesPK, "SRP", orgHeader, "ACE", "123546", 0);
			TestDataCreator.CreateOrgCusCode(orgHeader, "EOR", "123546", "DE");
		}

		Guid companyPK;
		Guid branchPK;
		Guid customerPK;
		Guid premisesPK;
		Guid premisesAddressPK;
		Guid storageRegHeaderPK;
		Guid storageRegLinePK;
		Guid storageRegLineTransactionPK;
		Guid storageRegLineItemPK;
		Guid orgHeader;

		protected void UpdateGlbBranchColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("GlbBranch", columnName, columnValue, columnType, "GB_PK", primaryKeyValue);
		}

		protected void UpdateCusTempStorageRegLineColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageRegLine", columnName, columnValue, columnType, "SRL_PK", primaryKeyValue);
		}

		protected void UpdateStorageRegHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageRegHeader", columnName, columnValue, columnType, "SRH_PK", primaryKeyValue);
		}

		protected void UpdateStorageRegLineTransactionColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusTempStorageRegLineTransaction", columnName, columnValue, columnType, "SRT_PK", primaryKeyValue);
		}

		protected void UpdatePremisesAddressColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("OrgAddress", columnName, columnValue, columnType, "OA_PK", primaryKeyValue);
		}

		protected void UpdateOrgCusCodeColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("OrgCusCode", columnName, columnValue, columnType, "OK_PK", primaryKeyValue);
		}

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_EUTemporaryStorageParameter.BranchPK);
			yield return (SqlDbType.SmallDateTime, Report_EUTemporaryStorageParameter.CreateDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_EUTemporaryStorageParameter.CreateDateTo);
			yield return (SqlDbType.VarChar, Report_EUTemporaryStorageParameter.TSDNumber);
			yield return (SqlDbType.UniqueIdentifier, Report_EUTemporaryStorageParameter.PremisesPK);
			yield return (SqlDbType.VarChar, Report_EUTemporaryStorageParameter.TransactionShown);
		}

		class Report_EUTemporaryStorageParameter
		{
			public const string BranchPK = "@BranchPK";
			public const string CreateDateFrom = "@CreateDateFrom";
			public const string CreateDateTo = "@CreateDateTo";
			public const string TSDNumber = "@TSDNumber";
			public const string PremisesPK = "@PremisesPK";
			public const string TransactionShown = "@TransactionShown";
		}
	}
}
