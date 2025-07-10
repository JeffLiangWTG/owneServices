using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPutawayLine_PreventDeletionIfIsPuttingAway))]
	class TG_WhsPutawayLine_PreventDeletionIfIsPuttingAwayTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPutawayLine_PreventDeletionIfIsPuttingAwayTest : TransactionedTestCase
	{
		public void TestTriggerDeleteLine_WPL_IsPuttingAway_False()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent deleting a IsPuttingAway == false line",
				() => WhsPutawayLine.DeleteInDB(Db.Connection, line.PK));
		}

		public void TestTriggerDeleteLine_WPL_IsPuttingAway_True()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID") { WPL_IsPuttingAway = true }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertTriggerPreventsPutawayLineDeletion(() => WhsPutawayLine.DeleteInDB(Db.Connection, line.PK));
		}

		public void TestTriggerDeleteLine_DifferentIsPuttingAwayValuesOnSameJob()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job1 = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLT1") { WPL_IsPuttingAway = true }.AppendInsertAndReturnObject(sql);
			var line2 = new WhsPutawayLine(job1, "PLT2").AppendInsertAndReturnObject(sql);

			var job2 = new WhsPutawayJob(whs) { WPJ_GS_NKUser = "U2" }.AppendInsertAndReturnObject(sql);
			var line3 = new WhsPutawayLine(job2, "PLT3").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Precondition: Line1 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));
			Assert("Precondition: Line2 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line2.PK));
			Assert("Precondition: Line3 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line3.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent deleting a IsPuttingAway == false line",
				() => WhsPutawayLine.DeleteInDB(Db.Connection, line3.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent deleting a IsPuttingAway == false line",
				() => WhsPutawayLine.DeleteInDB(Db.Connection, line2.PK));

			AssertEquals("1 Line must exist in DB", 1, WhsPutawayLine.CountInDB(Db.Connection));

			AssertTriggerPreventsPutawayLineDeletion(() => WhsPutawayLine.DeleteInDB(Db.Connection, line1.PK));
		}

		#region TestAssertions

		void AssertTriggerPreventsPutawayLineDeletion(AnonymousMethod codeToRun)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent deletion of IsPuttingAway == true Putaway Line",
				typeof(SqlException),
				"Attempt to delete a Putting Away WhsPutawayLine.",
				codeToRun,
				assertStartsWith: true
			);
		}

		#endregion
	}
}

