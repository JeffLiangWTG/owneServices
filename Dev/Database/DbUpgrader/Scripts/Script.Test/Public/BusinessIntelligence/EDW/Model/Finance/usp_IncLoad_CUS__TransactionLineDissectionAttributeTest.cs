using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__TransactionLineDissectionAttribute))]
	internal class usp_IncLoad_CUS__TransactionLineDissectionAttributeTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertEquals("Columns", 9, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "O1", "EUU", "OEU", "LOC", "SPS", "ETI", 1);
				AssertRowValues(resultTable, 2, 2, "O2", null, null, "EUT", null, null, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int lineKey, int? organizationKey, string attribute_org, string attribute_ocg, string attribute_lfe, string attribute_lfo, string attribute_spr, string attribute_tic, int count)
		{
			var sql = string.Format("AccGLTransactionLineKey = {0} AND OrganizationKey {1} AND Attribute_ORG {2} AND Attribute_OCG {3} AND Attribute_LFE {4} AND Attribute_LFO {5} AND Attribute_TIC {6} AND Attribute_SPR {7}",
				lineKey,
				organizationKey == null ? "IS NULL" : "=" + organizationKey,
				attribute_org == null ? "IS NULL" : "='" + attribute_org + "'",
				attribute_ocg == null ? "IS NULL" : "='" + attribute_ocg + "'",
				attribute_lfe == null ? "IS NULL" : "='" + attribute_lfe + "'",
				attribute_lfo == null ? "IS NULL" : "='" + attribute_lfo + "'",
				attribute_tic == null ? "IS NULL" : "='" + attribute_tic + "'",
				attribute_spr == null ? "IS NULL" : "='" + attribute_spr + "'"
			);

			var rows = resultTable.Select(sql);
			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncremantalLoadLineDissectionAttributes()
		{
			TestHelper.InsertTransactionLineDissectionAttribute(3, "OCG", "UUU", null);
			TestHelper.InsertTransactionLineDissectionAttribute(2, "SPR", "SPR", null);
			var sql = string.Format(
				@"DELETE FROM {0}.[Finance].[BAS__TransactionLineDissectionAttribute] where TransactionLineDissectionAttributeKey = 8;
				UPDATE {0}.[Finance].[BAS__TransactionLineDissectionAttribute] set AttributeValue = 'UYU' where TransactionLineDissectionAttributeKey = 2;
				INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1)
				VALUES
				('Finance', 'BAS__TransactionLineDissectionAttribute', 9, null),
				('Finance', 'BAS__TransactionLineDissectionAttribute', 10, null),
				('Finance', 'BAS__TransactionLineDissectionAttribute', 8, 2),
				('Finance', 'BAS__TransactionLineDissectionAttribute', 2, 1)
				", ScriptDbName);
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 3, resultTable.Rows.Count);
			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "O1", "UYU", "OEU", "LOC", "SPS", "ETI", 1);
				AssertRowValues(resultTable, 2, 2, "O2", null, null, null, "SPR", null, 1);
				AssertRowValues(resultTable, 3, null, null, "UUU", null, null, null, null, 1);
			});

			var transformedRows = SelectTransformedRows();
			AssertEquals(5, transformedRows.Rows.Count);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncremantalLoadOrganization()
		{
			var sql = string.Format(
				@"UPDATE {0}.Organization.BAS__Organization set Code = 'O4' WHERE OrganizationKey = 1;
				DELETE FROM {0}.Organization.BAS__Organization WHERE OrganizationKey = 2;
				INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue, RefValue1)
				VALUES
				('Organization', 'BAS__Organization', 1, newid(), 'O1'),
				('Organization', 'BAS__Organization', 2, newid(), 'O2')
				", ScriptDbName);
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "O4", "EUU", "OEU", "LOC", "SPS", "ETI", 1);
				AssertRowValues(resultTable, 2, null, null, null, null, "EUT", null, null, 1);
			});

			var transformedRows = SelectTransformedRows();
			AssertEquals(4, transformedRows.Rows.Count);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTestData();
			ExecuteTableLoad("InitialLoadQuery");
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__TransactionLineDissectionAttribute]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__TransactionLineDissectionAttribute'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery($"DELETE FROM {ScriptDbName}.Organization.BAS__Organization");
			var orgID1 = Guid.NewGuid();
			var orgID2 = Guid.NewGuid();
			TestHelper.InsertOrganization("O1", orgID1);
			TestHelper.InsertOrganization("O2", orgID2);
			TestHelper.InsertTransactionLineDissectionAttribute(1, "ORG", null, orgID1);
			TestHelper.InsertTransactionLineDissectionAttribute(1, "OCG", "EUU", null);
			TestHelper.InsertTransactionLineDissectionAttribute(1, "LFO", "LOC", null);
			TestHelper.InsertTransactionLineDissectionAttribute(1, "LFE", "OEU", null);
			TestHelper.InsertTransactionLineDissectionAttribute(1, "TIC", "ETI", null);
			TestHelper.InsertTransactionLineDissectionAttribute(1, "SPR", "SPS", null);

			TestHelper.InsertTransactionLineDissectionAttribute(2, "ORG", null, orgID2);
			TestHelper.InsertTransactionLineDissectionAttribute(2, "LFO", "EUT", null);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__TransactionLineDissectionAttribute'",
				sqlName, ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
