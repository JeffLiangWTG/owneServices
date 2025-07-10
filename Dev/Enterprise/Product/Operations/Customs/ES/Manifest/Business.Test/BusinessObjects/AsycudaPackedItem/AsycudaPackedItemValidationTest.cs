using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_Tariff()
		{
			var listMessageErrorText = "The code you have selected is not in the list.";
			var countryCode = Core.Constants.CountryCodes.Spain;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, Universal.Constants.TariffTypes.Export);
			helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "0101", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "010129", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "38012090", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
			header.AMA_ManifestType = ESManifestTypes.Codes.ICS;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItems.AddNewPackedItem();
			CombineAssertions(() =>
			{
				packedItem.API_Tariff = "0102";
				AssertHasMessageErrorContaining("When API_Tariff has an incorrect 4 digit code there should be an error", packedItem.API_TariffInfo, listMessageErrorText);
				packedItem.API_Tariff = "0101";
				AssertNoMessageErrors("When API_Tariff has a correct 4 digit code there should not be an error", packedItem.API_TariffInfo);
				packedItem.API_Tariff = "010128";
				AssertHasMessageErrorContaining("When API_Tariff has an incorrect 6 digit code there should be an error", packedItem.API_TariffInfo, listMessageErrorText);
				packedItem.API_Tariff = "010129";
				AssertNoMessageErrors("When API_Tariff has a correct 6 digit code there should not be an error", packedItem.API_TariffInfo);
				packedItem.API_Tariff = "38012091";
				AssertHasMessageErrorContaining("When API_Tariff has an incorrect 8 digit code there should be an error", packedItem.API_TariffInfo, listMessageErrorText);
				packedItem.API_Tariff = "38012090";
				AssertNoMessageErrors("When API_Tariff has a correct 8 digit code there should not be an error", packedItem.API_TariffInfo);
				packedItem.API_Tariff = "01010";
				AssertHasMessageErrorContaining("When API_Tariff has a code with 5 digits there should be an error", packedItem.API_TariffInfo, "Only 4, 6 or 8 digits are allowed");
			});
		}
	}
}
