using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPutawayLine_EnsureUniqueUnfinalisedPalletIDPerWarehouse))]
	class TG_WhsPutawayLine_EnsureUniqueUnfinalisedPalletIDPerWarehouseTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPutawayLine_EnsureUniqueUnfinalisedPalletIDPerWarehouseTest : TransactionedTestCase
	{
		public void TestTrigger_UnfinalisedPalletIDExists_InSameWhs()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);

			var job1 = new WhsPutawayJob(whs).AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));

			var insertNewJobSql = new SqlQueryBuilder();
			var job2 = new WhsPutawayJob(whs) { WPJ_GS_NKUser = "RFT" }.AppendInsertAndReturnObject(insertNewJobSql);
			new WhsPutawayLine(job2, "PLTID").AppendInsertAndReturnObject(insertNewJobSql);

			AssertTriggerPreventsNonUniqueUnfinalisedPalletIDs(insertNewJobSql.ToStringWithNewLineBetweenAppends());
		}

		public void TestTrigger_UnfinalisedPalletIDExists_InDifferentWhs()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
			var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(Db.Connection);
			var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(Db.Connection);

			var job1 = new WhsPutawayJob(whs1).AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line1 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));

			var insertNewJobSql = new SqlQueryBuilder();
			var job2 = new WhsPutawayJob(whs2).AppendInsertAndReturnObject(insertNewJobSql);
			var line2 = new WhsPutawayLine(job2, "PLTID").AppendInsertAndReturnObject(insertNewJobSql);

			AssertNoExceptionThrown(
				"Trigger should not prevent saving same pallet ID in different Whs",
				() => Db.Connection.ExecuteNonQuery(insertNewJobSql.ToStringWithNewLineBetweenAppends()));
			Assert("Line2 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line2.PK));
		}

		public void TestTrigger_UnfinalisedPalletIDExists_InDifferentWhs_DifferentUser()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
			var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(Db.Connection);
			var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(Db.Connection);

			var job1 = new WhsPutawayJob(whs1).AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLTID").AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));

			var insertNewJobSql = new SqlQueryBuilder();
			var job2 = new WhsPutawayJob(whs2) { WPJ_GS_NKUser = "RFT" }.AppendInsertAndReturnObject(insertNewJobSql);
			var line2 = new WhsPutawayLine(job2, "PLTID").AppendInsertAndReturnObject(insertNewJobSql);

			AssertNoExceptionThrown(
				"Trigger should not prevent saving same pallet ID in different Whs",
				() => Db.Connection.ExecuteNonQuery(insertNewJobSql.ToStringWithNewLineBetweenAppends()));
			Assert("Line2 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line2.PK));
		}

		public void TestTrigger_FinalisedPalletIDExists_InSameWhs()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(Db.Connection);

			var job1 = new WhsPutawayJob(whs) { WPJ_FinalizedTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLTID") { WPL_IsFinalized = true }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));

			var insertNewJobSql = new SqlQueryBuilder();
			var job2 = new WhsPutawayJob(whs).AppendInsertAndReturnObject(insertNewJobSql);
			var line2 = new WhsPutawayLine(job2, "PLTID").AppendInsertAndReturnObject(insertNewJobSql);

			AssertNoExceptionThrown(
				"Trigger should not prevent saving same finalised pallet ID in same Whs",
				() => Db.Connection.ExecuteNonQuery(insertNewJobSql.ToStringWithNewLineBetweenAppends()));
			Assert("Line2 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line2.PK));
		}

		public void TestTrigger_FinalisedPalletIDExists_InSameWhs_DifferentUser()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(Db.Connection);
			var job1 = new WhsPutawayJob(whs) { WPJ_FinalizedTimeUtc = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLTID") { WPL_IsFinalized = true }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));

			var insertNewJobSql = new SqlQueryBuilder();
			var job2 = new WhsPutawayJob(whs) { WPJ_GS_NKUser = "RFT" }.AppendInsertAndReturnObject(insertNewJobSql);
			var line2 = new WhsPutawayLine(job2, "PLTID").AppendInsertAndReturnObject(insertNewJobSql);

			AssertNoExceptionThrown(
				"Trigger should not prevent saving same finalised pallet ID in same Whs",
				() => Db.Connection.ExecuteNonQuery(insertNewJobSql.ToStringWithNewLineBetweenAppends()));
			Assert("Line2 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line2.PK));
		}

		public void TestTrigger_FinalisedPalletIDExists_InDifferentWhs()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
			var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(Db.Connection);
			var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(Db.Connection);

			var job1 = new WhsPutawayJob(whs1) { WPJ_FinalizedTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLTID") { WPL_IsFinalized = true }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));

			var insertNewJobSql = new SqlQueryBuilder();
			var job2 = new WhsPutawayJob(whs2).AppendInsertAndReturnObject(insertNewJobSql);
			var line2 = new WhsPutawayLine(job2, "PLTID").AppendInsertAndReturnObject(insertNewJobSql);

			AssertNoExceptionThrown(
				"Trigger should not prevent saving same pallet ID in different Whs",
				() => Db.Connection.ExecuteNonQuery(insertNewJobSql.ToStringWithNewLineBetweenAppends()));
			Assert("Line2 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line2.PK));
		}

		public void TestTrigger_FinalisedPalletIDExists_InDifferentWhs_DifferentUser()
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(Db.Connection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(Db.Connection);
			var whs1 = new WhsWarehouse("WH1", "PRW", branch1.PK).WithDockDoor(Db.Connection);
			var whs2 = new WhsWarehouse("WH2", "PRW", branch2.PK).WithDockDoor(Db.Connection);

			var job1 = new WhsPutawayJob(whs1) { WPJ_FinalizedTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);
			var line1 = new WhsPutawayLine(job1, "PLTID") { WPL_IsFinalized = true }.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			Assert("Line must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line1.PK));

			var insertNewJobSql = new SqlQueryBuilder();
			var job2 = new WhsPutawayJob(whs2) { WPJ_GS_NKUser = "RFT" }.AppendInsertAndReturnObject(insertNewJobSql);
			var line2 = new WhsPutawayLine(job2, "PLTID").AppendInsertAndReturnObject(insertNewJobSql);

			AssertNoExceptionThrown(
				"Trigger should not prevent saving same pallet ID in different Whs",
				() => Db.Connection.ExecuteNonQuery(insertNewJobSql.ToStringWithNewLineBetweenAppends()));
			Assert("Line2 must exist in DB", WhsPutawayLine.ExistsInDB(Db.Connection, line2.PK));
		}

		#region TestAssertions

		void AssertTriggerPreventsNonUniqueUnfinalisedPalletIDs(string actionSql)
		{
			AssertExceptionThrown(
				"Expected trigger to prevent deletion of IsPuttingAway == true Putaway Line",
				typeof(SqlException),
				"Unfinalized Putaway Line must have a unique Pallet ID and Putaway Job Warehouse combination.",
				() => Db.Connection.ExecuteNonQuery(actionSql),
				assertStartsWith: true
			);
		}

		#endregion
	}
}

