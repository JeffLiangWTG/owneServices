using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SupplementaryCodeValidationTest : EU.Business.Testing.SupplementaryCodeValidationTest
{
	public void TestCheckSupplementaryCodeContainsOnlyOneQCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var messageErrorExpected = "Only one VAT Additional Code can be used at a time";

		invoiceLine.JI_SupplementaryCode1 = "Q001";
		AssertNoMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);

		invoiceLine.JI_SupplementaryCode2 = "C002";
		AssertNoMessageErrorContaining(invoiceLine.JI_SupplementaryCode2Info, messageErrorExpected);

		invoiceLine.JI_SupplementaryCode2 = "Q002";
		AssertHasMessageErrorContaining(invoiceLine.JI_SupplementaryCode2Info, messageErrorExpected);

		invoiceLine.JI_SupplementaryCode2 = "";
		var additionalSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		additionalSupplementaryCode.CY_Code = "C003";
		AssertNoMessageErrorContaining(additionalSupplementaryCode.CY_CodeInfo, messageErrorExpected);

		additionalSupplementaryCode.CY_Code = "Q003";
		AssertHasMessageErrorContaining(additionalSupplementaryCode.CY_CodeInfo, messageErrorExpected);
	}

	public void TestCheckSupplementaryQVatAdditionalCodeIsValidForSelectedVAT()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateTaxOrFeeWithRelatedVatApplicability("IMP", "99999999", ("ORD", 21m, "Q001"));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "99999999";

		var messageErrorExpected = "The set VAT additional code is not valid for this Taric code";

		invoiceLine.JI_SupplementaryCode1 = "Q001";
		AssertNoMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);

		invoiceLine.JI_SupplementaryCode1 = "Q010";
		AssertHasMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);

		invoiceLine.JI_SupplementaryCode1 = "Z001";
		AssertNoMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);
	}

	public void TestCheckSupplementaryCodeIsInTheList()
	{
		var messageErrorExpected = ListValidation.InvalidCodeMessageError;

		SetUpTariffAndRate();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "1234512345";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
		invoiceLine.JI_PrimaryPreference = "STD";

		invoiceLine.JI_SupplementaryCode1 = "Q001";
		AssertHasMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);

		invoiceLine.JI_SupplementaryCode1 = "additionalcode";
		AssertNoMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);
	}

	public void TestCheckSupplementaryCodeIsFirstCharacterValid()
	{
		var messageErrorExpected = "The first character of the code you have selected is not valid. Please make sure the first character is ‘2, 3, 4, 6, 8, A, B, C, D or P’ for ‘Taric Additional Code’, or ‘Q, R, S, T, U or Z’ for ‘National Additional Code’.";

		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_SupplementaryCode1 = "Q001";
		AssertNoMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);

		invoiceLine.JI_SupplementaryCode1 = "1001";
		AssertHasMessageErrorContaining(invoiceLine.JI_SupplementaryCode1Info, messageErrorExpected);
	}

	void SetUpTariffAndRate()
	{
		var minSmallDateRange = ZDateTime.MinSmallDateTimeValue;
		var maxSmallDateRange = ZDateTime.MaxSmallDateTimeValue;
		var testHelper = new UniversalReferenceTestDataHelper(Factory);
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;

		var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountry, "STANDARD", minSmallDateRange, maxSmallDateRange);
		var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountry, "IMP");
		var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "Duty");
		var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
		var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", currentCountry);
		var cusTariff = testHelper.CreateTariff(currentCountry, hsnTariffType.PK, "1234512345", minSmallDateRange, maxSmallDateRange, "dummy Description 0");
		var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, minSmallDateRange, maxSmallDateRange, "0", preferencePk: preferenceSTD.PK);
		testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, minSmallDateRange, maxSmallDateRange, "additionalcode", "ordernumber");
		testHelper.CreateNewOrGetExistingCusCodeType("ADDCD", "Additional Codes");
		testHelper.CreateCusCodeList(currentCountry, "ADDCD", "additionalcode", "Additional Code 1 Descriptions", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
	}
}
