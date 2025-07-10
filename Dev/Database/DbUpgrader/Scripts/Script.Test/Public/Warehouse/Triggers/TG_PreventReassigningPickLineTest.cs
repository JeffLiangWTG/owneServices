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
	[TestedType(typeof(TG_PreventReassigningPickLine))]
	class TG_PreventReassigningPickLineTest : DBCreateTriggerScriptTest
	{
		const string expectedExceptionMessage = "Attempt to re-assigning pick line that is being picked.";

		#region TestTrigger_Reassign 

		public void TestTrigger_Reassign_1()
		{
			TriggerReassignCore(fromIsPicking: false, fromUser: "", toIsPicking: false, toUser: "USR", expectError: false);
		}

		public void TestTrigger_Reassign_2()
		{
			TriggerReassignCore(fromIsPicking: false, fromUser: "A1", toIsPicking: false, toUser: "A2", expectError: false);
		}

		public void TestTrigger_Reassign_3()
		{
			TriggerReassignCore(fromIsPicking: true, fromUser: "B1", toIsPicking: false, toUser: "", expectError: false);
		}

		public void TestTrigger_Reassign_4()
		{
			TriggerReassignCore(fromIsPicking: true, fromUser: "B1", toIsPicking: false, toUser: "B2", expectError: true);
		}

		public void TestTrigger_Reassign_5()
		{
			TriggerReassignCore(fromIsPicking: false, fromUser: "C1", toIsPicking: true, toUser: "C2", expectError: false);
		}

		public void TestTrigger_Reassign_6()
		{
			TriggerReassignCore(fromIsPicking: true, fromUser: "D1", toIsPicking: true, toUser: "D2", expectError: true);
		}

		#endregion

		#region TriggerReassignCore

		void TriggerReassignCore(bool fromIsPicking, string fromUser, bool toIsPicking, string toUser, bool expectError)
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

			// defer docket line trigger so pick line is also in DB at the time trigger is run
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			WhsPickLine
				.UpdateWhere(pickLine.PK)
				.Set(l => l.WZ_IsPicking, fromIsPicking)
				.Set(l => l.WZ_GS_NKAssignedTo, fromUser).Post(TestConnection);

			var updatePickLineSql = WhsPickLine
				.UpdateWhere(pickLine.PK)
				.Set(l => l.WZ_IsPicking, toIsPicking)
				.Set(l => l.WZ_GS_NKAssignedTo, toUser).AsSQL();
			if (expectError)
			{
				AssertExceptionThrown(typeof(SqlException), expectedExceptionMessage, () => TestConnection.ExecuteNonQuery(updatePickLineSql), assertStartsWith: true);
			}
			else
			{
				AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(updatePickLineSql));
			}
		}

		#endregion
	}
}
