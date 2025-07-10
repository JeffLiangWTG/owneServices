using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Assets;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

#region Test
namespace Enterprise.Build.Database.Script.Testing.Public.Accounting.Assets
{
	[TestedType(typeof(GetAssetAmount))]
	class GetAssetAmountTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = TestDbHelper.BranchBrnPK;
			var departmentPK = TestDbHelper.DepartmentBrnPK;
			var glAccountPkAAD = TestDbHelper.glAccountPkAAD;
			var glAccountPkADA = TestDbHelper.glAccountPkADA;
			var glAccountPkAFA = TestDbHelper.glAccountPkAFA;

			var date1 = new DateTime(2024, 01, 01);
			var date2 = new DateTime(2024, 01, 15);

			var assetGuid1 = helper.InsertAsset(companyPK, branchPK, departmentPK, date1, 1, "1");
			var assetGuid2 = helper.InsertAsset(companyPK, branchPK, departmentPK, date1, 2, "1");

			var date22 = date2.AddDays(10);
			var date33 = date22.AddMonths(4);
			var date44 = date22.AddMonths(8);

			var txnPk = helper.InsertAssetTransaction(assetGuid1, companyPK, branchPK, departmentPK, "APT", $"txnNum1", "EUR", postDate: date1, cancelledDate: null, supplierReferenceDate: date1, "supplierRef1");
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "AFA", sequence: 1, "EUR", postDate: date1, txnPk, glAccountPkAAD, 1000);

			txnPk = helper.InsertAssetTransaction(assetGuid1, companyPK, branchPK, departmentPK, "ADT", $"txnNum11", "EUR", postDate: date22, cancelledDate: null, supplierReferenceDate: date1, "supplierRef2");
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "ADA", sequence: 1, "EUR", postDate: date22, txnPk, glAccountPkADA, 200);
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "AAD", sequence: 2, "EUR", postDate: date22, txnPk, glAccountPkAFA, 200);

			txnPk = helper.InsertAssetTransaction(assetGuid1, companyPK, branchPK, departmentPK, "ADT", $"txnNum21", "EUR", postDate: date33, cancelledDate: null, supplierReferenceDate: date1, "supplierRef2");
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "ADA", sequence: 1, "EUR", postDate: date33, txnPk, glAccountPkADA, 300);
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "AAD", sequence: 2, "EUR", postDate: date33, txnPk, glAccountPkAFA, 300);

			txnPk = helper.InsertAssetTransaction(assetGuid1, companyPK, branchPK, departmentPK, "ADT", $"txnNum31", "EUR", postDate: date44, cancelledDate: null, supplierReferenceDate: date1, "supplierRef2");
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "ADA", sequence: 1, "EUR", postDate: date44, txnPk, glAccountPkADA, 400);
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "AAD", sequence: 2, "EUR", postDate: date44, txnPk, glAccountPkAFA, 400);

			txnPk = helper.InsertAssetTransaction(assetGuid2, companyPK, branchPK, departmentPK, "APT", $"txnNum2", "EUR", postDate: date1, cancelledDate: null, supplierReferenceDate: date1, "supplierRef11");
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "AFA", sequence: 1, "EUR", postDate: date1, txnPk, glAccountPkAAD, 2000);

			txnPk = helper.InsertAssetTransaction(assetGuid2, companyPK, branchPK, departmentPK, "ADT", $"txnNum12", "EUR", postDate: date22, cancelledDate: null, supplierReferenceDate: date1, "supplierRef12");
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "ADA", sequence: 1, "EUR", postDate: date22, txnPk, glAccountPkADA, 500);
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "AAD", sequence: 2, "EUR", postDate: date22, txnPk, glAccountPkAFA, 500);

			txnPk = helper.InsertAssetTransaction(assetGuid2, companyPK, branchPK, departmentPK, "ADT", $"txnNum22", "EUR", postDate: date33, cancelledDate: null, supplierReferenceDate: date1, "supplierRef12");
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "ADA", sequence: 1, "EUR", postDate: date33, txnPk, glAccountPkADA, 500);
			helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK, "AAD", sequence: 2, "EUR", postDate: date33, txnPk, glAccountPkAFA, 500);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccAssetHeader");
			AssertEquals("Waited lines", 2, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid1}', 'APT', 'AFA', null, '20240131')");
			AssertEquals("Waited lines", 1, result.Rows.Count);
			AssertEquals(1000m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid1}', 'ADT', 'ADA', null, '20240131')");
			AssertEquals("Waited lines", 1, result.Rows.Count);
			AssertEquals(200m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid1}', 'ADT', 'ADA', '20240131', '20241231')");
			AssertEquals("Waited lines", 1, result.Rows.Count);
			AssertEquals(700m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid1}', 'ADT', 'ADA', null, '20240831')");
			AssertEquals(500m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid1}', 'ADT', 'ADA', '20240831', '20241231')");
			AssertEquals(400m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid2}', 'APT', 'AFA', null, '20240131')");
			AssertEquals("Waited lines", 1, result.Rows.Count);
			AssertEquals(2000m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid2}', 'ADT', 'ADA', null, '20240131')");
			AssertEquals("Waited lines", 1, result.Rows.Count);
			AssertEquals(500m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid2}', 'ADT', 'ADA', '20240131', '20241231')");
			AssertEquals("Waited lines", 1, result.Rows.Count);
			AssertEquals(500m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid2}', 'ADT', 'ADA', null, '20240831')");
			AssertEquals(1000m, result.Rows[0]["totalAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT totalAmount FROM dbo.GetAssetAmount ('{assetGuid2}', 'ADT', 'ADA', '20240831', '20241231')");
			AssertEquals(0m, result.Rows[0]["totalAmount"]);
		}
	}
}
#endregion
