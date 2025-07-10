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
	[TestedType(typeof(TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised))]
	class TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalisedTest : DBCreateTriggerScriptTest
	{
		#region DocketLine

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised_DocketLine_Delete()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_StockOnHand = 1m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var pivot = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveWithDisableTrigger(sql));

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine.PK)
				.ExpectEquals("WE_DocketLineStatus", i => i.WE_DocketLineStatus, "FIN")
				.VerifyAll("Precondition");

			AssertExceptionThrown("Trigger should prevent delete.", typeof(SqlException),
				TriggerErrorMessage, () => WhsSerialNumberPivot.DeleteInDB(TestConnection, pivot.PK), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised_DocketLine_NotFinalised()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_ClientOrderedUnits = 2m, WE_StockOnHand = 2m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var pivot1 = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var pivot2 = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			AssertEquals("Precondition", 2, WhsSerialNumberPivot.CountInDB(TestConnection));
			WhsDocketLine.AssertFromDB(TestConnection, receiveLine.PK)
				.ExpectNotEquals("WE_DocketLineStatus", i => i.WE_DocketLineStatus, "FIN")
				.VerifyAll("Precondition");

			AssertNoExceptionThrown("Should delete without issue.", () => WhsSerialNumberPivot.DeleteInDB(TestConnection, pivot2.PK));
			AssertEquals("Should delete in DB.", 1, WhsSerialNumberPivot.CountInDB(TestConnection));
		}

		#endregion

		#region AsnLine

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised_AsnLine_Delete()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, location.PK) { WE_StockOnHand = 1m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);
			var pivot = new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveWithDisableTrigger(sql));

			WhsDocket.AssertFromDB(TestConnection, receive.PK)
				.ExpectEquals("WD_DocketStatus", i => i.WD_DocketStatus, "FIN")
				.VerifyAll("Precondition");

			AssertExceptionThrown("Trigger should prevent delete.", typeof(SqlException),
				TriggerErrorMessage, () => WhsSerialNumberPivot.DeleteInDB(TestConnection, pivot.PK), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised_AsnLine_NotFinalised()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_ClientOrderedUnits = 2m, WE_StockOnHand = 2m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var asnLine = new WhsAsnLine(receive, product.PK, 1m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			var pivot2 = new WhsSerialNumberPivot(asnLine.PK, WhsAsnLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveToDB(sql));

			AssertEquals("Precondition", 2, WhsSerialNumberPivot.CountInDB(TestConnection));
			WhsDocketLine.AssertFromDB(TestConnection, receiveLine.PK)
				.ExpectNotEquals("WE_DocketLineStatus", i => i.WE_DocketLineStatus, "FIN")
				.VerifyAll("Precondition");

			AssertNoExceptionThrown("Should delete without issue.", () => WhsSerialNumberPivot.DeleteInDB(TestConnection, pivot2.PK));
			AssertEquals("Should delete in DB.", 1, WhsSerialNumberPivot.CountInDB(TestConnection));
		}

		#endregion

		#region PickLine

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised_PickLine_Delete()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_StockOnHand = 0m, WE_DocketLineStatus = "FIN", WE_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, "Pick1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick.PK, WD_FinalisedDate = DateTime.Today, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m) { WE_DocketLineStatus = "DEP", WE_FinalisedDate = DateTime.Today, WE_AdjustmentArrivalDate = null }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 2m) { WZ_PickedDateTime = DateTime.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber1.PK) { WSV_IsReleaseCaptured = true }.AppendInsertAndReturnObject(sql);
			var pivot2 = new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber2.PK) { WSV_IsReleaseCaptured = true }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveWithDisableTrigger(sql));
			AssertEquals("Precondition", 2, WhsSerialNumberPivot.CountInDB(TestConnection));

			WhsPickLine.AssertFromDB(TestConnection, pickLine.PK)
				.ExpectEquals("WZ_PickedDateTime", i => i.WZ_PickedDateTime, DateTime.Today)
				.VerifyAll("Precondition");

			AssertExceptionThrown("Trigger should prevent delete.", typeof(SqlException),
				TriggerErrorMessage, () => WhsSerialNumberPivot.DeleteInDB(TestConnection, pivot2.PK), assertStartsWith: true);
		}

		public void Test_TG_WhsSerialNumberPivot_PreventDeleteWhenJobFinalised_PickLine_NotPicked()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "ENT", "R1").AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location.PK) { WE_ClientOrderedUnits = 2m, WE_StockOnHand = 2m, WE_OriginalInventoryStatus = "REC", WE_CurrentInventoryStatus = "REC", WE_UnloadedTime = DateTimeOffset.Now, WE_GS_NKUnloadedBy = "A" }.AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, "Pick1", "FIN") { WP_FinalizedDateUtc = DateTime.UtcNow, WP_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 2m) { WZ_OriginalReservedQty = 2m }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber1.PK) { WSV_IsReleaseCaptured = true }.AppendInsertAndReturnObject(sql);
			var pivot2 = new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber2.PK) { WSV_IsReleaseCaptured = true }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => SaveWithDisableTrigger(sql));

			WhsPickLine.AssertFromDB(TestConnection, pickLine.PK)
				.ExpectEquals("WZ_PickedDateTime", i => i.WZ_PickedDateTime, null)
				.VerifyAll("Precondition");

			AssertEquals("Precondition", 2, WhsSerialNumberPivot.CountInDB(TestConnection));
			WhsPickLine.AssertFromDB(TestConnection, pickLine.PK)
				.ExpectEquals("WZ_PickedDateTime", i => i.WZ_PickedDateTime, null)
				.VerifyAll("Precondition");

			AssertNoExceptionThrown("Should delete without issue.", () => WhsSerialNumberPivot.DeleteInDB(TestConnection, pivot2.PK));
			AssertEquals("Should delete in DB.", 1, WhsSerialNumberPivot.CountInDB(TestConnection));
		}

		#endregion

		#region Helper

		void SaveWithDisableTrigger(SqlQueryBuilder sqlQueryBuilder)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_WE_TransactionLine, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect))
			{
				TestConnection.ExecuteNonQuery(sqlQueryBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		void SaveToDB(SqlQueryBuilder sql) => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

		string TriggerErrorMessage => "Attempt to delete Serial Number for finalised job.";

		#endregion
	}
}
