using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	// There additional set of tests for Aggregation in Enterprise.Accounting.Business.Aggregator.Testing.BatchAggregatorTest

	abstract class BatchAggregatorAggregationTest : BatchAggregatorTest
	{
		public abstract void TestAggregation();
		public abstract void TestAggregationForDifferentPeriods();
		public abstract void TestAggregationForDifferentGLAccounts();
		public abstract void TestAggregationForDifferentBranches();
		public abstract void TestAggregationForDifferentDepartments();
	}

	abstract class BatchAggregatorValidationTest : BatchAggregatorTest
	{
		public abstract void TestAggregationValidation();
	}

	abstract class BatchAggregatorTest : DbCreateScriptTest
	{
		protected void RunAndAssertAggregation((decimal amount, string transCat, int period, string branch, string dept, string company, string agDesc)[] expectedAggregates,
			bool skipExpectedAggregatesBalanceCheck = false)
		{
			RunAggregation(TestDbHelper.DefaultCompanyPK);

			var aggregates = DataUtils.GetDataTableFromQuery(TestConnection, @"
select AA_Amount, AA_TransactionCategory, AA_Period, GB_Code, GE_Code, GC_Code, AG_Description
from dbo.AccGLAggregate
join dbo.AccGlHeader on AA_AG = AG_PK
join dbo.GlbDepartment on AA_GE = GE_PK
join dbo.GlbCompany on AA_GC = GC_PK
join dbo.GlbBranch on AA_GB = GB_PK");

			var actualAggregates = aggregates
				.AsEnumerable()
				.Select(r => ((decimal)r["AA_Amount"], (string)r["AA_TransactionCategory"], (int)r["AA_Period"], (string)r["GB_Code"], (string)r["GE_Code"], (string)r["GC_Code"], (string)r["AG_Description"]));

			AssertContainsExactElementsInAnyOrder(expectedAggregates, actualAggregates);
			if (!skipExpectedAggregatesBalanceCheck)
			{
				AssertEquals("Balance", 0m, expectedAggregates.Sum(x => x.amount));
			}

			var unaggregates = DataUtils.GetDataTableFromQuery(TestConnection, @"
select count(*) as count from dbo.AccTransactionHeader where AH_PostToGL = 'N' and AH_PostDate is not null
union all
select count(*) as count from dbo.AccTransactionLines where AL_PostToGL = 'N' and AL_PostDate is not null AND AL_LineType IN ('WIP', 'ACR')
union all
select count(*) as count from dbo.AccTransactionLines where AL_ReverseToGL = 'N' and AL_ReverseDate is not null
");

			CombineAssertions(() =>
			{
					AssertEquals("Expect no unaggregated AccTransactionHeaders, otherwise related index will constantly grow.", 0, (int)unaggregates.Rows[0][0]);
					AssertEquals("Expect no unaggregated AccTransactionLines (post), otherwise related index will constantly grow.", 0, (int)unaggregates.Rows[1][0]);
					AssertEquals("Expect no unaggregated AccTransactionLines (reverse), otherwise related index will constantly grow.", 0, (int)unaggregates.Rows[2][0]);
			});
		}

		protected string RunAndAssertAggregationWithError(bool isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd = false)
		{
			var accGLAggregateCountBefore = GetAccGLAggregateCount();
			var errorMessage = RunAggregation(TestDbHelper.DefaultCompanyPK, true);
			var accGLAggregateCountAfter = GetAccGLAggregateCount();

			AssertNotNullOrEmpty("An error is expected", errorMessage);
			if (!isTestForPostAggregationValidationWhereTranansactionedTestCaseCantRallbackDataBeforeTestEnd)
			{
				AssertEquals("There are no AccGLAggregate records added by aggregation", 0, accGLAggregateCountAfter - accGLAggregateCountBefore);
			}

			return errorMessage;

			int GetAccGLAggregateCount() => TestConnection.ExecuteScalar<int>(@"select count(1) from dbo.AccGLAggregate");
		}

		protected void PrepareForAggregation(
			bool insertPeriods = true,
			bool setAccountsOnChargeCodes = true,
			bool setupControlAccounts = true,
			bool insertBankAccount = true,
			bool skipCFXAccountSetup = false,
			bool skipJRJAccountSetup = false)
		{
			if (insertPeriods)
			{
				for (int i = 1; i <= 2; i++)
				{
					DbHelper.InsertAccPeriod(2007, i);
				}
			}

			if (setAccountsOnChargeCodes)
			{
				var glAccount = DbHelper.InsertGLAccount("1111.01.03", "Cost Account");

				TestConnection.ExecuteNonQuery($@"
UPDATE dbo.AccChargeCode
SET AC_AG_CostAccount = '{glAccount}', AC_SystemLastEditTimeUtc = GETUTCDATE(), AC_SystemLastEditUser = 'TST'
WHERE AC_ChargeType = 'OVR' AND AC_AG_CostAccount IS NULL");
			}

			var movementsHelper = new AccountingMovementsTestHelper(TestConnection);
			movementsHelper.SetupControlAccount(clearControlAccounts: !setupControlAccounts, clearCFXAccountSetup: skipCFXAccountSetup, clearJRJAccountSetup: skipJRJAccountSetup);

			if (insertBankAccount)
			{
				BankAccountPK = DbHelper.InsertBankAccount("USDBank", BankGLAccount.PK);
				BankAccount2PK = DbHelper.InsertBankAccount("AUDBank", BankGLAccount2.PK);
			}
		}

		protected Guid BankAccountPK { get; private set; }
		protected Guid BankAccount2PK { get; private set; }

		protected (Guid PK, string Name) BankGLAccount => (new Guid("DEC025E9-96CE-4388-AD60-A035F38090BC"), "BANK 2 IN USD");
		protected (Guid PK, string Name) BankGLAccount2 => (new Guid("f4d929fa-e894-4fb0-bbeb-188acaaf06d8"), "BANK 1 IN LOCAL CURRENCY");

		string RunAggregation(Guid companyPK, bool expectException = false)
		{
			try
			{
				TestConnection.ExecuteNonQuery($@"
BEGIN TRAN
EXEC TakeUpSubledgers @Company = '{companyPK}', @SystemLastEditUser = 'TST'
COMMIT");
			}
			catch (SqlException ex) when (expectException)
			{
				if (ex.Number == 50000)
				{
					return ex.Message;
				}
			}

			return string.Empty;
		}
	}
}
