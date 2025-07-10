using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(Report_TrialBalanceListofMovementsbyAccountPeriodBranchandDepartment))]
	class Report_TrialBalanceListofMovementsbyAccountPeriodBranchandDepartmentTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestTransactionCategoryGroup()
		{
			PrepareTestData();

			var result = Exec(companyPK, "202301", "202311", branchPK.ToString(), departmentPK.ToString(), "A1");

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertDateRow(result.Select("TransactionCategory = 'A1'")[0], "1000.10.10", -1m);

			result = Exec(companyPK, "202301", "202311", branchPK.ToString(), departmentPK.ToString(), "A1, A2");

			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			AssertDateRow(result.Select("TransactionCategory = 'A1'")[0], "1000.10.10", -1m);
			AssertDateRow(result.Select("TransactionCategory = 'A2'")[0], "2000.10.10", -2m);
		}

		public void TestDifferentDepartmentAndBranch()
		{
			PrepareTestData();

			var branchKey2 = 2;
			var departmentKey2 = 2;
			var gLAccountKey3 = 3;
			var branchPK = Guid.NewGuid();
			var departmentPK = Guid.NewGuid();

			CreateBASBranch(branchKey2, branchPK, currentCompanyKey);
			CreateBASDepartment(departmentKey2, departmentPK);
			CreateBASGLAccount(gLAccountKey3, "3000.10.10", "P&L");
			CreateGRPGeneralLedgerAggregateData(gLAccountKey3, 202311, 3m, 0, currentCompanyKey, departmentKey2, branchKey2);

			var result = Exec(companyPK, "202301", "202311", string.Empty, string.Empty, "A1,A2");

			AssertEquals("Result should have 3 rows", 3, result.Rows.Count);

			result = Exec(companyPK, "202301", "202311", branchPK.ToString(), string.Empty, "A1,A2");

			AssertEquals(result.Rows[0].Field<string>("Activity"), "Gateway");
			AssertEquals(result.Rows[0].Field<string>("Direction"), "Export");
			AssertEquals(result.Rows[0].Field<string>("Mode"), "Sea");

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertDateRow(result.Select("Account = '3000.10.10'")[0], "3000.10.10", -3m);

			result = Exec(companyPK, "202301", "202311", string.Empty, departmentPK.ToString(), "A1,A2");

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertDateRow(result.Select("Account = '3000.10.10'")[0], "3000.10.10", -3m);
		}

		public void TestCheckLastProcessedDate()
		{
			PrepareTestData();
			CreateGRPGeneralLedgerAggregateData(1, 202211, 1m, 0, currentCompanyKey, 1, 1, "A1");
			InsertPeriodForInputYear(2022);
			var helper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName);
			helper.InsertGLAggregate(2m, 202211, 1, presentationCategory: "A1");

			var result = Exec(companyPK, "202201", "202212", branchPK.ToString(), departmentPK.ToString(), "A1");

			AssertEquals("Result should have 1 row and data comes from BAS__GLAggregate", 1, result.Select("PeriodAmount = 2").Length);

			var lastProcessedDate = "2022-11-01 00:00:00.000";
			var sql = $@"UPDATE [{ScriptDbName}].[Finance].[BAS__StmDataDate] SET Value='{lastProcessedDate}' WHERE CompanyId = '{companyPK}' AND Name = 'JournalEntriesLastProcessedDate'";
			TestConnection.ExecuteNonQuery(sql);

			result = Exec(companyPK, "202201", "202212", branchPK.ToString(), departmentPK.ToString(), "A1");

			AssertEquals("Result should have 1 row and data comes from GRP__GeneralLedgerAggregateData", 1, result.Select("PeriodAmount = -1").Length);
		}

		void AssertDateRow(DataRow row, string account, decimal amount)
		{
			AssertEquals(account, row["Account"]);
			AssertEquals(amount, row["PeriodAmount"]);
		}

		void PrepareTestData()
		{
			var gLAccountKey1 = 1;
			var gLAccountKey2 = 2;
			var currentBranchKey = 1;
			var currentDepartmentKey = 1;

			CreateBASCompany(currentCompanyKey, companyPK);
			CreateBASBranch(currentBranchKey, branchPK, currentCompanyKey);
			CreateBASDepartment(currentDepartmentKey, departmentPK);
			CreateBASGLAccount(gLAccountKey1, "1000.10.10", "P&L");
			CreateBASGLAccount(gLAccountKey2, "2000.10.10", "BSH");
			CreateGRPGeneralLedgerAggregateData(gLAccountKey1, 202311, 1m, 0, currentCompanyKey, currentDepartmentKey, currentBranchKey, "A1");
			CreateGRPGeneralLedgerAggregateData(gLAccountKey2, 202311, 2m, 0, currentCompanyKey, currentDepartmentKey, currentBranchKey, "A2");

			InsertPeriodForInputYear(2023);
			CreateBASStmDataDate("JournalEntriesLastProcessedDate", "2023-01-01 00:00:00.000", companyPK, currentCompanyKey);
			CreateBASCurrency();
			CreateBASAccount("GL_AR_CONTROL_ACCOUNT", 100);
			CreateBASAccount("GL_AP_CONTROL_ACCOUNT", 200);
		}

		void InsertPeriodForInputYear(int year)
		{
			var sqlText = new StringBuilder();
			for (var i = 1; i <= 12; i++)
			{
				sqlText.AppendLine(GetInsertAccPeriodScript(year, i));
			}

			TestConnection.ExecuteNonQuery(sqlText.ToString());
		}

		string GetInsertAccPeriodScript(int year, int month)
		{
			var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
			var endDate = new DateTime(year, month, 1).AddMonths(1).AddMinutes(-1).ToString("yyyy-MM-dd");
			return $@"
					INSERT {ScriptDbName}.Finance.BAS__PeriodManagement
						([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '{startDate}', '{endDate}', {year * 100 + month}, {year});";
		}

		void CreateBASStmDataDate(string name, string value, Guid companyID, int companyKey)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
					VALUES
						(1, newid(), '{name}', '{companyID}', '{value}', '{companyKey}');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateBASGLAccount(int gLAccoutKey, string accountNo, string accountTypeCode = "P&L", string debitCreditCode = "CR", string units = "")
		{
			var sqlText = $@"
					INSERT [{ScriptDbName}].[Finance].[BAS__GLAccount]
						(GLAccountKey, GLAccountID, AccountTypeCode, AccountNo, AccountGroup, Description, DebitCreditCode, Units, SectionType)
					VALUES
						({gLAccoutKey}, newid(), '{accountTypeCode}', '{accountNo}', 'G', 'ACC{gLAccoutKey}', '{debitCreditCode}', '{units}', 'AP');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateBASAccount(string accountType, int gLAccountKey)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Customs].[BAS__Account]
					(AccountID, AccountKey, AccountType, GLAccountKey)
				VALUES
					(newid(), 1, '{accountType}', {gLAccountKey})";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateGRPGeneralLedgerAggregateData(int gLAccountKey, int period, decimal localCredit, decimal localDebit, int companyKey, int departmentKey, int branchKey, string transactionCategory = "")
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__GeneralLedgerAggregateData]
					( [GLAccountKey], [PostPeriod], [GLAmountLocalCredit], [GLAmountLocalDebit], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						('{gLAccountKey}', {period}, {localCredit}, {localDebit}, {localDebit}-{localCredit}, {companyKey}, '{transactionCategory}', {departmentKey}, {branchKey});";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateBASCompany(int companyKey, Guid companyID)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
				VALUES
					({companyKey}, '{companyID}', 'AUD', 'DAU', 1, 1);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateBASBranch(int branchKey, Guid branchPK, int companyKey)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey])
					VALUES
						({branchKey}, '{branchPK}' , {companyKey}, 'SYN', 1);";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateBASDepartment(int departmentKey, Guid pk)
		{
			var sqlText = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						({departmentKey}, '{pk}', 'GES');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateBASCurrency()
		{
			var sqlText = $@"
				INSERT {ScriptDbName}.Finance.BAS__Currency
				([CurrencyKey], [CurrencyID], [CurrencyCode], [SubUnitRatio])
					VALUES
						(1, newid(), 'AUD' , 100)";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable Exec(Guid companyPK, string periodFrom, string periodTo, string branchPKs, string departmentPKs, string transactionCategory)
		{
			var sql = $@"Select * From [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]('{companyPK}',{periodFrom}, {periodTo}, '{branchPKs}', '{departmentPKs}', '{transactionCategory}')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		const int currentCompanyKey = 1;

		readonly Guid companyPK = Guid.NewGuid();
		readonly Guid branchPK = Guid.NewGuid();
		readonly Guid departmentPK = Guid.NewGuid();
	}
}
