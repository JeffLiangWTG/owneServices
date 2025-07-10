using System;
using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PolandEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Poland;
		protected override string ExpectedAdditionalTraceLog => @"Transaction is Disbursement Invoice: False (Transaction Category: )
OrgHeader Category: BUS
Branch OrgProxy has PTU Code: True
Transaction Lines count: 1
Transaction has some lines valid for eInvoicing: True";

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

		public void TestIsTransactionEligible_OnlyNonDisbursementInvoiceCategories()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: new InvoiceTypesList().GetAllCodes(),
				eligibleValues: new InvoiceTypesList().GetAllCodes()
					.Except(new[]
					{
						InvoiceTypesList.Codes.DisbursementInvoice,
						InvoiceTypesList.Codes.DisbursementInvoice_Batching,
						InvoiceTypesList.Codes.DisbursementInForeignCurrency,
						InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
					}),
				setValue: (t, val) => t.TransactionCategory = val,
				validMessage: "non DCU, DBT, DBD and DCD Transaction Categories");

		public void TestIsTransactionEligible_OnlyNonGovernmentOrgCategory()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(OrgConstants.Category).GetConstantValues(),
				eligibleValues: typeof(OrgConstants.Category).GetConstantValues()
									.Except(new[] { OrgConstants.Category.Government }),
				setValue: (t, val) => ((FakeEligibilityLiteOrgHeader)t.OrgHeader).Category = val,
				validMessage: "non GOV Org Header Category");

		public void TestIsTransactionEligible_OnlyPolandDebtorPTURegCode_VaryCountry()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(CountryCodes).GetConstantValues(),
				eligibleValues: new[] { CountryCodes.Poland },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.BranchOrgProxy.RegistrationCodes.First()).CountryCode = val,
				validMessage: "Poland PTU Registration Code");

		public void TestIsTransactionEligible_OnlyPolandDebtorPTURegCode_VaryCode()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(OrgCusCode.PolandCodeTypes).GetConstantValues(),
				eligibleValues: new[] { OrgCusCode.PolandCodeTypes.PTU },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.BranchOrgProxy.RegistrationCodes.First()).CodeType = val,
				validMessage: "Poland PTU Registration Code");

		public void TestIsTransactionEligible_OnlyPolandDebtorPTURegCode_VaryNumber()
			=> AssertEligibilityForNonBlank(
				allPossibleValues: new[] { "1234", "99999999", "" },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.BranchOrgProxy.RegistrationCodes.First()).RegistrationNumber = val,
				validMessage: "Poland PTU Registration Code");

		public void TestIsTransactionEligible_OnlyTaxableLines_VaryTaxType()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(AccTaxRate.Types).GetConstantValues(),
				eligibleValues: typeof(AccTaxRate.Types).GetConstantValues()
								.Except(new[] { AccTaxRate.Types.ExcludedFromTheTaxBase, AccTaxRate.Types.NotReportable }),
				setValue: (t, val) => ((FakeEligibilityLiteTransactionLine)t.Lines.First()).TaxType = val,
				validMessage: "Taxable Lines");

		public void TestIsTransactionEligible_OnlyTaxableLines_NoLines()
		{
			var transaction = CreateEligibleTransaction();
			transaction.Lines = Array.Empty<FakeEligibilityLiteTransactionLine>();

			var eligibilityDecider = GetEligibilityDecider();
			var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with no line items should not be eligible", false, isEligible);
		}

		public void TestIsTransactionEligible_OnlyTaxableLines_ManyLines()
		{
			var transaction = CreateEligibleTransaction();
			transaction.Lines = new[]
			{
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated },
			};

			var eligibilityDecider = GetEligibilityDecider();
			var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Precondition: Transaction should be eligible with many lines", true, isEligible);

			foreach (var line in transaction.Lines)
			{
				((FakeEligibilityLiteTransactionLine)line).TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase;
				isEligible = eligibilityDecider.IsTransactionEligible(transaction);
				AssertEquals("Transaction with one ineligible line by TaxType is still eligible", true, isEligible);

				((FakeEligibilityLiteTransactionLine)line).TaxType = AccTaxRate.Types.Rated;
			}

			transaction.Lines = new[]
			{
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase },
			};
			isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with all ineligible lines by TaxType is ineligible", false, isEligible);
		}

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction()
			=> new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Poland,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				BranchOrgProxy = new FakeEligibilityLiteOrgHeader()
										.WithRegistrationCode(
											countryCode: CountryCodes.Poland,
											registrationNumber: "1234",
											codeType: OrgCusCode.PolandCodeTypes.PTU),
				OrgHeader = new FakeEligibilityLiteOrgHeader() { Category = OrgConstants.Category.Business },
				Lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated } },
			};
	}
}
