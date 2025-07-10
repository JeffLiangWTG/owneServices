using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SpainEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Spain;
		protected override string ExpectedAdditionalTraceLog => "Branch Org Proxy has SII Registration (NIF Number): True";

		public void TestIsTransactionEligible_OnlyARAPLedger()
			=> AssertEligibilityForClosedSet(
			allPossibleValues: typeof(LedgerTypes).GetConstantValues(),
			eligibleValues: new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable },
			setValue: (t, val) => t.Ledger = val,
			validMessage: "AR and AP Ledger");

		public void TestIsTransactionEligible_OnlyInvoiceAndCreditNoteAndAdjustmentNoteTypes()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(TransactionTypes).GetConstantValues(),
				eligibleValues: new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote },
				setValue: (t, val) => t.TransactionType = val,
				validMessage: "INV, CRD and ADJ Transaction Types");

		public void TestIsTransactionEligible_OnlySpainBranchOrgProxySIIRegCode_VaryCountry()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(CountryCodes).GetConstantValues(),
				eligibleValues: new[] { CountryCodes.Spain },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.BranchOrgProxy.RegistrationCodes.First()).CountryCode = val,
				validMessage: "Spain SII Registration Code");

		public void TestIsTransactionEligible_OnlySpainBranchOrgProxySIIRegCode_VaryCode()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(OrgCusCode.CodeTypes).GetConstantValues(),
				eligibleValues: new[] { SpainOrgCusCodeInfo.OrgCusCodes.SII },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.BranchOrgProxy.RegistrationCodes.First()).CodeType = val,
				validMessage: "Spain SII Registration Code");

		public void TestIsTransactionEligible_OnlySpainBranchOrgProxySIIRegCode_VaryNumber()
			=> AssertEligibilityForNonBlank(
				allPossibleValues: new[] { "1234", "99999999", "" },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.BranchOrgProxy.RegistrationCodes.First()).RegistrationNumber = val,
				validMessage: "Spain SII Registration Code");

		public void TestIsTransactionNotEligible_WithoutLines()
		{
			// Case: Transaction with no lines
			var transaction = CreateEligibleTransaction();
			transaction.Lines = Array.Empty<FakeEligibilityLiteTransactionLine>();

			var eligibilityDecider = GetEligibilityDecider();
			var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
			AssertEquals("Transaction with no line items should not be eligible", false, isEligible);
		}

		static readonly int[] LineCountsOfOneOrMore = new int[] { 1, 2, 3 };

		public void TestIsTransactionNotEligible_OnlyOneOrMoreZeroValueLines()
		{
			var eligibilityDecider = GetEligibilityDecider();

			foreach (var zeroValueLinesCount in LineCountsOfOneOrMore)
			{
				var transaction = CreateEligibleTransaction();
				transaction.Lines = Enumerable.Range(1, zeroValueLinesCount)
												.Select(i => new FakeEligibilityLiteTransactionLine() { LineAmount = 0 })
												.ToArray();

				var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
				AssertEquals("Transaction with only zero line values line items should not be eligible", false, isEligible);
			}
		}

		static readonly List<int[]> EligibleTransactionLineValueSets = new List<int[]>
		{
			new int[] { 1, 0 },
			new int[] { -1, 0 },
			new int[] { 0, 1 },
			new int[] { 0, -1 },
			new int[] { 1, -1 },
			new int[] { -1, 0, 1 },
		};

		public void TestIsTransactionEligible_ContainsZeroValueAndNonZeroValueLines()
		{
			var eligibilityDecider = GetEligibilityDecider();

			foreach (var lineValuesSet in EligibleTransactionLineValueSets)
			{
				var transaction = CreateEligibleTransaction();
				transaction.Lines = lineValuesSet.Select(lineValue => new FakeEligibilityLiteTransactionLine() { LineAmount = lineValue })
												.ToArray();

				var isEligible = eligibilityDecider.IsTransactionEligible(transaction);
				AssertEquals("Transactions with any non zero line values should be eligible", true, isEligible);
			}
		}

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Spain,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				BranchOrgProxy = new FakeEligibilityLiteOrgHeader()
										.WithRegistrationCode(
											countryCode: CountryCodes.Spain,
											registrationNumber: "1234",
											codeType: SpainOrgCusCodeInfo.OrgCusCodes.SII),
				Lines = new FakeEligibilityLiteTransactionLine[]
				{
					new FakeEligibilityLiteTransactionLine() { LineAmount = 1.0 }
				}
			};
	}
}
