using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_GoodsValue_Mandatory()
		{
			const string expectedMessage = "You have not entered a Postal Value.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			var propertyInfo = packedItem.API_GoodsValueInfo;

			AssertNoMessageError("Initial", propertyInfo, expectedMessage);
			CombineAssertions(() =>
			{
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				var additionalInfo = bill.AdditionalInfos.AddNew();
				packedItem.Validation.ValidateAPI_GoodsValue();
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; Has AdditionalInfo; API_GoodsValue empty", propertyInfo, expectedMessage);

				additionalInfo.CSI_Code = EUICS2AdditionalInfoTypes.Codes.CL701_10600;
				packedItem.Validation.ValidateAPI_GoodsValue();
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; Has AdditionalInfo with Code != 10900; API_GoodsValue empty", propertyInfo, expectedMessage);

				var matchingAdditionalInfo = bill.AdditionalInfos.AddNew();
				matchingAdditionalInfo.CSI_Code = EUICS2AdditionalInfoTypes.Codes.CL701_10900;
				packedItem.Validation.ValidateAPI_GoodsValue();
				AssertHasMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; Has AdditionalInfo with Code == 10900; API_GoodsValue empty", propertyInfo, expectedMessage);

				packedItem.API_GoodsValue = 1;
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; Has AdditionalInfo with Code == 10900; API_GoodsValue not empty", propertyInfo, expectedMessage);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				packedItem.API_GoodsValue = 0;
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == false; Has AdditionalInfo with Code == 10900; API_GoodsValue empty", propertyInfo, expectedMessage);
			});
		}

		public void TestCheckAPI_TypeOfGoods_Mandatory()
		{
			const string expectedMessage = "You have not entered a Type of Goods.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			var propertyInfo = packedItem.API_TypeOfGoodsInfo;

			AssertNoMessageError("Initial", propertyInfo, expectedMessage);
			CombineAssertions(() =>
			{
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				var fiscalReference = bill.AdditionalFiscalReferences.AddNew();
				packedItem.Validation.ValidateAPI_TypeOfGoods();
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; API_TypeOfGoods empty; has FiscalReferences, no AddiditionalInfo", propertyInfo, expectedMessage);

				var additionalInfo = bill.AdditionalInfos.AddNew();
				packedItem.Validation.ValidateAPI_TypeOfGoods();
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; API_TypeOfGoods empty; has FiscalReferences, has AddiditionalInfo", propertyInfo, expectedMessage);

				additionalInfo.CSI_Code = EUICS2AdditionalInfoTypes.Codes.CL701_10600;
				packedItem.Validation.ValidateAPI_TypeOfGoods();
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; API_TypeOfGoods empty; has FiscalReferences, has AddiditionalInfo with Code != 10900", propertyInfo, expectedMessage);

				var matchingAdditionalInfo = bill.AdditionalInfos.AddNew();
				matchingAdditionalInfo.CSI_Code = EUICS2AdditionalInfoTypes.Codes.CL701_10900;
				packedItem.Validation.ValidateAPI_TypeOfGoods();
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; API_TypeOfGoods empty; has FiscalReferences, has AddiditionalInfo with Code = 10900", propertyInfo, expectedMessage);

				fiscalReference.Delete();
				packedItem.Validation.ValidateAPI_TypeOfGoods();
				AssertHasMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; API_TypeOfGoods empty; no FiscalReferences, has AddiditionalInfo with Code = 10900", propertyInfo, expectedMessage);

				packedItem.API_TypeOfGoods = "TEST";
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == true; API_TypeOfGoods not empty; no FiscalReferences, has AddiditionalInfo with Code = 10900", propertyInfo, expectedMessage);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				packedItem.API_TypeOfGoods = ZString.Empty;
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == false; API_TypeOfGoods empty; no FiscalReferences, has AddiditionalInfo with Code = 10900", propertyInfo, expectedMessage);
			});
		}

		[TestDate]
		public void TestCheckAPI_TypeOfGoods_ListValidation()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "IC2GT");
			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var matchingCodeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "A", "EuropeanUnion 1", yesterday, tomorrow);
			var matchingCodeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "B", "EuropeanUnion 2", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting() as AsycudaPackedItem;

			CombineAssertions(() =>
			{
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				ValidationTestHelper.AssertInvalidCodeMessageError(packedItem.API_TypeOfGoodsInfo, new ZString[] { "XX", "YY" }, packedItem.Lookups.TypeOfGoodsList.GetAllCodesZString());

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				packedItem.API_TypeOfGoods = "XX";
				AssertNoMessageError("IsPackedItemTypeOfGoodsAndGoodsValueEnabled == false; invalid Code", packedItem.API_TypeOfGoodsInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckAPI_Tariff()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();

			void AssertValidation(string indicator)
			{
				bill.ConsigneePersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP1;
				bill.ShipperPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP1;
				header.SpecificCircumstanceIndicator = indicator;

				packedItem.Validation.ValidateAPI_Tariff();
				ValidationTestHelper.AssertFieldIsNotMandatory(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered, $"API_Tariff is not mandatory for personal consignor/consignee {indicator}");

				bill.ConsigneePersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;

				packedItem.Validation.ValidateAPI_Tariff();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered, $"API_Tariff is mandatory for {indicator}");
			}

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered, "By default API_Tariff is not mandatory");

				AssertValidation(EUICS2SpecificCircumstanceList.Codes.F14);
				AssertValidation(EUICS2SpecificCircumstanceList.Codes.F15);
				AssertValidation(EUICS2SpecificCircumstanceList.Codes.F22);
				AssertValidation(EUICS2SpecificCircumstanceList.Codes.F24);
				AssertValidation(EUICS2SpecificCircumstanceList.Codes.F26);
			});

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_GoodsDescriptionInfo);
		}

		public void TestCheckAPI_Tariff_Length()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();

			packedItem.API_Tariff = "1234567";
			AssertHasMessageErrorContaining($"Tariff Length: {packedItem.API_Tariff.Length}", packedItem.API_TariffInfo, "Tariff Code must have 6 or 8 digits.");

			packedItem.API_Tariff = "12345678";
			AssertNoMessageErrorContaining($"Tariff Length: {packedItem.API_Tariff.Length}", packedItem.API_TariffInfo, "Tariff Code must have 6 or 8 digits.");

			packedItem.API_Tariff = "123456";
			AssertNoMessageErrorContaining($"Tariff Length: {packedItem.API_Tariff.Length}", packedItem.API_TariffInfo, "Tariff Code must have 6 or 8 digits.");
		}

		public void TestCheckAPI_GoodsDescription()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				var pack = bill.Packs.AddNew();
				var packedItem = pack.PackedItemForTesting();

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_GoodsDescriptionInfo);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_GoodsDescriptionInfo);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(packedItem.API_GoodsDescriptionInfo);

				var maxlength = 512;
				packedItem.API_GoodsDescription = new string('X', maxlength) + $"Text after {maxlength}";
				AssertHasMessageErrorContaining($"Longer {maxlength} characters", packedItem.API_GoodsDescriptionInfo, $"Length of Goods Description must not exceed {maxlength} characters.");

				packedItem.API_GoodsDescription = new string('X', maxlength);
				AssertNoMessageErrorContaining($"Not longer {maxlength} characters", packedItem.API_GoodsDescriptionInfo, $"Length of Goods Description must not exceed {maxlength} characters.");
			});
		}

		[TestDate]
		public void TestCheckAPI_ChemicalSubstanceCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "ECICS");

			var today = ZDateTime.Today;
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "ValidCode", "Valid Chemical Code", today.AddDays(-1), today.AddDays(1));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var tempBill = header.Bills.AddNew();

			var pack = tempBill.Packs.AddNew();
			var packedItem = pack.PackedItem;

			ValidationTestHelper.AssertInvalidCodeMessageError(packedItem.API_ChemicalSubstanceCodeInfo, "000000-0", "ValidCode");
		}

		public void TestTariffListValidationCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "84145995", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "1A", "DRUM STEEL", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();

			packedItem.API_Tariff = "8414";
			AssertNoMessageError("Valid heading", packedItem.API_TariffInfo, ListValidation.InvalidCodeMessageError);

			packedItem.API_Tariff = "8888";
			AssertHasMessageError("Invalid heading", packedItem.API_TariffInfo, ListValidation.InvalidCodeMessageError);

			packedItem.API_Tariff = "841459";
			AssertNoMessageError("Valid subheading", packedItem.API_TariffInfo, ListValidation.InvalidCodeMessageError);

			packedItem.API_Tariff = "888888";
			AssertHasMessageError("Invalid subheading", packedItem.API_TariffInfo, ListValidation.InvalidCodeMessageError);

			packedItem.API_Tariff = "84145995";
			AssertNoMessageError("Valid tariff", packedItem.API_TariffInfo, ListValidation.InvalidCodeMessageError);

			packedItem.API_Tariff = "88888888";
			AssertHasMessageError("Invalid tariff", packedItem.API_TariffInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
