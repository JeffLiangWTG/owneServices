using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Public.Accounting;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(Report_ProfitandLossMovementsbyAccount))]
	class Report_ProfitandLossMovementsbyAccountTest : Report_ProfitandLossMovementsbyAccountBaseTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public override void CreateAccGLHeader(Guid pk, string accountNum, string description, string accountType, string statisticalUnits, string debitCredit, bool controlAccount, int totalLevel, string column, long gLAccoutKey = 1)
		{
			var sql = $@"
INSERT [{ScriptDbName}].[Finance].[BAS__GLAccount]
	(GLAccountKey, GLAccountID, AccountTypeCode, AccountNo, AccountGroup, Description, DebitCreditCode, Units, SectionType, TotalLevel)
VALUES
	(@gLAccoutKey, @pk, @accountTypeCode, @accountNo, 'G', @description, @debitCreditCode, @units, @column, @totalLevel);";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@gLAccoutKey", SqlDbType.BigInt, gLAccoutKey);
				command.AddParameter("@accountNo", SqlDbType.VarChar, accountNum);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@accountTypeCode", SqlDbType.VarChar, accountType);
				command.AddParameter("@units", SqlDbType.VarChar, statisticalUnits);
				command.AddParameter("@debitCreditCode", SqlDbType.VarChar, debitCredit);
				command.AddParameter("@totalLevel", SqlDbType.Int, totalLevel);
				command.AddParameter("@column", SqlDbType.VarChar, column);
				command.ExecuteNonQuery();
			}
		}

		public override void CreateAccountGLAggregate(decimal amount, int period, TestParameter account, TestParameter branch, TestParameter company, TestParameter department, string transactionCategory = "")
		{
			var sql = $@"
INSERT [{ScriptDbName}].[Finance].[GRP__GeneralLedgerAggregateData]
	([GLAccountKey], [PostPeriod], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
VALUES
	(@accountKey, @period, @amount, @companyKey, @transactionCategory, @departmentKey, @branchKey);";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@amount", SqlDbType.Money, amount);
				command.AddParameter("@period", SqlDbType.Int, period);
				command.AddParameter("@accountKey", SqlDbType.BigInt, account.EDWKey);
				command.AddParameter("@branchKey", SqlDbType.BigInt, branch.EDWKey);
				command.AddParameter("@companyKey", SqlDbType.BigInt, company.EDWKey);
				command.AddParameter("@departmentKey", SqlDbType.BigInt, department.EDWKey);
				command.AddParameter("@transactionCategory", SqlDbType.VarChar, transactionCategory);
				command.ExecuteNonQuery();
			}
		}

		public override Guid CreateCompany(TestParameter company, string countryCode, string currencyCode)
		{
			CreateCurrency(currencyCode);

			var companyPK = Guid.NewGuid();
			var sql = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Company]
					([CompanyKey], [CompanyID], [CountryCode], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )
				VALUES
					(@currentCompanyKey, @companyPK, @countryCode, @currencyCode, @companyCode, 1, 1);";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@currentCompanyKey", SqlDbType.BigInt, company.EDWKey);
				command.AddParameter("@companyCode", SqlDbType.VarChar, company.Code);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.AddParameter("@currencyCode", SqlDbType.VarChar, currencyCode);
				command.ExecuteNonQuery();
			}

			return companyPK;
		}

		void CreateCurrency(string currencyCode)
		{
			var sql = $@"
				INSERT {ScriptDbName}.Finance.BAS__Currency
				([CurrencyKey], [CurrencyID], [CurrencyCode], [SubUnitRatio])
					VALUES
						(1, newid(), @currencyCode , 100)";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@currencyCode", SqlDbType.VarChar, currencyCode);
				command.ExecuteNonQuery();
			}
		}

		public override Guid CreateBranch(TestParameter company, TestParameter branch, TestParameter homePort)
		{
			var branchPK = Guid.NewGuid();
			var sql = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [HomePortKey], [BranchCode], [OrganizationKey])
					VALUES
						(@branchKey, @branchPK, @companyKey, @homePortKey, @branchCode, 1);";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyKey", SqlDbType.BigInt, company.EDWKey);
				command.AddParameter("@branchKey", SqlDbType.BigInt, branch.EDWKey);
				command.AddParameter("@branchCode", SqlDbType.VarChar, company.Code);
				command.AddParameter("@homePortKey", SqlDbType.BigInt, homePort.EDWKey);
				command.ExecuteNonQuery();
			}

			return branchPK;
		}

		public override Guid CreateDepartment(TestParameter department)
		{
			var departmentPK = Guid.NewGuid();
			var sql = $@"
				INSERT [{ScriptDbName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(@departmentKey, @departmentPK, @departmentCode);";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@departmentCode", SqlDbType.VarChar, department.Code);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@departmentKey", SqlDbType.BigInt, department.EDWKey);
				command.ExecuteNonQuery();
			}

			return departmentPK;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName);
			helper.InsertStmDataDate("JournalEntriesLastProcessedDate", new DateTime(2019, 2, 1), defaultCompanyKey);
			helper.InsertPeriodForInputYear(2019);
			helper.InsertPeriodForInputYear(2020);
		}

		readonly int defaultCompanyKey = 1;
	}
}
