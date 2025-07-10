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
	[TestedType(typeof(TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ))]
	class TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQTest : DBCreateTriggerScriptTest
	{
	}

	class Trigger_TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQTest : TestCase
	{
		const string triggerName = "TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ";
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
				new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var partUnit = new OrgPartUnit(product, 10m, "BOX", "CAS");

				if (isDeferred)
				{
					var suspendAndInsertPartUnitSql = new StringBuilder();
					suspendAndInsertPartUnitSql
						.AppendLine($"SuspendTrigger '{triggerName}'")
						.Append(partUnit.GetInsertStatement());
					AssertNoExceptionThrown("If trigger is suspended then stored procedure should not be run.",
						() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, procedureName, suspendAndInsertPartUnitSql.ToString()));
				}
				else
				{
					AssertCheckProcedureRanForProducts("If trigger is not suspended then stored procedure should be run",
						mainConnection, partUnit.GetInsertStatement(), product);
				}
			}
		}

		#endregion

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var partUnit = new OrgPartUnit(product, 10m, "UNT", "CAS");

				AssertCheckProcedureRanForProducts(
					"If trigger is not suspended then stored procedure should be run when inserting new OrgPartUnit.",
					mainConnection,
					partUnit.GetInsertStatement(),
					product);
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update_PackType()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
				var partUnit = new OrgPartUnit(product, 10m, "CAS", "BOX").AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateSql = OrgPartUnit.UpdateWhere(partUnit.PK).Set(unit => unit.OF_PackType, "UNT").AsSQL();

				AssertCheckProcedureRanForProducts(
					"If trigger is not suspended then stored procedure should be run when updating OrgPartUnit's PackType.",
					mainConnection,
					updateSql,
					product);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_ParentPackType()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
				var partUnit = new OrgPartUnit(product, 10m, "UNT", "CAS").AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateSql = OrgPartUnit.UpdateWhere(partUnit.PK).Set(unit => unit.OF_ParentPackType, "BOX").AsSQL();

				AssertCheckProcedureRanForProducts(
					"If trigger is not suspended then stored procedure should be run when updating OrgPartUnit's ParentPackType.",
					mainConnection,
					updateSql,
					product);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_QuantityInParent()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
				var partUnit = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateSql = OrgPartUnit.UpdateWhere(partUnit.PK).Set(unit => unit.OF_QuantityInParent, 20m).AsSQL();

				AssertCheckProcedureRanForProducts(
					"If trigger is not suspended then stored procedure should be run when updating OrgPartUnit's QuantityInParent.",
					mainConnection,
					updateSql,
					product);
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_IrrelevantColumn()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
				var partUnit = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var updateSql = OrgPartUnit.UpdateWhere(partUnit.PK).Set(unit => unit.OF_Depth, 20m).AsSQL();

				AssertNoExceptionThrown("Update of irrelevant column should not run the stored procedure",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, procedureName, updateSql));
			}
		}

		#endregion

		#region TestTrigger_Delete

		[UseSnapshotProtection]
		public void TestTrigger_Delete()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
				var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
				var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
				var partUnit = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
				
				ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());

				var deleteSql = $"DELETE FROM dbo.OrgPartUnit WHERE OF_PK = '{partUnit.PK}'";

				AssertCheckProcedureRanForProducts(
					"If trigger is not suspended then stored procedure should be run when updating OrgPartUnit's QuantityInParent.",
					mainConnection,
					deleteSql,
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
