using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Assets;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

#region Test
namespace Enterprise.Build.Database.Script.Testing.Public.Accounting.Assets
{
	[TestedType(typeof(Report_AssetBook))]
	class Report_AssetBookTest : DbCreateScriptTest
	{
		//[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var companyPK = TestDbHelper.DefaultCompanyPK;
			var otherCompanyPK = TestDbHelper.OtherCompanyPK;
			var branchPK = TestDbHelper.BranchBrnPK;
			var branchPK2 = TestDbHelper.BranchBrnPK2;
			var departmentPK = TestDbHelper.DepartmentBrnPK;
			var departmentPK2 = TestDbHelper.DepartmentBrnPK2;
			var glAccountPkAAD = TestDbHelper.glAccountPkAAD;
			var glAccountPkADA = TestDbHelper.glAccountPkADA;
			var glAccountPkAFA = TestDbHelper.glAccountPkAFA;

			var date1 = new DateTime(2024, 01, 01);
			var date2 = new DateTime(2024, 01, 15);

			Guid[] assetGuid = new Guid[10];
			assetGuid[0] = helper.InsertAsset(otherCompanyPK, branchPK, departmentPK, date1, 1, "1");
			assetGuid[3] = helper.InsertAsset(otherCompanyPK, branchPK, departmentPK2, date1, 2, "1");
			assetGuid[6] = helper.InsertAsset(otherCompanyPK, branchPK2, departmentPK, date1, 3, "1");
			assetGuid[9] = helper.InsertAsset(otherCompanyPK, branchPK2, departmentPK2, date1, 4, "0");
			assetGuid[1] = helper.InsertAsset(companyPK, branchPK, departmentPK, date1, 5, "1");
			assetGuid[2] = helper.InsertAsset(companyPK, branchPK, departmentPK2, date1, 6, "1");
			assetGuid[4] = helper.InsertAsset(companyPK, branchPK2, departmentPK, date1, 7, "1");
			assetGuid[5] = helper.InsertAsset(companyPK, branchPK2, departmentPK2, date1, 8, "1");
			assetGuid[7] = helper.InsertAsset(companyPK, branchPK, departmentPK, date1, 88, "0");
			assetGuid[8] = helper.InsertAsset(companyPK, branchPK, departmentPK, date1, 9, "1");

			for (int i = 0; i < 10; i++)
			{
				var date22 = date2.AddDays(i * 10);
				var date33 = date22.AddMonths(4);
				var date44 = date22.AddMonths(8);

				var amount = 100 + i * 100;

				var txnPk = helper.InsertAssetTransaction(assetGuid[i], companyPK, branchPK, departmentPK2, "APT", $"txnNum{i}", "EUR", postDate: date1, cancelledDate: null, supplierReferenceDate: date1, $"supplierRef{i}");
				helper.InsertAssetTransactionLine(companyPK, branchPK, departmentPK2, "AFA", sequence: 1, "EUR", postDate: date1, txnPk, glAccountPkAAD, amount * 10);

				txnPk = helper.InsertAssetTransaction(assetGuid[i], companyPK, branchPK2, departmentPK2, "ADT", $"txnNum1{i}", "EUR", postDate: date22, cancelledDate: null, supplierReferenceDate: null, "");
				helper.InsertAssetTransactionLine(companyPK, branchPK2, departmentPK2, "ADA", sequence: 1, "EUR", postDate: date22, txnPk, glAccountPkADA, amount);
				helper.InsertAssetTransactionLine(companyPK, branchPK2, departmentPK2, "AAD", sequence: 2, "EUR", postDate: date22, txnPk, glAccountPkAFA, amount);

				txnPk = helper.InsertAssetTransaction(assetGuid[i], companyPK, branchPK2, departmentPK2, "ADT", $"txnNum2{i}", "EUR", postDate: date33, cancelledDate: null, supplierReferenceDate: date1, "supplierRef_ADT");
				helper.InsertAssetTransactionLine(companyPK, branchPK2, departmentPK2, "ADA", sequence: 1, "EUR", postDate: date33, txnPk, glAccountPkADA, amount);
				helper.InsertAssetTransactionLine(companyPK, branchPK2, departmentPK2, "AAD", sequence: 2, "EUR", postDate: date33, txnPk, glAccountPkAFA, amount);

				txnPk = helper.InsertAssetTransaction(assetGuid[i], companyPK, branchPK2, departmentPK2, "ADT", $"txnNum3{i}", "EUR", postDate: date44, cancelledDate: null, supplierReferenceDate: null, "supplierRef_ADT");
				helper.InsertAssetTransactionLine(companyPK, branchPK2, departmentPK2, "ADA", sequence: 1, "EUR", postDate: date44, txnPk, glAccountPkADA, amount);
				helper.InsertAssetTransactionLine(companyPK, branchPK2, departmentPK2, "AAD", sequence: 2, "EUR", postDate: date44, txnPk, glAccountPkAFA, amount);
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccAssetHeader");
			AssertEquals("Waited lines", 10, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetBook ('{companyPK}', NULL, NULL, NULL, '20240101', '20241231') ORDER BY AAH_Code");
			AssertEquals("Waited lines", 5, result.Rows.Count);

			AssertEquals("supplierRef1", result.Rows[0]["supplierReferenceNumber"]);
			AssertEquals(2000m, result.Rows[0]["PurchaseAmount"]);
			AssertEquals(0m, result.Rows[0]["BeginningDepreciationAmount"]);
			AssertEquals(600m, result.Rows[0]["CurrentDepreciationAmount"]);

			AssertEquals("supplierRef2", result.Rows[1]["supplierReferenceNumber"]);
			AssertEquals(3000m, result.Rows[1]["PurchaseAmount"]);
			AssertEquals(0m, result.Rows[1]["BeginningDepreciationAmount"]);
			AssertEquals(900m, result.Rows[1]["CurrentDepreciationAmount"]);

			AssertEquals("supplierRef4", result.Rows[2]["supplierReferenceNumber"]);
			AssertEquals(5000m, result.Rows[2]["PurchaseAmount"]);
			AssertEquals(0m, result.Rows[2]["BeginningDepreciationAmount"]);
			AssertEquals(1500m, result.Rows[2]["CurrentDepreciationAmount"]);

			AssertEquals("supplierRef5", result.Rows[3]["supplierReferenceNumber"]);
			AssertEquals(6000m, result.Rows[3]["PurchaseAmount"]);
			AssertEquals(0m, result.Rows[3]["BeginningDepreciationAmount"]);
			AssertEquals(1800m, result.Rows[3]["CurrentDepreciationAmount"]);

			AssertEquals("supplierRef8", result.Rows[4]["supplierReferenceNumber"]);
			AssertEquals(9000m, result.Rows[4]["PurchaseAmount"]);
			AssertEquals(0m, result.Rows[4]["BeginningDepreciationAmount"]);
			AssertEquals(2700m, result.Rows[4]["CurrentDepreciationAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetBook ('{companyPK}', '{branchPK}', '{departmentPK}', NULL, '20240601', '20241231') ORDER BY AAH_Code");
			AssertEquals("Waited lines", 2, result.Rows.Count);

			AssertEquals(2000m, result.Rows[0]["PurchaseAmount"]);
			AssertEquals(400m, result.Rows[0]["BeginningDepreciationAmount"]);
			AssertEquals(200m, result.Rows[0]["CurrentDepreciationAmount"]);

			AssertEquals(9000m, result.Rows[1]["PurchaseAmount"]);
			AssertEquals(900m, result.Rows[1]["BeginningDepreciationAmount"]);
			AssertEquals(1800m, result.Rows[1]["CurrentDepreciationAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetBook ('{otherCompanyPK}', NULL, NULL, NULL, '20240301', '20240930') ORDER BY AAH_Code");
			AssertEquals("Waited lines", 3, result.Rows.Count);

			AssertEquals(1000m, result.Rows[0]["PurchaseAmount"]);
			AssertEquals(100m, result.Rows[0]["BeginningDepreciationAmount"]);
			AssertEquals(200m, result.Rows[0]["CurrentDepreciationAmount"]);

			AssertEquals(4000m, result.Rows[1]["PurchaseAmount"]);
			AssertEquals(400m, result.Rows[1]["BeginningDepreciationAmount"]);
			AssertEquals(400m, result.Rows[1]["CurrentDepreciationAmount"]);

			AssertEquals(7000m, result.Rows[2]["PurchaseAmount"]);
			AssertEquals(0m, result.Rows[2]["BeginningDepreciationAmount"]);
			AssertEquals(1400m, result.Rows[2]["CurrentDepreciationAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetBook ('{companyPK}', '{branchPK}', NULL, NULL, '20240615', '20240930') ORDER BY AAH_Code");
			AssertEquals("Waited lines", 3, result.Rows.Count);

			AssertEquals(2000m, result.Rows[0]["PurchaseAmount"]);
			AssertEquals(400m, result.Rows[0]["BeginningDepreciationAmount"]);
			AssertEquals(200m, result.Rows[0]["CurrentDepreciationAmount"]);

			AssertEquals(3000m, result.Rows[1]["PurchaseAmount"]);
			AssertEquals(600m, result.Rows[1]["BeginningDepreciationAmount"]);
			AssertEquals(0m, result.Rows[1]["CurrentDepreciationAmount"]);

			AssertEquals(9000m, result.Rows[2]["PurchaseAmount"]);
			AssertEquals(900m, result.Rows[2]["BeginningDepreciationAmount"]);
			AssertEquals(900m, result.Rows[2]["CurrentDepreciationAmount"]);
		}
	}
}
#endregion
