using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using RefCusCodeListType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;
using RefDataGrouping = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;
using TariffTypes = Enterprise.Customs.Universal.Constants.TariffTypes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public sealed class CargoDescTestHelper : TestCaseWithFactory
{
	public static void AssertHarmonisedTariffCharacterCheck(BusinessObjectFactory factory, NctsCommonCargoDesc cargoDesc) => CombineAssertions("Tariff code length is not 6,8 or 11", () =>
	{
		new RefDataTestHelper(factory).CreateTariffsForTransit();

		cargoDesc.BY_HarmonisedTariff = "";
		AssertNoMessageError("No Error message should be displayed for not having a tariff code", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "123";
		AssertHasMessageError("Displays the NP70205 error message for not having the correct length", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "12345";
		AssertHasMessageError("Displays the NP70205 error message for not having the correct length", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "710121";
		AssertNoMessageError("a tariff code of length 6 should have no error message with a valid WCH HSN Tariff", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "710120";
		AssertHasMessageError("a tariff code of length 6 should have an error message with an invalid WCH HSN Tariff", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "12130091";
		AssertNoMessageError("a tariff code of length 8 should have no error message with a valid CH EXP Tariff", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "12130092";
		AssertHasMessageError("a tariff code of length 8 should have an error message with an invalid CH EXP Tariff", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "04069099";
		AssertNoMessageError("a tariff code of length 8 should have no error message with a valid 11-digit CH EXP Tariff", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);

		cargoDesc.BY_HarmonisedTariff = "84061000001";
		AssertNoMessageError("a tariff code of length 11 should have no error message with a valid CH EXP Tariff", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);
		AssertNoMessageErrors("Does not have EU error messages", cargoDesc.BY_HarmonisedTariffInfo);

		cargoDesc.BY_HarmonisedTariff = "84061000002";
		AssertHasMessageError("a tariff code of length 11 should have an error message with an invalid CH EXP Tariff", cargoDesc.BY_HarmonisedTariffInfo, PassarValidationMessages.MessageNP70205);
	});

	public static void AssertCus4NumberListValidation(NctsCommonCargoDesc cargoDesc) => CombineAssertions(() =>
	{
		const string message = "CUS Code does not belong to the commodity code";
		const string wcoTariff = "W23456";
		const string expTariff = "E2345678";

		var factory = cargoDesc.Factory;

		var testHelper = new RefDataTestHelper(factory);
		testHelper.CreateTariffs(TariffTypes.HarmonizedSystem, dataGrouping: RefDataGrouping.WorldCustomsOrganisationWCO).CreateTariff(wcoTariff);
		testHelper.CreateTariffs(TariffTypes.Export, dataGrouping: RefDataGrouping.EuropeanUnionEUN).CreateTariff(expTariff);
		testHelper.CreateCodeList(RefCusCodeListType.Code_ECICS, dataGrouping: RefDataGrouping.EuropeanUnionEUN)
			.CreateCode("C01")
			.CreateCode("C02").WithAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CombinedNomenclatureCode, wcoTariff)
			.CreateCode("C03").WithAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CombinedNomenclatureCode, expTariff);
		factory.Save();

		cargoDesc.BY_HarmonisedTariff = wcoTariff;
		cargoDesc.BY_CusC4Number = "C01";
		AssertHasMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		cargoDesc.BY_CusC4Number = "C02";
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		cargoDesc.BY_CusC4Number = "C03";
		AssertHasMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		cargoDesc.BY_CusC4Number = "C99";
		AssertHasMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);

		cargoDesc.BY_HarmonisedTariff = expTariff;
		cargoDesc.BY_CusC4Number = "C01";
		AssertHasMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		cargoDesc.BY_CusC4Number = "C02";
		AssertHasMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		cargoDesc.BY_CusC4Number = "C03";
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		cargoDesc.BY_CusC4Number = "C99";
		AssertHasMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);

		cargoDesc.BY_HarmonisedTariff = ZString.Empty;
		cargoDesc.BY_CusC4Number = "C01";
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, ListValidation.InvalidCodeMessageError.ToString());
		cargoDesc.BY_CusC4Number = "C02";
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, ListValidation.InvalidCodeMessageError.ToString());
		cargoDesc.BY_CusC4Number = "C03";
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, ListValidation.InvalidCodeMessageError.ToString());
		cargoDesc.BY_CusC4Number = "C99";
		AssertNoMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, message);
		AssertHasMessageError(AssertionMessage(), cargoDesc.BY_CusC4NumberInfo, ListValidation.InvalidCodeMessageError.ToString());

		string AssertionMessage([CallerLineNumber] int callerLineNumber = 0) => $"[{callerLineNumber}] Tariff={cargoDesc.BY_HarmonisedTariff}";
	});
}
