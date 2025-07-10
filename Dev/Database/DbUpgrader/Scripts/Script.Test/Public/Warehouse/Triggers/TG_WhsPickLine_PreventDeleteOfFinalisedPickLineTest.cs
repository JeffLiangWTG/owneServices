using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPickLine_PreventDeleteOfFinalisedPickLine))]
	class TG_WhsPickLine_PreventDeleteOfFinalisedPickLineTest : DBCreateTriggerScriptTest
	{
		public void TestTriggerThrowException_DeleteFinalisedPickLineHaveAttrib1()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, string.Format("P1_{0}", "ORD1"), "PIC").AppendInsertAndReturnObject(sql);
			var pickline = CreateOrderWithPickLine(sql, client, whs, product, location, pick, attrib1: "attrib1");

			SaveDataToDB(sql.ToStringWithNewLineBetweenAppends());

			AssertDeleteDataToDB("Delete WhsPickLine Of Finalised have RCAs is not Allowed.", TriggerExpectedBehavior.ThrowException, pickline, pick);
		}
		public void TestTriggerThrowException_DeleteFinalisedPickLineHaveAttrib2()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, string.Format("P1_{0}", "ORD1"), "PIC").AppendInsertAndReturnObject(sql);
			var pickline = CreateOrderWithPickLine(sql, client, whs, product, location, pick, attrib2: "attrib2");

			SaveDataToDB(sql.ToStringWithNewLineBetweenAppends());

			AssertDeleteDataToDB("Delete WhsPickLine Of Finalised have RCAs is not Allowed.", TriggerExpectedBehavior.ThrowException, pickline, pick);
		}
		public void TestTriggerThrowException_DeleteFinalisedPickLineHaveAttrib3()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, string.Format("P1_{0}", "ORD1"), "PIC").AppendInsertAndReturnObject(sql);
			var pickline = CreateOrderWithPickLine(sql, client, whs, product, location, pick, attrib3: "attrib3");

			SaveDataToDB(sql.ToStringWithNewLineBetweenAppends());

			AssertDeleteDataToDB("Delete WhsPickLine Of Finalised have RCAs is not Allowed.", TriggerExpectedBehavior.ThrowException, pickline, pick);
		}
		public void TestTriggerThrowException_DeleteFinalisedPickLineHaveSerialNumber()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, string.Format("P1_{0}", "ORD1"), "PIC").AppendInsertAndReturnObject(sql);
			var pickline = CreateOrderWithPickLine(sql, client, whs, product, location, pick, serialNumber: "SerialNumber");

			SaveDataToDB(sql.ToStringWithNewLineBetweenAppends());

			AssertDeleteDataToDB("Delete WhsPickLine Of Finalised have RCAs is not Allowed.", TriggerExpectedBehavior.ThrowException, pickline, pick);
		}

		public void TestTriggerAllowDelete_DeleteFinalisedPickLineHaveNoRCAs()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, string.Format("P1_{0}", "ORD1"), "PIC").AppendInsertAndReturnObject(sql);
			var pickline = CreateOrderWithPickLine(sql, client, whs, product, location, pick);

			SaveDataToDB(sql.ToStringWithNewLineBetweenAppends());

			AssertDeleteDataToDB("Delete WhsPickLine Of Finalised have no RCAs is Allowed.", TriggerExpectedBehavior.AllowDelete, pickline, pick);
		}

		void AssertDeleteDataToDB(string errorMessage, TriggerExpectedBehavior expectedBehavior, WhsPickLine pickLine, WhsPick pick)
		{
			var deleteSQL = $@"UPDATE dbo.WhsPick SET WP_FinalizedDateUtc = GETUTCDATE(), WP_GS_NKFinalizedBy = 'A', WP_PickStatus = 'FIN', WP_SystemLastEditTimeUtc = SYSUTCDATETIME(), WP_SystemLastEditUser = '~BP' WHERE WP_PK = '{pick.PK}';"
				+ $@"DELETE FROM dbo.WhsPickLine WHERE WZ_PK = '{pickLine.PK}';"
				+ $@"UPDATE dbo.WhsDocketLine SET WE_StockOnHand = 2.0, WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' WHERE WE_PK = '{pickLine.WZ_WE_InventoryLine}';" ;
			if (expectedBehavior == TriggerExpectedBehavior.AllowDelete)
			{
				AssertNoExceptionThrown(errorMessage, () => SaveDataToDB(deleteSQL));
			}
			else
			{
				AssertExceptionThrown(
					typeof(TestWhsDataSetupHelper.TestSqlStatementFailedException),
					"Error occured in test for attempted SQL Command, Error below:\r\nAttempt to delete WhsPickLine on a FIN Pick.",
					() => SaveDataToDB(deleteSQL),
					true);
			}
		}

		#region TestDataSetupMethods

		WhsDocketLine CreateInventoryLine(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, OrgSupplierPart product, WhsLocation location, decimal units, decimal stockOnHand)
		{
			var today = DateTime.Now;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, units, location.PK) { WE_StockOnHand = stockOnHand }.AppendInsertAndReturnObject(sql);

			return receiveLine;
		}

		WhsDocketLine CreateTransactionLine(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, OrgSupplierPart product, WhsPick pick)
		{
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "ORD1") { WD_WP = pick.PK, WD_FinalisedDate = DateTime.Today, WD_GS_NKFinalizedBy = "T" }.AppendInsertAndReturnObject(sql);
			var orderline = new WhsDocketLine(order, product.PK, 1m) { WE_DocketLineStatus = "DEP", WE_FinalisedDate = DateTime.Today }.AppendInsertAndReturnObject(sql);
			return orderline;
		}

		WhsPickLine CreateOrderWithPickLine(SqlQueryBuilder sql, OrgHeader client, WhsWarehouse whs, OrgSupplierPart product, WhsLocation location, WhsPick pick, string attrib1 = "", string attrib2 = "", string attrib3 = "", string serialNumber = "")
		{
			var receiveline = CreateInventoryLine(sql, client, whs, product, location, 2m, 1m);
			var orderline = CreateTransactionLine(sql, client, whs, product, pick);
			var pickLine = new WhsPickLine(receiveline, orderline, 1m) { WZ_PickedDateTime = DateTime.UtcNow, WZ_ReleaseCapturedPartAttrib1 = attrib1, WZ_ReleaseCapturedPartAttrib2 = attrib2, WZ_ReleaseCapturedPartAttrib3 = attrib3, WZ_ReleaseCapturedSerialNumber = serialNumber, WZ_GS_NKAssignedTo = "R" }.AppendInsertAndReturnObject(sql);
			return pickLine;
		}

		#endregion

		#region Implementation

		static void SaveDataToDB(string sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		enum TriggerExpectedBehavior
		{
			ThrowException,
			AllowDelete
		}

		#endregion
	}
}
