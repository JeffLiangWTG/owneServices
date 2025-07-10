using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick))]
	class TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPickTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CL1").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("WH1", branch1.PK) { WW_IsVirtualWarehouse = false }.WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var ddlType = new WhsLocationType("DDL") { WLT_LocationClass = "DDL", WLT_DefaultCycleCountGranularity = string.Empty }.AppendInsertAndReturnObject(sql);
			var dockdoor = new WhsLocation(row.PK, area.PK, area.PK, ddlType.PK) { WL_LocationStatus = "NOR" }.AppendInsertAndReturnObject(sql);

			var ddA = new WhsDockDoorAssignment(dockdoor).AppendInsertAndReturnObject(sql);

			var errorMessage = "Attempt to leave a Dock Door Assignment with no referencing Picks.";
			AssertExceptionThrown(
				"Expected trigger to prevent update of Pick",
				typeof(SqlException),
				errorMessage,
				() =>
				{
					TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				},
				assertStartsWith: true
			);
		}
	}
}

