using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly))]
	class TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnlyTest : DBCreateTriggerScriptTest
	{
		public void Test_TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly_Order()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 2m).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, orderLine, 1m) { WZ_OriginalReservedQty = 1m }.AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = new SqlQueryBuilder();
			updateBuilder.AppendLine(WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot.PK).Set(s => s.WSV_WZ_PickingLine, pickLine.PK).AsSQL());
			new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = true, WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(updateBuilder);

			AssertNoExceptionThrown("Should save without issue.", () => Save(updateBuilder));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivot.PK)
				.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine.PK)
				.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
				.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, pickLine.PK)
				.VerifyAll();
		}

		public void Test_TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly_WorkOrder()
		{
			Test_TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly(docketType: "WOR");
		}

		public void Test_TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly_DynamicWorkOrder()
		{
			Test_TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly(docketType: "DWO");
		}

		void Test_TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly(string docketType)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product1 = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var product2 = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			new OrgPartBOM(product1, product2) { OE_ComponentQty = 1m }.AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product2.PK, 50m, location.PK) { WE_StockOnHand = 50m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(whs, "Pick1", "PIC").AppendInsertAndReturnObject(sql);
			var workOrder = new WhsDocket(client.PK, whs.PK, docketType, "ASS", "PIC", "WO1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var workOrderMainLine = new WhsDocketLine(workOrder, product1.PK, 10m).AppendInsertAndReturnObject(sql);
			var workOrderComponentLine = new WhsDocketLine(workOrder, product2.PK, 10m) { WE_WE_ParentDocketLine = workOrderMainLine }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, workOrderComponentLine, 10m).AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product2, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = new SqlQueryBuilder();
			updateBuilder.AppendLine(WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot.PK).Set(s => s.WSV_WZ_PickingLine, pickLine.PK).AsSQL());
			new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = true, WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(updateBuilder);

			NUnit.Framework.Assert.That(() => Save(updateBuilder), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "Only for Order we can have release capture serial number.");
		}

		public void Test_TG_WhsSerialNumberPivot_EnsureReleaseIsCapturedForOrderOnly_Transfer()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TFR1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 10m, locationA2.PK) { WE_DocketLineStatus = "ENT", WE_WL_TransferFrom = locationA1.PK }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine, 10m).AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = new SqlQueryBuilder();
			updateBuilder.AppendLine(WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot.PK).Set(s => s.WSV_WZ_PickingLine, pickLine.PK).AsSQL());
			new WhsSerialNumberPivot(pickLine.PK, WhsPickLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_IsReleaseCaptured = true, WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(updateBuilder);

			NUnit.Framework.Assert.That(() => Save(updateBuilder), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "Only for Order we can have release capture serial number.");
		}

		#region Helper

		void Save(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		string TriggerErrorMessage => "Attempt to assign Release Capture Serial Number to a non-order.";

		#endregion
	}
}
