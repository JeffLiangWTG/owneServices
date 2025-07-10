using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__AlternateGLAccountDissection))]
	internal class usp_IncLoad_CUS__AlternateGLAccountDissectionTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertEquals("Columns", 9, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "1", null, "1", "1", "1", null, 1);
				AssertRowValues(resultTable, 2, 2, null, "0", null, null, null, "0", 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int alternateChartKey, int gLAccountKey, string attribute_LFO, string attribute_TIC, string attribute_ORG, string attribute_OCG, string attribute_LFE, string attribute_SPR, int count)
		{
			var sql = string.Format("AlternateChartKey = {0} AND GLAccountKey = {1} AND Attribute_LFO {2} AND attribute_TIC {3} AND attribute_ORG {4} AND attribute_OCG {5} AND attribute_LFE {6} AND attribute_SPR {7}",
				alternateChartKey,
				gLAccountKey,
				attribute_LFO == null ? "IS NULL" : $@"='{attribute_LFO}'",
				attribute_TIC == null ? "IS NULL" : $@"='{attribute_TIC}'",
				attribute_ORG == null ? "IS NULL" : $@"='{attribute_ORG}'",
				attribute_OCG == null ? "IS NULL" : $@"='{attribute_OCG}'",
				attribute_LFE == null ? "IS NULL" : $@"='{attribute_LFE}'",
				attribute_SPR == null ? "IS NULL" : $@"='{attribute_SPR}'"
			);

			var rows = resultTable.Select(sql);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalLoad()
		{
			TestHelper.InsertAlternateGLAccountDissection(1, 2, "ORG", 1);
			TestHelper.InsertAlternateGLAccountDissection(2, 2, "ORG", 1);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[BAS__AccAlternateGLAccountDissection] set SeparateNumbering =0 where AccAlternateGLAccountDissectionKey = 1
			DELETE FROM [{0}].[Finance].[BAS__AccAlternateGLAccountDissection] where AccAlternateGLAccountDissectionKey = 2

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue)
			VALUES
			('Finance', 'BAS__AccAlternateGLAccountDissection', 7),
			('Finance', 'BAS__AccAlternateGLAccountDissection', 8),
			('Finance', 'BAS__AccAlternateGLAccountDissection', 1),
			('Finance', 'BAS__AccAlternateGLAccountDissection', 2)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 3, resultTable.Rows.Count);
			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "1", null, "0", null, "1", null, 1);
				AssertRowValues(resultTable, 2, 2, null, "0", "1", null, null, "0", 1);
				AssertRowValues(resultTable, 2, 1, null, null, "1", null, null, null, 1);
			});

			var transformedRows = SelectTransformedRows();
			AssertEquals(5, transformedRows.Rows.Count);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NULL").Length);
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__AlternateGLAccountDissection]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__AlternateGLAccountDissection'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].Finance.BAS__AccAlternateGLAccountDissection;
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sql);
			TestHelper.InsertAlternateGLAccountDissection(1, 1, "ORG", 1);
			TestHelper.InsertAlternateGLAccountDissection(1, 1, "OCG", 1);
			TestHelper.InsertAlternateGLAccountDissection(1, 1, "LFO", 1);
			TestHelper.InsertAlternateGLAccountDissection(1, 1, "LFE", 1);
			TestHelper.InsertAlternateGLAccountDissection(2, 2, "TIC", 0);
			TestHelper.InsertAlternateGLAccountDissection(2, 2, "SPR", 0);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__AlternateGLAccountDissection'",
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
