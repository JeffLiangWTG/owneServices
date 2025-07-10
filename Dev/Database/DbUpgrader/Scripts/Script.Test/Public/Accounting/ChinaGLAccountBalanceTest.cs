using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ChinaGLAccountBalance))]
	class ChinaGLAccountBalanceTest : EdwHashTest
	{
		public void TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn()
		{
			var helper = new TestDbHelper(TestConnection);
			var currentCompany = TestDbHelper.DefaultCompanyPK;
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");
			var periodStartDate = new DateTime(2021, 1, 1, 00, 00, 00);
			var periodEndDate = new DateTime(2021, 1, 31, 00, 00, 00);
			helper.Insert(AccPeriodManagementSchema.Constants.TableName, new
			{
				AM_PK = Guid.NewGuid(),
				AM_Period = 202101,
				AM_Year = 2021,
				AM_StartDate = periodStartDate,
				AM_EndDate = periodEndDate,
				AM_GC_Company = currentCompany
			});
			var glAccountPK = GetFirstGLAccount();
			var bankAccountPK = helper.InsertBankAccount("BANKCDE", glAccountPK, currency: "USD");

			TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn(false, periodStartDate.AddMinutes(-1));
			TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn(true, periodStartDate);
			TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn(true, periodStartDate.AddMinutes(1));

			TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn(true, periodEndDate.AddMinutes(-1));
			TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn(true, periodEndDate);  // Should failed When use old sql
			TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn(false, periodEndDate.AddMinutes(1));

			void TestOsAmountForChinaCashBookRegister_DateRange_ForPostDateFilterColumn(bool shouldContainTransaction, DateTime postDate)
			{
				helper.InsertGLAggregate(10m, "", 202101, glAccountPK, currentBranch, currentDep, currentCompany);

				helper.InsertTransactionHeader("CB", "DPY", "00001001", 10m, periodStartDate, currentBranch, currentDep, bankAccountPK, currency: "USD");
				helper.InsertTransactionHeader("CB", "DPY", "00001002", 20m, postDate, currentBranch, currentDep, bankAccountPK, currency: "USD");

				var template = $@"EXEC ChinaGLAccountBalance
							@CompanyPK = '{currentCompany}',
							@EndPeriod = 202101,
							@BranchPKList = '',
							@IncludePeriodEndCLosing = ''";

				var result = DataUtils.GetDataTableFromQuery(TestConnection, template);
				var row = result.Select("Currency = 'USD'").FirstOrDefault();
				AssertEquals(shouldContainTransaction ? 30m : 10M, row["OsAmount"]);
				DataUtils.GetDataTableFromQuery(TestConnection, $"DELETE FROM dbo.AccTransactionHeader");
				DataUtils.GetDataTableFromQuery(TestConnection, $"DELETE FROM dbo.AccGLAggregate");
			}

			Guid GetFirstGLAccount()
			{
				var sQL = $"SELECT TOP 1 AG_PK FROM {Db.DatabaseName}.dbo.AccGLHeader";
				return (Guid)TestConnection.ExecuteScalar(sQL);
			}
		}
	
		protected override string expectedMainDbFunctionHash => "2FFF8C25F5660853618B4711633A9F74DD4C2EAB0A68C9CFEDB92FC022A374EF";
		protected override string expectedEdwDbFunctionHash => "FB4E70FFD736906A6112A25156DA97C51752C4CC9C193A5920A76D5430CF9FFB";

		protected override string edwScriptPath => "Function/Accounting/ChinaGLAccountBalance.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ChinaGLAccountBalance();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ChinaGLAccountBalance();
		}
	}
}
