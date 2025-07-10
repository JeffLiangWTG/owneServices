using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory))]
	class TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventoryTest : DBCreateTriggerScriptTest
	{
		public void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Update_NoPickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_UpdateCore(numberOfSNSetForTransfer: 0);
		}

		public void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Update_LessPickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_UpdateCore(numberOfSNSetForTransfer: 1);
		}

		public void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Update_MatchPickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_UpdateCore(numberOfSNSetForTransfer: 2);
		}

		public void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Update_MorePickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_UpdateCore(numberOfSNSetForTransfer: 3);
		}

		void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_UpdateCore(int numberOfSNSetForTransfer)
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location1.PK) { WE_StockOnHand = 2m }.AppendInsertAndReturnObject(sql);
			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 2m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = location1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine, 2m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var whsSerialNumber3 = new WhsSerialNumber(client, product, "SN3").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);
			for (var i = 0; i < numberOfSNSetForTransfer; i++)
			{
				var whsSerialNumberPK = i switch
				{
					0 => whsSerialNumber1.PK,
					1 => whsSerialNumber2.PK,
					2 => whsSerialNumber3.PK,
					_ => throw new ArgumentOutOfRangeException(nameof(numberOfSNSetForTransfer))
				};

				new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumberPK).AppendInsertAndReturnObject(sql);
			}

			AssertNoExceptionThrown(() => Save(sql));

			AssertEquals("Precondition", 2, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_WZ_PickingLine == pickLine.PK));
			if (numberOfSNSetForTransfer == 2)
			{
				AssertNoExceptionThrown(() => PickPickLine(pickLine));
			}
			else
			{
				AssertTriggerExceptionThrown("Expected to Trigger prevent apply changes when units not match with serial number.", () => PickPickLine(pickLine));
			}
		}

		public void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_NotSuspend()
		{
			Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Suspend(isSuspend: false);
		}

		public void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Suspend()
		{
			Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Suspend(isSuspend: true);
		}

		void Test_TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory_Suspend(bool isSuspend)
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, location1.PK) { WE_StockOnHand = 2m }.AppendInsertAndReturnObject(sql);
			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "T1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 2m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = location1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine, 2m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var whsSerialNumber3 = new WhsSerialNumber(client, product, "SN3").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertEquals("Precondition", 2, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_ParentID == receiveLine.PK));
			AssertEquals("Precondition", 1, WhsSerialNumberPivot.CountInDB(TestConnection, p => p.WSV_ParentID == transferLine.PK));
			if (isSuspend)
			{
				AssertNoExceptionThrown(() => PickPickLineWithDeffer(defer: true));
			}
			else
			{
				AssertTriggerExceptionThrown("Expected to Trigger prevent when is not suspended.", () => PickPickLineWithDeffer(defer: false));
			}

			void PickPickLineWithDeffer(bool defer)
			{
				var sqlUpdate = new SqlQueryBuilder();
				if (defer)
				{
					sqlUpdate.AppendLine($"EXEC dbo.SuspendTrigger '{nameof(TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory)}'");
				}
				sqlUpdate.AppendLine(WhsPickLine.UpdateWhere(pickLine.PK).Set(l => l.WZ_PickedDateTime, DateTime.Now).Set(l => l.WZ_GS_NKAssignedTo, "AA").AsSQL());
				ExecCommandWithDisableTriggers(sqlUpdate.ToStringWithNewLineBetweenAppends());
			}
		}

		#region Helper

		[ExpectNoExceptions]
		void AssertTriggerExceptionThrown(string msg, TestDelegate codeToRun)
		{
			NUnit.Framework.Assert.That(codeToRun, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), msg);
		}

		void Save(SqlQueryBuilder sql)
		{
			ExecCommandWithDisableTriggers(sql.ToStringWithNewLineBetweenAppends());
		}

		void PickPickLine(WhsPickLine pickLine)
		{
			ExecCommandWithDisableTriggers(WhsPickLine.UpdateWhere(pickLine.PK).Set(l => l.WZ_PickedDateTime, DateTime.Now).Set(l => l.WZ_GS_NKAssignedTo, "AA").AsSQL());
		}

		void ExecCommandWithDisableTriggers(string sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_PreventUnPickedPickLinesOnFinalisedJobs), WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory), WhsSerialNumberPivotSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql);
			}
		}

		string TriggerErrorMessage => "Attempt to pick a pickline where the serial numbers do not match the pivot picking serial numbers.";

		#endregion
	}
}
