using System;
using System.IO;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class AccAssetDepreciationBookDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new AccAssetDepreciationBookDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestLoadDataFromDatabase_OnlyIsSystemBooksAreFound()
		{
			DataHelpers.ClearTable(AccAssetDepreciationBookUpgradeTaskTest.TableName);

			var bookOnlyIsSystem = AccAssetDepreciationBookUpgradeTaskTest.InsertBookData("DEF", "bookOnlyIsSystem", true, true);

			var bookOnlyIsSystemNotActive = AccAssetDepreciationBookUpgradeTaskTest.InsertBookData("STD", "bookOnlyIsSystemNotActive", true, false);

			var company1 = new GlbCompany("Co1", "AU").InsertAndReturnObject(TestConnection);
			var bookCompanyIsSystem = AccAssetDepreciationBookUpgradeTaskTest.InsertBookData("DEF", "bookCompanyIsSystem", true, true, company1);

			var company2 = new GlbCompany("Co2", "AU").InsertAndReturnObject(TestConnection);
			var bookCompanyIsSystemNotActive = AccAssetDepreciationBookUpgradeTaskTest.InsertBookData("DEF", "bookCompanyIsSystemNotActive", true, false, company2);

			var company3 = new GlbCompany("Co3", "AU").InsertAndReturnObject(TestConnection);
			var bookCompanyNotSystem = AccAssetDepreciationBookUpgradeTaskTest.InsertBookData("TST", "bookCompanyNotSystem", false, true, company3);

			var company4 = new GlbCompany("Co4", "AU").InsertAndReturnObject(TestConnection);
			var bookCompanyNotSystemNotLive = AccAssetDepreciationBookUpgradeTaskTest.InsertBookData("TST", "bookCompanyNotSystemNotLive", false, false, company4);

			var data = new AccAssetDepreciationBookDataFile().LoadDataFromDatabase();

			CombineAssertions(() =>
			{
				AssertEquals("Table Count", 1, data.Tables.Count);
				var tableRows = data.Tables[AccAssetDepreciationBookUpgradeTaskTest.TableName].Rows;
				AssertEquals("AccAssetDepreciationBook row count", 4, tableRows.Count);
				var bookPKs = new[] { bookOnlyIsSystem, bookOnlyIsSystemNotActive, bookCompanyIsSystem, bookCompanyIsSystemNotActive }.ToList();
				for(int i = 0; i < 4;  i++)
				{
					Assert($"DataFile is correct - Row {i}", bookPKs.Contains((Guid)tableRows[i]["ADB_PK"]));
				}
			});
		}
	}
}
