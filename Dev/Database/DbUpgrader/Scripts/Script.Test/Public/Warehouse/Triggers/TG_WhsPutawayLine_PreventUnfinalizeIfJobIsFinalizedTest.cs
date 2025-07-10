using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPutawayLine_PreventUnfinalizeIfJobIsFinalized))]
	class TG_WhsPutawayLine_PreventUnfinalizeIfJobIsFinalizedTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPutawayLine_PreventUnfinalizeIfJobIsFinalizedTest : TransactionedTestCase
	{
		#region TestTrigger_FinalisedPutawayLine

		public void TestTrigger_FinalisedPutawayLine_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID")
			{
				WPL_IsFinalized = true
			}.AppendInsertAndReturnObject(sql);
			AssertNoExceptionThrown(
				"Trigger should not prevent setting WPL_IsFinalized = 1 on any job",
				() => Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		public void TestTrigger_FinalisedPutawayLine_Update()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent setting WPL_IsFinalized = 1 on any job",
				() => WhsPutawayLine.UpdateWhere(line.PK).Set(p => p.WPL_IsFinalized, true).Post(Db.Connection));
		}

		#endregion

		#region TestTrigger_UnfinalisedPutawayLine

		public void TestTrigger_UnfinalisedPutawayLineOnFinalisedJob_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs)
			{
				WPJ_FinalizedTimeUtc = DateTime.UtcNow,
			}.AppendInsertAndReturnObject(sql);

			var line = new WhsPutawayLine(job, "PLTID")
			{
				WPL_IsFinalized = false
			}.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
				"Expected trigger to prevent change to create unfinalised Putaway Line on finalised Putaway Job",
				typeof(SqlException),
				() => Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()),
				assertStartsWith: true);
		}

		public void TestTrigger_UnfinalisedPutawayLineOnFinalisedJob_Update()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs)
			{
				WPJ_FinalizedTimeUtc = DateTime.UtcNow,
			}.AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID")
			{
				WPL_IsFinalized = true
			}.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent change of WPL_IsFinalized on Putaway Line",
				typeof(SqlException),
				() => WhsPutawayLine.UpdateWhere(line.PK).Set(p => p.WPL_IsFinalized, false).Post(Db.Connection),
				assertStartsWith: true);
		}

		public void TestTrigger_UnfinalisedPutawayLineOnUnfinalisedJob_Insert()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID")
			{
				WPL_IsFinalized = false
			}.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(
				"Trigger should not prevent setting WPL_IsFinalized = 0 on unfinalized job",
				() => Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		public void TestTrigger_UnfinalisedPutawayLineOnUnfinalisedJob_Update()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(Db.Connection);
			var job = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line = new WhsPutawayLine(job, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line.PK));

			AssertNoExceptionThrown(
				"Trigger should not prevent setting WPL_IsFinalized = 0 on unfinalized job",
				() => WhsPutawayLine.UpdateWhere(line.PK).Set(p => p.WPL_IsFinalized, false).Post(Db.Connection));
		}

		#endregion
	}
}
