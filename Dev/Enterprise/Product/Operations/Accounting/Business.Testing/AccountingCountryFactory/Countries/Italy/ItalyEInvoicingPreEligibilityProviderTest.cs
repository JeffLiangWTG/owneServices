using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(ItalyEInvoicingPreEligibilityProvider))]
	public class ItalyEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => CountryCodes.Italy;

		public override void TestCanEvaluateByTransaction()
		{
			var complianceInfoEInvoice = GetInvoicingPreEligibilityProvider();
			AssertNotNull(complianceInfoEInvoice);

			var trans = Factory.NewWithValidTestData<AccTransactionHeader>();
			AssertEquals("Return true if transaction is not in DB", true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

			trans.AH_Ledger = LedgerTypes.IncompleteTransactions;
			trans.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			Factory.Save();

			AssertEquals(false, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

			trans.AH_Ledger = LedgerTypes.AccountsPayable;
			trans.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

			trans.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			trans.AH_TransactionType = TransactionTypes.UAInvoice;
			AssertEquals(false, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

			trans.AH_Ledger = LedgerTypes.AccountsPayable;
			trans.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

			trans.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			trans.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			AssertEquals(false, complianceInfoEInvoice.CanEvaluateByTransaction(trans));

			trans.AH_Ledger = LedgerTypes.AccountsPayable;
			trans.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(true, complianceInfoEInvoice.CanEvaluateByTransaction(trans));
		}
	}
}
