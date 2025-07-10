using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_RN_NKCountryOfDestination_MustBeFilled()
		{
			const string message = "Destination Country must be filled either in Declaration tab or Goods tab.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDestinationInfo;
			CombineAssertions(() =>
			{
				goodsItem.BY_RN_NKCountryOfDestination = "";
				AssertHasMessageError("Destination Country not specified", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
				AssertNoMessageError("Destination Country specified on goods item level", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDestination = "";
				nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
				AssertNoMessageError("Destination Country specified on declaration level", targetInfo, message);
			});
		}

		public void TestCheckBY_RN_NKCountryOfDestination_ListC0009()
		{
			const string atLeast15AOr17AIsFromCodeList9 = "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties).";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.China, "China", ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			CombineAssertions(() =>
			{
				var propertyInfo = goodsItem.BY_RN_NKCountryOfDestinationInfo;
				goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
				AssertHasMessageError("DeclarationType is T2, CountryOfDispatch is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
				AssertNoMessageError("DeclarationType is T2, CountryOfDestination is C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
				AssertNoMessageError("DeclarationType is not T2, CountryOfDestination is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
				AssertNoMessageError("Header.DeclarationType is T2 and GoodsItem.DeclarationType is not T2, CountryOfDestination is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				goodsItem.BY_Type = ZString.Empty;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
				AssertHasMessageError("Header.DeclarationType is T2 and GoodsItem.DeclarationType is empty, CountryOfDestination is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);
			});
		}

		public void TestCheckBY_RN_NKCountryOfDestination_CannotBeBothFilled()
		{
			const string message = "Destination Country must be filled either in Declaration tab or Goods tab but not both.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDestinationInfo;
			CombineAssertions(() =>
			{
				goodsItem.BY_RN_NKCountryOfDestination = "";
				AssertNoMessageError("Destination Country not specified", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
				AssertNoMessageError("Destination Country specified on goods item level", targetInfo, message);

				nctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
				AssertHasMessageError("Destination Country specified on declaration and goods item level", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDestination = "";
				AssertNoMessageError("Destination Country specified on declaration level", targetInfo, message);
			});
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_MustBeFiled()
		{
			const string message = "Dispatch Country must be filled either in Declaration tab or Goods tab.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDispatchInfo;
			CombineAssertions(() =>
			{
				goodsItem.BY_RN_NKCountryOfDispatch = "";
				AssertHasMessageError("Dispatch Country not specified", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
				AssertNoMessageError("Dispatch Country specified on goods item level", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDispatch = "";
				nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.China;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
				AssertNoMessageError("Dispatch Country specified on declaration level", targetInfo, message);
			});
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_CannotBeBothFilled()
		{
			const string message = "Dispatch Country must be filled either in Declaration tab or Goods tab but not both.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDispatchInfo;
			CombineAssertions(() =>
			{
				goodsItem.BY_RN_NKCountryOfDispatch = "";
				AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
				AssertNoMessageError("Dispatch Country specified on goods item level", targetInfo, message);

				nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.China;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
				AssertHasMessageError("Dispatch Country specified on declaration and goods item level", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfDispatch = "";
				AssertNoMessageError("Dispatch Country specified on declaration level", targetInfo, message);
			});
		}

		public void TestCheckBY_RN_NKCountryOfOrigin()
		{
			const string message = "You have not entered an Origin Country. The ADD/CVD/Safeguarding won't be calculated when the Value is blank.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfOriginInfo;
			CombineAssertions(() =>
			{
				goodsItem.BY_RN_NKCountryOfOrigin = "";
				goodsItem.Validation.ValidateBY_RN_NKCountryOfOrigin();
				AssertHasWarning("Origin Country not specified", targetInfo, message);

				goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
				AssertNoWarning("Origin Country specified on goods item level", targetInfo, message);
			});
		}

		public void TestCheckCheckBY_CustomsThirdQuantity()
		{
			var errorMessage = $"The number 100,000,000,000.000 is too large, the maximum value allowed for {goodsItem.BY_CustomsThirdQuantityInfo.Description} is 99,999,999,999.999.";
			goodsItem.Validation.ValidateBY_CustomsThirdQuantity();
			AssertNoError(goodsItem.BY_CustomsThirdQuantityInfo, errorMessage);
			goodsItem.BY_CustomsThirdQuantity = 100000000000.000m;
			AssertHasError(goodsItem.BY_CustomsThirdQuantityInfo, errorMessage);
			goodsItem.BY_CustomsThirdQuantity = 20.303m;
			AssertNoError(goodsItem.BY_CustomsThirdQuantityInfo, errorMessage);
		}

		public void TestCheckBY_CustomsThirdUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			goodsItem.Validation.ValidateBY_CustomsThirdUnitQty();
			AssertNoMessageError(goodsItem.BY_CustomsThirdUnitQtyInfo, ListValidation.InvalidCodeMessageError);
			goodsItem.BY_CustomsThirdUnitQty = "X#";
			AssertHasMessageError(goodsItem.BY_CustomsThirdUnitQtyInfo, ListValidation.InvalidCodeMessageError);
			goodsItem.BY_CustomsThirdUnitQty = "DEF";
			AssertNoMessageError(goodsItem.BY_CustomsThirdUnitQtyInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		}
		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
	}
}
