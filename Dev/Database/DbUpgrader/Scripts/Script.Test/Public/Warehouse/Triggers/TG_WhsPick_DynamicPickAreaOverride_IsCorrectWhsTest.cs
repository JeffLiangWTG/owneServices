using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPick_DynamicPickAreaOverride_IsCorrectWhs))]
	class TG_WhsPick_DynamicPickAreaOverride_IsCorrectWhsTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_AreaInSameWhs_Insert()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);

			AssertNoExceptionThrown(
				"Update of WP_WA_DynamicPickAreaOverride should succeed.",
				() => new WhsPick(warehouse, "14", "NEW") { WP_WA_DynamicPickAreaOverride = area }.InsertAndReturnObject(TestConnection)
			);
		}

		public void TestTrigger_AreaInSameWhs_Update()
		{
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(warehouse, "14", "NEW").InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown(
				"Update of WP_WA_DynamicPickAreaOverride should succeed.",
				() => WhsPick.UpdateWhere(pick.PK).Set(l => l.WP_WA_DynamicPickAreaOverride, area).Post(TestConnection)
			);
		}

		public void TestTrigger_AreaMustBeInSameWhs_Insert()
		{
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var warehouse1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(TestConnection);
			var row2 = new WhsRow(warehouse2, "B").InsertAndReturnObject(TestConnection);
			var area2 = new WhsArea(warehouse2.PK, "AREA2").InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsIncorrectAreaChange(() => new WhsPick(warehouse1, "14", "NEW") { WP_WA_DynamicPickAreaOverride = area2 }.InsertAndReturnObject(TestConnection));
		}

		public void TestTrigger_AreaMustBeInSameWhs_Update()
		{
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var warehouse1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(TestConnection);
			var row2 = new WhsRow(warehouse2, "B").InsertAndReturnObject(TestConnection);
			var area2 = new WhsArea(warehouse2.PK, "AREA2").InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(warehouse1, "14", "NEW").InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsIncorrectAreaChange(() => WhsPick.UpdateWhere(pick.PK).Set(l => l.WP_WA_DynamicPickAreaOverride, area2).Post(TestConnection));
		}

		void AssertTriggerPreventsIncorrectAreaChange(AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent update of Dynamic Pick Area on WhsPick",
				typeof(SqlException),
				"The Area set in DynamicPickAreaOverride must be from the same Warehouse as the one on the Pick.\r\nThe transaction ended in the trigger. The batch has been aborted.",
				codeToRun
			);
		}
	}
}

