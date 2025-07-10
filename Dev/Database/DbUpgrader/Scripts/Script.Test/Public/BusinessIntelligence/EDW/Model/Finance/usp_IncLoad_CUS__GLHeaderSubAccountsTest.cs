using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GLHeaderSubAccounts))]
	internal class usp_IncLoad_CUS__GLHeaderSubAccountsTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertEquals("Columns", 6, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, 0, 0, 0, 1);
				AssertRowValues(resultTable, 2, 1, 1, 0, 1, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int gLAccountKey, int hasSalesExpenseGroupsSubAccount, int hasOrganisationSubAccount, int hasStaffAndResourcesSubAccount, int hasStaffGroupSubAccount, int count)
		{
			var sql = string.Format("GLAccountKey = {0} AND HasSalesExpenseGroupsSubAccount = {1} AND HasOrganisationSubAccount = {2} AND HasStaffAndResourcesSubAccount = {3} AND HasStaffGroupSubAccount = {4}",
				gLAccountKey,
				hasSalesExpenseGroupsSubAccount,
				hasOrganisationSubAccount,
				hasStaffAndResourcesSubAccount,
				hasStaffGroupSubAccount
			);

			var rows = resultTable.Select(sql);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalLoad()
		{
			TestHelper.InsertGLHeaderSubAccount(1, "OH");
			TestHelper.InsertGLHeaderSubAccount(1, "GS");

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[BAS__GLHeaderSubAccount] set SubClass = 'GG' where GLHeaderSubAccountKey = 2
			DELETE FROM [{0}].[Finance].[BAS__GLHeaderSubAccount] where GLHeaderSubAccountKey = 4

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1)
			VALUES
			('Finance', 'BAS__GLHeaderSubAccount', 6, null),
			('Finance', 'BAS__GLHeaderSubAccount', 7, null),
			('Finance', 'BAS__GLHeaderSubAccount', 2, 1),
			('Finance', 'BAS__GLHeaderSubAccount', 4, 2)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertRowValues(resultTable, 1, 1, 1, 1, 1, 1);
			AssertRowValues(resultTable, 2, 1, 1, 0, 0, 1);

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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__GlHeaderSubAccounts]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__GlHeaderSubAccounts'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].Finance.BAS__GLHeaderSubAccount;
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sql);

			TestHelper.InsertGLHeaderSubAccount(1, "AR");
			TestHelper.InsertGLHeaderSubAccount(1, "AT");
			TestHelper.InsertGLHeaderSubAccount(2, "AR");
			TestHelper.InsertGLHeaderSubAccount(2, "GG");
			TestHelper.InsertGLHeaderSubAccount(2, "OH");
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__GlHeaderSubAccounts'",
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
