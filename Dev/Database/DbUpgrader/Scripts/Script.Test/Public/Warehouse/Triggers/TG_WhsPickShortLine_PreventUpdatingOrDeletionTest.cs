using System;
using System.Linq.Expressions;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPickShortLine_PreventUpdatingOrDeletion))]
	class TG_WhsPickShortLine_PreventUpdatingOrDeletionTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_WhsPickShortLine_PreventUpdatingOrDeletionTest : TransactionedTestCase
	{
		public void TestTriggerPreventsDeletingLine()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK, ddlLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 42m
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P00000002", "PIC") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "W00000002") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, orderLine, 8m) { WZ_PickedDateTime = DateTime.Now, WZ_GS_NKAssignedTo = "~BP" }.AppendInsertAndReturnObject(sql);

			var pickShortLine = new WhsPickShortLine(receiveLine, orderLine, 2m).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			Assert("Line must exist in DB", WhsPickShortLine.ExistsInDB(TestConnection, pickShortLine.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent deletion.",
				typeof(SqlException),
				"Pick Short Lines cannot be deleted.",
				() => WhsPickShortLine.DeleteInDB(TestConnection, pickShortLine.PK),
				assertStartsWith: true
			);
		}

		public void TestTriggerPreventsUpdatingLine_WZS_ShortUnits()
		{
			TestTriggerPreventsUpdatingLine_Core(p => p.WZS_ShortUnits, 3m);
		}

		public void TestTriggerPreventsUpdatingLine_WZS_GS_NKShortedBy()
		{
			TestTriggerPreventsUpdatingLine_Core(p => p.WZS_GS_NKShortedBy, "BRS");
		}

		public void TestTriggerPreventsUpdatingLine_WZS_ShortedDateTimeUtc()
		{
			TestTriggerPreventsUpdatingLine_Core(p => p.WZS_ShortedDateTimeUtc, DateTime.Today.AddHours(-1));
		}

		void TestTriggerPreventsUpdatingLine_Core<TSet>(Expression<Func<WhsPickShortLine, TSet>> valueSetter,	TSet valueToSet)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK, ddlLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 42m
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P00000002", "PIC") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "W00000002") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, orderLine, 8m) { WZ_PickedDateTime = DateTime.Now, WZ_GS_NKAssignedTo = "~BP" }.AppendInsertAndReturnObject(sql);

			var pickShortLine = new WhsPickShortLine(receiveLine, orderLine, 2m).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			Assert("Line must exist in DB", WhsPickShortLine.ExistsInDB(TestConnection, pickShortLine.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent updating.",
				typeof(SqlException),
				"Pick Short Lines cannot be updated.",
				() => WhsPickShortLine
						.UpdateWhere(pickShortLine.PK)
						.Set(valueSetter, valueToSet)
						.Post(TestConnection),
				assertStartsWith: true
			);
		}

		public void TestTriggerPreventsUpdatingLine_WZS_WE_InventoryLine()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK, ddlLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 42m
			}.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 50m
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P00000002", "PIC") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "W00000002") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine1, orderLine, 8m) { WZ_PickedDateTime = DateTime.Now, WZ_GS_NKAssignedTo = "~BP" }.AppendInsertAndReturnObject(sql);

			var pickShortLine = new WhsPickShortLine(receiveLine1, orderLine, 2m).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			Assert("Line must exist in DB", WhsPickShortLine.ExistsInDB(TestConnection, pickShortLine.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent updating.",
				typeof(SqlException),
				"Pick Short Lines cannot be updated.",
				() => WhsPickShortLine
						.UpdateWhere(pickShortLine.PK)
						.Set(p => p.WZS_WE_InventoryLine.FK, receiveLine2.PK)
						.Post(TestConnection),
				assertStartsWith: true
			);
		}

		public void TestTriggerPreventsUpdatingLine_WZS_WE_TransactionLine()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];

			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(sql);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK, ddlLocationType.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = DateTime.Now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 50m, locationA1.PK)
			{
				WE_StockOnHand = 42m
			}.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "P00000002", "PIC") { WP_WL_DockDoor = locationA2 }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "W00000002") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine, orderLine1, 8m) { WZ_PickedDateTime = DateTime.Now, WZ_GS_NKAssignedTo = "~BP" }.AppendInsertAndReturnObject(sql);

			var pickShortLine = new WhsPickShortLine(receiveLine, orderLine1, 2m).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			Assert("Line must exist in DB", WhsPickShortLine.ExistsInDB(TestConnection, pickShortLine.PK));

			AssertExceptionThrown(
				"Expected trigger to prevent updating.",
				typeof(SqlException),
				"Pick Short Lines cannot be updated.",
				() => WhsPickShortLine
						.UpdateWhere(pickShortLine.PK)
						.Set(p => p.WZS_WE_TransactionLine.FK, orderLine2.PK)
						.Post(TestConnection),
				assertStartsWith: true
			);
		}
	}
}

