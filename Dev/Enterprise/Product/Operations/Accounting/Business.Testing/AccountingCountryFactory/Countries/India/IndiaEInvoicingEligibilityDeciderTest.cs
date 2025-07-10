using System.Linq;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class IndiaEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.India;

		protected override string ExpectedAdditionalTraceLog => @"Compliance Sub Type (TXI) is valid: True
Is OrgHeader GST Registered True
Is OrgHeader GST Unreported False
Place of Supply: ";

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

		public void TestIsTransactionEligible_OnlyThreeComplianceSubTypes()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(IndiaComplianceInfo.ComplianceSubTypeCodes).GetConstantValues(),
				eligibleValues: new[]
				{
					IndiaComplianceInfo.ComplianceSubTypeCodes.TXI,
					IndiaComplianceInfo.ComplianceSubTypeCodes.TXC,
					IndiaComplianceInfo.ComplianceSubTypeCodes.TXD,
				},
				setValue: (t, val) => t.ComplianceSubType = val,
				validMessage: "TXI, TXC and TXD Compliance Sub-Types");

		public void TestIsTransactionEligible_OnlyIndiaDebtorGSTRegCode_VaryCountry()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(CountryCodes).GetConstantValues(),
				eligibleValues: new[] { CountryCodes.India },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.OrgHeader.RegistrationCodes.First()).CountryCode = val,
				validMessage: "India GST Registration Code");

		public void TestIsTransactionEligible_OnlyIndiaDebtorGSTRegCode_VaryCode()
			=> AssertEligibilityForClosedSet(
				allPossibleValues: typeof(OrgCusCode.CodeTypes).GetConstantValues(),
				eligibleValues: new[] { OrgCusCode.CodeTypes.GSTCode },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.OrgHeader.RegistrationCodes.First()).CodeType = val,
				validMessage: "India GST Registration Code");

		public void TestIsTransactionEligible_OnlyIndiaDebtorGSTRegCode_VaryNumber()
			=> AssertEligibilityForNonBlank(
				allPossibleValues: new[] { "1234", "99999999", "" },
				setValue: (t, val) => ((FakeEligibilityLiteRegistrationCode)t.OrgHeader.RegistrationCodes.First()).RegistrationNumber = val,
				validMessage: "India GST Registration Code");

		public void TestIsTransactionEligible_OnlyDebtorsOutsideOfIndia_WhenGSTRegCodeIsUnreported()
		{
			var eligibilityDecider = GetEligibilityDecider();
			var t = CreateEligibleTransaction();

			var indiaCompany = Factory.NewWithValidTestData<GlbCompany>();
			indiaCompany.SetCountry(CountryCodes.India);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, PlaceOfSupplyTypes.Country.Code, PlaceOfSupplyTypes.TaxZone.Code, PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				var allPossibleValues = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(indiaCompany).ToList<CodeDescriptionPair>();
				var eligibleValue = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;

				foreach (var value in allPossibleValues)
				{
					t.PlaceOfSupply = value.Code;
					Assert("All place of supply codes are eligible without unreported GST code", eligibilityDecider.IsTransactionEligible(t));
				}

				((FakeEligibilityLiteRegistrationCode)t.OrgHeader.RegistrationCodes.First()).RegistrationNumber = "URP";
				((FakeEligibilityLiteRegistrationCode)t.OrgHeader.RegistrationCodes.First()).CodeType = OrgCusCode.CodeTypes.GSTCode;

				foreach (var value in allPossibleValues)
				{
					t.PlaceOfSupply = value.Code;
					var expectedResult = value.Code == eligibleValue;
					AssertEquals($"Only {eligibleValue} should be eligible, but '{value.Code}' is reported eligible.", expectedResult, eligibilityDecider.IsTransactionEligible(t));
				}
			}
		}

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.India,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				ComplianceSubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI,
				OrgHeader = new FakeEligibilityLiteOrgHeader()
										.WithRegistrationCode(
											countryCode: CountryCodes.India,
											registrationNumber: "1234",
											codeType: OrgCusCode.CodeTypes.GSTCode)
			};
	}
}
