using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public class CusSealValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBK_SealNumber()
		{
			seal.BK_SealNumber = ZString.Empty;
			AssertHasMessageErrorContaining("When no value entered", seal.BK_SealNumberInfo, MandatoryValidation.YouHaveNotEntered);
			seal.BK_SealNumber = "111";
			AssertNoMessageErrorContaining("When a value entered", seal.BK_SealNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestBK_SealType()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusMapType("MSELT", "BTH", "Global Manifest Seal Type", true);
			helper.CreateCusMap("MSELT", "E", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("MSELT", "M", "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			factory.Save();

			seal.Validation.ValidateAll();
			AssertHasMessageErrorContaining("When no value entered", seal.BK_SealTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("When no list entry", seal.BK_SealTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

			seal.BK_SealType = "_";
			AssertNoMessageErrorContaining("When a value entered", seal.BK_SealTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("When invalid list entry", seal.BK_SealTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

			seal.BK_SealType = SealTypeList.Codes.ElectronicSeal;
			AssertNoMessageErrorContaining("When a value entered", seal.BK_SealTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("When valid list entry", seal.BK_SealTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestBK_UnloadingState()
		{
			seal.Validation.ValidateAll();
			AssertHasMessageErrorContaining("When no value entered", seal.BK_UnloadingStateInfo, MandatoryValidation.YouHaveNotEntered);

			seal.BK_UnloadingState = UnloadingStates.Codes.New;
			AssertNoMessageErrorContaining("When a value entered", seal.BK_UnloadingStateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBK_SealingPartyType()
		{
			const string expectedMessage = "The code you have selected is not in the list.";
			CreateCusMap(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
				RefCusMapTypeList.Codes.STYPE,
				Core.Constants.ContainerSealParties.Codes.CarrierShippingLine);
			Factory.Save();

			seal.BK_SealingPartyType = "_";
			AssertHasMessageErrorContaining(
				"Should show error for invalid sealing party type code",
				seal.BK_SealingPartyTypeInfo,
				expectedMessage);

			seal.BK_SealingPartyType = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			seal.Validation.ValidateAll();
			AssertNoMessageErrorContaining(
				"Should not show error for valid sealing party type code",
				seal.BK_SealingPartyTypeInfo,
				expectedMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			seal = container.AdditionalSeals.AddNew();
			seal.BK_SealNumber = "SEAL1";
		}

		void CreateCusMap(string countryCode, string typeName, string wtgCode, string customsCode = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(countryCode);
			helper.CreateCusMapType(typeName, MapDirectionList.Codes.BTH, typeName, false);
			helper.CreateCusMap(typeName, wtgCode, customsCode ?? wtgCode, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), countryCode);
		}

		CusSeal seal;
	}
}
