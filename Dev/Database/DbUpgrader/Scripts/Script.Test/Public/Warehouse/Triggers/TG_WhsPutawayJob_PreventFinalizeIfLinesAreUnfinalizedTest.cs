using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Warehouse.Triggers
{
	[TestedType(typeof(TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalized))]
	class TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalizedTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalizedTest : TransactionedTestCase
	{
		#region TestTrigger_UnfinalisedPutawayJob

		public void TestTrigger_UnfinalisedPutawayJob_UpdateLineFinalised()
		{
			TestTrigger_UnfinalisedPutawayJob_UpdateLineCore(isFinalisedLine: true);
		}

		public void TestTrigger_UnfinalisedPutawayJob_UpdateLineUnfinalised()
		{
			TestTrigger_UnfinalisedPutawayJob_UpdateLineCore(isFinalisedLine: false);
		}

		void TestTrigger_UnfinalisedPutawayJob_UpdateLineCore(bool isFinalisedLine)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID") { WPL_IsFinalized = isFinalisedLine }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Job must exist in DB", WhsPutawayJob.ExistsInDB(Db.Connection, job.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent setting null WPJ_FinalizedTimeUtc",
				() => WhsPutawayJob.UpdateWhere(job.PK).Set(p => p.WPJ_FinalizedTimeUtc, null).Post(Db.Connection));
		}

		#endregion

		#region TestTrigger_FinalisedPutawayJob

		public void TestTrigger_FinalisedPutawayJob_UpdateLineFinalised()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID") { WPL_IsFinalized = true }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Job must exist in DB", WhsPutawayJob.ExistsInDB(Db.Connection, job.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent setting non-null WPJ_FinalizedTimeUtc",
				() => WhsPutawayJob.UpdateWhere(job.PK).Set(p => p.WPJ_FinalizedTimeUtc, DateTime.Now).Post(Db.Connection));
		}

		public void TestTrigger_FinalisedPutawayJob_UpdateLineUnfinalised()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID") { WPL_IsFinalized = false }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Job must exist in DB", WhsPutawayJob.ExistsInDB(Db.Connection, job.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent change to create unfinalised Putaway Line on finalised Putaway Job",
				typeof(SqlException),
				() => WhsPutawayJob.UpdateWhere(job.PK).Set(p => p.WPJ_FinalizedTimeUtc, DateTime.Now).Post(Db.Connection),
				assertStartsWith: true);
		}

		#endregion
	}
}
