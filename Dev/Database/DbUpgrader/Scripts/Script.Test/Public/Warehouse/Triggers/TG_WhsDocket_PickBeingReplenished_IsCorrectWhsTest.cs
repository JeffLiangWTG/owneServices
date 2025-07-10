using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocket_PickBeingReplenished_IsCorrectWhs))]
	class TG_WhsDocket_PickBeingReplenished_IsCorrectWhsTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_AreaInSameWhs_Insert()
		{
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(warehouse, "14", "NEW").InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown(
				"Update of WD_WP_PickBeingReplenished should succeed.",
				() => new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_PickBeingReplenished = pick }.InsertAndReturnObject(TestConnection)
			);
		}

		public void TestTrigger_AreaInSameWhs_Update()
		{
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var warehouse = new WhsWarehouse("WHS").WithDockDoor(TestConnection);
			var row = new WhsRow(warehouse, "A").InsertAndReturnObject(TestConnection);
			var area = new WhsArea(warehouse.PK, "AREA").InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(warehouse, "14", "NEW").InsertAndReturnObject(TestConnection);
			var docket = new WhsDocket(client.PK, warehouse.PK, "TFR", "TFR", "ENT", "T1").InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown(
				"Update of WD_WP_PickBeingReplenished should succeed.",
				() => WhsDocket.UpdateWhere(docket.PK).Set(l => l.WD_WP_PickBeingReplenished, pick).Post(TestConnection)
			);
		}

		public void TestTrigger_AreaMustBeInSameWhs_Insert()
		{
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var warehouse1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(TestConnection);
			var row2 = new WhsRow(warehouse2, "B").InsertAndReturnObject(TestConnection);
			var area2 = new WhsArea(warehouse2.PK, "AREA2").InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(warehouse2, "14", "NEW").InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsIncorrectPickChange(() => new WhsDocket(client.PK, warehouse1.PK, "TFR", "TFR", "ENT", "T1") { WD_WP_PickBeingReplenished = pick }.InsertAndReturnObject(TestConnection));
		}

		public void TestTrigger_AreaMustBeInSameWhs_Update()
		{
			var client = new OrgHeader("CLIENT").InsertAndReturnObject(TestConnection);
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var warehouse1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(TestConnection);
			var row2 = new WhsRow(warehouse2, "B").InsertAndReturnObject(TestConnection);
			var area2 = new WhsArea(warehouse2.PK, "AREA2").InsertAndReturnObject(TestConnection);

			var pick = new WhsPick(warehouse2, "14", "NEW").InsertAndReturnObject(TestConnection);
			var docket = new WhsDocket(client.PK, warehouse1.PK, "TFR", "TFR", "ENT", "T1").InsertAndReturnObject(TestConnection);
			AssertTriggerPreventsIncorrectPickChange(() => WhsDocket.UpdateWhere(docket.PK).Set(l => l.WD_WP_PickBeingReplenished, pick).Post(TestConnection));
		}

		void AssertTriggerPreventsIncorrectPickChange(AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent update of Dynamic Pick Area on WhsPick",
				typeof(SqlException),
				"The Pick set in PickBeingReplenished must be from the same Warehouse as the one on the Transfer.\r\nThe transaction ended in the trigger. The batch has been aborted.",
				codeToRun
			);
		}
	}
}

