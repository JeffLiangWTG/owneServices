using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[UseSnapshotProtection(new[] { DatabaseType.EDW })]
	[TestedType(typeof(AccMultilingualBalanceSheet))]
	class AccMultilingualBalanceSheetEDWTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestContainUnits()
		{
			PrepareData(new DateTime(2012, 01, 01));
			var result = Exec();

			AssertEquals("Result should have 3 rows", 3, result.Rows.Count);

			var rowSelected = result.Rows.Cast<DataRow>().FirstOrDefault(row => row["AccountNumber"].ToString().StartsWith("2201"));
			AssertResult("", -700, -1200);
			rowSelected = result.Rows.Cast<DataRow>().FirstOrDefault(row => row["AccountNumber"].ToString().StartsWith("2202"));
			AssertResult("KWH", -800, -1400);

			void AssertResult(string units, decimal changeFromPrevious, decimal currentBalance)
			{
				AssertEquals("Units field", units, rowSelected["Units"]);
				AssertEquals("ChangeFromPrevious field", changeFromPrevious, rowSelected["ChangeFromPrevious"]);
				AssertEquals("CurrentBalance field", currentBalance, rowSelected["CurrentBalance"]);
			}
		}

		public void TestPLAppropriationAccount()
		{
			PrepareData(new DateTime(2012, 01, 01));
			var result = Exec();

			AssertEquals(3, result.Rows.Count);

			var rowSelected = result.Rows.Cast<DataRow>().FirstOrDefault(row => row["AccountNumber"].ToString().StartsWith("2205"));
			AssertResult("", -100, -100);

			void AssertResult(string units, decimal changeFromPrevious, decimal currentBalance)
			{
				AssertEquals("Units field", units, rowSelected["Units"]);
				AssertEquals("ChangeFromPrevious field", changeFromPrevious, rowSelected["ChangeFromPrevious"]);
				AssertEquals("CurrentBalance field", currentBalance, rowSelected["CurrentBalance"]);
			}
		}

		public void TestHasValue_WhenPeriodBeforeThanLastProcessedDate()
		{
			PrepareData(new DateTime(2012, 01, 01));
			var result = Exec();

			AssertEquals("Result should have 3 rows", 3, result.Rows.Count);

			var rowSelected = result.Rows.Cast<DataRow>().FirstOrDefault(row => row["AccountNumber"].ToString().StartsWith("2201"));
			AssertResult("", -700, -1200);
			rowSelected = result.Rows.Cast<DataRow>().FirstOrDefault(row => row["AccountNumber"].ToString().StartsWith("2202"));
			AssertResult("KWH", -800, -1400);

			void AssertResult(string units, decimal changeFromPrevious, decimal currentBalance)
			{
				AssertEquals("Units field", units, rowSelected["Units"]);
				AssertEquals("ChangeFromPrevious field", changeFromPrevious, rowSelected["ChangeFromPrevious"]);
				AssertEquals("CurrentBalance field", currentBalance, rowSelected["CurrentBalance"]);
			}
		}

		DataTable Exec()
		{
			var sql = $"SELECT * FROM [{ScriptDbName}].[dbo].[AccMultilingualBalanceSheet](201202, '{TestDbHelper.DefaultCompanyPK}', '0000.00.00', '9999.99.99', '0000.00.00', '9999.99.99', '', '', 'Y')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		void PrepareData(DateTime lastProcessDate)
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertStmData(lastProcessDate);

			Helper.InsertPeriodForInputYear(2012);
			Helper.InsertPeriodForInputYear(2013);

			FunctionTestingHelper.InsertGLAccount(accountNo: "2201.02.01", accountType: "P&L", description: "Test Account 1", units: "");
			FunctionTestingHelper.InsertGLAccount(accountNo: "2202.02.01", accountType: "P&L", description: "Test Account 2", units: "");
			FunctionTestingHelper.InsertGLAccount(accountNo: "2203.02.01", accountType: "P&L", description: "Test Account 3", units: "");
			FunctionTestingHelper.InsertGLAccount(accountNo: "2204.02.01", accountType: "P&L", description: "Test Account 4", units: "KWH");
			FunctionTestingHelper.InsertGLAccount(accountNo: "2205.02.01", accountType: "P&L", description: "Test Account 5", units: "");

			var accountKey3 = GetGLAccountKey("2203.02.01", "Test Account 3");
			var accountKey4 = GetGLAccountKey("2204.02.01", "Test Account 4");
			var accountKey5 = GetGLAccountKey("2205.02.01", "Test Account 5");

			Helper.InsertGLAggregate(amount: 500, period: 201201, glAccountKey: accountKey3);
			Helper.InsertGLAggregate(amount: 600, period: 201201, glAccountKey: accountKey4);
			Helper.InsertGLAggregate(amount: 700, period: 201202, glAccountKey: accountKey3);
			Helper.InsertGLAggregate(amount: 800, period: 201202, glAccountKey: accountKey4);
			Helper.InsertGLAggregate(amount: 100, period: 201202, glAccountKey: accountKey5);

			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 1, gLAccountKey: accountKey3, localAccountNumber: "2201.02.01", debitCredit: "CR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 2, gLAccountKey: accountKey4, localAccountNumber: "2202.02.01", debitCredit: "CR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 3, gLAccountKey: accountKey5, localAccountNumber: "2205.02.01", debitCredit: "CR");
			Helper.InsertAccountDescriptorPivot(gLAccountDescriptorKey: 1, gLAccountKey: accountKey3);
			Helper.InsertAccountDescriptorPivot(gLAccountDescriptorKey: 2, gLAccountKey: accountKey4);
			Helper.InsertAccountDescriptorPivot(gLAccountDescriptorKey: 3, gLAccountKey: accountKey5);
		}

		int GetGLAccountKey(string accountNo, string desc)
		{
			var sql = $"SELECT GLAccountKey FROM [{ScriptDbName}].[Finance].[BAS__GLAccount] WHERE AccountNo = '{accountNo}' AND Description = '{desc}'";
			return Convert.ToInt32(TestConnection.Command(sql).ExecuteScalar());
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;

		AccountingFunctionTestingHelper FunctionTestingHelper => accountingFunctionTestingHelper ?? (accountingFunctionTestingHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper accountingFunctionTestingHelper;
	}
}
