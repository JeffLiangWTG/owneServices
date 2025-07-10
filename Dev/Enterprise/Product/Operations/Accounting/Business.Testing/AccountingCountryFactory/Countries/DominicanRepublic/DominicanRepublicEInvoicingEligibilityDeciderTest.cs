using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class DominicanRepublicEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.DominicanRepublic;
		protected override string ExpectedAdditionalTraceLog => "Compliance Sub Type (TEI) is valid: True";

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
				allPossibleValues: typeof(DominicanRepublicComplianceInfo.ComplianceSubTypeCodes).GetConstantValues().Append(""),
				eligibleValues: new[]
				{
					DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEI,
					DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEF,
					DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEC,
					DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TED,
					DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TES,
					DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEG,
				},
				setValue: (t, val) => t.ComplianceSubType = val,
				validMessage: "TEI ,TEF ,TEC ,TED ,TES and TEG Compliance Sub-Types");

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.DominicanRepublic,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				ComplianceSubType = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEI,
			};
	}
}
