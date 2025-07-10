using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ProductionRuleSetDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new ProductionRuleSetDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestLoadDataFromDatabase_OnlyIsSystemRulesAsShown()
		{
			var removeSetsAndRulesSql = $@"
DELETE FROM dbo.ProductionRule;
DELETE FROM dbo.ProductionRuleSet;";
			TestConnection.ExecuteNonQuery(removeSetsAndRulesSql);

			var sql = new StringBuilder();
			var ruleSetOnlyIsSystem = new ProductionRuleSet("PWP", "ruleSetOnlyIsSystem") { PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);

			var branch1 = new GlbBranch("Br1").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("Wh1", branch1.PK).WithDockDoor(TestConnection);
			var ruleSetWhs2IsSystem = new ProductionRuleSet("PWP", "ruleSetWhs2IsSystem") { PRS_WW_Warehouse = whs1, PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);

			var branch2 = new GlbBranch("Br2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("Wh2", branch2.PK).WithDockDoor(TestConnection);
			var ruleSetWhs2NotIsSystem = new ProductionRuleSet("PWP", "ruleSetWhs2NotIsSystem") { PRS_WW_Warehouse = whs2, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);

			var branch3 = new GlbBranch("Br3").InsertAndReturnObject(TestConnection);
			var whs3 = new WhsWarehouse("Wh3", branch3.PK).WithDockDoor(TestConnection);
			var ruleSetIsLiveIsSystem = new ProductionRuleSet("PWP", "ruleSetIsLiveIsSystem") { PRS_WW_Warehouse = whs3, PRS_IsLive = true, PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);

			var branch4 = new GlbBranch("Br4").InsertAndReturnObject(TestConnection);
			var whs4 = new WhsWarehouse("Wh4", branch4.PK).WithDockDoor(TestConnection);
			var ruleSetIsLiveNotIsSystem = new ProductionRuleSet("PWP", "ruleSetIsLiveNotIsSystem") { PRS_WW_Warehouse = whs4, PRS_IsLive = true, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);

			var branch7 = new GlbBranch("Br7").InsertAndReturnObject(TestConnection);
			var whs7 = new WhsWarehouse("Wh7", branch7.PK).WithDockDoor(TestConnection);
			var ruleSetWithRuleIsSystem = new ProductionRuleSet("PWP", "ruleSetWithRuleIsSystem") { PRS_WW_Warehouse = whs7, PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);
			var ruleIsSystem = new ProductionRule(ruleSetWithRuleIsSystem, "ruleIsSystem", "Define here").InsertAndReturnObject(TestConnection);

			var branch8 = new GlbBranch("Br8").InsertAndReturnObject(TestConnection);
			var whs8 = new WhsWarehouse("Wh8", branch8.PK).WithDockDoor(TestConnection);
			var ruleSetWithRuleNotIsSystem = new ProductionRuleSet("PWP", "ruleSetWithRuleNotIsSystem") { PRS_WW_Warehouse = whs8, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);
			var ruleNotIsSystem = new ProductionRule(ruleSetWithRuleNotIsSystem, "ruleNotIsSystem", "Define here").InsertAndReturnObject(TestConnection);

			var file = new ProductionRuleSetDataFile();
			var data = file.LoadDataFromDatabase();

			CombineAssertions(() =>
			{
				AssertEquals("Table Count", 2, data.Tables.Count);
				AssertEquals("ProductionRuleSet row count", 4, data.Tables["ProductionRuleSet"].Rows.Count);
				var ruleSetPKs = new[] { ruleSetOnlyIsSystem.PK, ruleSetIsLiveIsSystem.PK, ruleSetWhs2IsSystem.PK, ruleSetWithRuleIsSystem.PK }.ToList();
				AssertEquals("DataFile is correct - Row 0", true, ruleSetPKs.Contains((Guid)data.Tables["ProductionRuleSet"].Rows[0]["PRS_PK"]));
				AssertEquals("DataFile is correct - Row 1", true, ruleSetPKs.Contains((Guid)data.Tables["ProductionRuleSet"].Rows[1]["PRS_PK"]));
				AssertEquals("DataFile is correct - Row 2", true, ruleSetPKs.Contains((Guid)data.Tables["ProductionRuleSet"].Rows[2]["PRS_PK"]));
				AssertEquals("DataFile is correct - Row 3", true, ruleSetPKs.Contains((Guid)data.Tables["ProductionRuleSet"].Rows[3]["PRS_PK"]));

				Assert("ProductionRule row count", data.Tables["ProductionRule"].Rows.Count == 1);
				AssertEquals("Rule is correct", ruleIsSystem.PK, data.Tables["ProductionRule"].Rows[0]["PRL_PK"]);
			});
		}
	}
}
