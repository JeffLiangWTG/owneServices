using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class UniversalReferenceHelperTest : TestCaseWithFactory
{
	public void TestGetSupplementaryQuantityUOMs()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		Factory.Save();

		var tariffWithMultipleUnits = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "XXX");

		helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var tariffWithOneUnit = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.AdditionalUOMType, "SSS");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertExceptionThrown<ArgumentNullException>("The parameter cannot be null", () => { UniversalReferenceHelper.GetSupplementaryQuantityUOMs(null); });

		AssertExceptionThrown<ArgumentNullException>("The parameter cannot be null", () =>
		{
			invoiceLine.JI_Tariff = "";
			UniversalReferenceHelper.GetSupplementaryQuantityUOMs(invoiceLine.UniversalTariff);
		});

		AssertExceptionThrown<ArgumentNullException>("The parameter cannot be null", () =>
		{
			invoiceLine.JI_Tariff = "1";
			UniversalReferenceHelper.GetSupplementaryQuantityUOMs(invoiceLine.UniversalTariff);
		});

		invoiceLine.JI_Tariff = "3333333333";
		var unitOfMeasures = UniversalReferenceHelper.GetSupplementaryQuantityUOMs(invoiceLine.UniversalTariff);
		AssertEquals("No UOMs available", 0, unitOfMeasures.Count());

		invoiceLine.JI_Tariff = "2222222222";
		unitOfMeasures = UniversalReferenceHelper.GetSupplementaryQuantityUOMs(invoiceLine.UniversalTariff);
		AssertEquals("1 UOM available", 1, unitOfMeasures.Count());
		Assert("SSS is available", unitOfMeasures.Contains("SSS"));

		invoiceLine.JI_Tariff = "1111111111";
		unitOfMeasures = UniversalReferenceHelper.GetSupplementaryQuantityUOMs(invoiceLine.UniversalTariff);
		AssertEquals("2 UOMs available", 2, unitOfMeasures.Count());
		Assert("NAR is available", unitOfMeasures.Contains("NAR"));
		Assert("XXX is available", unitOfMeasures.Contains("XXX"));
	}

	public void TestGetThridQuantityUOM()
	{
		SetupTariffForTestintQuantityUOMs(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

		AssertExceptionThrown<ArgumentNullException>("The parameter cannot be null", () => { UniversalReferenceHelper.GetThirdQuantityUOM(null); });

		AssertExceptionThrown<ArgumentNullException>("The parameter cannot be null", () =>
		{
			invoiceLine.JI_Tariff = "";
			UniversalReferenceHelper.GetThirdQuantityUOM(invoiceLine.UniversalDutyRate);
		});

		AssertExceptionThrown<ArgumentNullException>("The parameter cannot be null", () =>
		{
			invoiceLine.JI_Tariff = "1";
			UniversalReferenceHelper.GetThirdQuantityUOM(invoiceLine.UniversalDutyRate);
		});

		invoiceLine.JI_Tariff = "1602321900";
		AssertEquals("Third Quantity UOM should be empty", "", UniversalReferenceHelper.GetThirdQuantityUOM(invoiceLine.UniversalDutyRate));

		invoiceLine.JI_Tariff = "8501538190";
		AssertEquals("Third Quantity UOM should be empty", "", UniversalReferenceHelper.GetThirdQuantityUOM(invoiceLine.UniversalDutyRate));

		invoiceLine.JI_Tariff = "2009893579";
		AssertEquals("Third Quantity UOM should be empty", "", UniversalReferenceHelper.GetThirdQuantityUOM(invoiceLine.UniversalDutyRate));

		invoiceLine.JI_Tariff = "1702907100";
		AssertEquals("Third Quantity UOM should be", "DTNZ", UniversalReferenceHelper.GetThirdQuantityUOM(invoiceLine.UniversalDutyRate));

		invoiceLine.JI_Tariff = "2208403900";
		AssertEquals("Third Quantity UOM should be", "HLT", UniversalReferenceHelper.GetThirdQuantityUOM(invoiceLine.UniversalDutyRate));
	}

	public static RefCusTariffType SetupTariffForTestintQuantityUOMs(BusinessObjectFactory factory, string tariffTypeCode = Constants.TariffTypes.Import)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var dataGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: dataGroup);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, tariffTypeCode);
		var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Italy, "1011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "ERGA OMNES");
		helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(+1));
		var preference100 = helper.CreatePreferenceForCountry("100", "Desc", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		factory.Save();
		var rateCodeType = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY", description: "DESC", ensureDataGroupingExists: true);
		factory.Save();

		void ConfigureTariff(ZString tariffCode, ZString rateFormula, ZString supplementaryUnit, params ZString[] ratesUOM)
		{
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, tariffCode.SubstringSafe(0, 5), rateCodeType.PK);
			factory.Save();
			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula, preferencePk: preference100.PK, dataGrouping: "EUN");
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			if (!supplementaryUnit.IsEmpty)
			{
				helper.CreateTariffUOM(tariff, "CU2", supplementaryUnit, dataGrouping: "IT");
			}
			ratesUOM.Where(x => !x.IsEmpty).ToList().ForEach(rateUOMs => helper.CreateRateUOM(rate.PK, rateUOMs));
		}

		ConfigureTariff("1602321900", "VFD * 0.080", "");
		ConfigureTariff("8501538190", "787.81 * [TNE]", "", "TNE");
		ConfigureTariff("2009893579", "VFD * 0.336 + 20.600 * [DTN]", "", "DTN");
		ConfigureTariff("9102290000", "MIN(MAX(VFD * 0.045, 0.300 * [NAR]), 0.800 * [NAR])", "NAR", "NAR");
		ConfigureTariff("0710400090", "VFD * 0.051 + 9.400 * [DTNE]", "", "DTNE");
		ConfigureTariff("1702907100", "0.400 * [DTNZ]", "", "DTNZ");
		ConfigureTariff("2208403900", "0.600 * [ASVX] + 3.200 * [HLT]", "ASVX", "ASVX", "HLT");

		factory.Save();
		return tariffType;
	}

	public void TestGetPortTaxRateList()
	{
		new ITUniversalReferenceTestDataHelper(Factory).SetupPortTaxRates();
		Factory.Save();

		var portTaxRateList = UniversalReferenceHelper.GetPortTaxRateList(Factory);
		AssertEquals("Port Tax Rate List count should be", 3, portTaxRateList.Count);
		CombineAssertions(() =>
		{
			AssertEquals("Port Tax Rates lookups codes", "A1, A2, A3", portTaxRateList.CodesAsString);
			AssertSame("Cached", UniversalReferenceHelper.GetPortTaxRateList(Factory), portTaxRateList);
		});
	}

	[TestDate(2024, 10, 11)]
	public void TestGetEuropeanUnionEUNCustomsUQCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ASVX", "Hectolitre", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CTM", "Carats", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTQ", "Cubic meter", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2024, 10, 10));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TNE", "Tonne", new ZDateTime(2024, 10, 12), ZDateTime.MaxSmallDateTimeUtc);
		Factory.Save();

		CombineAssertions(() =>
		{
			var list = UniversalReferenceHelper.GetEuropeanUnionEUNCustomsUQCodeList(Factory);
			AssertEquals("CodesAsString", "ASVX, CTM", list.CodesAsString);
			AssertSame("Cached", list, UniversalReferenceHelper.GetEuropeanUnionEUNCustomsUQCodeList(Factory));
		});
	}
}
