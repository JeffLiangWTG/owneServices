using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescPhase5ValidationTest : TestCaseWithFactory
{
	public void TestCheckBY_Status_ValidCode()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(nctsCargoDesc.BY_StatusInfo, "AA", "DLR", ListValidation.InvalidCodeError);
			ValidationTestHelper.AssertErrorIfInvalidCode(nctsCargoDesc.BY_StatusInfo, "NBR", "DLR", ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckBY_HarmonisedTariff_InvalidCode()
	{
		CreateHarmonisedTariffCodesForTest();

		ValidationTestHelper.AssertInvalidCodeMessageError(nctsCargoDesc.BY_HarmonisedTariffInfo, new ZString[] { "0110", "99999999" }, new ZString[] { "01", "01001", "01000001" });
	}

	public void TestCheckBY_HarmonisedTariffDoNotValidateAllowedFieldLength()
	{
		CreateHarmonisedTariffCodesForTest();

		nctsCargoDesc.BY_HarmonisedTariff = "01001";
		AssertNoMessageErrors(nctsCargoDesc.BY_HarmonisedTariffInfo);
	}

	public void TestCheckBY_HarmonisedTariffDoNotValidateFixedNumberOfCharacters()
	{
		CreateHarmonisedTariffCodesForTest();

		nctsCargoDesc.BY_HarmonisedTariff = "01";
		AssertNoMessageErrors(nctsCargoDesc.BY_HarmonisedTariffInfo);
	}

	public void TestCheckBY_RN_NKCountryOfDispatch_ListC0009()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.China, "China", ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
		Factory.Save();

		CombineAssertions(() =>
		{
			var propertyInfo = nctsCargoDesc.BY_RN_NKCountryOfDispatchInfo;
			nctsCargoDesc.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			nctsCargoDesc.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
			AssertNoNotifications(propertyInfo);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertNoNotifications(propertyInfo);

			nctsCargoDesc.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Australia;
			AssertNoNotifications(propertyInfo);

			nctsCargoDesc.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			nctsCargoDesc.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
			AssertNoNotifications(propertyInfo);

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertNoNotifications(propertyInfo);

			nctsCargoDesc.BY_Type = ZString.Empty;
			nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfDispatch();
			AssertNoNotifications(propertyInfo);
		});
	}

	public void TestCheckBY_RN_NKCountryOfDestination_ListC0009()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.China, "China", ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
		Factory.Save();

		CombineAssertions(() =>
		{
			var propertyInfo = nctsCargoDesc.BY_RN_NKCountryOfDestinationInfo;
			nctsCargoDesc.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			nctsCargoDesc.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
			AssertNoNotifications(propertyInfo);
			
			nctsCargoDesc.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
			AssertNoNotifications(propertyInfo);

			nctsCargoDesc.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			nctsCargoDesc.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
			AssertNoNotifications(propertyInfo);

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
			nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfDestination();
			AssertNoNotifications(propertyInfo);

			nctsCargoDesc.BY_Type = ZString.Empty;
			nctsCargoDesc.Validation.ValidateBY_RN_NKCountryOfDestination();
			AssertNoNotifications(propertyInfo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsCargoDesc = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
	}

	void CreateHarmonisedTariffCodesForTest()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);

		var itTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import).PK;
		var euTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Universal.Constants.TariffTypes.Export).PK;
		helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Italy, itTariffTypePK, "01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Italy, itTariffTypePK, "01001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, euTariffTypePK, "01000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		Factory.Save();
	}

	NctsHeader nctsHeader;
	NctsDepartureCargoDesc nctsCargoDesc;
}
