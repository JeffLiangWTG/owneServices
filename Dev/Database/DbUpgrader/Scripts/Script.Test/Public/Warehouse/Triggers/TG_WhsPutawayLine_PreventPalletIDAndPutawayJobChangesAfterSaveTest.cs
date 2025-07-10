using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPutawayLine_PreventPalletIDAndPutawayJobChangesAfterSave))]
	class TG_WhsPutawayLine_PreventPalletIDAndPutawayJobChangesAfterSaveTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPutawayLine_PreventPalletIDAndPutawayJobChangesAfterSaveTest : TransactionedTestCase
	{
		#region TestTrigger_WPL_PalletID_Update

		public void TestTrigger_WPL_PalletID_UpdateAfterSaved()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertTriggerPreventsChanges(
				() => WhsPutawayLine.UpdateWhere(line.PK)
							.Set(p => p.WPL_PalletID, "Changed")
							.Post(Db.Connection));
		}

		public void TestTrigger_WPL_PalletID_UpdateToPalletID()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent changing to same putaway job",
				() => WhsPutawayLine.UpdateWhere(line.PK)
							.Set(p => p.WPL_PalletID, "PLTID")
							.Post(Db.Connection));

			WhsPutawayLine.AssertFromDB(Db.Connection, line.PK)
				.ExpectEquals("PalletID should not change", l => l.WPL_PalletID, "PLTID")
				.VerifyAll();
		}

		public void TestTrigger_WPL_PalletID_Update_UnrelatedField()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should allow changes to any field different from WPL_PalletID",
				() => WhsPutawayLine.UpdateWhere(line.PK)
							.Set(p => p.WPL_IsPuttingAway, true)
							.Post(Db.Connection));

			WhsPutawayLine.AssertFromDB(Db.Connection, line.PK)
				.ExpectEquals("WPL_IsPuttingAway: ", r => r.WPL_IsPuttingAway, true)
				.VerifyAll();
		}

		#endregion

		#region TestTrigger_WPL_PalletID_Delete

		public void TestTrigger_WPL_PalletID_Delete()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should allow deletion of WhsPutawayLine",
				() => WhsPutawayLine.DeleteInDB(Db.Connection, line.PK));
			AssertEquals("Line must exist in DB", false, WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));
		}

		#endregion

		#region TestTrigger_WPL_WPJ_PutawayJob_Update

		public void TestTrigger_WPL_WPJ_PutawayJob_UpdateAfterSaved()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job1 = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var job2 = new WhsPutawayJob(whs) { WPJ_GS_NKUser = "U2" }.AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job1, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertTriggerPreventsChanges(
				() => WhsPutawayLine.UpdateWhere(line.PK)
							.Set(p => p.WPL_WPJ_PutawayJob, job2)
							.Post(Db.Connection));
		}

		public void TestTrigger_WPL_WPJ_PutawayJob_UpdateToSameJob()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job1 = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var job2 = new WhsPutawayJob(whs) { WPJ_GS_NKUser = "U2" }.AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job1, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent changing to same putaway job",
				() => WhsPutawayLine.UpdateWhere(line.PK)
							.Set(p => p.WPL_WPJ_PutawayJob, job1)
							.Post(Db.Connection));
		}

		public void TestTrigger_WPL_WPJ_PutawayJob_Update_UnrelatedField()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should allow changes to any field different from WPL_WPJ_PutawayJob",
				() => WhsPutawayLine.UpdateWhere(line.PK)
							.Set(p => p.WPL_IsPuttingAway, true)
							.Post(Db.Connection));

			WhsPutawayLine.AssertFromDB(Db.Connection, line.PK)
				.ExpectEquals("WPL_IsPuttingAway: ", r => r.WPL_IsPuttingAway, true)
				.VerifyAll();
		}

		#endregion

		#region TestTrigger_WPL_WPJ_PutawayJob_Delete

		public void TestTrigger_WPL_WPJ_PutawayJob_Delete()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should allow deletion of WhsPutawayLine",
				() => WhsPutawayLine.DeleteInDB(Db.Connection, line.PK));
			AssertEquals("Line must exist in DB", false, WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));
		}

		#endregion

		#region TestAssertions

		void AssertTriggerPreventsChanges(AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent change of Putaway Job or Pallet ID on Putaway Line",
				typeof(SqlException),
				"Attempt to change the Putaway Job or Pallet ID for a Putaway Line that is already saved.",
				codeToRun,
				assertStartsWith: true
			);
		}

		#endregion
	}
}

