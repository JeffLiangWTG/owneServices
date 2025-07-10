using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Testing
{
	[TestedType(typeof(UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart))]
	class UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPartTest : DbCreateScriptTest
	{
		const string orgPartUnitTriggerName = "TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ";
		const string orgSupplierPartTriggerName = "TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ";
		const string orgPartRelationTriggerName = "TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ";
		const string procedureName = "UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart";

		#region TestProcedure_InsertOrgPartUnit

		public void TestProcedure_InsertOrgPartUnit_NoConversion()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
			var partUnit = new OrgPartUnit(product, 10m, "UNT", "CAS").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartUnitTriggerName, product.PK);

			var clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		public void TestProcedure_InsertOrgPartUnit_ConversionToClientUQ()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
			var partUnit = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartUnitTriggerName, product.PK);

			var clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		#endregion

		#region TestProcedure_UpdateOrgPartUnit

		public void TestProcedure_UpdateOrgPartUnit_PackType()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
			var partUnit = new OrgPartUnit(product, 10m, "CAS", "BOX").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartUnitTriggerName, product.PK);

			var clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0m, clientProductRelationFromDB.OU_UnitsPerClientUQ);

			var updateSql = OrgPartUnit.UpdateWhere(partUnit.PK).Set(u => u.OF_PackType, "UNT").AsSQL();

			SaveDataWithoutTriggerAndRunProcedure(updateSql, orgPartUnitTriggerName, product.PK);

			clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		public void TestProcedure_UpdateOrgPartUnit_ParentPackType()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
			var partUnit = new OrgPartUnit(product, 10m, "UNT", "CAS").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartUnitTriggerName, product.PK);

			var clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0m, clientProductRelationFromDB.OU_UnitsPerClientUQ);

			var updateSql = OrgPartUnit.UpdateWhere(partUnit.PK).Set(u => u.OF_ParentPackType, "BOX").AsSQL();

			SaveDataWithoutTriggerAndRunProcedure(updateSql, orgPartUnitTriggerName, product.PK);

			clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		public void TestProcedure_UpdateOrgPartUnit_QuantityInParent()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
			var partUnit = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartUnitTriggerName, product.PK);

			var clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);

			var updateSql = OrgPartUnit.UpdateWhere(partUnit.PK).Set(u => u.OF_QuantityInParent, 20).AsSQL();

			SaveDataWithoutTriggerAndRunProcedure(updateSql, orgPartUnitTriggerName, product.PK);

			clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.05m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		#endregion

		#region TestProcedure_DeleteOrgPartUnit

		public void TestProcedure_DeleteOrgPartUnit()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
			var partUnit = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartUnitTriggerName, product.PK);

			var clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);

			var updateSql = $"DELETE FROM dbo.OrgPartUnit WHERE OF_PK = '{partUnit.PK}';";

			SaveDataWithoutTriggerAndRunProcedure(updateSql, orgPartUnitTriggerName, product.PK);

			clientProductRelationFromDB =
				OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		#endregion

		#region TestProcedure_UpdateOrgSupplierPart

		public void TestProcedure_UpdateOrgSupplierPart_StockKeepingUnit()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);

			var partUnit1 = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			var partUnit2 = new OrgPartUnit(product, 20m, "CAS", "BOX").AppendInsertAndReturnObject(sql);
			
			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgSupplierPartTriggerName, product.PK);

			var clientProductRelationFromDB = OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);

			var updateSql = OrgSupplierPart.UpdateWhere(product.PK).Set(u => u.OP_StockKeepingUnit, "CAS").AsSQL();

			SaveDataWithoutTriggerAndRunProcedure(updateSql, orgSupplierPartTriggerName, product.PK);

			clientProductRelationFromDB = OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.05m, clientProductRelationFromDB.OU_UnitsPerClientUQ);

			updateSql = OrgSupplierPart.UpdateWhere(product.PK).Set(u => u.OP_StockKeepingUnit, "UNT").AsSQL();

			SaveDataWithoutTriggerAndRunProcedure(updateSql, orgSupplierPartTriggerName, product.PK);

			clientProductRelationFromDB = OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		#endregion

		#region TestProcedure_InsertOrgPartRelation

		public void TestProcedure_InsertOrgPartRelation()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var partUnit = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartRelationTriggerName, product.PK);

			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" };
			SaveDataWithoutTriggerAndRunProcedure(clientProductRelation.GetInsertStatement(), orgPartRelationTriggerName, product.PK);

			var clientProductRelationFromDB = OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		#endregion

		#region TestProcedure_UpdateOrgPartRelation

		public void TestProcedure_UpdateOrgPartRelation_OU_ClientUQ()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("P1") { OP_StockKeepingUnit = "UNT" }.AppendInsertAndReturnObject(sql);
			var clientProductRelation = new OrgPartRelation(client, product, "OWN") { OU_ClientUQ = "BOX" }.AppendInsertAndReturnObject(sql);
			var partUnit1 = new OrgPartUnit(product, 10m, "UNT", "BOX").AppendInsertAndReturnObject(sql);
			var partUnit2 = new OrgPartUnit(product, 20m, "UNT", "CAS").AppendInsertAndReturnObject(sql);

			SaveDataWithoutTriggerAndRunProcedure(sql.ToStringWithNewLineBetweenAppends(), orgPartRelationTriggerName, product.PK);

			var clientProductRelationFromDB = OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.1m, clientProductRelationFromDB.OU_UnitsPerClientUQ);

			var updateSql = OrgPartRelation.UpdateWhere(clientProductRelation.PK).Set(u => u.OU_ClientUQ, "CAS").AsSQL();

			SaveDataWithoutTriggerAndRunProcedure(updateSql, orgPartRelationTriggerName, product.PK);

			clientProductRelationFromDB = OrgPartRelation.ShallowLoadFromDB(Db.Connection, clientProductRelation.PK);
			AssertEquals(0.05m, clientProductRelationFromDB.OU_UnitsPerClientUQ);
		}

		#endregion

		#region HelperFunctions

		void SaveDataWithoutTriggerAndRunProcedure(string sql, string triggerName, Guid productPK)
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(triggerName, OrgPartUnitSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}

			var procedureSql = $@"
DECLARE @OrgSupplierPartPKs dbo.TVP_uniqueidentifier;
INSERT INTO @OrgSupplierPartPKs
SELECT
	OP_PK
FROM
	OrgSupplierPart
WHERE
	OP_PK = '{productPK}'

EXEC {procedureName} @OrgSupplierPartPKs;";

			Db.Connection.ExecuteNonQuery(procedureSql);
		}

		#endregion
	}
}
