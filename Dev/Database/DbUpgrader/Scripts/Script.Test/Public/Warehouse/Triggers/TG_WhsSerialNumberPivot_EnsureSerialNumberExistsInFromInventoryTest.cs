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
	[TestedType(typeof(TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory))]
	class TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventoryTest : DBCreateTriggerScriptTest
	{
		public void Test_TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory_SerialNumberExistsInInventory()
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
			var transferLine = new WhsDocketLine(transfer, product.PK, 1m, locationA2.PK) { WE_DocketLineStatus = "ENT", WE_WL_TransferFrom = locationA1.PK }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine, 1m).AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var whsSerialNumberPivot = new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = AssignSerialNumberToTransferLine();

			AssertNoExceptionThrown("Should save without issue.", () => Save(updateBuilder));

			WhsSerialNumberPivot.AssertFromDB(TestConnection, whsSerialNumberPivot.PK)
				.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine.PK)
				.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
				.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
				.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, pickLine.PK)
				.VerifyAll();

			SqlQueryBuilder AssignSerialNumberToTransferLine()
			{
				var updateBuilder = new SqlQueryBuilder();
				updateBuilder.AppendLine(WhsSerialNumberPivot.UpdateWhere(whsSerialNumberPivot.PK).Set(s => s.WSV_WZ_PickingLine, pickLine.PK).AsSQL());
				new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(updateBuilder);
				return updateBuilder;
			}
		}

		public void Test_TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory_HasInventorySerialNumber()
		{
			Test_TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory_Core(hasInventorySN: true);
		}

		public void Test_TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory_NotHaveInventorySerialNumber()
		{
			Test_TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory_Core(hasInventorySN: false);
		}

		void Test_TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory_Core(bool hasInventorySN)
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
			var receiveLine = new WhsDocketLine(receive, product.PK, 1m, locationA1.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today, WE_StockOnHand = 1m }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TFR1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 1m, locationA2.PK) { WE_DocketLineStatus = "ENT", WE_WL_TransferFrom = locationA1.PK }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine, transferLine, 1m).AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var receiveLineSNPivot = hasInventorySN ?
				new WhsSerialNumberPivot(receiveLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK) { WSV_WZ_PickingLine = pickLine.PK }.AppendInsertAndReturnObject(sql)
				: null;

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = new SqlQueryBuilder();
			var transferSNPivot = new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(updateBuilder);

			if (hasInventorySN)
			{
				AssertNoExceptionThrown("When a Serial Number is in source location, no exception should be thrown.", () => Save(updateBuilder));
				WhsSerialNumberPivot.AssertFromDB(TestConnection, receiveLineSNPivot.PK)
					.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, receiveLine.PK)
					.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
					.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
					.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, pickLine.PK)
					.VerifyAll();

				WhsSerialNumberPivot.AssertFromDB(TestConnection, transferSNPivot.PK)
					.ExpectEquals("WSV_ParentID: ", i => i.WSV_ParentID, transferLine.PK)
					.ExpectEquals("WSV_ParentTableCode: ", i => i.WSV_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)
					.ExpectEquals("WSV_IsReleaseCaptured: ", i => i.WSV_IsReleaseCaptured, false)
					.ExpectEquals("WSV_WZ_PickingLine: ", i => i.WSV_WZ_PickingLine, null)
					.VerifyAll();
			}
			else
			{
				NUnit.Framework.Assert.That(() => Save(updateBuilder), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "When a Serial Number is not in source location, an exception should be thrown.");
				AssertEquals("Should not save in DB.", 0, WhsSerialNumberPivot.CountInDB(TestConnection));
			}
		}

		public void Test_TG_WhsSerialNumberPivot_EnsureSerialNumberExistsInFromInventory_SNExistsInDifferentLocation()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var locationA1 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var locationA2 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 2 }.AppendInsertAndReturnObject(sql);
			var locationA3 = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 3 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 10m, locationA1.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive, product.PK, 10m, locationA3.PK) { WE_DocketLineStatus = "FIN", WE_FinalisedDate = today, WE_StockOnHand = 10m }.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "ENT", "TFR1").AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 1m, locationA2.PK) { WE_DocketLineStatus = "ENT", WE_WL_TransferFrom = locationA1.PK }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine1, transferLine, 1m).AppendInsertAndReturnObject(sql);

			var whsSerialNumber = new WhsSerialNumber(client, product, "SN1").AppendInsertAndReturnObject(sql);
			var receiveLineSNPivot = new WhsSerialNumberPivot(receiveLine2.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should save without issue.", () => Save(sql));

			var updateBuilder = new SqlQueryBuilder();
			var transferSNPivot = new WhsSerialNumberPivot(transferLine.PK, WhsDocketLineSchema.Constants.Prefix, whsSerialNumber.PK).AppendInsertAndReturnObject(updateBuilder);

			NUnit.Framework.Assert.That(() => Save(updateBuilder), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), TriggerErrorMessage, true), "When a Serial Number is not in source location, an exception should be thrown.");
		}

		#region Helper

		void Save(SqlQueryBuilder sql)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		string TriggerErrorMessage => "Attempt to assign a serial number which does not exists in from location.";

		#endregion
	}
}
