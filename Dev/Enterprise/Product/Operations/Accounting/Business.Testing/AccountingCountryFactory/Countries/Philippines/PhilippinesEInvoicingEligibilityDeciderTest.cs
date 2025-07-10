using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PhilippinesEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Philippines;
		protected override string ExpectedAdditionalTraceLog => null;

		public void TestIsTransactionEligible_OnlyAccountsReceivableLedger()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(LedgerTypes).GetConstantValues(),
				eligibleValues: new[] { LedgerTypes.AccountsReceivable },
				setValue: (t, val) => t.Ledger = val,
				validMessage: "AR Ledger");

		public void TestIsTransactionEligible_OnlyInvoiceAndCreditNoteTypes()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(TransactionTypes).GetConstantValues(),
				eligibleValues: new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote },
				setValue: (t, val) => t.TransactionType = val,
				validMessage: "INV, CRD and ADJ Transaction Types");

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction()
			=> new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Philippines,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
			};

		public void TestEligibleTransactionHeader()
		{
			var eligibleTransactionSettings = new[] {
				(Ledger: LedgerTypes.AccountsReceivable, TransactionType: TransactionTypes.Invoice),
				(Ledger: LedgerTypes.AccountsReceivable, TransactionType: TransactionTypes.CreditNote),
				(Ledger: LedgerTypes.AccountsReceivable, TransactionType: TransactionTypes.AdjustmentNote),
			};

			var testConfigurations =
				from ledger in typeof(LedgerTypes).GetConstantValues()
				join transactionType in typeof(TransactionTypes).GetConstantValues()
					on 1 equals 1
				join eligibleTransactionSetting in eligibleTransactionSettings
					on new { Ledger = ledger, TransactionType = transactionType }
						equals
						new { eligibleTransactionSetting.Ledger, eligibleTransactionSetting.TransactionType }
					into eligibleTransactionSettingMatchedResult
				select new
				{
					Expected = eligibleTransactionSettingMatchedResult.Any(),
					Ledger = ledger,
					TransactionType = transactionType
				};

			var decider = GetEligibilityDecider();
			foreach (var testConfiguration in testConfigurations)
			{
				var transaction = new Mock<IEInvoicingEligibilityLiteTransaction>();
				transaction.Setup(x => x.CountryCode).Returns(CountryCodes.Philippines);
				transaction.Setup(x => x.Ledger).Returns(testConfiguration.Ledger);
				transaction.Setup(x => x.TransactionType).Returns(testConfiguration.TransactionType);

				AssertEquals(
					$"{nameof(CountryCodes.Philippines)} {LedgerTypes.AccountsReceivable} {testConfiguration.TransactionType}",
					testConfiguration.Expected,
					decider.IsTransactionEligible(transaction.Object)
				);
			}
		}
	}
}
