using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	abstract class Report_ProfitandLossMovementsbyAccountBaseTest : BaseDbScriptTest
	{
		public void TestTransactionCategory()
		{
			var accGLHeaderPK = Guid.NewGuid();
			CreateAccGLHeader(accGLHeaderPK, "1330.20.30", "PNL Account 1", "P&L", string.Empty, "CR", true, 0, "TS");
			var accGLHeader = new TestParameter(string.Empty, 1, accGLHeaderPK);

			CreateAccountGLAggregate(100, 202001, accGLHeader, Branch, Company, Department, "AAA");
			CreateAccountGLAggregate(200, 202002, accGLHeader, Branch, Company, Department, "BBB");
			CreateAccountGLAggregate(300, 202003, accGLHeader, Branch, Company, Department, "CCC");
			CreateAccountGLAggregate(400, 202004, accGLHeader, Branch, Company, Department);
			CreateAccountGLAggregate(500, 202005, accGLHeader, Branch, Company, Department);

			var result = Exec(Company.PK, 202001, 202012, "AAA");
			AssertEquals("Should only find 3 account that matches criteria", 3, result.Rows.Count);
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202001'"), 100, "AAA");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202004'"), 400, "");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202005'"), 500, "");

			result = Exec(Company.PK, 202001, 202012, "BBB");
			AssertEquals("Should only find 3 account that matches criteria", 3, result.Rows.Count);
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202002'"), 200, "BBB");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202004'"), 400, "");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202005'"), 500, "");

			result = Exec(Company.PK, 202001, 202012, "CCC");
			AssertEquals("Should only find 3 account that matches criteria", 3, result.Rows.Count);
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202003'"), 300, "CCC");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202004'"), 400, "");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202005'"), 500, "");

			result = Exec(Company.PK, 202001, 202012, "");
			AssertEquals("Should only find 2 account that matches criteria", 2, result.Rows.Count);
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202004'"), 400, "");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202005'"), 500, "");

			result = Exec(Company.PK, 202001, 202012, "AAA,BBB");
			AssertEquals("Should only find 4 account that matches criteria", 4, result.Rows.Count);
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202001'"), 100, "AAA");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202002'"), 200, "BBB");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202004'"), 400, "");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202005'"), 500, "");

			result = Exec(Company.PK, 202001, 202012, "AAA,BBB,CCC");
			AssertEquals("Should only find 5 account that matches criteria", 5, result.Rows.Count);
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202001'"), 100, "AAA");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202002'"), 200, "BBB");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202003'"), 300, "CCC");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202004'"), 400, "");
			AssertSelectRow(result.Select("Account = '1330.20.30' AND Period = '202005'"), 500, "");

			void AssertSelectRow(DataRow[] selectRows, decimal periodAmount, string category)
			{
				AssertEquals("Should only find 1 account that matches criteria", 1, selectRows.Length);
				AssertEquals($"Period amount should be {periodAmount}", periodAmount, (decimal)selectRows[0]["PeriodAmount"]);
				AssertEquals($"Transaction category should be {category}", category, selectRows[0]["TransactionCategory"]);
			}
		}

		public void TestContainUnits()
		{
			var accGLHeaderPK1 = Guid.NewGuid();
			CreateAccGLHeader(accGLHeaderPK1, "1330.20.30", "PNL Account 1", "P&L", string.Empty, "CR", true, 0, "TS", 1);

			var accGLHeaderPK2 = Guid.NewGuid();
			CreateAccGLHeader(accGLHeaderPK2, "2330.20.30", "NTE Account 1", "NTE", "KWH", "CR", true, 0, "TS", 2);

			var accGLHeader1 = new TestParameter(string.Empty, 1, accGLHeaderPK1);
			var accGLHeader2 = new TestParameter(string.Empty, 2, accGLHeaderPK2);

			CreateAccountGLAggregate(100, 202001, accGLHeader2, Branch, Company, Department, "AAA");
			CreateAccountGLAggregate(200, 202002, accGLHeader2, Branch, Company, Department);
			CreateAccountGLAggregate(300, 202003, accGLHeader1, Branch, Company, Department);

			var result = Exec(Company.PK, 202001, 202012, "");
			AssertEquals("Should only find 2 account that matches criteria", 2, result.Rows.Count);

			var rowselected = result.Select("Account = '1330.20.30' AND Period = '202003'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rowselected.Length);
			AssertEquals(300M, (decimal)rowselected[0]["PeriodAmount"]);
			AssertEquals("", rowselected[0]["Units"].ToString());

			rowselected = result.Select("Account = '2330.20.30' AND Period = '202002'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rowselected.Length);
			AssertEquals(200M, (decimal)rowselected[0]["PeriodAmount"]);
			AssertEquals("KWH", rowselected[0]["Units"].ToString());
		}

		DataTable Exec(Guid companyPK, int periodFrom, int periodTo, string transactionCategory)
		{
			var sql = $@"Select * From [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}](@companyPK, @periodFrom, @periodTo, @transactionCategory)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@periodFrom", SqlDbType.Int, periodFrom);
				command.AddParameter("@periodTo", SqlDbType.Int, periodTo);
				command.AddParameter("@transactionCategory", SqlDbType.VarChar, transactionCategory);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Company = new TestParameter("TST", 1, Guid.Empty);
			Company.PK = CreateCompany(Company, "CA", "CNY");

			Branch = new TestParameter("TB1", 1, Guid.Empty);
			var homePort = new TestParameter("CAYVR", 1, Guid.Empty);
			Branch.PK = CreateBranch(Company, Branch, homePort);

			Department = new TestParameter("TD1", 1, Guid.Empty);
			Department.PK = CreateDepartment(Department);
		}

		TestParameter Company;
		TestParameter Branch;
		TestParameter Department;

		public abstract void CreateAccGLHeader(Guid pk, string accountNum, string description, string accountType, string statisticalUnits, string debitCredit, bool controlAccount, int totalLevel, string column, long gLAccoutKey = 1);

		public abstract void CreateAccountGLAggregate(decimal amount, int period, TestParameter account, TestParameter branch, TestParameter company, TestParameter department, string transactionCategory = "");

		public abstract Guid CreateCompany(TestParameter companyParameter, string countryCode, string currencyCode);

		public abstract Guid CreateBranch(TestParameter companyParameter, TestParameter branchParameter, TestParameter homePortParameter);

		public abstract Guid CreateDepartment(TestParameter departmentParameter);

		public struct TestParameter
		{
			public TestParameter(string code, long key, Guid pk)
			{
				PK = pk;
				Code = code;
				EDWKey = key;
			}

			public Guid PK;
			public string Code;
			public long EDWKey;
		}
	}
}
