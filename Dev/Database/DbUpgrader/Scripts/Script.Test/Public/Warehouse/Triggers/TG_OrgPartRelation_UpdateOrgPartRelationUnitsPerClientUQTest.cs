using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Warehouse.Triggers
{
	[TestedType(typeof(TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ))]
	class TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQTest : TestCase
	{
		const string triggerName = "TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ";
		const string procedureName = "UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart";

		#region TestTrigger_IsDeferred

		[UseSnapshotProtection]
		public void TestTrigger_IsDeferred()
		{
			TestTrigger_DeferralCore(true);
		}

		[UseSnapshotProtection]
		public void TestTrigger_IsNotDeferred()
		{
			TestTrigger_DeferralCore(false);
		}

		void TestTrigger_DeferralCore(bool isDeferred)
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" };

				if (isDeferred)
				{
					var suspendAndInsertClientProductRelation = new StringBuilder();
					suspendAndInsertClientProductRelation
						.AppendLine($"SuspendTrigger '{triggerName}'")
						.Append(clientProductRelation.GetInsertStatement());
					AssertNoExceptionThrown("If trigger is suspended then stored procedure should not be run.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, procedureName, suspendAndInsertClientProductRelation.ToString()));
				}
				else
				{
					AssertCheckProcedureRanForProducts("If trigger is not suspended then stored procedure should be run",
						mainConnection, clientProductRelation.GetInsertStatement(), product);
				}
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update_ClientUQ()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				
				var updateSql = OrgPartRelation.UpdateWhere(clientProductRelation.PK).Set(unit => unit.OU_ClientUQ, "UNT").AsSQL();

				AssertCheckProcedureRanForProducts(
					"If trigger is not suspended then stored procedure should be run when updating OrgPartRelation's OU_ClientUQ.",
					mainConnection,
					updateSql,
					product);
			}
		}

		#endregion

		#region HelperFunctions

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}

		void AssertCheckProcedureRanForProducts(string errorMessage, DbConnection connection, string sql,
			params OrgSupplierPart[] expectedProductsInProcedure)
		{
			var actualPKs = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection,
				procedureName, sql, expectedProductsInProcedure);
			if (actualPKs == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedProductsInProcedure.Select(l => l.PK), actualPKs);
			}
		}

		#endregion
	}
}
