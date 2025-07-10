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
	[TestedType(typeof(WhsCheckTransferSerialNumberMatchWithnventory))]
	class WhsCheckTransferSerialNumberMatchWithnventoryTest : DbCreateScriptTest
	{
		public void Test_WhsCheckTransferSerialNumberMatchWithnventory_Update_NoPickingLineSet()
		{
			Test_WhsCheckTransferSerialNumberMatchWithnventory_UpdateCore(numberOfSNSetForTransfer: 0);
		}

		public void Test_WhsCheckTransferSerialNumberMatchWithnventory_Update_LessPickingLineSet()
		{
			Test_WhsCheckTransferSerialNumberMatchWithnventory_UpdateCore(numberOfSNSetForTransfer: 1);
		}

		public void Test_WhsCheckTransferSerialNumberMatchWithnventory_Update_MatchPickingLineSet()
		{
			Test_WhsCheckTransferSerialNumberMatchWithnventory_UpdateCore(numberOfSNSetForTransfer: 2);
		}

		public void Test_WhsCheckTransferSerialNumberMatchWithnventory_Update_MorePickingLineSet()
		{
			Test_WhsCheckTransferSerialNumberMatchWithnventory_UpdateCore(numberOfSNSetForTransfer: 3);
		}

		void Test_WhsCheckTransferSerialNumberMatchWithnventory_UpdateCore(int numberOfSNSetForTransfer)
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
			var pickLine = new WhsPickLine(receiveLine, transferLine, 2m) { WZ_PickedDateTime = DateTime.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
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
				AssertNoExceptionThrown(() => RunSP(pickLine.PK));
			}
			else
			{
				AssertTriggerExceptionThrown("Expected to Trigger prevent apply changes when units not match with serial number.", () => RunSP(pickLine.PK));
			}
		}

		public void Test_WhsCheckTransferSerialNumberMatchWithnventory_IWS()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var locationB = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, locationA.PK) { WE_StockOnHand = 2m }.AppendInsertAndReturnObject(sql);
			var transfer_Master = new WhsDocket(client.PK, whs.PK, "TFR", "IWS", "FIN", "TR1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master = new WhsDocketLine(transfer_Master, product.PK, 1m, locationB.PK) { WE_WL_TransferFrom = locationA.PK, WE_GS_NKPutawayBy = "AA" }.AppendInsertAndReturnObject(sql);
			var transfer_Child = new WhsDocket(client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR2") { WD_FinalisedDate = today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
			var transferLine_Child = new WhsDocketLine(transfer_Child, product.PK, 1m, locationB.PK) { WE_WL_TransferFrom = locationA.PK, WE_GS_NKPutawayBy = "AA", WE_WE_ParentDocketLine = transferLine_Master, WE_StockOnHand = 1m }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine_Master, 1m) { WZ_PickedDateTime = DateTime.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertTriggerExceptionThrown("Expected to Trigger prevent apply changes when transfer line does not have serial number.", () => RunSP(pickLine.PK));
		}

		public void Test_WhsCheckTransferSerialNumberMatchWithnventory_IWD()
		{
			var today = DateTime.UtcNow.ToSmallDateTimeFloor();
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);
			var locationA = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);
			var area2 = new WhsArea(whs2.PK, "AREA2").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs2, "B").AppendInsertAndReturnObject(sql);
			var locationB = new WhsLocation(row2.PK, area2.PK, area2.PK).AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Product").AppendInsertAndReturnObject(sql);
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 2m, locationA.PK) { WE_StockOnHand = 2m }.AppendInsertAndReturnObject(sql);
			var transfer_Master = new WhsDocket(client.PK, whs2.PK, "TFR", "IWD", "FIN", "TR1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var transferLine_Master = new WhsDocketLine(transfer_Master, product.PK, 1m, locationB.PK) { WE_WL_TransferFrom = locationA.PK, WE_GS_NKPutawayBy = "AA", WE_StockOnHand = 1m }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine_Master, 1m) { WZ_PickedDateTime = DateTime.Today, WZ_GS_NKAssignedTo = "E" }.AppendInsertAndReturnObject(sql);
			var transfer_Child = new WhsDocket(client.PK, whs.PK, "TFR", "IWS", "FIN", "TR2") { WD_FinalisedDate = today, WD_WD_ParentDocket = transfer_Master }.AppendInsertAndReturnObject(sql);
			var transferLine_Child = new WhsDocketLine(transfer_Child, product.PK, 1m, locationB.PK) { WE_WL_TransferFrom = locationA.PK, WE_GS_NKPutawayBy = "AA", WE_WE_ParentDocketLine = transferLine_Master }.AppendInsertAndReturnObject(sql);
			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => Save(sql));

			AssertTriggerExceptionThrown("Expected to Trigger prevent apply changes when transfer line does not have serial number.", () => RunSP(pickLine.PK));
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

		void ExecCommandWithDisableTriggers(string sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(WhsCheckTransferSerialNumberMatchWithnventory), WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_PreventUnPickedPickLinesOnFinalisedJobs), WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory), WhsSerialNumberPivotSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(TestConnection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql);
			}
		}

		void RunSP(params Guid[] pickLinePKs)
		{
			const string sql = @"EXEC WhsCheckTransferSerialNumberMatchWithnventory @PickLinePKs;";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@PickLinePKs", "dbo.TVP_uniqueidentifier", pickLinePKs);
				sqlCommand.ExecuteNonQuery();
			}
		}

		string TriggerErrorMessage => "Attempt to pick a pickline where the serial numbers do not match the pivot picking serial numbers.";

		#endregion
	}
}

