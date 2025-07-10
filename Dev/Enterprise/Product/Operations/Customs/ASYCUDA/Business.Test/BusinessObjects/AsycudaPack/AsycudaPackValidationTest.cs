using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLinePrice()
		{
			var pack = GetPack(Core.Constants.CountryCodes.Fiji, "ASY");
			RunValueAndCurrencyTest(pack.LinePriceInfo, pack.LinePriceCurrencyInfo);
		}

		public void TestMarks()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.PackageMarksByContainerMode, "You must not sound your horn", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MANDATORYFORCONTAINERMODE, "XXX");
			Factory.Save();

			var packZA = GetPack(Core.Constants.CountryCodes.SouthAfrica, "HAB");
			packZA.Validation.ValidateAPA_MarksAndNumbers();
			AssertNoMessageErrorContaining(packZA.APA_MarksAndNumbersInfo, "horn");

			var packGB = GetPack(Core.Constants.CountryCodes.UnitedKingdom, "ICS");
			packGB.Validation.ValidateAPA_MarksAndNumbers();
			AssertNoMessageErrorContaining(packGB.APA_MarksAndNumbersInfo, "horn");
			packGB.Bill.Header.AMA_ContainerMode = "AAA";
			packGB.Validation.ValidateAPA_MarksAndNumbers();
			AssertNoMessageErrorContaining(packGB.APA_MarksAndNumbersInfo, "horn");

			packZA.Bill.Header.AMA_ContainerMode = "XXX";
			packZA.Validation.ValidateAPA_MarksAndNumbers();
			AssertHasMessageErrorContaining(packZA.APA_MarksAndNumbersInfo, "horn");
			packZA.APA_MarksAndNumbers = "Poop";
			AssertNoMessageErrorContaining(packZA.APA_MarksAndNumbersInfo, "horn");
		}

		public void TestValidateContainer()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			AssertNoNotifications(pack.ContainerPKInfo);

			pack.ContainerPK = ZGuid.Invalid;
			AssertHasErrors(pack.ContainerPKInfo);
		}

		public void TestContainerPK_ForContainerizedCargo()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = ZGuid.Empty;
			AssertHasMessageError(pack.ContainerPKInfo, "For containerized cargo, all packs must be linked to a container.");

			var cont = header.Containers.AddNew();
			pack.ContainerPK = cont.PK;
			AssertNoMessageError(pack.ContainerPKInfo, "For containerized cargo, all packs must be linked to a container.");
		}

		public void TestPackQtyAndUQ()
		{
			var pack = GetPack(Core.Constants.CountryCodes.Fiji, "ASY");
			pack.Validation.ValidateAll();
			AssertNoMessageErrorContaining(pack.APA_PackQtyInfo, "zero");
			pack.APA_PackUQ = "~Z";
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, "list");
			pack.APA_PackUQ = pack.Lookups.PackUQList[0].Code;
			AssertNoMessageErrorContaining(pack.APA_PackUQInfo, "list");
			AssertHasMessageErrorContaining(pack.APA_PackQtyInfo, "zero");
			pack.APA_PackQty = 69;
			AssertNoMessageErrorContaining(pack.APA_PackQtyInfo, "zero");

			pack.APA_VINNumber = "VIN";
			AssertHasMessageError(pack.APA_PackQtyInfo, "The No of Packs must be 1 if the VIN Number is captured.");
			pack.APA_PackQty = 1;
			AssertNoMessageError(pack.APA_PackQtyInfo, "The No of Packs must be 1 if the VIN Number is captured.");
			pack.APA_PackQty = 69;
			pack.APA_VINNumber = "";
			AssertNoMessageError(pack.APA_PackQtyInfo, "The No of Packs must be 1 if the VIN Number is captured.");
		}

		public void TestWeightAndUQ()
		{
			var pack = GetPack(Core.Constants.CountryCodes.Fiji, "ASY");
			pack.Validation.ValidateAll();
			AssertNoMessageErrorContaining(pack.APA_WeightInfo, "zero");
			pack.APA_WeightUQ = "~Z";
			pack.Validation.ValidateAPA_Weight();
			AssertHasMessageErrorContaining(pack.APA_WeightUQInfo, "list");
			pack.APA_WeightUQ = pack.Lookups.WeightUQList[0].Code;
			AssertNoMessageErrorContaining(pack.APA_WeightUQInfo, "list");
			AssertHasMessageErrorContaining(pack.APA_WeightInfo, "zero");
			pack.APA_Weight = 69;
			AssertNoMessageErrorContaining(pack.APA_WeightInfo, "zero");
			pack.APA_WeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_WeightUQInfo, "You have not entered a Weight Unit.");
		}

		public void TestVolumeAndUQ()
		{
			var pack = GetPack(Core.Constants.CountryCodes.Fiji, "ASY");
			pack.Validation.ValidateAll();
			AssertNoMessageErrorContaining(pack.APA_VolumeInfo, "zero");
			pack.APA_VolumeUQ = "~Z";
			pack.Validation.ValidateAPA_Volume();
			AssertHasMessageErrorContaining(pack.APA_VolumeUQInfo, "list");
			pack.APA_VolumeUQ = pack.Lookups.VolumeUQList[0].Code;
			AssertNoMessageErrorContaining(pack.APA_VolumeUQInfo, "list");
			AssertHasMessageErrorContaining(pack.APA_VolumeInfo, "zero");
			pack.APA_Volume = 69;
			AssertNoMessageErrorContaining(pack.APA_VolumeInfo, "zero");
		}

		AsycudaPack GetPack(string countryCode, string manifestType)
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, countryCode, manifestType);
			header.FillWithValidTestData();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			return pack;
		}

		static void RunValueAndCurrencyTest(ZPropertyInfo valueInfo, ZPropertyInfo currencyInfo)
		{
			AssertNoMessageErrorContaining(valueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(currencyInfo, MandatoryValidation.YouHaveNotEntered);
			valueInfo.SetValueFromString("10");
			AssertNoMessageErrorContaining(valueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(currencyInfo, MandatoryValidation.YouHaveNotEntered);
			currencyInfo.SetValueFromString(Core.Constants.CurrencyCodes.UnitedKingdom);
			AssertNoMessageErrorContaining(currencyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(currencyInfo, ListValidation.InvalidCodeError);
			AssertNoErrorContaining(currencyInfo, ListValidation.InvalidCodeError);
			currencyInfo.SetValueFromString("XXX");
			AssertNoMessageErrorContaining(currencyInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(currencyInfo, ListValidation.InvalidCodeError);
		}
	}
}
