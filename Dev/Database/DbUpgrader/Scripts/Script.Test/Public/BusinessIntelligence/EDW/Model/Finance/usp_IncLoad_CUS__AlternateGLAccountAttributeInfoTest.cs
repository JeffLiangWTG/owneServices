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
	[TestedType(typeof(vw_CUS__AlternateGLAccountAttributeInfo))]
	internal class usp_IncLoad_CUS__AlternateGLAccountAttributeInfoTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);
			AssertEquals("Columns", 16, resultTable.Columns.Count);

			AssertRowValues(resultTable, 1, 1, "ETB", "ETA", "UUU", "TPY", "ETT", "ETY", 1);
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

		public void TestIncrementalLoadGLAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[BAS__GLAccount] set CashFlowType = 'TTT', Units = 'KG', AccountNo = '887' where GLAccountKey = 1

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue)
			VALUES
			('Finance', 'BAS__GLAccount', 1)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);
			AssertEquals("Data changes", 1, resultTable.Select("CashFlowType = 'TTT' and Units = 'KG' and ParentGLAccountNo = '887'").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(2, transformedRows.Rows.Count);
			AssertEquals(1, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(1, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalLoadAlternateGLAccountAttribute()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			DELETE FROM [{0}].[Finance].[BAS__AlternateGLAccountAttribute] WHERE AlternateGLAccountAttributeKey = 2
			UPDATE [{0}].[Finance].[BAS__AlternateGLAccountAttribute] set AttributeValue = 'TTT' where AlternateGLAccountAttributeKey = 3

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue2)
			VALUES
			('Finance', 'BAS__AlternateGLAccountAttribute', 3, 1),
			('Finance', 'BAS__AlternateGLAccountAttribute', 2, 1)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);
			AssertEquals("Data changes", 1, resultTable.Select("Attribute_OCG = '' and Attribute_TIC = 'TTT'").Length);
		}

		public void TestIncrementalLoadAlternateOrganization()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Organization].[BAS__Organization] set Code = 'TTT' where OrganizationKey = 1

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1)
			VALUES
			('Organization', 'BAS__Organization', 1, 'yyy')
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);
			AssertEquals("Data changes", 1, resultTable.Select("Attribute_ORG = 'TTT'").Length);
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__AlternateGLAccountAttributeInfo]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__AlternateGLAccountAttributeInfo'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM [{0}].[Organization].[BAS__Organization]
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
			TestHelper.InsertGLAccount("1.2.3.444", units: "UU");

			var organizationID = Guid.NewGuid();
			TestHelper.InsertOrganization("UUU", organizationID);
			TestHelper.InsertAlternateAccountAttribute(1, 1, "ORG", null, 1, 1, organizationID);
			TestHelper.InsertAlternateAccountAttribute(1, 1, "OCG", "TPY", 1, 1, organizationID);
			TestHelper.InsertAlternateAccountAttribute(1, 1, "TIC", "ETA", 1, 1, organizationID);
			TestHelper.InsertAlternateAccountAttribute(1, 1, "LFE", "ETT", 1, 1, organizationID);
			TestHelper.InsertAlternateAccountAttribute(1, 1, "LFO", "ETB", 1, 1, organizationID);
			TestHelper.InsertAlternateAccountAttribute(1, 1, "SPR", "ETY", 1, 1, organizationID);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__AlternateGLAccountAttributeInfo'",
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
