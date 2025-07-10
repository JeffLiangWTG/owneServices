using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing;

sealed class TemporaryStoragePackedItemValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAPI_Tariff()
	{
		const string errorMessage = "You have not entered a Tariff.";

		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
		header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;

		var tempBill = header.Bills.AddNew();
		var packedItem = tempBill.PackedItems.AddNew();
		packedItem.API_Tariff = ZString.Empty;

		AssertHasMessageError("API_Tariff is mandatory when UCC6", packedItem.API_TariffInfo, errorMessage);

		packedItem.API_Tariff = "1234567890";
		AssertNoMessageError("When API_Tariff is not empty", packedItem.API_TariffInfo, errorMessage);

		packedItem.API_Tariff = ZString.Empty;
		header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
		header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;

		packedItem.Validation.ValidateAPI_Tariff();

		AssertNoMessageError("API_Tariff is not mandatory when UCC5 && TC", packedItem.API_TariffInfo, errorMessage);

		header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
		header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;

		AssertNoMessageError("API_Tariff is not mandatory when UCC5 && TS", packedItem.API_TariffInfo, errorMessage);
	}
}
