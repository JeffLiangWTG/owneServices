using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class IsraelEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Israel;
		protected override string ExpectedAdditionalTraceLog => @"Invoice Address Country: IL
OrgHeader Category: BUS
Transaction Lines count: 1
Transaction Lines with Tax Rate count: 1
Invoice Amount: 26000
Transaction has some lines valid for eInvoicing: True
Invoice Date restricted Amount: 25000";
		protected string ExpectedAdditionalTraceLogForPayable => @"Invoice Address Country: IL
OrgHeader Category: BUS
Transaction Lines count: 3
Transaction Lines with Tax Rate count: 3
Invoice Amount: -26000
Transaction Type: INV, Govt.ID: '', VAT Amount is eligible: True";

		#region Accounts Receivable

		public void TestIsTransactionEligible_AccountsReceivableLedger()
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

		public void TestIsTransactionEligible_OnlyTaxableLines_VaryTaxType()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(AccTaxRate.Types).GetConstantValues(),
				eligibleValues: new[] { AccTaxRate.Types.CapitalRated, AccTaxRate.Types.Rated },
				setValue: (t, val) => ((FakeEligibilityLiteTransactionLine)t.Lines.First()).TaxType = val,
				validMessage: "Only CAP and RAT Taxable Lines");

		public void TestIsTransactionEligible_NoLines()
		{
			var transaction = CreateEligibleTransaction();
			transaction.Lines = Array.Empty<FakeEligibilityLiteTransactionLine>();

			var eligibilityDecider = GetEligibilityDecider();
			var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with no line items should not be eligible", false, isEligible);
		}

		public void TestIsTransactionEligible_ManyLines()
		{
			var transaction = CreateEligibleTransaction();
			transaction.Lines = new[]
			{
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10 },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.CapitalRated, TaxRate = 20 },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 30 },
			};

			var eligibilityDecider = GetEligibilityDecider();
			var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction should be eligible with many lines with CAP/RAT Type and TaxRate > 0", true, isEligible);

			foreach (var line in transaction.Lines)
			{
				((FakeEligibilityLiteTransactionLine)line).TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase;
				isEligible = eligibilityDecider.IsTransactionEligible(transaction);
				AssertEquals("Transaction with one ineligible line by TaxType is still eligible", true, isEligible);

				((FakeEligibilityLiteTransactionLine)line).TaxType = AccTaxRate.Types.Rated;
			}

			transaction.InvoiceAmount = 24000;
			isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with all eligible lines is not eligible because of the InvoiceAmount < 25000", false, isEligible);

			transaction.InvoiceAmount = 25000;
			isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with all eligible lines and InvoiceAmount >= 25000 is eligible", true, isEligible);

			transaction.InvoiceAmount = 26000;
			transaction.Lines = new[]
			{
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase },
			};
			isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with all ineligible lines by TaxType is ineligible", false, isEligible);

			transaction.Lines = new[]
			{
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 0 },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.ExcludedFromTheTaxBase }
			};
			isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with an eligible line by RAT TaxType is ineligible because TaxRate = 0", false, isEligible);

			transaction.Lines = new[]
			{
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.CapitalRated, TaxRate = 0 },
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.NotReportable }
			};
			isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with an eligible line by CAP TaxType is ineligible because TaxRate = 0", false, isEligible);
		}

		public void TestIsTransactionEligible_ShouldDependOnInvoiceAmountBoundariesRegistryItem()
		{
			var eligibilityDecider = GetEligibilityDecider();

			var transaction = CreateEligibleTransaction();
			transaction.Lines = new[]
			{
				new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10 },
			};

			transaction.InvoiceDate = new ZDateTime(2025, 1, 1);
			transaction.InvoiceAmount = 16000m;
			var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Not eligible, invoice amount < 20000", false, isEligible);

			transaction.InvoiceAmount = 21000m;
			isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Eligible, invoice amount >= 20000", true, isEligible);
		}

		public void TestIsTransactionEligible_AbsoluteInvoiceAmount_CreditNote()
		{
			AssertTransactionEligible_AbsoluteInvoiceAmount(TransactionTypes.CreditNote);
		}

		public void TestIsTransactionEligible_AbsoluteInvoiceAmount_AdjustmentNote()
		{
			AssertTransactionEligible_AbsoluteInvoiceAmount(TransactionTypes.AdjustmentNote);
		}

		void AssertTransactionEligible_AbsoluteInvoiceAmount(string transactionType)
		{
			var eligibilityDecider = GetEligibilityDecider();
			var transaction = CreateEligibleTransaction();
			transaction.TransactionType = transactionType;

			transaction.InvoiceAmount = 25001m;
			Assert($"transaction {transactionType} with absolute amount greater than 25000 (positive) is eligible", eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = -25001m;
			Assert($"transaction {transactionType} with absolute amount greater than 25000 (negative) is eligible", eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = 25000m;
			Assert($"transaction {transactionType} with absolute amount equals to 25000 (positive) is eligible", eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = -25000m;
			Assert($"transaction {transactionType} with absolute amount equals to 25000 (negative) is eligible", eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = 24999m;
			Assert($"transaction {transactionType} with absolute amount less than 25000 (positive) is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = -24999m;
			Assert($"transaction {transactionType} with absolute amount less than 25000 (negative) is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));
		}

		public void TestIsTransactionEligible_AR_OrganizationCategories()
		{
			var ledgerType = LedgerTypes.AccountsReceivable;
			AssertTransactionEligible_OrganizationCategory(ledgerType, category: OrgConstants.Category.Business, expectedEligibility: true);
			AssertTransactionEligible_OrganizationCategory(ledgerType, category: OrgConstants.Category.NonGovernmentOrganisation, expectedEligibility: true);
			AssertTransactionEligible_OrganizationCategory(ledgerType, category: OrgConstants.Category.NaturalPersonIndividual, expectedEligibility: false);
		}

		public void TestIsTransactionEligible_AR_OrganizationAddress()
		{
			AssertIsTransactionEligible_OrganizationAddress(LedgerTypes.AccountsReceivable);
		}

		#endregion

		#region Accounts Payable

		public void TestIsTransactionEligible_AccountsPayableLedger_WithGovernmentAllocatedID()
		{
			var eligibilityDecider = GetEligibilityDecider();
			var transaction = CreateEligibleAPTransaction(governmentAllocatedID: "AZ123");

			var transTypes = new List<string> {
				TransactionTypes.Invoice,
				TransactionTypes.AdjustmentNote,
				TransactionTypes.CreditNote
			};

			foreach (var transType in transTypes)
			{
				transaction.TransactionType = transType;
				Assert($"{transType} transaction with GovernmentAllocatedID is eligible", eligibilityDecider.IsTransactionEligible(transaction));
			}
		}

		public void TestIsTransactionEligible_AccountsPayableLedger_NoGovernmentAllocatedID_EligibleVATAmount()
		{
			var eligibilityDecider = GetEligibilityDecider();

			var transaction = CreateEligibleAPTransaction(transactionType: TransactionTypes.Invoice);
			Assert("AP Invoice with Eligible VAT Amount is eligible", eligibilityDecider.IsTransactionEligible(transaction));

			transaction.TransactionType = TransactionTypes.AdjustmentNote;
			Assert("AP AdjustmentNote with positive amount and Eligible VAT Amount is eligible", eligibilityDecider.IsTransactionEligible(transaction));

			transaction.TransactionType = TransactionTypes.CreditNote;
			Assert("AP CreditNote with Eligible VAT Amount is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = 26000;
			var lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10, GSTVATAmount = 2600 } };
			transaction.Lines = lines;
			Assert("AP AdjustmentNote with negative amount and Eligible VAT Amount is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = -10000;
			lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10, GSTVATAmount = -1000 } };
			transaction.Lines = lines;

			transaction.TransactionType = TransactionTypes.Invoice;
			Assert("AP Invoice with no Eligible VAT Amount is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.TransactionType = TransactionTypes.AdjustmentNote;
			Assert("AP AdjustmentNote with positive amount and not Eligible VAT Amount is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = transaction.InvoiceAmount * -1;
			Assert("AP AdjustmentNote with negative amount and not Eligible VAT Amount is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.TransactionType = TransactionTypes.CreditNote;
			Assert("AP CreditNote with not Eligible VAT Amount is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));
		}

		public void TestIsTransactionEligible_AccountsPayableLedger_NoGovernmentAllocatedID_NoRatedTAXRate()
		{
			var eligibilityDecider = GetEligibilityDecider();
			var transaction = CreateEligibleAPTransaction(transactionType: TransactionTypes.Invoice);

			var lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Exempt, TaxRate = 10, GSTVATAmount = -2600 } };
			transaction.Lines = lines;

			transaction.TransactionType = TransactionTypes.Invoice;
			Assert("AP Invoice with no Eligible VAT Amount is not eligible (No proper AccTaxRate.Types found)", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.TransactionType = TransactionTypes.CreditNote;
			Assert("AP CreditNote with not Eligible VAT Amount is not eligible (No proper AccTaxRate.Types found)", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.TransactionType = TransactionTypes.AdjustmentNote;
			Assert("AP AdjustmentNote with positive amount and not Eligible VAT Amount is not eligible (No proper AccTaxRate.Types found)", !eligibilityDecider.IsTransactionEligible(transaction));

			transaction.InvoiceAmount = 26000;
			lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Exempt, TaxRate = 10, GSTVATAmount = 2600 } };
			transaction.Lines = lines;
			Assert("AP AdjustmentNote with negative amount and not Eligible VAT Amount is not eligible (No proper AccTaxRate.Types found)", !eligibilityDecider.IsTransactionEligible(transaction));
		}

		public void TestIsTransactionEligible_AP_OrganizationCategories()
		{
			var ledgerType = LedgerTypes.AccountsPayable;
			AssertTransactionEligible_OrganizationCategory(ledgerType, category: OrgConstants.Category.Business, expectedEligibility: true);
			AssertTransactionEligible_OrganizationCategory(ledgerType, category: OrgConstants.Category.NonGovernmentOrganisation, expectedEligibility: true);
			AssertTransactionEligible_OrganizationCategory(ledgerType, category: OrgConstants.Category.NaturalPersonIndividual, expectedEligibility: false);
		}

		public void TestIsTransactionEligible_AP_OrganizationAddress()
		{
			AssertIsTransactionEligible_OrganizationAddress(LedgerTypes.AccountsPayable);
		}

		public void TestGetAdditionalTraceLogForPayable()
		{
			var decider = GetEligibilityDecider();
			var t = CreateEligibleAPTransaction();
			AssertEquals("Expected the following log", ExpectedAdditionalTraceLogForPayable, decider.GetAdditionalTraceLog(t));
		}

		#endregion

		void AssertIsTransactionEligible_OrganizationAddress(string ledgerType)
		{
			var eligibilityDecider = GetEligibilityDecider();

			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);

			AssertIsTransactionEligible_OrgAddress_AddressOverride(ledgerType, eligibilityDecider, countries);
			AssertIsTransactionEligible_OrgAddress_MainAddress(ledgerType, eligibilityDecider, countries);
		}

		void AssertIsTransactionEligible_OrgAddress_AddressOverride(string ledgerType, IEInvoicingEligibilityDecider eligibilityDecider, RefCountry[] countries)
		{
			var transaction = ledgerType == LedgerTypes.AccountsReceivable ? CreateEligibleTransaction() : CreateEligibleAPTransaction();
			foreach (var country in countries)
			{
				transaction.InvoiceOrgAddressOverride = new FakeEligibilityLiteOrgAddress() { CountryCode = country.Code };
				if (country.Code == CountryCodes.Israel)
				{
					Assert("Transaction with InvoiceOrgAddressOverride from Israel is eligible", eligibilityDecider.IsTransactionEligible(transaction));
				}
				else
				{
					Assert($"Transaction with InvoiceOrgAddressOverride from {country.HumanReadableName} is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));
				}
			}
		}

		void AssertIsTransactionEligible_OrgAddress_MainAddress(string ledgerType, IEInvoicingEligibilityDecider eligibilityDecider, RefCountry[] countries)
		{
			var transaction = ledgerType == LedgerTypes.AccountsReceivable ? CreateEligibleTransaction() : CreateEligibleAPTransaction();
			transaction.InvoiceOrgAddressOverride = new FakeEligibilityLiteOrgAddress() { CountryCode = string.Empty };
			foreach (var country in countries)
			{
				transaction.OrgHeader = new FakeEligibilityLiteOrgHeader() { MainAddress = new FakeEligibilityLiteOrgAddress() { CountryCode = country.Code } };
				if (country.Code == CountryCodes.Israel)
				{
					Assert("Transaction with OrgHeader.MainAddress from Israel is eligible", eligibilityDecider.IsTransactionEligible(transaction));
				}
				else
				{
					Assert($"Transaction with OrgHeader.MainAddress from {country.HumanReadableName} is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));
				}
			}
		}

		void AssertTransactionEligible_OrganizationCategory(string ledgerType, string category, bool expectedEligibility)
		{
			var eligibilityDecider = GetEligibilityDecider();
			var transaction = ledgerType == LedgerTypes.AccountsReceivable ? CreateEligibleTransaction() : CreateEligibleAPTransaction();
			transaction.OrgHeader = new FakeEligibilityLiteOrgHeader().WithCategory(category);

			if (expectedEligibility)
			{
				Assert($"{ledgerType} Transaction with {category} Organization Category is eligible", eligibilityDecider.IsTransactionEligible(transaction));
			}
			else
			{
				Assert($"{ledgerType} Transaction with {category} Organization Category is not eligible", !eligibilityDecider.IsTransactionEligible(transaction));
			}
		}

		FakeEligibilityLiteTransaction CreateEligibleAPTransaction(ZString? governmentAllocatedID = null, ZString? transactionType = null)
			=> new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Israel,
				Ledger = LedgerTypes.AccountsPayable,
				TransactionType = transactionType ?? TransactionTypes.Invoice,
				InvoiceOrgAddressOverride = new FakeEligibilityLiteOrgAddress() { CountryCode = CountryCodes.Israel },
				OrgHeader = new FakeEligibilityLiteOrgHeader().WithCategory(OrgConstants.Category.Business),
				Lines = new FakeEligibilityLiteTransactionLine[]
				{
					new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10, GSTVATAmount = -1000 },
					new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.CapitalRated, TaxRate = 10, GSTVATAmount = -1200 },
					new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10, GSTVATAmount = -400 }
				},
				InvoiceDate = new ZDateTime(2024, 6, 1),
				InvoiceAmount = -26000,
				GovernmentAllocatedID = governmentAllocatedID ?? ZString.Empty
			};

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction()
			=> new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Israel,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				InvoiceOrgAddressOverride = new FakeEligibilityLiteOrgAddress() { CountryCode = CountryCodes.Israel },
				OrgHeader = new FakeEligibilityLiteOrgHeader().WithCategory(OrgConstants.Category.Business),
				Lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated, TaxRate = 10 } },
				InvoiceDate = new ZDateTime(2024, 6, 1),
				InvoiceAmount = 26000
			};
	}
}
