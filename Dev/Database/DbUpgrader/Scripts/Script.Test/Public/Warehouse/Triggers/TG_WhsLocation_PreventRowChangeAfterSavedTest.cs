using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLocation_PreventRowChangeAfterSaved))]
	class TG_WhsLocation_PreventRowChangeAfterSavedTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsLocation_PreventRowChangeAfterSavedTest : TestCase
	{
		#region TestTrigger_LocationRow_Update

		public void TestTrigger_LocationRow_UpdateAfterSaved()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row1 = new WhsRow(whs, "ROW1").AppendInsertAndReturnObject(sql);
				var row2 = new WhsRow(whs, "ROW2").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row1.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Location must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsLocation WHERE WL_PK = '{location.PK}'"));
				AssertEquals("Row1 and Row2 must exist in DB", 2, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsRow WHERE WR_PK IN ('{row1.PK}', '{row2.PK}')"));

				var updateSQL = new SqlQueryBuilder(WhsLocation
					.UpdateWhere(location.PK)
					.Set(l => l.WL_WR, row2.PK).AsSQL());

				AssertTriggerPreventsRowChange(mainConnection, updateSQL);
			}
		}

		public void TestTrigger_LocationRow_UpdateToSameRow()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "ROW1").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Location must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsLocation WHERE WL_PK = '{location.PK}'"));
				AssertEquals("Row must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsRow WHERE WR_PK = '{row.PK}'"));

				var updateSQL = new SqlQueryBuilder(WhsLocation
					.UpdateWhere(location.PK)
					.Set(l => l.WL_WR, row.PK).AsSQL());
				AssertNoExceptionThrown("Trigger should not prevent changing to same Row", () => SaveToDB(mainConnection, updateSQL));
			}
		}

		public void TestTrigger_LocationRow_Update_UnrelatedField()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(mainConnection);
				var area1 = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var area2 = new WhsArea(whs.PK, "AREA2").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "ROW1").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area1.PK, area1.PK).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Location must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsLocation WHERE WL_PK = '{location.PK}'"));

				var updateSQL = new SqlQueryBuilder(WhsLocation
					.UpdateWhere(location.PK)
					.Set(l => l.WL_WA_PickingArea, area2.PK).AsSQL());
				AssertNoExceptionThrown("Trigger should not prevent changing fields different from WL_WR", () => SaveToDB(mainConnection, updateSQL));
				AssertEquals("Location must have its picking area changed.", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) from dbo.WhsLocation WHERE WL_PK = '{location.PK}' AND WL_WA_PickingArea = '{area2.PK}'"));
			}
		}

		#endregion

		#region TestTrigger_LocationRow_Delete

		public void TestTrigger_LocationRow_Delete()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(mainConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "ROW1").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Location must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsLocation WHERE WL_PK = '{location.PK}'"));

				var updateSQL = new SqlQueryBuilder($"DELETE dbo.WhsLocation WHERE WL_PK = '{location.PK}'");
				AssertNoExceptionThrown("Trigger should not prevent deleting a location.", () => SaveToDB(mainConnection, updateSQL));
				AssertEquals("Location must *not* exist in DB", 0, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsLocation WHERE WL_PK = '{location.PK}'"));
			}
		}

		#endregion

		#region TestAssertions

		void AssertTriggerPreventsRowChange(DbConnection connection, SqlQueryBuilder actionSql)
		{
			AssertExceptionThrown("Expected trigger to prevent change of Row for Location", typeof(SqlException), "Attempt to change the Row for a Location that is already saved.",
				() => connection.ExecuteNonQuery(actionSql.ToStringWithNewLineBetweenAppends()), assertStartsWith: true);
		}

		#endregion

		#region Implementation

		void SaveToDB(DbConnection connection, SqlQueryBuilder sql)
		{
			connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}
		#endregion
	}
}

