using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class ChileEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Chile;
		protected override string ExpectedAdditionalTraceLog => "Compliance Sub Type (DXI) is valid: True";

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
				allPossibleValues: typeof(ChileComplianceInfo.ComplianceSubTypeCodes).GetConstantValues().Append(""),
				eligibleValues: new[]
				{
					ChileComplianceInfo.ComplianceSubTypeCodes.DXI,
					ChileComplianceInfo.ComplianceSubTypeCodes.DCR,
					ChileComplianceInfo.ComplianceSubTypeCodes.DCD,
					ChileComplianceInfo.ComplianceSubTypeCodes.DEX,
				},
				setValue: (t, val) => t.ComplianceSubType = val,
				validMessage: "DXI, DCR, DCD and DEX Compliance Sub-Types");

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Chile,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				ComplianceSubType = ChileComplianceInfo.ComplianceSubTypeCodes.DXI,
			};
	}
}
