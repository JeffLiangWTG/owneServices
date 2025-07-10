using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventUnPickedPickLinesOnFinalisedPicks))]
	class TG_PreventUnPickedPickLinesOnFinalisedPicksTest : DBCreateTriggerScriptTest
	{
		#region TestPreventUnPickedPickLinesOnFinalisedPicks

		public void TestPreventUnPickedPickLinesOnFinalisedPicks()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);
			var now = DateTime.Now.ToSmallDateTimeFloor();

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = now }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 100m, locationA1.PK) { WE_StockOnHand = 90m }.AppendInsertAndReturnObject(sql);

			var pick1 = new WhsPick(whs, "P1", "PIC").AppendInsertAndReturnObject(sql);
			var pick2 = new WhsPick(whs, "P2", "PIC").AppendInsertAndReturnObject(sql);
			var pick3 = new WhsPick(whs, "P3", "PIC").AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O1") { WD_WP = pick1.PK, WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);
			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "O3") { WD_WP = pick3.PK, WD_FinalisedDate = now, WD_GS_NKFinalizedBy = "E" }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order2, product.PK, 10m).AppendInsertAndReturnObject(sql);
			var orderLine3 = new WhsDocketLine(order3, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var pickLineWithPickedTime = new WhsPickLine(receiveLine, orderLine1, 10m) { WZ_PickedDateTime = now, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			var pickLineWithoutPickedTime1 = new WhsPickLine(receiveLine, orderLine2, 10m).AppendInsertAndReturnObject(sql);
			var pickLineWithoutPickedTime2 = new WhsPickLine(receiveLine, orderLine3, 10m).AppendInsertAndReturnObject(sql);

			// defer triggers to run in the end
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
			AssertNoExceptionThrown("Change a Pick status to FIN with picked pickline should not thorw exception.",
				() => WhsPick
				.UpdateWhere(pick1.PK)
				.Set(p => p.WP_PickStatus, "FIN")
				.Set(p => p.WP_GS_NKFinalizedBy, "A")
				.Set(p => p.WP_FinalizedDateUtc, DateTime.UtcNow).Post(TestConnection));
			AssertNoExceptionThrown("Change a Pick status to CAN with unpicked pickline should not throw exception.",
				() => WhsPick
				.UpdateWhere(pick2.PK)
				.Set(p => p.WP_PickStatus, "CAN").Post(TestConnection));
			AssertExceptionThrown(typeof(SqlException), "Attempt to save a Finalised Pick which has unpicked pickedline.",
				() => WhsPick
				.UpdateWhere(pick3.PK)
				.Set(p => p.WP_PickStatus, "FIN")
				.Set(p => p.WP_GS_NKFinalizedBy, "A")
				.Set(p => p.WP_FinalizedDateUtc, DateTime.UtcNow).Post(TestConnection), assertStartsWith: true);
		}

		#endregion
	}
}

