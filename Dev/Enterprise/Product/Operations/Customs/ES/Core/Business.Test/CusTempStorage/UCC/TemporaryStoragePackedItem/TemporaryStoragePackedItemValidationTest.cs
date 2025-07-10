using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

sealed class TemporaryStoragePackedItemValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAPI_LineNo()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		var bill = header.Bills.AddNew();
		var item = bill.PackedItems.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("item1.API_LineNo", (ZInt)1, item.API_LineNo);
			AssertNoMessageErrors("No messege error when value is less than 99999", item.API_LineNoInfo);
			item.API_LineNo = 100000;
			AssertEquals("item1.API_LineNo", (ZInt)100000, item.API_LineNo);
			AssertHasMessageError("No messege error when value is more than 99999", item.API_LineNoInfo, "Line Nº should be lower than 99.999.");
			item.API_LineNo = 99999;
			AssertEquals("item1.API_LineNo", (ZInt)99999, item.API_LineNo);
			AssertNoMessageErrors("No messege error when value is 99999", item.API_LineNoInfo);
		});
	}

	public void TestCheckAPI_DuplicatedTariff() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var parentDataGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "ES", parentDataGroup);

		var impType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
		Factory.Save();

		var startDate = ZDate.Today.AddDays(-2);
		var endDate = ZDate.Today.AddDays(2);
		_ = helper.CreateTariff(Core.Constants.CountryCodes.Spain, impType.PK, "1234567890", startDate, endDate);
		_ = helper.CreateTariff(Core.Constants.CountryCodes.Spain, impType.PK, "1122334455", startDate, endDate);
		_ = helper.CreateTariff(Core.Constants.CountryCodes.Spain, impType.PK, "1234560011", startDate, endDate);
		Factory.Save();
		var tariffCode1 = "1234567890";
		var tariffCode2 = "1122334455";

		var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var storageBill = storageHeader.Bills.AddNew();
		var packedItem1 = storageBill.PackedItems.AddNew();
		var packedItem2 = storageBill.PackedItems.AddNew();

		AssertNoNotifications("No error when the tariff is empty.", packedItem1.API_FormattedTariffInfo);
		AssertNoNotifications("No error when the tariff is empty.", packedItem2.API_FormattedTariffInfo);

		packedItem1.API_Tariff = tariffCode1;
		packedItem2.API_Tariff = tariffCode1;
		storageHeader.RunPreSaveValidation();

		AssertNoNotifications("Error when the tariff of both the items is same.", packedItem1.API_FormattedTariffInfo);
		AssertNoNotifications("Error when the tariff of both the items is same.", packedItem2.API_FormattedTariffInfo);

		packedItem2.API_Tariff = tariffCode2;
		storageHeader.RunPreSaveValidation();

		AssertNoNotifications("Error when the tariff of both the items isn't same.", packedItem1.API_FormattedTariffInfo);
		AssertNoNotifications("Error when the tariff of both the items isn't same.", packedItem2.API_FormattedTariffInfo);

		packedItem2.API_Tariff = "1234560011";
		storageHeader.RunPreSaveValidation();

		AssertNoNotifications("Error when the tariff of each item is different but first 6 digits of both is same.", packedItem1.API_FormattedTariffInfo);
		AssertNoNotifications("Error when the tariff of each item is different but first 6 digits of both is same.", packedItem2.API_FormattedTariffInfo);
	});
}
