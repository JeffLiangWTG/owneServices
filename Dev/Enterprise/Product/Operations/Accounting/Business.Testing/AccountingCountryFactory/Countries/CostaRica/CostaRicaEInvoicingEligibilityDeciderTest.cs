using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class CostaRicaEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.CostaRica;
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
				eligibleValues: new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote },
				setValue: (t, val) => t.TransactionType = val,
				validMessage: "INV and CRD Transaction Types");

		public void TestIsTransactionEligible_ComplianceSubTypes()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(CostaRicaComplianceInfo.ComplianceSubTypeCodes).GetConstantValues().Append(""),
				eligibleValues: new[]
				{
					CostaRicaComplianceInfo.ComplianceSubTypeCodes.TCD,
					CostaRicaComplianceInfo.ComplianceSubTypeCodes.TCR,
					CostaRicaComplianceInfo.ComplianceSubTypeCodes.TXE,
					CostaRicaComplianceInfo.ComplianceSubTypeCodes.TXI,
				},
				setValue: (t, val) => t.ComplianceSubType = val,
				validMessage: "TCD ,TCR ,TXE and TXI Compliance Sub-Types");

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction()
			=> new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.CostaRica,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				ComplianceSubType = CostaRicaComplianceInfo.ComplianceSubTypeCodes.TXI,
			};
	}
}
