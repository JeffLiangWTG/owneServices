using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Assets;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

#region Test
namespace Enterprise.Build.Database.Script.Testing.Public.Accounting.Assets
{
	[TestedType(typeof(Report_AssetList))]
	class Report_AssetListTest : DbCreateScriptTest
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

			var tempDate = new DateTime(2024, 01, 03);

			var guid1 = helper.InsertAsset(otherCompanyPK, branchPK, departmentPK, tempDate, 1, "1");
			var guid2 = helper.InsertAsset(otherCompanyPK, branchPK2, departmentPK, tempDate.AddDays(1), 2, "1");
			var guid3 = helper.InsertAsset(otherCompanyPK, branchPK, departmentPK2, tempDate.AddDays(2), 3, "1");
			var guid4 = helper.InsertAsset(otherCompanyPK, branchPK2, departmentPK2, tempDate.AddDays(3), 4, "0");

			for (var i = 7; i < 300; i = i + 3)
			{
				helper.InsertAsset(companyPK, branchPK, departmentPK, tempDate, i);
			}
			for (var i = 8; i < 200; i = i + 3)
			{
				helper.InsertAsset(companyPK, branchPK2, departmentPK, tempDate, i);
			}
			for (var i = 200; i < 300; i = i + 3)
			{
				helper.InsertAsset(companyPK, branchPK2, departmentPK, tempDate, i, "0");
			}
			for (var i = 9; i < 150; i = i + 3)
			{
				helper.InsertAsset(companyPK, branchPK, departmentPK2, tempDate, i);
			}
			for (var i = 150; i < 300; i = i + 3)
			{
				helper.InsertAsset(companyPK, branchPK2, departmentPK2, tempDate, i);
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccAssetHeader");
			AssertEquals("Waited lines", 297, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', '', NULL, NULL, NULL)");
			AssertEquals("Waited lines", 293, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', 'Yes', '{branchPK}', NULL, NULL, NULL, NULL, '', '', NULL, NULL, NULL, NULL)");
			AssertEquals("Waited lines", 145, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{otherCompanyPK}', 'All', '{branchPK2}', NULL, NULL, NULL, NULL, NULL, NULL, '', '', NULL, NULL)");
			AssertEquals("Waited lines", 2, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', 'Yes', NULL, '{departmentPK}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)");
			AssertEquals("Waited lines", 162, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', 'Yes', '{branchPK}', '{departmentPK}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', '')");
			AssertEquals("Waited lines", 98, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', 'Yes', NULL, '{departmentPK2}', NULL, '', '', NULL, NULL, NULL, NULL, NULL, NULL)");
			AssertEquals("Waited lines", 97, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', 'All', '{branchPK},{branchPK2}', '{departmentPK},{departmentPK2}', NULL, NULL, NULL, '', '', '20240201', '20240228', '', '')");
			AssertEquals("Waited lines", 28, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', 'Yes', '{branchPK2}', NULL, NULL, '20240301', '20240331', '', '', NULL, NULL, NULL, NULL)");
			AssertEquals("Waited lines", 10, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{companyPK}', 'No', '{branchPK2}', NULL, NULL, NULL, NULL, '20240301', '20241231', NULL, NULL, NULL, NULL)");
			AssertEquals("Waited lines", 34, result.Rows.Count);

			var listAsset = guid1 + "," + guid2 + "," + "," + guid4;
			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_AssetList ('{otherCompanyPK}', 'All', NULL, NULL, '{listAsset}', '20240101', '20240131', NULL, '', '', NULL, NULL, NULL)");
			AssertEquals("Waited lines", 3, result.Rows.Count);
		}
	}
}
#endregion
