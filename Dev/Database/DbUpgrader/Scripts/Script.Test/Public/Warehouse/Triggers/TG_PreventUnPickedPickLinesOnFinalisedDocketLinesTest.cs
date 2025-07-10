using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventUnPickedPickLinesOnFinalisedDocketLines))]
	class TG_PreventUnPickedPickLinesOnFinalisedDocketLinesTest : DBCreateTriggerScriptTest
	{
		#region TestPreventUnPickedPickLinesOnFinalisedTransferLine

		public void TestPreventUnPickedPickLinesOnFinalisedTransferLine()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateNewTestData(sql);
			var finalisedTime = DateTime.Now;

			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = finalisedTime, WD_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Part.PK, 100m, data.Location.PK) { WE_DocketLineStatus = "FIN", WE_AdjustmentArrivalDate = finalisedTime, WE_FinalisedDate = finalisedTime, WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(data.Client.PK, data.Whs.PK, "TFR", "TFR", "ENT", "TFR1").AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsDocketLine(transfer, data.Part.PK, 10m, data.Location.PK) { WE_DocketLineStatus = "ENT", WE_WL_TransferFrom = data.Location.PK }.AppendInsertAndReturnObject(sql);
			var transferLine2 = new WhsDocketLine(transfer, data.Part.PK, 10m, data.Location.PK) { WE_DocketLineStatus = "ENT", WE_WL_TransferFrom = data.Location.PK }.AppendInsertAndReturnObject(sql);
			var pickedPickLine = new WhsPickLine(receiveLine, transferLine1, 10m) { WZ_GS_NKAssignedTo = "AA", WZ_PickedDateTime = finalisedTime }.AppendInsertAndReturnObject(sql);
			var unpickedPickLine = new WhsPickLine(receiveLine, transferLine2, 10m).AppendInsertAndReturnObject(sql);
			SaveData(sql);

			AssertNoExceptionThrown("Finalise a docketline with picked pickline should not throw exception.",
				() => WhsDocketLine
				.UpdateWhere(transferLine1.PK)
				.Set(l => l.WE_DocketLineStatus, "FIN")
				.Set(l => l.WE_FinalisedDate, finalisedTime)
				.Set(l => l.WE_PutawayTime, finalisedTime)
				.Set(l => l.WE_GS_NKPutawayBy, "E")
				.Set(l => l.WE_StockOnHand, 10m).Post(TestConnection));

			AssertExceptionThrown(typeof(SqlException), "Attempt to save a Finalised Docket Line with an unpicked pickline.",
				() => WhsDocketLine
				.UpdateWhere(transferLine2.PK)
				.Set(l => l.WE_DocketLineStatus, "FIN")
				.Set(l => l.WE_FinalisedDate, finalisedTime)
				.Set(l => l.WE_PutawayTime, finalisedTime)
				.Set(l => l.WE_GS_NKPutawayBy, "E")
				.Set(l => l.WE_StockOnHand, 10m).Post(TestConnection), assertStartsWith: true);
		}

		#endregion

		#region TestPreventUnPickedPickLinesOnFinalisedAdjustmentLine

		public void TestPreventUnPickedPickLinesOnFinalisedAdjustmentLine()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateNewTestData(sql);
			var finalisedTime = DateTime.Now;
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = finalisedTime, WD_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Part.PK, 100m, data.Location.PK) { WE_DocketLineStatus = "FIN", WE_AdjustmentArrivalDate = finalisedTime, WE_FinalisedDate = finalisedTime, WE_StockOnHand = 100m }.AppendInsertAndReturnObject(sql);

			var adjustment = new WhsDocket(data.Client.PK, data.Whs.PK, "ADJ", "NEA", "NEW", "AD1").AppendInsertAndReturnObject(sql);
			var adjustmentLine = new WhsDocketLine(adjustment, data.Part.PK, -10m, data.Location.PK).AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, adjustmentLine, 10m).AppendInsertAndReturnObject(sql);
			SaveData(sql);

			DisablePreventSaveOfFinalisedDocketWithUnfinalisedLinesTrigger();

			WhsDocket
				.UpdateWhere(adjustment.PK)
				.Set(d => d.WD_DocketStatus, "FIN")
				.Set(d => d.WD_FinalisedDate, finalisedTime)
				.Set(d => d.WD_GS_NKFinalizedBy, "A").Post(TestConnection);

			AssertExceptionThrown(typeof(SqlException), "Attempt to save a Finalised Docket Line with an unpicked pickline.",
				() => WhsDocketLine
				.UpdateWhere(adjustmentLine.PK)
				.Set(l => l.WE_DocketLineStatus, "FIN")
				.Set(l => l.WE_FinalisedDate, finalisedTime).Post(TestConnection), assertStartsWith: true);
		}

		#endregion

		#region TestPreventUnPickedPickLinesOnFinalisedOrderLines_NoException

		public void TestPreventUnPickedPickLinesOnFinalisedOrderLines_NoException()
		{
			var sql = new SqlQueryBuilder();
			var data = CreateNewTestData(sql);
			var finalisedTime = DateTime.Now;
			var receive = new WhsDocket(data.Client.PK, data.Whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = finalisedTime, WD_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, data.Part.PK, 100m, data.Location.PK) { WE_DocketLineStatus = "FIN", WE_AdjustmentArrivalDate = finalisedTime, WE_FinalisedDate = finalisedTime, WE_StockOnHand = 100m }.AppendInsertAndReturnObject(sql);

			var pick = new WhsPick(data.Whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(data.Client.PK, data.Whs.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, data.Part.PK, 10m).AppendInsertAndReturnObject(sql);
			var unpickedPickline = new WhsPickLine(receiveLine, orderLine, 10m).AppendInsertAndReturnObject(sql);
			SaveData(sql);

			DisablePreventSaveOfFinalisedDocketWithUnfinalisedLinesTrigger();

			WhsDocket
				.UpdateWhere(order.PK)
				.Set(d => d.WD_DocketStatus, "PIC")
				.Set(d => d.WD_FinalisedDate, finalisedTime)
				.Set(d => d.WD_GS_NKFinalizedBy, "A").Post(TestConnection);

			AssertNoExceptionThrown("Finalise a order line with unpicked pickline should not throw exception.",
				() => WhsDocketLine
				.UpdateWhere(orderLine.PK)
				.Set(l => l.WE_DocketLineStatus, "FIN")
				.Set(l => l.WE_FinalisedDate, finalisedTime).Post(TestConnection));
		}

		#endregion

		#region Implementations

		void SaveData(SqlQueryBuilder sql)
		{
			// defer triggers to run in the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		TestData CreateNewTestData(SqlQueryBuilder sql)
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var part = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			return new TestData(whs, client, part, locationA1);
		}

		void DisablePreventSaveOfFinalisedDocketWithUnfinalisedLinesTrigger()
		{
			TestConnection.ExecuteNonQuery(@"
			IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_PreventMismatchOnDocketStatusAndDateWithLines')
			ALTER TABLE dbo.WhsDocket DISABLE TRIGGER TG_PreventMismatchOnDocketStatusAndDateWithLines");
		}

		class TestData
		{
			internal TestData(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part, WhsLocation location)
			{
				Whs = whs;
				Client = client;
				Part = part;
				Location = location;
			}

			public readonly WhsWarehouse Whs;
			public readonly OrgHeader Client;
			public readonly OrgSupplierPart Part;
			public readonly WhsLocation Location;
		}
		#endregion
	}
}
