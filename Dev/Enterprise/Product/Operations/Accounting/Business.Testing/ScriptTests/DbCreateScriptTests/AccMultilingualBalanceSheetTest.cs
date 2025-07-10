using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests
{
	[UseSnapshotProtection(new[] { DatabaseType.Main })]
	class AccMultilingualBalanceSheetTest : ScriptTest
	{
		protected string ScriptDbName => Db.DatabaseName;

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
			TestObjectCreator.InsertStmData("JournalEntriesLastProcessedDate", TestDbHelper.DefaultCompanyPK, lastProcessDate.ToString());

			TestObjectCreator.CreateTestPeriodsForEntireYear(2012);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2013);

			var accountPK = TestObjectCreator.InsertGLHeader("2201.02.01", "Test Account 1", "P&L", "");
			var accountPK1 = TestObjectCreator.InsertGLHeader("2202.02.01", "Test Account 2", "P&L", "");
			var accountPK2 = TestObjectCreator.InsertGLHeader("2203.02.01", "Test Account 3", "P&L", "");
			var accountPK3 = TestObjectCreator.InsertGLHeader("2204.02.01", "Test Account 4", "P&L", "KWH");
			var accountPK4 = TestObjectCreator.InsertGLHeader("2205.02.01", "Test Account 5", "P&L", "");

			TestObjectCreator.CreateGLAggregate("", 5, 100, 201201, accountPK2, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 6, 100, 201201, accountPK3, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 7, 100, 201202, accountPK2, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 8, 100, 201202, accountPK3, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 1, 100, 201202, accountPK4, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var account2 = Factory.Load<AccGLHeader>(accountPK2);
			var account3 = Factory.Load<AccGLHeader>(accountPK3);
			var account4 = Factory.Load<AccGLHeader>(accountPK4);

			TestObjectCreator.CreateAccountDescriptor(account2, "2201.02.01", AccGLAccountDescriptor.ReportTypeCOA, "", "", "Test", "", "CR");
			TestObjectCreator.CreateAccountDescriptor(account3, "2202.02.01", AccGLAccountDescriptor.ReportTypeCOA, "", "", "Test", "", "CR");
			TestObjectCreator.CreateAccountDescriptor(account4, "2205.02.01", AccGLAccountDescriptor.ReportTypeCOA, "", "", "Test_PL", "", "CR");

			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK4);

			Factory.Save();
		}
	}
}
