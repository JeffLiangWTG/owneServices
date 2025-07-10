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
	[TestedType(typeof(TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits))]
	class TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnitsTest : DBCreateTriggerScriptTest
	{
		public void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_Update_NoPickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 0);
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_Update_LessPickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 1);
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_Update_MatchPickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 2);
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_Update_MorePickingLineSet()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_UpdateCore(numberOfPickingLineSet: 3);
		}

		void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_UpdateCore(int numberOfPickingLineSet)
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
			var pickLine = new WhsPickLine(receiveLine, transferLine, 2m).AppendInsertAndReturnObject(sql);
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

			var isSetPickingMatchWithSN = numberOfPickingLineSet == pickLine.WZ_Units;
			if (isSetPickingMatchWithSN)
			{
				AssertNoExceptionThrown(() => PickPickLine(pickLine));
			}
			else
			{
				AssertTriggerExceptionThrown(() => PickPickLine(pickLine));
			}
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_Update_Units()
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
			var pickLine = new WhsPickLine(receiveLine, transferLine, 2m).AppendInsertAndReturnObject(sql);
			var whsSerialNumber1 = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumber2 = new WhsSerialNumber(client, product, "SN2").AppendInsertAndReturnObject(sql);
			var whsSerialNumber3 = new WhsSerialNumber(client, product, "SN3").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber1.PK).AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber2.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertNoExceptionThrown(() => PickPickLine(pickLine));

			WhsPickLine.AssertFromDB(TestConnection, pickLine.PK)
				.ExpectNotEquals("WZ_PickedDateTime", r => r.WZ_PickedDateTime, null)
				.ExpectEquals("WZ_GS_NKAssignedTo", r => r.WZ_GS_NKAssignedTo, "AA")
				.VerifyAll("Precondition");

			AssertTriggerExceptionThrown(() => ExecCommandWithDisableTriggers(WhsPickLine.UpdateWhere(pickLine.PK).Set(l => l.WZ_Units, 3).AsSQL()));
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_Insert_Suspend()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_InsertCore(isSuspend: true);
		}

		public void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_Insert_NotSuspend()
		{
			Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_InsertCore(isSuspend: false);
		}

		void Test_TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits_InsertCore(bool isSuspend)
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
			var transferLine = new WhsDocketLine(transfer, product.PK, 1m, location2.PK) { WE_StockOnHand = 0m, WE_WL_TransferFrom = location1.PK, WE_DocketLineStatus = "ENT" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));
			AssertEquals("Precondition", 0, WhsPickLine.CountInDB(TestConnection));

			if (isSuspend)
			{
				AssertNoExceptionThrown(() => InsertPickedPickLine(defer: true));
				AssertEquals("Should insert pickLine in DB.", 1, WhsPickLine.CountInDB(TestConnection));
			}
			else
			{
				AssertTriggerExceptionThrown(() => InsertPickedPickLine(defer: false));
				AssertEquals("Should not insert pickLine in DB.", 0, WhsPickLine.CountInDB(TestConnection));
			}

			void InsertPickedPickLine(bool defer)
			{
				var sqlInsert = new SqlQueryBuilder();
				if (defer)
				{
					sqlInsert.AppendLine($"EXEC dbo.SuspendTrigger '{nameof(TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits)}'");
				}
				new WhsPickLine(receiveLine, transferLine, 1m) { WZ_PickedDateTime = today, WZ_GS_NKAssignedTo = "AA" }.AppendInsertAndReturnObject(sqlInsert);
				ExecCommandWithDisableTriggers(sqlInsert.ToStringWithNewLineBetweenAppends());
			}
		}

		#region Helper

		[ExpectNoExceptions]
		void AssertTriggerExceptionThrown(TestDelegate codeToRun)
		{
			NUnit.Framework.Assert.That(codeToRun, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "Expected to Trigger prevent apply changes.");
		}

		void Save(SqlQueryBuilder sql) => ExecCommandWithDisableTriggers(sql.ToStringWithNewLineBetweenAppends());

		void PickPickLine(WhsPickLine pickLine)
		{
			ExecCommandWithDisableTriggers(WhsPickLine.UpdateWhere(pickLine.PK).Set(l => l.WZ_PickedDateTime, DateTime.Now).Set(l => l.WZ_GS_NKAssignedTo, "AA").AsSQL());
		}

		void ExecCommandWithDisableTriggers(string sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory), WhsPickLineSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery(sql);
			}
		}

		string TriggerErrorMessage => "TriggerLikelyConcurrencyError: Attempt to pick a pickline where the units do not match the pivot picking serial numbers.";

		#endregion
	}
}
