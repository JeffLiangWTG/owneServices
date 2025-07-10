using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsUnloadedCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_HarmonisedTariff()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var exportTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Spain, Constants.TariffTypes.Export).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, exportTariffTypePK, "12345678", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var importTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Spain, Constants.TariffTypes.Import).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, exportTariffTypePK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			CombineAssertions(() =>
			{
				var unloadedCargoDesc = Factory.New<NctsUnloadedCargoDesc>();
				unloadedCargoDesc.BY_UnloadedState = "DIF";
				unloadedCargoDesc.BY_HarmonisedTariff = "12345678";
				AssertNoMessageErrors("No error cause Tariff has 8 digits and exists", unloadedCargoDesc.BY_HarmonisedTariffInfo);

				unloadedCargoDesc.BY_HarmonisedTariff = "123456";
				AssertHasMessageError("Error cause tariff code not exits", unloadedCargoDesc.BY_HarmonisedTariffInfo, "The code you have selected is not in the list.");

				unloadedCargoDesc.BY_HarmonisedTariff = "1234567890";
				AssertNoMessageErrors("No error cause Tariff has 10 digits and exists", unloadedCargoDesc.BY_HarmonisedTariffInfo);
			});
		}
	}
}
