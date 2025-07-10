using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventForLocationHaveClassNotFIXIfPickFace))]
	class TG_PreventForLocationHaveClassNotFIXIfPickFaceTest : DBCreateTriggerScriptTest
	{
		public void TestTG_PreventLocationTypeUpdateForLocationUsedByPickFace()
		{
			var sql = new SqlQueryBuilder();
			var locationTypeFIXUsed = new WhsLocationType("PF1") { WLT_LocationClass = "FIX", WLT_MaximumNumberOfProducts = 1 }.AppendInsertAndReturnObject(sql);
			var locationTypeFIXNotUsed = new WhsLocationType("PF2") { WLT_LocationClass = "FIX", WLT_MaximumNumberOfProducts = 1 }.AppendInsertAndReturnObject(sql);
			var locationTypeNOR = new WhsLocationType("TST") { WLT_LocationClass = "NOR" }.AppendInsertAndReturnObject(sql);

			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "B") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLT001").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PRD001").AppendInsertAndReturnObject(sql);

			var locationUsed = new WhsLocation(row.PK, area.PK, area.PK, locationTypeFIXUsed.PK)
			{
				WL_MaxQuantity = 0,
				WL_Column = 1
			}.AppendInsertAndReturnObject(sql);

			var locationNotUsed = new WhsLocation(row.PK, area.PK, area.PK, locationTypeFIXNotUsed.PK)
			{
				WL_MaxQuantity = 0,
				WL_Column = 2
			}.AppendInsertAndReturnObject(sql);

			var pickFace = new WhsPickFace(product.PK, client.PK, locationUsed.PK).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown(
				() => WhsLocation
				.UpdateWhere(locationUsed.PK)
				.Set(l => l.WL_WLT_LocationType, locationTypeFIXNotUsed.PK).Post(TestConnection));

			AssertNoExceptionThrown(
				() => WhsLocation
				.UpdateWhere(locationNotUsed.PK)
				.Set(l => l.WL_WLT_LocationType, locationTypeNOR.PK).Post(TestConnection));

			AssertExceptionThrown(typeof(SqlException), "A location used in a Pick Face must have a location type that has a class of FIX",
				() => WhsLocation
				.UpdateWhere(locationUsed.PK)
				.Set(l => l.WL_WLT_LocationType, locationTypeNOR.PK).Post(TestConnection), true);
		}
	}
}
