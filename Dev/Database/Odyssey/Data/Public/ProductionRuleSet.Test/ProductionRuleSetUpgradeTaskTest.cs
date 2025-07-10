using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ProductionRuleSetUpgradeTaskTest : TransactionedTestCase
	{
		public void TestEmptyDatabase_ProductWarehousePutaway()
		{
			RemoveDBRuleSetsAndRulesForTest();
			new ProductionRuleSetUpgradeTask().Run();

			var default3plRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection).Single(prs => prs.PRS_Context == "PWP");
			default3plRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();

			var ruleCount = ProductionRule.CountInDB(Db.Connection, prl => prl.PRL_PRS_RuleSet.FK == default3plRuleSet.PK);
			AssertEquals("Should have 1 rule.", 1, ruleCount);

			var default3plRule = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == default3plRuleSet.PK);
			default3plRule.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "Prefer Empty")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Prefer Empty")
				.ExpectEquals("PRL_Priority: ", i => i.PRL_Priority, (short)10)
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""conditions"":[],""action"":{""$type"":""PutawayActionState"",""conditions"":[],""sortByCriteria"":[{""propertyPath"":""IsEmpty"",""direction"":""descending""},{""propertyPath"":""RowName"",""direction"":""ascending""},{""propertyPath"":""PutawaySequence"",""direction"":""ascending""}]}}")
				.VerifyAll();
		}

		public void TestEmptyDatabase_TransitWarehousePutaway()
		{
			RemoveDBRuleSetsAndRulesForTest();
			new ProductionRuleSetUpgradeTask().Run();

			var defaultTwhRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection).Single(prs => prs.PRS_Context == "TWP");
			defaultTwhRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default TWH Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Transit Warehouse Directed Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();

			var ruleCount = ProductionRule.CountInDB(Db.Connection, prl => prl.PRL_PRS_RuleSet.FK == defaultTwhRuleSet.PK);
			AssertEquals("Should have 3 rules.", 3, ruleCount);

			var defaultTwhRule1 = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultTwhRuleSet.PK && prl.PRL_Priority == 1);
			defaultTwhRule1.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "Putaway Packages")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Putaway packages to first valid location, reserving temperature controlled locations for temperature controlled packages.")
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""action"":{""$type"":""PutawayActionState"",""conditions"":[{""fieldPath"":""IsTempControlledPackageOrNotTCLLocation"",""operation"":""equals"",""value"":true}],""sortByCriteria"":[{""propertyPath"":""RowName"",""direction"":""ascending""},{""propertyPath"":""Column"",""direction"":""ascending""},{""propertyPath"":""Level"",""direction"":""ascending""},{""propertyPath"":""Tray"",""direction"":""ascending""}]},""conditions"":[]}")
				.VerifyAll();

			var defaultTwhRule2 = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultTwhRuleSet.PK && prl.PRL_Priority == 500);
			defaultTwhRule2.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "Separate By Receive Consignment ID")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Separate the group of packages to putaway by their Receive Consignment IDs (preserves Handling units).")
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""action"":{""$type"":""PartitionPackagesActionState"",""propertyPaths"":[""ReceiveConsignmentID""]},""conditions"":[]}")
				.VerifyAll();

			var defaultTwhRule3 = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultTwhRuleSet.PK && prl.PRL_Priority == 1000);
			defaultTwhRule3.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "Separate All Packages")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Separate any remaining packages to putaway (preserves Handling Units).")
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""action"":{""$type"":""PartitionPackagesActionState"",""propertyPaths"":[""PackageID""]},""conditions"":[]}")
				.VerifyAll();
		}

		public void TestEmptyDatabase_ProductWarehouseAllocation()
		{
			RemoveDBRuleSetsAndRulesForTest();
			new ProductionRuleSetUpgradeTask().Run();

			var defaultAllocationRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection).Single(prs => prs.PRS_Context == "PWA");
			defaultAllocationRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Allocation Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Allocation Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();

			var ruleCount = ProductionRule.CountInDB(Db.Connection, prl => prl.PRL_PRS_RuleSet.FK == defaultAllocationRuleSet.PK);
			AssertEquals("Should have 4 rules.", 4, ruleCount);

			var defaultHplRule = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultAllocationRuleSet.PK && prl.PRL_Priority == 100);
			defaultHplRule.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "High Priority Locations")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "High Priority Locations Default Rule")
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""action"":{""$type"":""AllocateStockActionState"",""allocateCases"":true,""allocatePallets"":true,""allocateSplitCase"":true,""canBreakUOMs"":true,""conditions"":[{""fieldPath"":""Location.LocationClass"",""operation"":""equals"",""value"":""HPL""}],""sortByCriteria"":[{""propertyPath"":""ExpiryDate"",""direction"":""ascending""},{""propertyPath"":""PackingDate"",""direction"":""ascending""},{""propertyPath"":""Location.IsFixedPickFace"",""direction"":""descending""},{""propertyPath"":""ArrivalDate"",""direction"":""ascending""},{""propertyPath"":""Location.RowPathSequence"",""direction"":""ascending""},{""propertyPath"":""Location.PickPathSequence"",""direction"":""ascending""},{""propertyPath"":""PartAttribute1"",""direction"":""ascending""},{""propertyPath"":""PartAttribute2"",""direction"":""ascending""},{""propertyPath"":""PartAttribute3"",""direction"":""ascending""},{""propertyPath"":""SerialNumber"",""direction"":""ascending""}]},""conditions"":[{""fieldPath"":""Product.IsDynamic"",""operation"":""equals"",""value"":false}]}")
				.VerifyAll();

			var defaultDynamicRule = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultAllocationRuleSet.PK && prl.PRL_Priority == 200);
			defaultDynamicRule.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "Dynamic Pick Face")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Default Dynamic Pick Face Rule")
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""action"":{""$type"":""AllocateStockActionState"",""allocateCases"":true,""allocatePallets"":true,""allocateSplitCase"":true,""canBreakUOMs"":true,""conditions"":[{""fieldPath"":""Location.IsDynamicPickFace"",""operation"":""equals"",""value"":true}],""sortByCriteria"":[{""propertyPath"":""ExpiryDate"",""direction"":""ascending""},{""propertyPath"":""PackingDate"",""direction"":""ascending""},{""propertyPath"":""ArrivalDate"",""direction"":""ascending""},{""propertyPath"":""Location.RowPathSequence"",""direction"":""ascending""},{""propertyPath"":""Location.PickPathSequence"",""direction"":""ascending""},{""propertyPath"":""PartAttribute1"",""direction"":""ascending""},{""propertyPath"":""PartAttribute2"",""direction"":""ascending""},{""propertyPath"":""PartAttribute3"",""direction"":""ascending""},{""propertyPath"":""SerialNumber"",""direction"":""ascending""}]},""conditions"":[{""fieldPath"":""Product.IsDynamic"",""operation"":""equals"",""value"":true}]}")
				.VerifyAll();

			var defaultFixRule = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultAllocationRuleSet.PK && prl.PRL_Priority == 300);
			defaultFixRule.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "Fixed Pick Face")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Default Fixed Pick Face Rule")
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""action"":{""$type"":""AllocateStockActionState"",""allocateCases"":true,""allocatePallets"":true,""allocateSplitCase"":true,""canBreakUOMs"":true,""conditions"":[{""fieldPath"":""Location.IsFixedPickFace"",""operation"":""equals"",""value"":true}],""sortByCriteria"":[{""propertyPath"":""ExpiryDate"",""direction"":""ascending""},{""propertyPath"":""PackingDate"",""direction"":""ascending""},{""propertyPath"":""ArrivalDate"",""direction"":""ascending""},{""propertyPath"":""Location.RowPathSequence"",""direction"":""ascending""},{""propertyPath"":""Location.PickPathSequence"",""direction"":""ascending""},{""propertyPath"":""PartAttribute1"",""direction"":""ascending""},{""propertyPath"":""PartAttribute2"",""direction"":""ascending""},{""propertyPath"":""PartAttribute3"",""direction"":""ascending""},{""propertyPath"":""SerialNumber"",""direction"":""ascending""}]},""conditions"":[{""fieldPath"":""Product.HasPickFaces"",""operation"":""equals"",""value"":true}]}")
				.VerifyAll();

			var defaultFifoRule = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultAllocationRuleSet.PK && prl.PRL_Priority == 500);
			defaultFifoRule.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "FIFO Fallback")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Default FIFO Fallback Rule")
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""action"":{""$type"":""AllocateStockActionState"",""allocateCases"":true,""allocatePallets"":true,""allocateSplitCase"":true,""canBreakUOMs"":true,""conditions"":[],""sortByCriteria"":[{""propertyPath"":""BondedEntryDate"",""direction"":""ascending""},{""propertyPath"":""ExpiryDate"",""direction"":""ascending""},{""propertyPath"":""PackingDate"",""direction"":""ascending""},{""propertyPath"":""Location.IsFixedPickFace"",""direction"":""descending""},{""propertyPath"":""ArrivalDate"",""direction"":""ascending""},{""propertyPath"":""Location.RowPathSequence"",""direction"":""ascending""},{""propertyPath"":""Location.PickPathSequence"",""direction"":""ascending""},{""propertyPath"":""BondedEntryKey"",""direction"":""ascending""},{""propertyPath"":""PartAttribute1"",""direction"":""ascending""},{""propertyPath"":""PartAttribute2"",""direction"":""ascending""},{""propertyPath"":""PartAttribute3"",""direction"":""ascending""},{""propertyPath"":""SerialNumber"",""direction"":""ascending""}]},""conditions"":[{""fieldPath"":""Product.IsDynamic"",""operation"":""equals"",""value"":false}]}")
				.VerifyAll();
		}

		public void TestDatabaseAlreadyHasRuleSet()
		{
			RemoveDBRuleSetsAndRulesForTest();
			var ruleSet = new ProductionRuleSet("PWP", "System Default Putaway Rules") { PRS_Description = "System Default Putaway Rules", PRS_IsLive = true, PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);
			var newRuleSet = ProductionRuleSet.UpdateWhere(ruleSet.PK).Set(r => r.PK, new Guid("3f955f2c-fbfb-400a-8849-37eae9e358bd")).Post(TestConnection);
			var ruleDefinition = @"{""conditions"":[],""action"":{""$type"":""PutawayActionState"",""conditions"":[],""sortByCriteria"":[{""propertyPath"":""IsEmpty"",""direction"":""descending""},{""propertyPath"":""RowName"",""direction"":""ascending""},{""propertyPath"":""PutawaySequence"",""direction"":""ascending""}]}}";
			var rule = new ProductionRule(newRuleSet, "Prefer Empty", ruleDefinition) { PRL_Description = "Prefer Empty", PRL_Priority = 10 }.InsertAndReturnObject(TestConnection);
			ProductionRule.UpdateWhere(rule.PK).Set(r => r.PK, new Guid("5c1c7c6d-318e-40d5-a21a-4276bc9ac9f5")).Post(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			var defaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection).Single(prs => prs.PRS_Context == "PWP");
			defaultRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();

			var defaultRule = ProductionRule.ShallowLoadFromDB(Db.Connection).Single(prl => prl.PRL_PRS_RuleSet.FK == defaultRuleSet.PK);
			defaultRule.BuildAssertion(TestConnection)
				.ExpectEquals("PRL_PRS_RuleSet: ", i => i.PRL_PRS_RuleSet.FK, defaultRuleSet.PK)
				.ExpectEquals("PRL_Name: ", i => i.PRL_Name, "Prefer Empty")
				.ExpectEquals("PRL_Description: ", i => i.PRL_Description, "Prefer Empty")
				.ExpectEquals("PRL_Priority: ", i => i.PRL_Priority, (short)10)
				.ExpectEquals("PRL_RuleDefinition: ", i => i.PRL_RuleDefinition, @"{""conditions"":[],""action"":{""$type"":""PutawayActionState"",""conditions"":[],""sortByCriteria"":[{""propertyPath"":""IsEmpty"",""direction"":""descending""},{""propertyPath"":""RowName"",""direction"":""ascending""},{""propertyPath"":""PutawaySequence"",""direction"":""ascending""}]}}")
				.VerifyAll();
		}

		#region TestInsert

		public void TestInsert_OtherNullWhsRuleSet_Live()
		{
			RemoveDBRuleSetsAndRulesForTest();
			var ruleSetTestPRSSet = new ProductionRuleSet("PWP", "Test PRS Name") { PRS_Description = "Test PRS Desc", PRS_IsLive = true, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			AssertEquals($"ProductionRuleSet Count is correct.", 7, ProductionRuleSet.CountInDB(Db.Connection));
			ProductionRuleSet.AssertFromDB(Db.Connection, ruleSetTestPRSSet.PK)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "Test PRS Name")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "Test PRS Desc")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, false)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();

			var sysDefaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection, r => r.PK != ruleSetTestPRSSet.PK).Single(prs => prs.PRS_Context == "PWP");
			sysDefaultRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, false)
				.VerifyAll();
		}

		public void TestInsert_OtherNullWhsRuleSet_NotLive()
		{
			RemoveDBRuleSetsAndRulesForTest();
			var ruleSetTestPRSSet = new ProductionRuleSet("PWP", "Test PRS Name") { PRS_Description = "Test PRS Desc", PRS_IsLive = false, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			AssertEquals($"ProductionRuleSet Count is correct.", 7, ProductionRuleSet.CountInDB(Db.Connection));
			ProductionRuleSet.AssertFromDB(Db.Connection, ruleSetTestPRSSet.PK)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "Test PRS Name")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "Test PRS Desc")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, false)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, false)
				.VerifyAll();

			var sysDefaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection, r => r.PK != ruleSetTestPRSSet.PK).Single(prs => prs.PRS_Context == "PWP");
			sysDefaultRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();
		}

		public void TestInsert_OtherWhsRuleSet_Live()
		{
			RemoveDBRuleSetsAndRulesForTest();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var ruleSetTestPRSSet = new ProductionRuleSet("PWP", "Test PRS Name") { PRS_WW_Warehouse = whs, PRS_Description = "Test PRS Desc", PRS_IsLive = true, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			AssertEquals($"ProductionRuleSet Count is correct.", 7, ProductionRuleSet.CountInDB(Db.Connection));
			ProductionRuleSet.AssertFromDB(Db.Connection, ruleSetTestPRSSet.PK)
				.ExpectEquals("PRS_WW_Warehouse: ", i => i.PRS_WW_Warehouse.FK, whs.PK)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "Test PRS Name")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "Test PRS Desc")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, false)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();

			var sysDefaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection, r => r.PK != ruleSetTestPRSSet.PK).Single(prs => prs.PRS_Context == "PWP");
			sysDefaultRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();
		}

		public void TestInsert_OtherWhsRuleSet_NotLive()
		{
			RemoveDBRuleSetsAndRulesForTest();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var ruleSetTestPRSSet = new ProductionRuleSet("PWP", "Test PRS Name") { PRS_WW_Warehouse = whs, PRS_Description = "Test PRS Desc", PRS_IsLive = false, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			AssertEquals($"ProductionRuleSet Count is correct", 7, ProductionRuleSet.CountInDB(Db.Connection));
			ProductionRuleSet.AssertFromDB(Db.Connection, ruleSetTestPRSSet.PK)
				.ExpectEquals("PRS_WW_Warehouse: ", i => i.PRS_WW_Warehouse.FK, whs.PK)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "Test PRS Name")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "Test PRS Desc")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, false)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, false)
				.VerifyAll();

			var sysDefaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection, r => r.PK != ruleSetTestPRSSet.PK).Single(prs => prs.PRS_Context == "PWP");
			sysDefaultRuleSet.BuildAssertion(TestConnection)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();
		}

		#endregion

		#region TestUpdate

		public void TestUpdate_Live()
		{
			RemoveDBRuleSetsAndRulesForTest();
			var ruleSet = new ProductionRuleSet("PWP", "Lovely Potates") { PRS_Description = "Potato for dinner Everyone", PRS_IsLive = true, PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);
			ProductionRuleSet.UpdateWhere(ruleSet.PK).Set(r => r.PK, new Guid("3f955f2c-fbfb-400a-8849-37eae9e358bd")).Post(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			var defaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection).Single(prs => prs.PRS_Context == "PWP");
			ProductionRuleSet.AssertFromDB(TestConnection, defaultRuleSet.PK)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();
		}

		public void TestUpdate_NotLive()
		{
			RemoveDBRuleSetsAndRulesForTest();
			var ruleSet = new ProductionRuleSet("PWP", "System Default Goose") { PRS_Description = "No changes", PRS_IsLive = false, PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);
			ProductionRuleSet.UpdateWhere(ruleSet.PK).Set(r => r.PK, new Guid("3f955f2c-fbfb-400a-8849-37eae9e358bd")).Post(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			var defaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection).Single(prs => prs.PRS_Context == "PWP");
			ProductionRuleSet.AssertFromDB(TestConnection, defaultRuleSet.PK)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, false)
				.VerifyAll();
		}

		#endregion

		public void TestDelete()
		{
			RemoveDBRuleSetsAndRulesForTest();
			new ProductionRuleSet("PWP", "Test PRS Name") { PRS_Description = "Test PRS Desc", PRS_IsLive = false, PRS_IsSystem = true }.InsertAndReturnObject(TestConnection);

			new ProductionRuleSetUpgradeTask().Run();

			var defaultRuleSet = ProductionRuleSet.ShallowLoadFromDB(Db.Connection).Single(prs => prs.PRS_Context == "PWP");
			ProductionRuleSet.AssertFromDB(TestConnection, defaultRuleSet.PK)
				.ExpectEquals("PRS_Name: ", i => i.PRS_Name, "System Default 3PL Putaway Rules")
				.ExpectEquals("PRS_Description: ", i => i.PRS_Description, "System Default Product Warehouse Putaway Rules")
				.ExpectEquals("PRS_IsSystem: ", i => i.PRS_IsSystem, true)
				.ExpectEquals("PRS_IsLive: ", i => i.PRS_IsLive, true)
				.VerifyAll();
		}

		#region Implementation

		void RemoveDBRuleSetsAndRulesForTest()
		{
			var removeSetsAndRulesSql = $@"
DELETE FROM dbo.ProductionRule;
DELETE FROM dbo.ProductionRuleSet;";
			Db.Connection.ExecuteNonQuery(removeSetsAndRulesSql);
		}

		#endregion
	}
}
