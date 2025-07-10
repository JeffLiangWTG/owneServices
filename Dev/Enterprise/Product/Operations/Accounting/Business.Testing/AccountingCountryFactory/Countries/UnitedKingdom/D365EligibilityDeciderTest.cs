using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class D365EligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.UnitedKingdom;

		protected override string ExpectedAdditionalTraceLog => null;

		public void TestIsTransactionEligible_OnlyAccountsReceivableLedger()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(LedgerTypes).GetConstantValues(),
				eligibleValues: new[] { LedgerTypes.AccountsReceivable },
				setValue: (t, val) => t.Ledger = val,
				validMessage: "AR Ledger");

		public void TestIsTransactionEligible_OnlyInvoiceAndCreditNoteAndAdjustmentNoteTypes()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(TransactionTypes).GetConstantValues(),
				eligibleValues: new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote },
				setValue: (t, val) => t.TransactionType = val,
				validMessage: "INV, CRD and ADJ Transaction Types");

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction()
			=> new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.UnitedKingdom,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
			};
	}
}
