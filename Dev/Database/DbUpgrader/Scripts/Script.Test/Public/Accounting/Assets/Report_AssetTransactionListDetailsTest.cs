using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Assets;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Accounting.Assets;

[TestedType(typeof(Report_AssetTransactionListDetails))]
class Report_AssetTransactionListDetailsTest : DbCreateScriptTest
{
	public void TestSampleCall()
	{
		var helper = new TestDbHelper(TestConnection);

		var companyPk = TestDbHelper.DefaultCompanyPK;
		var companyPk2 = TestDbHelper.OtherCompanyPK;
		var branchPk = TestDbHelper.BranchBrnPK;
		var branchPk2 = TestDbHelper.BranchBrnPK2;
		var departmentPk = TestDbHelper.DepartmentBrnPK;
		var departmentPk2 = TestDbHelper.DepartmentBrnPK2;

		var date1 = new DateTime(2024, 10, 01);
		var date2 = new DateTime(2024, 10, 02);
		var date3 = new DateTime(2024, 10, 03);
		var date4 = new DateTime(2024, 10, 04);

		var glAccountPk1 = DbHelper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
		var glAccountPk2 = DbHelper.InsertGLAccount("1234.55.02", "TestGLAccount 2");

		// Company 1
		var assetPk = InsertAsset(companyPk, branchPk, departmentPk, purchaseDate: date1, receivedDate: date1, saleDisposalDate: null, assetNumber: 1, isActive: true);
		var asset11 = assetPk;
		var txnPk = helper.InsertAssetTransaction(assetPk, companyPk, branchPk, departmentPk2, "ADT", "txnNum11", "EUR", postDate: date1, cancelledDate: null, supplierReferenceDate: date1, "supplierRef1");
		helper.InsertAssetTransactionLine(companyPk, branchPk, departmentPk2, "AAD", sequence: 1, "EUR", postDate: date1, txnPk, glAccountPk1);
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk, branchPk2, departmentPk2, "APT", "txnNum12", "EUR", postDate: date2, cancelledDate: null, supplierReferenceDate: date1, "supplierRef2");
		helper.InsertAssetTransactionLine(companyPk, branchPk2, departmentPk2, "ADA", sequence: 1, "EUR", postDate: date2, txnPk, glAccountPk2);
		helper.InsertAssetTransactionLine(companyPk, branchPk2, departmentPk2, "AFA", sequence: 2, "EUR", postDate: date2, txnPk, glAccountPk2);

		assetPk = InsertAsset(companyPk, branchPk, departmentPk, purchaseDate: date2, receivedDate: date2, saleDisposalDate: date1, assetNumber: 2, isActive: false);
		var asset12 = assetPk;
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk, branchPk2, departmentPk2, "APT", "txnNum13", "EUR", postDate: date3, cancelledDate: null, supplierReferenceDate: date2, "supplierRef3");
		helper.InsertAssetTransactionLine(companyPk, branchPk2, departmentPk2, "ADA", sequence: 1, "EUR", postDate: date3, txnPk, glAccountPk2);

		assetPk = InsertAsset(companyPk, branchPk2, departmentPk, purchaseDate: date1, receivedDate: date3, saleDisposalDate: date1, assetNumber: 3, isActive: true);
		var asset13 = assetPk;
		InsertAssetDepreciationRule(assetPk, "SLD", date1);
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk, branchPk, departmentPk, "APT", "txnNum14", "EUR", postDate: date1, cancelledDate: null, supplierReferenceDate: date3, "supplierRef1");
		helper.InsertAssetTransactionLine(companyPk, branchPk, departmentPk, "AFA", sequence: 1, "EUR", postDate: date1, txnPk, glAccountPk2);
		helper.InsertAssetTransactionLine(companyPk, branchPk, departmentPk, "AAD", sequence: 2, "EUR", postDate: date1, txnPk, glAccountPk2);

		assetPk = InsertAsset(companyPk, branchPk, departmentPk, purchaseDate: null, receivedDate: date3, saleDisposalDate: date2, assetNumber: 4, isActive: true);
		var asset14 = assetPk;
		InsertAssetDepreciationRule(assetPk, "SLD", date2);
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk, branchPk, departmentPk2, "APT", "txnNum15", "GBP", postDate: date2, cancelledDate: null, supplierReferenceDate: date3, "supplierRef2");
		helper.InsertAssetTransactionLine(companyPk, branchPk, departmentPk2, "ADA", sequence: 1, "GBP", postDate: date2, txnPk, glAccountPk1);
		helper.InsertAssetTransactionLine(companyPk, branchPk, departmentPk2, "AAD", sequence: 2, "GBP", postDate: date2, txnPk, glAccountPk1);

		assetPk = InsertAsset(companyPk, branchPk, departmentPk2, purchaseDate: date3, receivedDate: null, saleDisposalDate: date3, assetNumber: 5, isActive: true);
		InsertAssetDepreciationRule(assetPk, "SLD", date3);
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk, branchPk2, departmentPk2, "APT", "txnNum16", "EUR", postDate: date3, cancelledDate: null, supplierReferenceDate: date3, "supplierRef2");
		helper.InsertAssetTransactionLine(companyPk, branchPk2, departmentPk2, "ADA", sequence: 1, "EUR", postDate: date3, txnPk, glAccountPk2);

		// Cancelled transaction
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk, branchPk2, departmentPk2, "APT", "txnNum17", "EUR", postDate: date3, cancelledDate: date4, supplierReferenceDate: date3, "supplierRef1");
		helper.InsertAssetTransactionLine(companyPk, branchPk2, departmentPk2, "AAD", sequence: 1, "EUR", postDate: date3, txnPk, glAccountPk2);

		// Company 2
		assetPk = InsertAsset(companyPk2, branchPk2, departmentPk, purchaseDate: date1, receivedDate: date1, saleDisposalDate: date2, assetNumber: 1, isActive: true);
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk2, branchPk, departmentPk, "APT", "txnNum21", "EUR", postDate: date1, cancelledDate: null, supplierReferenceDate: date1, "supplierRef1");
		helper.InsertAssetTransactionLine(companyPk2, branchPk, departmentPk, "AFA", sequence: 1, "EUR", postDate: date1, txnPk, glAccountPk2);
		helper.InsertAssetTransactionLine(companyPk2, branchPk, departmentPk, "AAD", sequence: 2, "EUR", postDate: date1, txnPk, glAccountPk2);

		assetPk = InsertAsset(companyPk2, branchPk, departmentPk, purchaseDate: null, receivedDate: date2, saleDisposalDate: null, assetNumber: 2, isActive: true);
		txnPk = helper.InsertAssetTransaction(assetPk, companyPk2, branchPk, departmentPk2, "APT", "txnNum22", "EUR", postDate: date2, cancelledDate: null, supplierReferenceDate: date2, "supplierRef2");
		helper.InsertAssetTransactionLine(companyPk2, branchPk, departmentPk2, "ADA", sequence: 1, "EUR", postDate: date2, txnPk, glAccountPk1);
		helper.InsertAssetTransactionLine(companyPk2, branchPk, departmentPk2, "AAD", sequence: 2, "EUR", postDate: date2, txnPk, glAccountPk1);

		var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccAssetTransactionLine");
		AssertEquals("Precondition: Total transaction lines", 14, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Company. Rows count", 9, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk2}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Company. Rows count", 4, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', 'All', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("IsActive - All. Rows count", 9, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', 'Yes', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("IsActive - Yes. Rows count", 8, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', 'No', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("IsActive - No. Rows count", 1, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', 'All', '', '', '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', '', NULL, NULL, NULL, NULL, '', '', NULL)");
		AssertEquals("Empty strings in parameters. Rows count", 9, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, '{branchPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Branch. Rows count", 5, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, '{branchPk2}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Branch. Rows count", 4, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, '{branchPk},{branchPk2}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Branch list. Rows count", 9, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, '{departmentPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Department. Rows count", 2, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, '{departmentPk2}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Department. Rows count", 7, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, '{departmentPk2},{departmentPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Department list. Rows count", 9, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, '{asset11},{asset12},{asset13}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Asset list. Rows count", 6, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, '{asset12},{asset13},{asset14}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("Asset list. Rows count", 5, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, '20241001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("PurchaseDate. Rows count", 5, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, '20241001', '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("PurchaseDate. Rows count", 6, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("PurchaseDate. Rows count", 2, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("ReceivedDate. Rows count", 3, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, '20241001', '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("ReceivedDate. Rows count", 4, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("ReceivedDate. Rows count", 5, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("SaleDisposalDate. Rows count", 3, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("SaleDisposalDate. Rows count", 5, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("SaleDisposalDate. Rows count", 3, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("DepreciationStartDate. Rows count", 2, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("DepreciationStartDate. Rows count", 4, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("DepreciationStartDate. Rows count", 3, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ADT', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("TransactionType. Rows count", 1, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'APT', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("TransactionType. Rows count", 8, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EUR', NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("NKTransactionCurrency. Rows count", 7, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GBP', NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("NKTransactionCurrency. Rows count", 2, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("PostDate. Rows count", 3, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', '20241002', NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("PostDate. Rows count", 7, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241002', NULL, NULL, NULL, NULL, NULL, NULL)");
		AssertEquals("PostDate. Rows count", 6, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', NULL, NULL, NULL)");
		AssertEquals("SupplierReferenceDate. Rows count", 3, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241001', '20241002', NULL, NULL, NULL)");
		AssertEquals("SupplierReferenceDate. Rows count", 4, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '20241002', NULL, NULL, NULL, NULL)");
		AssertEquals("SupplierReferenceDate. Rows count", 6, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'supplierRef1', NULL, NULL)");
		AssertEquals("SupplierReferenceNumber. Rows count", 3, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'supplierRef2', NULL, NULL)");
		AssertEquals("SupplierReferenceNumber. Rows count", 5, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AFA', NULL)");
		AssertEquals("TransactionLineType. Rows count", 2, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AAD', NULL)");
		AssertEquals("TransactionLineType. Rows count", 3, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ADA', NULL)");
		AssertEquals("TransactionLineType. Rows count", 4, result.Rows.Count);

		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '{glAccountPk1}')");
		AssertEquals("GLAccount. Rows count", 3, result.Rows.Count);
		result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetTransactionListDetails ('{companyPk}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '{glAccountPk2}')");
		AssertEquals("GLAccount. Rows count", 6, result.Rows.Count);

		Guid InsertAsset(Guid companyPk, Guid branchPk, Guid departmentPk, DateTime? purchaseDate, DateTime? receivedDate,
			DateTime? saleDisposalDate, int assetNumber, bool isActive)
		{
			var purchaseDateStr = purchaseDate.HasValue ? $"'{purchaseDate.Value:yyyyMMdd}'" : "NULL";
			var receivedDateStr = receivedDate.HasValue ? $"'{receivedDate.Value:yyyyMMdd}'" : "NULL";
			var saleDisposalDateStr = saleDisposalDate.HasValue ? $"'{saleDisposalDate.Value:yyyyMMdd}'" : "NULL";

			var code = $"{assetNumber}";
			var desc = $"Description {assetNumber}";
			var serialNumber = $"SN{assetNumber}";
			var note = $"Note Asset {assetNumber}";
			var isActiveStr = isActive ? "1" : "0";

			var assetPk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(
				"INSERT INTO AccAssetHeader (AAH_PK, AAH_AutoVersion, AAH_IsActive, AAH_Code, AAH_Description, AAH_SerialNumber, " +
				"AAH_Note, AAH_PurchaseDate, AAH_ReceivedDate, AAH_SaleDisposalDate, AAH_GC_Company, AAH_GB_Branch, AAH_GE_Department, " +
				"AAH_SystemCreateTimeUtc, AAH_SystemCreateUser, AAH_SystemLastEditTimeUtc, AAH_SystemLastEditUser )" +
				$" VALUES ('{assetPk}', 1, {isActiveStr}, '{code}', '{desc}', '{serialNumber}', " +
				$"'{note}', {purchaseDateStr}, {receivedDateStr}, {saleDisposalDateStr}, '{companyPk}', '{branchPk}', '{departmentPk}', " +
				$"'20241101', 'XXX', '20241101', 'XXX' )");

			return assetPk;
		}

		Guid InsertAssetDepreciationRule(Guid assetPk, string depreciationMethod, DateTime? depreciationStartDate)
		{
			var depreciationStartDateStr = depreciationStartDate.HasValue ? $"'{depreciationStartDate.Value:yyyyMMdd}'" : "NULL";
			var rulePk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(
				"INSERT INTO AccAssetDepreciationRule (" +
				"ADR_PK, ADR_AutoVersion, ADR_DepreciationMethod, ADR_DepreciationPercentage, ADR_DepreciationStartDate, " +
				"ADR_AAH_Asset, ADR_SystemCreateTimeUtc, ADR_SystemCreateUser, ADR_SystemLastEditTimeUtc, ADR_SystemLastEditUser)" +
				$" VALUES ('{rulePk}', 1, '{depreciationMethod}', 0.1 , {depreciationStartDateStr}, " +
				$"'{assetPk}', '20241101', 'XXX', '20241101', 'XXX' )");

			return rulePk;
		}
	}
}
