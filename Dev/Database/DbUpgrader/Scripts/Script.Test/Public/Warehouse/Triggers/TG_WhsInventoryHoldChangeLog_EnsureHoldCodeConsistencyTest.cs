using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Warehouse.Triggers
{
	[TestedType(typeof(TG_WhsInventoryHoldChangeLog_EnsureHoldCodeConsistency))]
	class TG_WhsInventoryHoldChangeLog_EnsureHoldCodeConsistencyTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsInventoryHoldChangeLog_EnsureHoldCodeConsistencyTest : TransactionedTestCase
	{
		public void TestMismatchedInventoryHoldCode()
		{
			var receiveLine = GetParentDocketLine();

			var sql = new SqlQueryBuilder();
			var whsHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine)
			{
				WHL_WHC_NKCode = "LCC",
				WHL_Reason = receiveLine.WE_CurrentHoldReason,
				WHL_LogVersion = 1,
			}.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
				"Expected Trigger to detect incorrect HoldCode",
				typeof(SqlException),
				"WhsInventoryHoldChangeLog should have matching hold code and reason to its parent DocketLine",
				() => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()),
				assertStartsWith: true);
		}

		public void TestMismatchedHoldReason()
		{
			var receiveLine = GetParentDocketLine();

			var sql = new SqlQueryBuilder();
			var whsHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine)
			{
				WHL_Reason = "Wrong Reason",
				WHL_WHC_NKCode = receiveLine.WE_WHC_NKCurrentInventoryHeldCode,
				WHL_LogVersion = 1
			}.AppendInsertAndReturnObject(sql);

			AssertExceptionThrown(
				"Expected Trigger to detect incorrect Hold Reason",
				typeof(SqlException),
				"WhsInventoryHoldChangeLog should have matching hold code and reason to its parent DocketLine",
				() => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()),
				assertStartsWith: true);
		}

		public void TestCorrectlyMatchedHoldCodeAndReason()
		{
			var receiveLine = GetParentDocketLine();

			var sql = new SqlQueryBuilder();
			var whsHoldChangeLog = new WhsInventoryHoldChangeLog(receiveLine)
			{
				WHL_LogVersion = 1,
				WHL_WHC_NKCode = receiveLine.WE_WHC_NKCurrentInventoryHeldCode,
				WHL_Reason = receiveLine.WE_CurrentHoldReason,
			}.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Expected no error to be thrown when values are consistent", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		WhsDocketLine GetParentDocketLine()
		{
			var sql = new SqlQueryBuilder();
			var today = new DateTime(2023, 2, 1);
			var client = new OrgHeader("NASA").AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Thingamabob").AppendInsertAndReturnObject(sql);

			var whs = new WhsWarehouse("BUN", "PRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA51").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 21m, location.PK)
			{
				WE_StockOnHand = 21m,
				WE_OriginalInventoryStatus = "AVL",
				WE_WHC_NKOriginalInventoryHeldCode = "",
				WE_CurrentInventoryStatus = "HEL",
				WE_WHC_NKCurrentInventoryHeldCode = "DAM",
				WE_CurrentHoldReason = "Completely borked",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			return receiveLine;
		}
	}
}
