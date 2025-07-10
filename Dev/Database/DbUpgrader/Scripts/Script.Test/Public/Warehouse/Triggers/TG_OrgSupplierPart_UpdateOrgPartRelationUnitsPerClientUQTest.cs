using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_OrgSupplierPart_UpdateOrgPartRelationUnitsPerClientUQ))]
	class TG_OrgSupplierPart_UpdateOrgPartRelationUnitsPerClientUQTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_OrgSupplierPart_UpdateOrgPartRelationUnitsPerClientUQTest : TestCase
	{
		const string triggerName = "TG_OrgSupplierPart_UpdateOrgPartRelationUnitsPerClientUQ";
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
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateSql = OrgSupplierPart.UpdateWhere(product.PK).Set(p => p.OP_StockKeepingUnit, "CAS").AsSQL();

				if (isDeferred)
				{
					var suspendAndUpdateSql = new StringBuilder();
					suspendAndUpdateSql
						.AppendLine($"SuspendTrigger '{triggerName}'")
						.Append(updateSql);
					AssertNoExceptionThrown("If trigger is suspended then stored procedure should not be run.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, procedureName, suspendAndUpdateSql.ToString()));
				}
				else
				{
					AssertCheckProcedureRanForProducts("If trigger is not suspended then stored procedure should be run",
						mainConnection, updateSql, product);
				}
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
