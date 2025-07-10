using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsCycleCountLocationVariance_CannotBeModifiedOrDeleted))]
	class TG_WhsCycleCountLocationVariance_CannotBeModifiedOrDeletedTest : DBCreateTriggerScriptTest
	{
		const string ChangingKeyFieldTriggerErrorMessage = "Attempted to change key fields.";
		const string CannotDeleteVarianceTriggerErrorMessage = "Cycle Count Variances cannot be deleted.";

		#region TestTrigger_Insert

		public void TestTrigger_Insert()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN", 5m) { WCC_IsCountingPalletsOnly = true }.AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Ensure we can insert correct variances.", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestTrigger_Update

		#region TestTrigger_UpdateWCC_Status

		public void TestTrigger_UpdateWCC_Status_Approved()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN", 5m) { WCC_OH_Client = client.PK, WCC_OP_Product = product.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not blowup as there was no change to the value.",
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "OPN").Post(TestConnection));

			AssertNoExceptionThrown("Should not blowup as approving variance is allowed.",
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "APP")
				.Set(v => v.WCC_WD_RelatedAdjustment, adjustment.PK).Post(TestConnection));

			AssertExceptionThrown("Once approved, variance cannot be re-opened.", typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "OPN")
				.Set(v => v.WCC_WD_RelatedAdjustment, null).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_Status_Rejected()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN", 5m) { WCC_OH_Client = client.PK, WCC_OP_Product = product.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not blowup as there was no change to the value.",
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "OPN").Post(TestConnection));

			AssertNoExceptionThrown("Should not blowup as rejecting variance is allowed.",
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "REJ").Post(TestConnection));

			AssertExceptionThrown("Once rejected, variance cannot be re-opened.", typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "OPN")
				.Set(v => v.WCC_WD_RelatedAdjustment, null).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_Status_ChangeApprovedToRejected()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "APP", 5m) { WCC_OH_Client = client.PK, WCC_OP_Product = product.PK, WCC_WD_RelatedAdjustment = adjustment.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Once approved, variance cannot be changed to rejected.", typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "REJ")
				.Set(v => v.WCC_WD_RelatedAdjustment, null).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_Status_ChangeRejectedToApproved()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var adjustment = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "REJ", 5m) { WCC_OH_Client = client.PK, WCC_OP_Product = product.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Once rejected, variance cannot be changed to approved.", typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "APP")
				.Set(v => v.WCC_WD_RelatedAdjustment, adjustment.PK).Post(TestConnection), true);
		}

		#endregion

		#region TestTrigger_UpdateWCC_WD_RelatedAdjustment

		public void TestTrigger_UpdateWCC_WD_RelatedAdjustment()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var adjustment1 = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A1").AppendInsertAndReturnObject(sql);
			var adjustment2 = new WhsDocket(client.PK, whs.PK, "ADJ", "NEA", "ENT", "A2").AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN", 5m) { WCC_OH_Client = client.PK, WCC_OP_Product = product.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertNoExceptionThrown("Should not blowup as there was no change to the value.",
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_WD_RelatedAdjustment, null).Post(TestConnection));

			AssertNoExceptionThrown("Should not blowup as approving variance is allowed.",
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_Status, "APP")
				.Set(v => v.WCC_WD_RelatedAdjustment, adjustment1.PK).Post(TestConnection));

			AssertExceptionThrown("Once approved, related adjustment cannot be changed.", typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_WD_RelatedAdjustment, adjustment2.PK).Post(TestConnection), true);
		}

		#endregion

		#region TestTrigger_Update_CannotUpdateAnyOtherColumn

		public void TestTrigger_UpdateWCC_WCL_CycleCountLocation()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH2", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R2").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC0001" }.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_WCL_CycleCountLocation, cycleCountLocation).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_WL_ExpectedStockLocation()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH2", branch.PK).WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R2").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_WL_ExpectedStockLocation, location.PK)
				.Set(v => v.WCC_PalletID, "PLT-1").Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_OH_Client()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_OH_Client, client.PK).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_OP_Product()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("CLIENT2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P2").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_OP_Product, product.PK).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_IsCountingPalletsOnly()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_IsCountingPalletsOnly, true)
				.Set(v => v.WCC_OP_Product, null)
				.Set(v => v.WCC_OH_Client, null).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_PalletID()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_PalletID, "PLT-1").Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_PartAttrib1()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_PartAttrib1, "Attr123").Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_PartAttrib2()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_PartAttrib2, "Attr123").Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_PartAttrib3()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_PartAttrib3, "Attr123").Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_SerialNumber()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_SerialNumber, "SN123").Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_ExpiryDate()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_ExpiryDate, DateTime.UtcNow).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_PackingDate()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_PackingDate, DateTime.UtcNow).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_ExpectedQty()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_ExpectedQty, 10m).Post(TestConnection), true);
		}

		public void TestTrigger_UpdateWCC_VarianceQty()
		{
			var cycleCountVariance = TestTrigger_CannotUpdateSetup();

			AssertExceptionThrown(typeof(SqlException), ChangingKeyFieldTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.UpdateWhere(cycleCountVariance.PK)
				.Set(v => v.WCC_VarianceQty, 10m).Post(TestConnection), true);
		}

		WhsCycleCountLocationVariance TestTrigger_CannotUpdateSetup()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN", 5m) { WCC_OH_Client = client.PK, WCC_OP_Product = product.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			return cycleCountVariance;
		}

		#endregion

		#endregion

		#region TestTrigger_Delete

		public void TestTrigger_Delete()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
			var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN", 5m) { WCC_IsCountingPalletsOnly = true }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown(typeof(SqlException), CannotDeleteVarianceTriggerErrorMessage,
				() => WhsCycleCountLocationVariance
				.DeleteInDB(TestConnection, cycleCountVariance.PK), true);
		}
		#endregion
	}

	public class Trigger_WhsCycleCountLocationVariance_CannotBeModifiedOrDeletedTest : TestCase
	{
		const string SuspendTriggerProc = nameof(SuspendTrigger);
		const string TriggerName = nameof(TG_WhsCycleCountLocationVariance_CannotBeModifiedOrDeleted);

		[UseSnapshotProtection]
		public void TestTrigger_Delete_Suspended()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var cycleCountLocation = new WhsCycleCountLocation(location.PK, "PWP") { WCL_JobID = "WC00000001", WCL_StartTime = DateTimeOffset.Now, WCL_EndTime = DateTimeOffset.Now.AddMinutes(10), WCL_GS_NKAssignedTo = "AAA" }.AppendInsertAndReturnObject(sql);
				var cycleCountVariance = new WhsCycleCountLocationVariance(cycleCountLocation, "OPN", 5m) { WCC_IsCountingPalletsOnly = true }.AppendInsertAndReturnObject(sql);

				using (mainConnection.BeginTransactionWithManager())
				{
					mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					mainConnection.CommitTransaction();
				}

				using (Db.DisposableActionForDbConnection())
				using (var newConnection = Db.NewExtraConnectionToMainDb())
				using (newConnection.BeginTransactionWithManager())
				{
					newConnection.ExecuteNonQuery($"{SuspendTriggerProc} '{TriggerName}'");
					WhsCycleCountLocationVariance.DeleteInDB(newConnection, cycleCountVariance.PK);

					AssertNoExceptionThrown(() => newConnection.CommitTransaction());
				}
			}
		}
	}
}

