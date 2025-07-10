using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__OrganizationCompanyData))]
	internal class usp_IncLoad_CUS__OrganizationCompanyDataTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 3, resultTable.Rows.Count);
			AssertEquals("Columns", 5, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 2, 3, "INT", "E", 1);
				AssertRowValues(resultTable, 1, 2, "INT", "E", 1);
				AssertRowValues(resultTable, 1, 1, "TPY", "U", 1);
			});
		}

		public void TestIncrementalLoadCompanyData()
		{
			TestHelper.InsertOrgCompanyData(3, 4, "E");
			var sql = string.Format(@$"
			DELETE [{ScriptDbName}].[Organization].[BAS__OrganizationCompanyData] where OrganizationCompanyDataKey = 1;
			INSERT INTO [{ScriptDbName}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1)
			VALUES
			('Organization', 'BAS__OrganizationCompanyData', 1, 1),
			('Organization', 'BAS__OrganizationCompanyData', 4, null),
			('Organization', 'BAS__OrganizationCompanyData', 5, 4)
			");
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 3, resultTable.Rows.Count);
			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 2, "INT", "E", 1);
				AssertRowValues(resultTable, 2, 3, "INT", "E", 1);
				AssertRowValues(resultTable, 4, 3, "INT", "E", 1);
			});

			var transformedRows = SelectTransformedRows();
			AssertEquals(4, transformedRows.Rows.Count);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalCategory()
		{
			var sql = string.Format(@$"
			UPDATE [{ScriptDbName}].[Finance].[GRP__ConsolidatedAccountingCategory] set Code = 'II' where Code = 'E';
			UPDATE [{ScriptDbName}].[Finance].[GRP__ConsolidatedAccountingCategory] set Class = 'IIU' where Code = 'U';
			INSERT INTO [{ScriptDbName}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1)
			VALUES
			('Finance', 'GRP__ConsolidatedAccountingCategory', 1, 'E'),
			('Finance', 'GRP__ConsolidatedAccountingCategory', 4, 'U')
			");
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);
			AssertRowValues(resultTable, 1, 1, "IIU", "U", 1);

			var transformedRows = SelectTransformedRows();
			AssertEquals(4, transformedRows.Rows.Count);
			AssertEquals(1, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		void AssertRowValues(DataTable resultTable, int organizationKey, int companykey, string attribute_ocg, string category, int count)
		{
			var sql = string.Format("OrganizationKey = {0} AND CompanyKey = {1} AND Attribute_OCG = '{2}' AND ConsolidatedAccountingCategory = '{3}'",
				organizationKey,
				companykey,
				attribute_ocg,
				category
			);

			var rows = resultTable.Select(sql);

			AssertEquals($"Row count should be {count}", count, rows.Length);
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__OrganizationCompanyData]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__OrganizationCompanyData'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			TestHelper.InsertConsolidatedAccountingCategory("U");
			TestHelper.InsertConsolidatedAccountingCategory("E", "INT");
			TestHelper.InsertOrgCompanyData(1, 1, "U");
			TestHelper.InsertOrgCompanyData(2, 1, "E");
			TestHelper.InsertOrgCompanyData(3, 2, "E");
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__OrganizationCompanyData'",
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
