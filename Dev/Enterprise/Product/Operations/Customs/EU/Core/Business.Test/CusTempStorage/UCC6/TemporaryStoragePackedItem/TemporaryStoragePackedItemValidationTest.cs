using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStoragePackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_TariffMultiple()
		{
			var tariffCode1 = "1234567890";
			var tariffCode2 = "1122334455";
			var message = "Items under the same bill can’t have the same Tariff Sub Heading Value.";

			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var storageBill = storageHeader.Bills.AddNew();
			var packedItem1 = storageBill.PackedItems.AddNew();
			var packedItem2 = storageBill.PackedItems.AddNew();

			CombineAssertions("There should be no error when the tariff of both the items is empty.", () =>
			{
				AssertNoMessageError(packedItem1.API_FormattedTariffInfo, message);
				AssertNoMessageError(packedItem2.API_FormattedTariffInfo, message);
			});

			packedItem1.API_Tariff = tariffCode1;
			packedItem2.API_Tariff = tariffCode1;
			storageHeader.RunPreSaveValidation();

			CombineAssertions("Each item should have an error when the tariff of both the items is same.", () =>
			{
				AssertHasMessageError(packedItem1.API_FormattedTariffInfo, message);
				AssertHasMessageError(packedItem2.API_FormattedTariffInfo, message);
			});

			packedItem2.API_Tariff = tariffCode2;
			storageHeader.RunPreSaveValidation();

			CombineAssertions("There should be no error when the tariff of each item is fully different from the other.", () =>
			{
				AssertNoMessageError(packedItem1.API_FormattedTariffInfo, message);
				AssertNoMessageError(packedItem2.API_FormattedTariffInfo, message);
			});

			packedItem2.API_Tariff = "1234560011";
			storageHeader.RunPreSaveValidation();

			CombineAssertions("Each item should have an error when the tariff of each item is different but the first 6 digits of both the tariffs is same.", () =>
			{
				AssertHasMessageError(packedItem1.API_FormattedTariffInfo, message);
				AssertHasMessageError(packedItem2.API_FormattedTariffInfo, message);
			});

			var packedItem3 = storageBill.PackedItems.AddNew();
			packedItem2.API_Tariff = tariffCode2;
			packedItem3.API_Tariff = "";
			storageHeader.RunPreSaveValidation();

			CombineAssertions("When the tariff of one of the items is empty and the other two items have fully different tariffs,  there should be no error.", () =>
			{
				AssertNoMessageError(packedItem1.API_FormattedTariffInfo, message);
				AssertNoMessageError(packedItem2.API_FormattedTariffInfo, message);
				AssertNoMessageError(packedItem3.API_FormattedTariffInfo, message);
			});

			packedItem2.API_Tariff = tariffCode1;
			storageHeader.RunPreSaveValidation();

			CombineAssertions("When the tariff of one of the items is empty and the other two items have the same tariff, then the item without a tariff should not have an error and each of the two other items with a tariff should have an error.", () =>
			{
				AssertHasMessageError(packedItem1.API_FormattedTariffInfo, message);
				AssertHasMessageError(packedItem2.API_FormattedTariffInfo, message);
				AssertNoMessageError(packedItem3.API_FormattedTariffInfo, message);
			});

			packedItem3.API_Tariff = tariffCode2;
			storageHeader.RunPreSaveValidation();

			CombineAssertions("When two items have the same tariff and the third one has a fully different tariff, then the item with a different tariff should not have an error and each of the two other items with the same tariff should have an error.", () =>
			{
				AssertHasMessageError(packedItem1.API_FormattedTariffInfo, message);
				AssertHasMessageError(packedItem2.API_FormattedTariffInfo, message);
				AssertNoMessageError(packedItem3.API_FormattedTariffInfo, message);
			});
		}

		public void TestCheckAPI_Tariff()
		{
			using var deciderTestContext = new TemporaryStoragePackedItemValidationDeciderTestContext<ITemporaryStoragePackedItemValidationDecider>(Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGroup);

			var impType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var startDate = ZDate.Today.AddDays(-2);
			var endDate = ZDate.Today.AddDays(2);
			helper.CreateTariff(Core.Constants.CountryCodes.Latvia, impType.PK, "1111111111", startDate, endDate);
			Factory.Save();

			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			deciderTestContext.EnableRule(x => x.IsAPI_TariffMandatory);

			CombineAssertions("When IsAPI_GoodsDescriptionMandatory = true", () =>
			{
				var storageBill = storageHeader.Bills.AddNew();
				var packedItem = storageBill.PackedItems.AddNew();
				packedItem.API_Tariff = "111111111";
				AssertHasMessageError(packedItem.API_FormattedTariffInfo, "The code you have selected is not in the list.");
				packedItem.API_Tariff = "12345678";
				AssertHasMessageError(packedItem.API_FormattedTariffInfo, "The code you have selected is not in the list.");
				packedItem.API_Tariff = "11111111";
				AssertNoMessageError(packedItem.API_FormattedTariffInfo, "The code you have selected is not in the list.");
				packedItem.API_Tariff = "1234567890";
				AssertHasMessageError(packedItem.API_FormattedTariffInfo, "The code you have selected is not in the list.");
				packedItem.API_Tariff = "1111111111";
				AssertNoMessageError(packedItem.API_FormattedTariffInfo, "The code you have selected is not in the list.");

				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_FormattedTariffInfo);
			});

			deciderTestContext.DisableRule(x => x.IsAPI_TariffMandatory);

			var storageBill = storageHeader.Bills.AddNew();
			var packedItem = storageBill.PackedItems.AddNew();
			packedItem.API_Tariff = "";

			AssertNoMessageErrorContaining("When IsAPI_TariffMandatory = false", packedItem.API_FormattedTariffInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_GoodsDescription()
		{
			using var deciderTestContext = new TemporaryStoragePackedItemValidationDeciderTestContext<ITemporaryStoragePackedItemValidationDecider>(Factory);
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			deciderTestContext.EnableRule(x => x.IsAPI_GoodsDescriptionMandatory);

			CombineAssertions("When IsAPI_GoodsDescriptionMandatory = true", () =>
			{
				var storageBill = storageHeader.Bills.AddNew();
				var packedItem = storageBill.PackedItems.AddNew();
				packedItem.API_GoodsDescription = "";
				AssertHasMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				packedItem.API_GoodsDescription = "abcd";
				AssertNoMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});

			deciderTestContext.DisableRule(x => x.IsAPI_GoodsDescriptionMandatory);

			var storageBill = storageHeader.Bills.AddNew();
			var packedItem = storageBill.PackedItems.AddNew();
			packedItem.API_GoodsDescription = "";

			AssertNoMessageErrorContaining("When IsAPI_GoodsDescriptionMandatory = false", packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPI_GrossWeight()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var storageBill = storageHeader.Bills.AddNew();
			var packedItem = storageBill.PackedItems.AddNew();
			packedItem.API_GrossWeight = 0;
			AssertHasMessageError(packedItem.API_GrossWeightInfo, "The entered value must be greater than 0.");
			packedItem.API_GrossWeight = -1;
			AssertHasMessageError(packedItem.API_GrossWeightInfo, "The entered value must be greater than 0.");
			packedItem.API_GrossWeight = 1;
			AssertNoMessageErrors(packedItem.API_GrossWeightInfo);

			packedItem.AsycudaPackPackedItemPivots.AddNew();
			packedItem.API_GrossWeight = -1;
			AssertHasMessageError(packedItem.API_GrossWeightInfo, "The entered value must be greater than or equal to 0.");
			packedItem.API_GrossWeight = 0;
			AssertNoMessageErrors(packedItem.API_GrossWeightInfo);
			packedItem.API_GrossWeight = 1;
			AssertNoMessageErrors(packedItem.API_GrossWeightInfo);
		}

		public void TestCheckAPI_GrossWeightUQ()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var storageBill = storageHeader.Bills.AddNew();
			var packedItem = storageBill.PackedItems.AddNew();
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(packedItem.API_GrossWeightUQInfo, packedItem.API_GrossWeightInfo);
		}

		public void TestCheckAPI_NetWeightUQ_InvalidCode()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			var packedItem = tempBill.PackedItems.AddNew();

			ValidationTestHelper.AssertInvalidCodeMessageError(packedItem.API_NetWeightUQInfo, "PP", "KG");
		}
		public void TestCheckAPI_NetWeightUQ_MustBeEntered()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			var packedItem = tempBill.PackedItems.AddNew();

			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(packedItem.API_NetWeightUQInfo, packedItem.API_NetWeightInfo);
		}
	}
}
