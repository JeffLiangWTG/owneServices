using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetGLAggregate))]
	class GetGLAggregateTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestGetGLAggregateWithGLD()
		{
			var hashedValuesTable = new DataTable();
			hashedValuesTable.Columns.Add("Value", typeof(long));
			hashedValuesTable.Rows.Add(1);
			hashedValuesTable.Rows.Add(2);

			PrepareTestData();

			TestHelper.CreateBASBranch(1, Guid.NewGuid(), companyKey: 1, code: "SYN");
			TestHelper.CreateBASBranch(2, Guid.NewGuid(), companyKey: 1, code: "JRO");
			TestHelper.CreateBASDepartment(1, Guid.NewGuid(), code: "AU1");
			TestHelper.CreateBASDepartment(2, Guid.NewGuid(), code: "AU2");

			var sql = $@"use [{ScriptDbName}]
			SELECT * FROM [dbo].[GetGLAggregate](1, 'SYN,JRO', 'AU1,AU2')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			var countGLAccount1 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("1") select row).Count();
			var countGLAccount2 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("2") select row).Count();
			var countGLAccount3 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("3") select row).Count();
			var countGLAccount4 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("4") select row).Count();
			var countGLAccount5 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("5") select row).Count();
			var countGLAccount6 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("6") select row).Count();
			var countGLAccount7 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("7") select row).Count();
			var countGLAccount8 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("8") select row).Count();
			var countGLAccount9 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("9") select row).Count();
			var countGLAccount10 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("10") select row).Count();
			var countGLAccount11 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("11") select row).Count();
			var countGLAccount12 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("12") select row).Count();
			var countGLAccount13 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("13") select row).Count();
			var countGLAccount14 = (from DataRow row in result.Rows where row["GLAccountKey"].ToString().Equals("14") select row).Count();

			AssertEquals(1, countGLAccount1);
			AssertEquals(1, countGLAccount2);
			AssertEquals(0, countGLAccount3);
			AssertEquals(0, countGLAccount4);
			AssertEquals(1, countGLAccount5);
			AssertEquals(1, countGLAccount6);
			AssertEquals(1, countGLAccount7);
			AssertEquals(0, countGLAccount8);
			AssertEquals(0, countGLAccount9);
			AssertEquals(0, countGLAccount10);
			AssertEquals(1, countGLAccount11);
			AssertEquals(0, countGLAccount12);
			AssertEquals(0, countGLAccount13);
			AssertEquals(0, countGLAccount14);

			TestHelper.DeleteStmDataDate("JournalEntriesLastProcessedDate", 1);
			var resultWithoutLastProcessedDate = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(4, resultWithoutLastProcessedDate.Rows.Count);
		}

		void PrepareTestData()
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
				VALUES
					(1, newid(), 'AUD', 'DAU', 1, 1);";

			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.InsertPeriodForInputYear(2023);
			TestHelper.InsertStmDataDate("JournalEntriesLastProcessedDate", new DateTime(2023, 2, 1), 1);
			TestHelper.InsertGLAggregate(100, 201808, 1);
			TestHelper.InsertGLAggregate(200, 202301, 2);
			TestHelper.InsertGLAggregate(200, 202301, 2);
			TestHelper.InsertGLAggregate(300, 202302, 3);

			TestHelper.InsertGLAggregateForGLD(400, 202301, 4);
			TestHelper.InsertGLAggregateForGLD(500, 202302, 5);
			TestHelper.InsertGLAggregateForGLD(600, 202303, 6);

			TestHelper.InsertGLAggregate(300, 201808, 7, branchKey: 2, departmentKey: 2);
			TestHelper.InsertGLAggregate(300, 201808, 8, branchKey: 3, departmentKey: 3);
			TestHelper.InsertGLAggregate(300, 201808, 9, branchKey: 2, departmentKey: 3);
			TestHelper.InsertGLAggregate(300, 201808, 10, branchKey: 3, departmentKey: 2);
			TestHelper.InsertGLAggregateForGLD(600, 202303, 11, branchKey: 2, departmentKey: 2);
			TestHelper.InsertGLAggregateForGLD(600, 202303, 12, branchKey: 3, departmentKey: 3);
			TestHelper.InsertGLAggregateForGLD(600, 202303, 13, branchKey: 2, departmentKey: 3);
			TestHelper.InsertGLAggregateForGLD(600, 202303, 14, branchKey: 3, departmentKey: 2);
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
