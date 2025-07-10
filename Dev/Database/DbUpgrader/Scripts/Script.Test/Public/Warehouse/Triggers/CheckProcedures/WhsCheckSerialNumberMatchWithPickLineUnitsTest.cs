using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Testing
{
	[TestedType(typeof(WhsCheckSerialNumberMatchWithPickLineUnits))]
	class WhsCheckSerialNumberMatchWithPickLineUnitsTest : DbCreateScriptTest
	{
		public void Test_WhsCheckSerialNumberMatchWithPickLineUnits_Update_NoPickingLineSet()
		{
			Test_WhsCheckSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 0);
		}

		public void Test_WhsCheckSerialNumberMatchWithPickLineUnits_Update_LessPickingLineSet()
		{
			Test_WhsCheckSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 1);
		}

		public void Test_WhsCheckSerialNumberMatchWithPickLineUnits_Update_MatchPickingLineSet()
		{
			Test_WhsCheckSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 2);
		}

		public void Test_WhsCheckSerialNumberMatchWithPickLineUnits_Update_MorePickingLineSet()
		{
			Test_WhsCheckSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 3);
		}

		void Test_WhsCheckSerialNumberMatchWithPickLineUnits_UpdateCore(int numberOfPickingLineSet)
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var location1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var location2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 3m, location1.PK) { WE_StockOnHand = 3m }.AppendInsertAndReturnObject(sql);
			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 2m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = location1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine, 2m) { WZ_PickedDateTime = DateTime.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var whsSerialNumber3 = new WhsSerialNumber(client, product, "SN3").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK) { WSV_WZ_PickingLine = (numberOfPickingLineSet > 0 ? pickLine.PK : null) }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK) { WSV_WZ_PickingLine = (numberOfPickingLineSet > 1 ? pickLine.PK : null) }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber3.PK) { WSV_WZ_PickingLine = (numberOfPickingLineSet > 2 ? pickLine.PK : null) }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);
			// SN3 not transfered

			AssertNoExceptionThrown(() => Save(sql));

			AssertEquals("Precondition", numberOfPickingLineSet, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_WZ_PickingLine != null));
			AssertEquals("Precondition", numberOfPickingLineSet, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_WZ_PickingLine == pickLine.PK));
			if (numberOfPickingLineSet == 2)
			{
				AssertNoExceptionThrown(() => RunSP(pickLine.PK));
			}
			else
			{
				AssertTriggerExceptionThrown(() => RunSP(pickLine.PK));
			}
		}

		#region Helper

		[ExpectNoExceptions]
		void AssertTriggerExceptionThrown(TestDelegate codeToRun)
		{
			NUnit.Framework.Assert.That(codeToRun, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "Expected to thrown SerialNumber not match with PickLine Units.");
		}

		void Save(SqlQueryBuilder sql)
		{
			ExecCommandWithDisableTriggers(sql.ToStringWithNewLineBetweenAppends());
		}

		void ExecCommandWithDisableTriggers(string sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory), WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_PreventUnPickedPickLinesOnFinalisedJobs), WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql);
			}
		}

		void RunSP(params Guid[] pickLinePKs)
		{
			const string sql = @"EXEC WhsCheckSerialNumberMatchWithPickLineUnits @PickLinePKs;";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@PickLinePKs", "dbo.TVP_uniqueidentifier", pickLinePKs);
				sqlCommand.ExecuteNonQuery();
			}
		}

		string TriggerErrorMessage => "TriggerLikelyConcurrencyError: Attempt to pick a pickline where the units do not match the pivot picking serial numbers.";

		#endregion
	}
}

