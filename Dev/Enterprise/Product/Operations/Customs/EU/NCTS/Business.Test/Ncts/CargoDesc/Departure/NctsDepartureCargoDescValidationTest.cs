using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_CustomsSecondQuantity()
		{
			NCTSTestHelper.AssertCheckBY_CustomsSecondQuantity_RuleTR0084(Factory, goodsItem);
		}

		public void TestCheckBM_InBondEntryType_ConditionC901()
		{
			goodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			goodsItem.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(goodsItem, "C901");
			var supportingDoc = goodsItem.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = NctsHeaderValidationHelper.TirCarnetDocumentCode;
			goodsItem.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(goodsItem, "C901");
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_RN_NKCountryOfDispatchInfo, "X#", Core.Constants.CountryCodes.France);
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_ListC0009()
		{
			const string atLeast15AOr17AIsFromCodeList9 = "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties).";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.China, "China", ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			CombineAssertions(() =>
			{
				var propertyInfo = goodsItem.BY_RN_NKCountryOfDispatchInfo;
				goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
				AssertHasMessageError("DeclarationType is T2, CountryOfDispatch is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Australia;
				AssertNoMessageError("DeclarationType is T2, CountryOfDispatch is C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
				AssertNoMessageError("DeclarationType is not T2, CountryOfDispatch is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
				AssertNoMessageError("Header.DeclarationType is T2 and GoodsItem.DeclarationType is not T2, CountryOfDispatch is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);

				goodsItem.BY_Type = ZString.Empty;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
				AssertHasMessageError("Header.DeclarationType is T2 and GoodsItem.DeclarationType is empty, CountryOfDispatch is not C0009 code", propertyInfo, atLeast15AOr17AIsFromCodeList9);
			});
		}

		public void TestCheckBY_RN_NKCountryOfDestination_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_RN_NKCountryOfDestinationInfo, "X#", Core.Constants.CountryCodes.Germany);
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

		public void TestCheckBY_RN_NKCountryOfOrigin()
		{
			NCTSTestHelper.AssertCheckBY_RN_NKCountryOfOriginIsValid(goodsItem);
		}

		public void TestCheckBY_CustomsSecondUnitQty()
		{
			NCTSTestHelper.TestCheckBY_CustomsSecondUnitQtyIsValid(Factory, goodsItem);
		}

		public void TestCheckBY_ZZF_NKTaxType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("605", 0.1m, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0.1m, 0.1m, "VAT");
			Factory.Save();

			goodsItem.Validation.ValidateBY_ZZF_NKTaxType();
			AssertNoMessageError(goodsItem.BY_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
			goodsItem.BY_ZZF_NKTaxType = "X#";
			AssertHasMessageError(goodsItem.BY_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
			goodsItem.BY_ZZF_NKTaxType = "EUR";
			AssertHasMessageError(goodsItem.BY_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
			goodsItem.BY_ZZF_NKTaxType = "605";
			AssertNoMessageError(goodsItem.BY_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
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
