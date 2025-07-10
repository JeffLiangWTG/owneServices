using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusClassPartPivotAddInfoValidationTest : AUAddInfoValidationTest
	{
		public void TestCheckZA_AQISProduceType_Hidden()
		{
			Assert("Pre-Condition no errors", !ExportPivot.AddInfo.ZA_AQISProduceType_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISProduceType_Hidden = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Dairy is a valid code", !ExportPivot.AddInfo.ZA_AQISProduceType_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISProduceType_Hidden = "SCO";
			Assert("Invalid code should have message errors", ExportPivot.AddInfo.ZA_AQISProduceType_HiddenInfo.HasMessageErrors());
		}

		public void TestCheckZA_WAR()
		{
			var establishmentCode = Factory.New<CMREstablishmentCodes>();
			establishmentCode.EC_EstablishmentCode = "9515C";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = Class.PK;
			pivot.CI_OP = Product.PK;

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.AddInfo.ZA_WAR = "9555N";
			AssertNoNotifications(pivot.AddInfo.ZA_WARInfo);
			pivot.AddInfo.ZA_WAR = "";
			AssertNoNotifications(pivot.AddInfo.ZA_WARInfo);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.AddInfo.ZA_WAR = "9555N";
			AssertHasMessageErrorContaining(pivot.AddInfo.ZA_WARInfo, EstablishmentCodeValidation.InvalidEstablishmentCodeNNNNA);
			AssertNoMessageErrorContaining(pivot.AddInfo.ZA_WARInfo, ListValidation.InvalidCodeMessageError);
			pivot.AddInfo.ZA_WAR = "9555B";
			AssertNoMessageErrorContaining(pivot.AddInfo.ZA_WARInfo, EstablishmentCodeValidation.InvalidEstablishmentCodeNNNNA);
			AssertHasMessageErrorContaining(pivot.AddInfo.ZA_WARInfo, ListValidation.InvalidCodeMessageError);
			pivot.AddInfo.ZA_WAR = "9515C";
			AssertNoNotifications(pivot.AddInfo.ZA_WARInfo);
		}

		public void TestCheckZA_AQISProduct_Hidden()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PRODD", "Dairy");
			helper.CreateNewOrGetExistingCusCodeList("AU", "PRODD", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertNoMessageErrors("Pre-Condition", ExportPivot.AddInfo.ZA_AQISProduct_HiddenInfo);

			ExportPivot.AddInfo.ZA_AQISProduct_Hidden = "B";
			AssertHasMessageError("Product is invalid for any commodity", ExportPivot.AddInfo.ZA_AQISProduct_HiddenInfo, CusClassPartPivotAddInfoValidation.ProductTypeMessage);

			ExportPivot.AddInfo.ZA_AQISProduceType_Hidden = EXDOCCommodityCodes.Codes.Dairy;
			ExportPivot.AddInfo.ZA_AQISProduct_Hidden = "BUT";
			AssertNoMessageError("Product is valid for dairy", ExportPivot.AddInfo.ZA_AQISProduct_HiddenInfo, CusClassPartPivotAddInfoValidation.ProductTypeMessage);
		}

		public void TestCheckZA_AQISCategoryCode_Hidden()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NPRCD", "Dairy");

			var codeBUT = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NPRCD", "BUT", "BUTTER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NPRCD", "CHD", "CHEDDAR CHEESE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName("ProductType", "Desc.", "NPRCD", Core.Constants.CountryCodes.Australia);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(codeBUT.PK, "ProductType", "MIL");

			Factory.Save();

			string msgAttributeSMK = string.Format(CusClassPartPivotAddInfoValidation.CategoryShouldHaveProductTypeAttribute, "SMK");
			ExportPivot.AddInfo.ZA_AQISProduceType_Hidden = EXDOCCommodityCodes.Codes.Dairy;

			ExportPivot.AddInfo.ZA_AQISProduct_Hidden = ZString.Empty;
			ExportPivot.AddInfo.ZA_AQISCategoryCode_Hidden = "CML";
			ExportPivot.AddInfo.Validation.ValidateZA_AQISCategoryCode_Hidden();

			AssertHasMessageErrorContaining(ExportPivot.AddInfo.ZA_AQISCategoryCode_HiddenInfo, CusClassPartPivotAddInfoValidation.ProductTypeMustBeEnteredBeforeCategoryCode);

			ExportPivot.AddInfo.ZA_AQISCategoryCode_Hidden = ZString.Empty;
			ExportPivot.AddInfo.Validation.ValidateZA_AQISCategoryCode_Hidden();

			AssertNoMessageErrorContaining(ExportPivot.AddInfo.ZA_AQISCategoryCode_HiddenInfo, CusClassPartPivotAddInfoValidation.ProductTypeMustBeEnteredBeforeCategoryCode);

			ExportPivot.AddInfo.ZA_AQISProduct_Hidden = "SMK";
			ExportPivot.AddInfo.ZA_AQISCategoryCode_Hidden = "BUT";

			ExportPivot.AddInfo.Validation.ValidateZA_AQISCategoryCode_Hidden();
			AssertHasMessageErrorContaining(ExportPivot.AddInfo.ZA_AQISCategoryCode_HiddenInfo, msgAttributeSMK);
		}

		public void TestCheckZA_AQISSupplementaryCode_Hidden()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("SUPP", "SUPP");
			var dmSupp = helper.CreateNewOrGetExistingCusCodeList("AU", "SUPP", "DM", "MANUFACTURING GRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(dmSupp.PK, "IsDairy", "");

			Factory.Save();

			Assert("Pre-Condition", !ExportPivot.AddInfo.ZA_AQISSupplementaryCode_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISSupplementaryCode_Hidden = "Z";
			Assert("Supplementary code is invalid for any commodity", ExportPivot.AddInfo.ZA_AQISSupplementaryCode_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISProduceType_Hidden = EXDOCCommodityCodes.Codes.Dairy;
			ExportPivot.AddInfo.ZA_AQISSupplementaryCode_Hidden = "DM";
			Assert("Supplementary code is valid for dairy", !ExportPivot.AddInfo.ZA_AQISSupplementaryCode_HiddenInfo.HasMessageErrors());
		}

		public void TestCheckZA_AQISPackType_Hidden()
		{
			Assert("Pre-Condition", !ExportPivot.AddInfo.ZA_AQISPackType_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISPackType_Hidden = "Z";
			Assert("Pack type is invalid", ExportPivot.AddInfo.ZA_AQISPackType_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISPackType_Hidden = EXDOCPackTypeCodes.Codes.BagInABox;
			Assert("Pack type is valid", !ExportPivot.AddInfo.ZA_AQISPackType_HiddenInfo.HasMessageErrors());
		}

		public void TestZA_AQISPreservation_Hidden()
		{
			Assert("Pre-Condition", !ExportPivot.AddInfo.ZA_AQISPreservation_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISPreservation_Hidden = "Z";
			Assert("Preservation type is invalid", ExportPivot.AddInfo.ZA_AQISPreservation_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISPreservation_Hidden = EXDOCPreservationTypeCodes.Codes.Chilled;
			Assert("Preservation type is valid", !ExportPivot.AddInfo.ZA_AQISPreservation_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISProduceType_Hidden = EXDOCCommodityCodes.Codes.Fish;
			ExportPivot.AddInfo.ZA_AQISPreservation_Hidden = ZString.Empty;
			Assert("Preservation type cannot be empty for fish", ExportPivot.AddInfo.ZA_AQISPreservation_HiddenInfo.HasMessageErrors());
			ExportPivot.AddInfo.ZA_AQISPreservation_Hidden = EXDOCPreservationTypeCodes.Codes.Frozen;
			Assert("Preservation type is not empty for fish", !ExportPivot.AddInfo.ZA_AQISPreservation_HiddenInfo.HasMessageErrors());
		}

		public void TestCheckZA_AQISCutCode_Hidden()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUTCD", "Dairy");
			helper.CreateNewOrGetExistingCusCodeList("AU", "CUTCD", "DC0215", "Jack Cheese", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertNoMessageErrors("Pre-Condition", ExportPivot.AddInfo.ZA_AQISCutCode_HiddenInfo);

			ExportPivot.AddInfo.ZA_AQISCutCode_Hidden = "Z";
			AssertHasMessageError("Cut code is invalid", ExportPivot.AddInfo.ZA_AQISCutCode_HiddenInfo, CusClassPartPivotAddInfoValidation.CutCodeMessage);

			ExportPivot.AddInfo.ZA_AQISProduceType_Hidden = EXDOCCommodityCodes.Codes.Dairy;
			ExportPivot.AddInfo.ZA_AQISCutCode_Hidden = "DC0215";
			AssertNoMessageError("Cut code is valid", ExportPivot.AddInfo.ZA_AQISCutCode_HiddenInfo, CusClassPartPivotAddInfoValidation.CutCodeMessage);
		}

		CusClassPartPivot fPivot;
		CusClassPartPivot ExportPivot
		{
			get
			{
				if (fPivot == null)
				{
					fPivot = Factory.New<CusClassPartPivot>();
					fPivot.CI_CC = Class.PK;
					fPivot.CI_OP = Product.PK;
					fPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				}
				return fPivot;
			}
		}

		Classification fClass;
		Classification Class
		{
			get
			{
				if (fClass == null)
				{
					fClass = Factory.New<Classification>();
				}
				return fClass;
			}
		}

		AUOrgSupplierPart fProduct;
		AUOrgSupplierPart Product
		{
			get
			{
				if (fProduct == null)
				{
					fProduct = Factory.New<AUOrgSupplierPart>();
					fProduct.OP_PartNum = fProduct.PK.ToString().Replace("-", "");
				}
				return fProduct;
			}
		}
	}
}
