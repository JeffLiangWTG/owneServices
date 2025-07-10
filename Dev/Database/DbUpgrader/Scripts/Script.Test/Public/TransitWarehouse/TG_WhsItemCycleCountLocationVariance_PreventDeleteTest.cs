using System;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemCycleCountLocationVariance_PreventDelete))]
	class TG_WhsItemCycleCountLocationVariance_PreventDeleteTest : DBCreateTriggerScriptTest
	{
		const string DeleteVarianceTriggerErrorMessage = "Cycle Count Variances cannot be deleted.";

		public void TestTrigger_Delete()
		{
			var today = DateTimeOffset.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var cycleCountLocation = new WhsItemCycleCountLocation(location.PK)
			{
				WIC_Status = "CMP",
				WIC_StartTime = today,
				WIC_ProcessingTime = today.AddMinutes(2),
				WIC_EndTime = today.AddMinutes(2),
				WIC_GS_NKAssignedTo = "AAA"
			}.AppendInsertAndReturnObject(sql);
			var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertExceptionThrown("Should throw exception", typeof(SqlException), DeleteVarianceTriggerErrorMessage,
				() => WhsItemCycleCountLocationVariance.DeleteInDB(TestConnection, variance.PK), true);
		}
	}

	public class Trigger_WhsItemCycleCountLocationVariance_PreventDeleteTest : TestCase
	{
		const string SuspendTriggerProc = nameof(SuspendTrigger);
		const string TriggerName = nameof(TG_WhsItemCycleCountLocationVariance_PreventDelete);

		[UseSnapshotProtection]
		public void TestTrigger_Delete_Suspended()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var today = DateTimeOffset.Now;
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var row = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql);
				var area = new WhsArea(whs.PK, "A1").AppendInsertAndReturnObject(sql);
				var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var cycleCountLocation = new WhsItemCycleCountLocation(location.PK)
				{
					WIC_Status = "CMP",
					WIC_StartTime = today,
					WIC_ProcessingTime = today.AddMinutes(2),
					WIC_EndTime = today.AddMinutes(2),
					WIC_GS_NKAssignedTo = "AAA"
				}.AppendInsertAndReturnObject(sql);
				var variance = new WhsItemCycleCountLocationVariance(cycleCountLocation, "OPN").AppendInsertAndReturnObject(sql);

				using (mainConnection.BeginTransactionWithManager())
				{
					mainConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
					mainConnection.CommitTransaction();
				}

				using (Db.DisposableActionForDbConnection())
				using (var newConnection = Db.NewExtraConnectionToMainDb())
				using (newConnection.BeginTransactionWithManager())
				{
					var deleteSql = new StringBuilder();
					deleteSql
						.AppendLine($"{SuspendTriggerProc} '{TriggerName}'")
						.Append($"DELETE FROM dbo.WhsItemCycleCountLocationVariance WHERE WIV_PK = '{variance.PK}'");
					newConnection.ExecuteNonQuery(deleteSql.ToString());

					AssertNoExceptionThrown(() => newConnection.CommitTransaction());
				}
			}
		}
	}
}
