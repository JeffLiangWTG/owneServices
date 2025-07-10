using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	internal class PaymentApprovalLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHeaders()
		{
			APPaymentApprovalWithoutAuthorisation aPApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			AccPaymentApprovalLookups lookups = new AccPaymentApprovalLookups(aPApproval);
			AssertNotNull("OrgHeaders should not be null", aPApproval.Lookups.Headers);
			AssertEquals("OrgHeaders Type should be Creditor Collection", typeof(CreditorCollection), aPApproval.Lookups.Headers.GetType());
			ARPaymentApprovalWithoutAuthorisation aRApproval = Factory.New<ARPaymentApprovalWithoutAuthorisation>();
			lookups = new AccPaymentApprovalLookups(aRApproval);
			AssertNotNull("OrgHeaders should not be null", aRApproval.Lookups.Headers);
			AssertEquals("OrgHeaders Type should be Debtor Collection", typeof(DebtorCollection), aRApproval.Lookups.Headers.GetType());
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

			APPaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();

			AssertNotNull("BankAccounts should not be null", approval.Lookups.BankAccounts);
			AssertEquals("BankAccounts Type", typeof(AccBankAccountCollection), approval.Lookups.BankAccounts.GetType());
			approval.Lookups.BankAccounts.Load();
			AssertEquals("BankAccounts Count", 2, approval.Lookups.BankAccounts.Count);
			AssertEquals("BankAccounts contains Bank 1", true, approval.Lookups.BankAccounts.Contains(bank1.PK));
			AssertEquals("BankAccounts contains Bank 2", true, approval.Lookups.BankAccounts.Contains(bank2.PK));
			AssertEquals("BankAccounts contains Bank 3", false, approval.Lookups.BankAccounts.Contains(bank3.PK));
		}

		public void TestBankAccounts_ContainsOnlyActiveBanks()
		{
			AccBankAccount activeBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;

			APPaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval.Lookups.BankAccounts.Load();
			AssertEquals("Should contain active bank", true, approval.Lookups.BankAccounts.Contains(activeBank));
			AssertEquals("Should not contain inactive bank", false, approval.Lookups.BankAccounts.Contains(inactiveBank));
		}

		public void TestChequeBooks()
		{
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();

			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook bank1Book1 = Factory.NewWithValidTestData<AccChequeBook>();
			bank1Book1.AK_AB = bank1.PK;
			bank1Book1.AK_GB = GlbBranch.CurrentBranch.PK;

			AccChequeBook bank1Book2 = Factory.NewWithValidTestData<AccChequeBook>();
			bank1Book2.AK_AB = bank1.PK;
			bank1Book2.AK_GB = newBranch.PK;

			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook bank2Book1 = Factory.NewWithValidTestData<AccChequeBook>();
			bank2Book1.AK_AB = bank2.PK;
			bank2Book1.AK_GB = GlbBranch.CurrentBranch.PK;

			AccChequeBook bank2Book2 = Factory.NewWithValidTestData<AccChequeBook>();
			bank2Book2.AK_AB = bank2.PK;
			bank2Book2.AK_GB = newBranch.PK;

			APPaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			//AccPaymentApprovalLookups Lookups = new AccPaymentApprovalLookups(Approval);

			AssertNotNull("ChequeBooks should not be null", approval.Lookups.ChequeBooks);
			AssertEquals("ChequeBooks Type", typeof(ActiveChequeBookCollection), approval.Lookups.ChequeBooks.GetType());

			approval.AV_AB = ZGuid.Empty;
			approval.Lookups.ChequeBooks.Load();
			AssertEquals("ChequeBooks Count", 0, approval.Lookups.ChequeBooks.Count);
			AssertEquals("ChequeBooks contains Bank2Book1", false, approval.Lookups.ChequeBooks.Contains(bank1Book1.PK));
			AssertEquals("ChequeBooks contains Bank2Book2", false, approval.Lookups.ChequeBooks.Contains(bank2Book1.PK));

			approval.AV_AB = bank1.PK;
			approval.Lookups.ChequeBooks.Load();
			AssertEquals("ChequeBooks Count", 1, approval.Lookups.ChequeBooks.Count);
			AssertEquals("ChequeBooks contains Bank1Book1", true, approval.Lookups.ChequeBooks.Contains(bank1Book1.PK));
			AssertEquals("ChequeBooks contains Bank1Book2", false, approval.Lookups.ChequeBooks.Contains(bank1Book2.PK));

			approval.AV_AB = bank2.PK;
			approval.Lookups.ChequeBooks.Load();
			AssertEquals("ChequeBooks Count", 1, approval.Lookups.ChequeBooks.Count);
			AssertEquals("ChequeBooks contains Bank2Book1", true, approval.Lookups.ChequeBooks.Contains(bank2Book1.PK));
			AssertEquals("ChequeBooks contains Bank2Book2", false, approval.Lookups.ChequeBooks.Contains(bank2Book2.PK));
		}

		public void TestTransactionHeaders()
		{
			APPaymentApprovalWithoutAuthorisation aPApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			AccPaymentApprovalLookups lookups = new AccPaymentApprovalLookups(aPApproval);
			AssertNotNull("OrgHeaders should not be null", aPApproval.Lookups.TransactionHeaders);
			AssertEquals("OrgHeaders Type should be Creditor Collection", typeof(APTransactionHeaderCollection), aPApproval.Lookups.TransactionHeaders.GetType());
			ARPaymentApprovalWithoutAuthorisation aRApproval = Factory.New<ARPaymentApprovalWithoutAuthorisation>();
			lookups = new AccPaymentApprovalLookups(aRApproval);
			AssertNotNull("OrgHeaders should not be null", aRApproval.Lookups.TransactionHeaders);
			AssertEquals("OrgHeaders Type should be Debtor Collection", typeof(ARTransactionHeaderCollection), aRApproval.Lookups.TransactionHeaders.GetType());
		}

		public void TestAddresses()
		{
			PaymentApprovalBase approval = Factory.New<ARPaymentApprovalWithAuthorisation>();
			AccPaymentApprovalLookups lookups = new PaymentApprovalLookups(approval);

			AssertNotNull("Addresses should not be null", approval.Lookups.Addresses);
		}
	}
}