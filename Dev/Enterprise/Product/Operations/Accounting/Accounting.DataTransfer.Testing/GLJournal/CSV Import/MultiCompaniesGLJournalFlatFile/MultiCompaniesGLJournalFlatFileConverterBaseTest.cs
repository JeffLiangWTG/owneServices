using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	public abstract class MultiCompaniesGLJournalFlatFileConverterBaseTest : TestCaseWithFactory
	{
		public void TestImportFlatFileWithGLAccountIsNotGlobalAndAccountCompanyNotContainCurrentCompany()
		{
			TestData.PostDate = "20220630";
			TestData.ReverseOrEndDate = "20220630";
			TestData.PostPeriod = "";
			TestData.CompanyCode = "DAN";
			TestData.BranchCode = "SY1";
			TestData.GLAccount = "2010.00.00";
			TestData.Currency = "USD";
			AssertImportFlatFileWithGLAccountIsNotGlobalAndAccountCompanyNotContainCurrentCompany((string e) => AssertImport_ErrorMessage(e, TestData));
		}

		public void TestImportFlatFileWithNotHaveRightsToPostToControlAccounts()
		{
			TestData.PostDate = "20220630";
			TestData.ReverseOrEndDate = "20220630";
			TestData.PostPeriod = "";
			TestData.CompanyCode = "DAN";
			TestData.BranchCode = "SY1";
			TestData.GLAccount = "2010.00.00";
			TestData.Currency = "AUD";
			AssertImportFlatFileWithNotHaveRightsToPostToControlAccounts((string e) => AssertImport_ErrorMessage(e, TestData));
		}

		public void TestImportFlatFileWithPostDateIsNotLastDayForPeriod()
		{
			TestData.PostDate = "20220601";
			TestData.ReverseOrEndDate = "20220630";
			TestData.PostPeriod = "";
			TestData.CompanyCode = "DAN";
			TestData.BranchCode = "SY1";
			TestData.GLAccount = "2010.00.00";
			TestData.Currency = "AUD";
			AssertImportFlatFileWithPostDateIsNotLastDayForPeriod((string e) => AssertImport_ErrorMessage(e, TestData));
		}

		public void TestImportFlatFileWithGLAccountIsControlAccountAndForeignCurrency()
		{
			TestData.PostDate = "20220630";
			TestData.ReverseOrEndDate = "20220630";
			TestData.PostPeriod = "";
			TestData.CompanyCode = "DAN";
			TestData.BranchCode = "SY1";
			TestData.GLAccount = "2010.00.00";
			TestData.Currency = "USD";
			AssertImportFlatFileWithGLAccountIsControlAccountAndForeignCurrency((string e) => AssertImport_ErrorMessage(e, TestData));
		}

		public delegate void TestHandler(string errorMessage);

		public void AssertImportFlatFileWithPostDateIsNotLastDayForPeriod(TestHandler assert)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");

			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				assert?.Invoke("Error: Header[1].DAN - The post date '20220601' does not match the 'End Date' of a valid period or the relative period has not been setup. Please check the specified value against your Period Management setup.\r\n");
			}
		}

		public void AssertImportFlatFileWithNotHaveRightsToPostToControlAccounts(TestHandler assert)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");

			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			var glHeader = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			glHeader.AG_ControlAccount = true;

			Factory.Save();

			Env.Security.GeneralLedgerJournalPostToControlAccounts.IsAllowed = false;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var errorMessage = "Error: Header[1].Line[1].DAN - 2010.00.00: This account is flagged as a control account. You do not have security rights to post to control accounts.\r\n" + Env.Security.GeneralLedgerJournalPostToControlAccounts.ErrorMessageForNotAllowed + "\r\n";
				assert?.Invoke(errorMessage);
			}
		}

		public void AssertImportFlatFileWithGLAccountIsNotGlobalAndAccountCompanyNotContainCurrentCompany(TestHandler assert)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");
			Factory.Save();

			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			var glHeader = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			glHeader.AG_IsGlobal = false;
			glHeader.CompanyFilters.RemoveAll();

			glHeader.AG_AccountType = AccountType.BalanceSheetAccount;
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.PeriodEndRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				testObjectCreator.CreateExchangeRate(testObjectCreator.USD, ExchangeRateTypes.Code.PeriodEndRate, 5m, new ZDateTime(2022, 06, 01), new ZDateTime(2022, 07, 30));
				Factory.Save();
			}

			var currentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var test = Env.CurrentCompany;
			var sY2Branch = testObjectCreator.CreateNewBranch(currentCompany, "SY2");
			Factory.Save();

			testObjectCreator.CreateTestPeriodsForEntireYear(currentCompany, 2022);
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY2Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				testObjectCreator.CreateExchangeRate(testObjectCreator.USD, ExchangeRateTypes.Code.PeriodEndRate, 5m, new ZDateTime(2022, 06, 01), new ZDateTime(2022, 07, 30));
				Factory.Save();
				assert?.Invoke("Error: Header[1].Line[1].DAN - 2010.00.00: This GL account cannot be used for posting in this company.\r\n");
			}
		}

		public void AssertImportFlatFileWithGLAccountIsControlAccountAndForeignCurrency(TestHandler assert)
		{
			GLJournalLineHelper.ControlOrLinkAccountsForTest = null;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");
			testObjectCreator.CreateTestPeriodsForEntireYear(company, 2022);
			var glHeader = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			glHeader.AG_ControlAccount = true;

			glHeader.AG_AccountType = AccountType.BalanceSheetAccount;
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.PeriodEndRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				testObjectCreator.CreateExchangeRate(testObjectCreator.USD, ExchangeRateTypes.Code.PeriodEndRate, 5m, new ZDateTime(2022, 06, 01), new ZDateTime(2022, 07, 30));
				Factory.Save();

				var errorMessage = "Error: Header[1].Line[1].DAN - 2010.00.00: For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry\r\n";

				assert?.Invoke(errorMessage);

				glHeader.AG_ControlAccount = false;
				Factory.Save();

				using (AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid()))
				{
					assert?.Invoke(errorMessage);
				}

				using (AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid()))
				{
					assert?.Invoke(errorMessage);
				}

				using (AccountingConfigurationRegistry.Instance.CFXAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid()))
				{
					assert?.Invoke(errorMessage);
				}
			}
		}

		protected virtual void AssertImport_ErrorMessage(string errorMessage, GLTestData data)
		{
		}

		public struct GLTestData
		{
			public string PostDate;
			public string ReverseOrEndDate;
			public string PostPeriod;
			public string CompanyCode;
			public string BranchCode;
			public string GLAccount;
			public string Currency;
		}

		GLTestData TestData;

		protected override void SetUp()
		{
			base.SetUp();
			TestData = new GLTestData();
			testObjectCreator = new TestObjectCreator(Factory, true);
		}

		protected TestObjectCreator testObjectCreator;
	}
}
