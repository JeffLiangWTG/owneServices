using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;

	internal class AccCollectionOrderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDebtors()
		{
			var order = Factory.New<AccCollectionOrder>();
			AssertNotNull(order.Lookups.Debtors);
			AssertType<OrgHeaderCollection>(order.Lookups.Debtors);
		}

		public void TestBankAccounts()
		{
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			bank1.AB_GB = GlbBranch.CurrentBranch.PK;
			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();
			bank2.AB_GB = ZGuid.Empty;
			AccBankAccount bank3 = Factory.NewWithValidTestData<AccBankAccount>();
			bank3.AB_GB = newBranch.PK;
			var order = Factory.New<AccCollectionOrder>();
			AssertNotNull("BankAccounts should not be null", order.Lookups.BankAccounts);
			AssertEquals("BankAccounts Type", typeof(AccBankAccountCollection), order.Lookups.BankAccounts.GetType());
			order.Lookups.BankAccounts.Load();
			AssertEquals("BankAccounts Count", 2, order.Lookups.BankAccounts.Count);
			AssertEquals("BankAccounts contains Bank 1", true, order.Lookups.BankAccounts.Contains(bank1.PK));
			AssertEquals("BankAccounts contains Bank 2", true, order.Lookups.BankAccounts.Contains(bank2.PK));
			AssertEquals("BankAccounts contains Bank 3", false, order.Lookups.BankAccounts.Contains(bank3.PK));
		}
	}
}