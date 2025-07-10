using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment.Testing
{
	internal class OpeningPaymentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBankAccounts()
		{
			AccBankAccount activeBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;
			AccBankAccount bankForCurrentCompany = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bankForAnotherCompany = Factory.NewWithValidTestData<AccBankAccount>();
			bankForCurrentCompany.AB_GC = GlbCompany.CurrentCompany.PK;
			bankForAnotherCompany.AB_GC = Guid.NewGuid();
			AccBankAccount bankForCurrentCompanyAndCurrentBranch = Factory.NewWithValidTestData<AccBankAccount>();
			bankForCurrentCompanyAndCurrentBranch.AB_GC = GlbCompany.CurrentCompany.PK;
			bankForCurrentCompanyAndCurrentBranch.AB_GB = GlbBranch.CurrentBranch.PK;
			AccBankAccount bankForCurrentCompanyButAnotherBranch = Factory.NewWithValidTestData<AccBankAccount>();
			bankForCurrentCompanyButAnotherBranch.AB_GC = GlbCompany.CurrentCompany.PK;
			bankForCurrentCompanyButAnotherBranch.AB_GB = Guid.NewGuid();

			OpeningPayment payment = Factory.New<OpeningPayment>();
			payment.Lookups.BankAccounts.Load();
			AssertEquals("Should contain active bank", true, payment.Lookups.BankAccounts.Contains(activeBank));
			AssertEquals("Should not contain inactive bank", false, payment.Lookups.BankAccounts.Contains(inactiveBank));
			AssertEquals("Should contain bank for current company", true, payment.Lookups.BankAccounts.Contains(bankForCurrentCompany));
			AssertEquals("Should not contain bank for another company", false, payment.Lookups.BankAccounts.Contains(bankForAnotherCompany));
			AssertEquals("Should contain bank for current company and current branch", true, payment.Lookups.BankAccounts.Contains(bankForCurrentCompanyAndCurrentBranch));
			AssertEquals("Should not contain bank for current company but another branch ", false, payment.Lookups.BankAccounts.Contains(bankForCurrentCompanyButAnotherBranch));
		}
	}
}