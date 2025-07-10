using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisation))]
	class TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisationTest : DBCreateTriggerScriptTest
	{
	}

	[UseSnapshotProtection]
	class Trigger_TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisationTest : TestCase
	{
		#region TestTrigger_PickFinalisation

		public void TestTrigger_PickFinalisation()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick1 = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);

				var pick2 = new WhsPick(whs1, "P2", "ENT").AppendInsertAndReturnObject(sql);
				var order2 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "PIC", "O2") { WD_WP = pick2.PK };
				order2.WD_DocketStatus = "DEP";
				order2.AppendInsertAndReturnObject(sql);

				ExecuteSQLInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update("Update must be allowed when the docket status have been set departed.", mainConnection, pick2.PK, "FIN", new DateTime(2024, 3, 19), TestTriggerResult.Success);
				AssertTrigger_Update("Update must be prevented when the docket status have NOT been set departed.", mainConnection, pick1.PK, "FIN", new DateTime(2024, 3, 19), TestTriggerResult.Fail);
				AssertTrigger_Update("Update must be allowed when the update for the pick is not Finalisation.", mainConnection, pick1.PK, "PIS", null, TestTriggerResult.Success);
			}
		}

		public void TestTrigger_PickFinalisation_OtherDocketTypes()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var today = new DateTime(2024, 3, 19);
				var sql = new SqlQueryBuilder();
				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(mainConnection);
				var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(mainConnection);

				var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1").AppendInsertAndReturnObject(sql);

				var pick1 = new WhsPick(whs1, "P1", "ENT").AppendInsertAndReturnObject(sql);
				var order1 = new WhsDocket(client.PK, whs1.PK, "ORD", "ORD", "ATP", "O1") { WD_WP = pick1.PK }.AppendInsertAndReturnObject(sql);

				var pick2 = new WhsPick(whs1, "P2", "ENT").AppendInsertAndReturnObject(sql);
				var docketWithWORType = new WhsDocket(client.PK, whs1.PK, "WOR", "ASS", "ATP", "WO1") { WD_WP = pick2.PK }.AppendInsertAndReturnObject(sql);

				var pick3 = new WhsPick(whs1, "P3", "ENT").AppendInsertAndReturnObject(sql);
				var docketWithDWOType = new WhsDocket(client.PK, whs1.PK, "DWO", "ASS", "ATP", "DO1") { WD_WP = pick3.PK }.AppendInsertAndReturnObject(sql);

				ExecuteSQLInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				AssertTrigger_Update("Update must be prevented when the docket status have NOT been set departed.", mainConnection, pick1.PK, "FIN", today, TestTriggerResult.Fail);
				AssertTrigger_Update("Update must be allowed when the docket type is NOT ORD.", mainConnection, pick2.PK, "FIN", today, TestTriggerResult.Success);
				AssertTrigger_Update("Update must be allowed when the docket type is NOT ORD.", mainConnection, pick3.PK, "FIN", today, TestTriggerResult.Success);
			}
		}

		#endregion

		#region TestAssertions

		void AssertTrigger_Update(string errorMessage, DbConnection connection, Guid pickPK, string newPickStatus, DateTime? finaliseDate, TestTriggerResult expectedResult)
		{
			var sqlBuilder = WhsPick.UpdateWhere(pickPK).Set(p => p.WP_PickStatus, newPickStatus);
			if (finaliseDate != null)
			{
				sqlBuilder.Set(p => p.WP_FinalizedDateUtc, finaliseDate);
				sqlBuilder.Set(p => p.WP_GS_NKFinalizedBy, "A");
			}
			var sql = sqlBuilder.AsSQL();

			if (expectedResult == TestTriggerResult.Success)
			{
				AssertNoExceptionThrown(errorMessage, () => ExecuteSQLInTransaction(connection, sql));
			}
			else
			{
				AssertExceptionThrown(typeof(SqlException), "Attempt to finalise Pick without changing the status of the attached Orders to Departed.", () => ExecuteSQLInTransaction(connection, sql), true);
			}
		}

		void ExecuteSQLInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		#endregion

		#region Implementation

		enum TestTriggerResult
		{
			Success,
			Fail
		}

		#endregion
	}
}
