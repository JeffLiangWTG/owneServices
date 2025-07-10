using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PanamaEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Panama;
		protected override string ExpectedAdditionalTraceLog => "Compliance Sub Type (TXI) is valid: True";

		public void TestIsTransactionEligible_OnlyAccountsReceivableLedger()
			=> AssertEligibilityForClosedSet(
			allPossibleValues: new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable },
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
				allPossibleValues: typeof(PanamaComplianceInfo.ComplianceSubTypeCodes).GetConstantValues().Append(""),
				eligibleValues: new[]
				{
					PanamaComplianceInfo.ComplianceSubTypeCodes.TXI,
					PanamaComplianceInfo.ComplianceSubTypeCodes.TCD,
					PanamaComplianceInfo.ComplianceSubTypeCodes.TCR,
					PanamaComplianceInfo.ComplianceSubTypeCodes.XCL,
				},
				setValue: (t, val) => t.ComplianceSubType = val,
				validMessage: "TXI, TCR, TCD, XCL Compliance Sub-Types");

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Panama,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				ComplianceSubType = PanamaComplianceInfo.ComplianceSubTypeCodes.TXI,
			};
	}
}
