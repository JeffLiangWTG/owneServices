using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsRow_PreventWarehouseChangeAfterSaved))]
	class TG_WhsRow_PreventWarehouseChangeAfterSavedTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsRow_PreventWarehouseChangeAfterSavedTest : TestCase
	{
		#region TestTrigger_WW_Whs_Update

		public void TestTrigger_WW_Whs_UpdateAfterSaved()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var branch2 = new GlbBranch("BR2").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(mainConnection);
				var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(mainConnection);
				var row = new WhsRow(whs1, "ROW1").AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Row must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsRow WHERE WR_PK = '{row.PK}'"));

				AssertTriggerPreventsWarehouseChange(mainConnection, new StringBuilder(WhsRow
					.UpdateWhere(row.PK)
					.Set(r => r.WR_WW_Whs, whs2).AsSQL()));
			}
		}

		public void TestTrigger_WW_Whs_UpdateToSameWhs()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(mainConnection);
				var row = new WhsRow(whs, "ROW1").AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Row must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsRow WHERE WR_PK = '{row.PK}'"));

				var updateSQL = new SqlQueryBuilder(WhsRow
					.UpdateWhere(row.PK)
					.Set(r => r.WR_WW_Whs, whs).AsSQL());
				AssertNoExceptionThrown("Trigger should not prevent changing to same Whs", () => SaveToDB(mainConnection, updateSQL));
			}
		}

		public void TestTrigger_WW_Whs_Update_UnrelatedField()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(mainConnection);
				var row = new WhsRow(whs, "ROW1").AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Row must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsRow WHERE WR_PK = '{row.PK}'"));

				var updateSQL = new SqlQueryBuilder(WhsRow
					.UpdateWhere(row.PK)
					.Set(r => r.WR_Name, "CHANGED")
					.Set(r => r.WR_Columns, (short)5)
					.Set(r => r.WR_Levels, (short)3)
					.Set(r => r.WR_Trays, (short)10)
					.Set(r => r.WR_PickPathSequence, (short)4).AsSQL());
				AssertNoExceptionThrown("Trigger should allow changes to any field different from WR_WW_Whs", () => SaveToDB(mainConnection, updateSQL));
				WhsRow.AssertFromDB(mainConnection, row.PK)
					.ExpectEquals("WR_Columns: ", r => r.WR_Columns, 5)
					.ExpectEquals("WR_Levels: ", r => r.WR_Levels, 3)
					.ExpectEquals("WR_Trays: ", r => r.WR_Trays, 10)
					.ExpectEquals("WR_PickPathSequence: ", r => r.WR_PickPathSequence, 4)
					.VerifyAll();
			}
		}

		#endregion

		#region TestTrigger_WW_Whs_Delete

		public void TestTrigger_WW_Whs_Delete()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(mainConnection);
				var row = new WhsRow(whs, "ROW1").AppendInsertAndReturnObject(sql);

				SaveToDB(mainConnection, sql);
				AssertEquals("Row must exist in DB", 1, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsRow WHERE WR_PK = '{row.PK}'"));

				var updateSQL = new SqlQueryBuilder($"DELETE FROM dbo.WhsRow WHERE WR_PK = '{row.PK}'");
				AssertNoExceptionThrown("Trigger should allow deletion of WhsRow", () => SaveToDB(mainConnection, updateSQL));
				AssertEquals("Row must *not* exist in DB", 0, mainConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsRow WHERE WR_PK = '{row.PK}'"));
			}
		}

		#endregion

		#region TestAssertions

		void AssertTriggerPreventsWarehouseChange(DbConnection connection, StringBuilder actionSql)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent change of Warehouse in Row",
				typeof(SqlException),
				"Attempt to change the Warehouse for a Row that is already saved.",
				() => connection.ExecuteNonQuery(actionSql.ToString()),
				assertStartsWith: true
			);
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

