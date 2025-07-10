using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SaudiArabiaEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.SaudiArabia;
		protected override string ExpectedAdditionalTraceLog => null;

		public void TestIsTransactionEligible_OnlyAccountsReceivableLedger()
			=> AssertEligibilityForClosedSet(
			allPossibleValues: new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable },
			eligibleValues: new[] { LedgerTypes.AccountsReceivable },
			setValue: (t, val) => t.Ledger = val,
			validMessage: "AR Ledger");

		public void TestIsTransactionEligible_OnlyInvoiceCreditNoteAndAdjustmentNoteTypes()
		=> AssertEligibilityForClosedSet(
			allPossibleValues: typeof(TransactionTypes).GetConstantValues(),
			eligibleValues: new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote },
			setValue: (t, val) => t.TransactionType = val,
			validMessage: "INV, ADJ and CRD Transaction Types");

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.SaudiArabia,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
			};
	}
}
