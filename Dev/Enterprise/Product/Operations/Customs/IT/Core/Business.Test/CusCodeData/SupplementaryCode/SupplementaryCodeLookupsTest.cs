using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SupplementaryCodeLookupsTest : TestCaseWithFactory
{
	public void TestCYCodeList_ParentIsJobComInvoiceLine()
	{
		SetUpTariffAndRate();

		invoiceLine.JI_Tariff = "1234512345";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;

		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		var lookups = new SupplementaryCodeLookups(supplementaryCode);

		AssertCodeDescriptionPairList(lookups.CY_CodeList, ("additionalcode", "Additional Code 1 Descriptions"));
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

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
	}

	JobComInvoiceLine invoiceLine;
}
