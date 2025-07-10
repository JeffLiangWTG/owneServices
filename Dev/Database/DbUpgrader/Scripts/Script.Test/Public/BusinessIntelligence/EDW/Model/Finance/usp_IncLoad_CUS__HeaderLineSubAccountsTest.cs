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
	[TestedType(typeof(vw_CUS__HeaderLineSubAccounts))]
	internal class usp_IncLoad_CUS__HeaderLineSubAccountsTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertEquals("Columns", 11, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, "L", groupsID, orgID, null, null, "ORG", null, null, "GGG", 1);
				AssertRowValues(resultTable, 2, "H", null, null, staffID, groupID, null, "STG", "GRU", null, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int parentKey, string parentTableCode, Guid? salesExpenseGroupsID, Guid? organizationID, Guid? staffID, Guid? staffGroupID, string orgCode, string staffCode, string groupCode, string groupsCode, int count)
		{
			var sql = string.Format("parentKey = {0} AND parentTableCode = '{1}' AND SalesExpenseGroupsID {2} AND OrganizationID {3} AND StaffID {4} AND StaffGroupID {5} AND OrganisationSubAccount {6} AND StaffAndResourcesSubAccount {7} AND StaffGroupSubAccount {8} AND SalesExpenseGroupsSubAccount {9}",
				parentKey,
				parentTableCode,
				salesExpenseGroupsID == null ? "IS NULL" : $@"='{salesExpenseGroupsID}'",
				organizationID == null ? "IS NULL" : $@"='{organizationID}'",
				staffID == null ? "IS NULL" : $@"='{staffID}'",
				staffGroupID == null ? "IS NULL" : $@"='{staffGroupID}'",
				orgCode == null ? "IS NULL" : $@"='{orgCode}'",
				staffCode == null ? "IS NULL" : $@"='{staffCode}'",
				groupCode == null ? "IS NULL" : $@"='{groupCode}'",
				groupsCode == null ? "IS NULL" : $@"='{groupsCode}'"
			);

			var rows = resultTable.Select(sql);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalLoadHeaderLineSubAccount()
		{
			TestHelper.InsertTransactionLineSubAccount(1, "GS", staffID);
			TestHelper.InsertTransactionHeaderSubAccount(2, "OH", orgID);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[BAS__TransactionHeaderSubAccount] set subclassParentTableCode = 'GF' where TransactionHeaderSubAccountKey = 2
			DELETE FROM [{0}].[Finance].[BAS__TransactionLineSubAccount] where TransactionLineSubAccountKey = 1

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1)
			VALUES
			('Finance', 'BAS__TransactionLineSubAccount', 1, 1),
			('Finance', 'BAS__TransactionLineSubAccount', 3, null),
			('Finance', 'BAS__TransactionHeaderSubAccount', 4, null),
			('Finance', 'BAS__TransactionHeaderSubAccount', 2, 2)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertRowValues(resultTable, 1, "L", null, orgID, staffID, null, "ORG", "STG", null, null, 1);
			AssertRowValues(resultTable, 2, "H", null, orgID, null, groupID, "ORG", null, "GRU", null, 1);

			var transformedRows = SelectTransformedRows();
			AssertEquals(4, transformedRows.Rows.Count);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalLoadOrganization()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
			UPDATE [{0}].[Organization].[BAS__Organization] set code = 'OOU'
			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1, PKValue)
			VALUES
			('Organization', 'BAS__Organization', 1, 'ORG', '{1}')
			", ScriptDbName, orgID);
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertRowValues(resultTable, 1, "L", groupsID, orgID, null, null, "OOU", null, null, "GGG", 1);
		}

		public void TestIncrementalLoadGroups()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
			UPDATE [{0}].[Finance].[BAS__Groups] set code = 'OOU'
			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1, PKValue)
			VALUES
			('Finance', 'BAS__Groups', 1, 'ORG', '{1}')
			", ScriptDbName, groupsID);
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertRowValues(resultTable, 1, "L", groupsID, orgID, null, null, "ORG", null, null, "OOU", 1);
		}

		public void TestIncrementalLoadGroup()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
			UPDATE [{0}].[Organization].[BAS__Group] set code = 'OOU'
			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1, PKValue)
			VALUES
			('Organization', 'BAS__Group', 1, 'ORG', '{1}')
			", ScriptDbName, groupID);
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertRowValues(resultTable, 2, "H", null, null, staffID, groupID, null, "STG", "OOU", null, 1);
		}

		public void TestIncrementalLoadStaff()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
			UPDATE [{0}].[Organization].[BAS__Staff] set code = 'OOU'
			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue2, PKValue)
			VALUES
			('Organization', 'BAS__Staff', 1, 'ORG', '{1}')
			", ScriptDbName, staffID);
			TestConnection.ExecuteNonQuery(sql);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);
			AssertRowValues(resultTable, 2, "H", null, null, staffID, groupID, null, "OOU", "GRU", null, 1);
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__HeaderLineSubAccounts]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__HeaderLineSubAccounts'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].Finance.BAS__TransactionLineSubAccount;
			DELETE FROM [{0}].Finance.BAS__TransactionHeaderSubAccount;
			DELETE FROM [{0}].Organization.BAS__Organization;
			DELETE FROM [{0}].Finance.BAS__Groups;
			DELETE FROM [{0}].Organization.BAS__Staff;
			DELETE FROM [{0}].Organization.BAS__Group;
			INSERT INTO [{0}].Organization.BAS__Organization(OrganizationKey, OrganizationID, Code)
					VALUES
			(1, '{1}', 'ORG')
			INSERT INTO [{0}].Organization.BAS__Staff(StaffKey, StaffID, Code)
					VALUES
			(1, '{2}', 'STG')
			INSERT INTO [{0}].Organization.BAS__Group(GroupKey, GroupID, Code)
					VALUES
			(1, '{3}', 'GRU')
			INSERT INTO [{0}].Finance.BAS__Groups(GroupsKey, GroupsID, Code)
					VALUES
			(1, '{4}', 'GGG')
			", ScriptDbName, orgID, staffID, groupID, groupsID);
			TestConnection.ExecuteNonQuery(sql);

			TestHelper.InsertTransactionLineSubAccount(1, "AR", groupsID);
			TestHelper.InsertTransactionLineSubAccount(1, "OH", orgID);
			TestHelper.InsertTransactionHeaderSubAccount(2, "GG", groupID);
			TestHelper.InsertTransactionHeaderSubAccount(2, "GS", staffID);
			TestHelper.InsertTransactionHeaderSubAccount(2, "GX", staffID);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__HeaderLineSubAccounts'",
				sqlName, ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		readonly Guid orgID = Guid.NewGuid();
		readonly Guid staffID = Guid.NewGuid();
		readonly Guid groupID = Guid.NewGuid();
		readonly Guid groupsID = Guid.NewGuid();

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
