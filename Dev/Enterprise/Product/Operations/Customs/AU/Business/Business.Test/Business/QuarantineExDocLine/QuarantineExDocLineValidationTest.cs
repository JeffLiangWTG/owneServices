using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckQL_FarmType()
		{
			var newFactory = new BusinessObjectFactory();
			var refHelper = new UniversalReferenceTestDataHelper(newFactory);
			refHelper.CreateNewOrGetExistingCusCodeType("NXEFT", "EGG Farm Type");
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "NXEFT", "CODE1", "Desc1", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			newFactory.Save();

			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCLine.QL_FarmType = "INVALID";
			eXDOCLine.Validation.ValidateQL_FarmType();
			AssertHasMessageErrorContaining(eXDOCLine.QL_FarmTypeInfo, ListValidation.InvalidCodeMessageError);
			eXDOCLine.QL_FarmType = "CODE1";
			eXDOCLine.Validation.ValidateQL_FarmType();
			AssertNoMessageErrorContaining(eXDOCLine.QL_FarmTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckQL_LabelApprovalNumber()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCLine.QL_LabelApprovalNumber = "NUM";
			AssertHasMessageErrorContaining(eXDOCLine.QL_LabelApprovalNumberInfo, "Label approval number may only be present when produce type is Meat.");
			eXDOCLine.QL_LabelApprovalNumber = "";
			AssertNoMessageErrorContaining(eXDOCLine.QL_LabelApprovalNumberInfo, "Label approval number may only be present when produce type is Meat.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_LabelApprovalNumber = "NUM";
			AssertNoMessageErrorContaining(eXDOCLine.QL_LabelApprovalNumberInfo, "Label approval number may only be present when produce type is Meat.");
			eXDOCLine.QL_LabelApprovalNumber = "";
			AssertNoMessageErrorContaining(eXDOCLine.QL_LabelApprovalNumberInfo, "Label approval number should be entered when the Label Approval Indicator is set.");
			eXDOCLine.QL_LabelApprovalIndicator = true;
			AssertHasMessageErrorContaining(eXDOCLine.QL_LabelApprovalNumberInfo, "Label approval number should be entered when the Label Approval Indicator is set.");
			eXDOCLine.QL_LabelApprovalNumber = "NUM";
			AssertNoMessageErrorContaining(eXDOCLine.QL_LabelApprovalNumberInfo, "Label approval number should be entered when the Label Approval Indicator is set.");
		}

		public void TestCheckQL_LabelApprovalIndicator()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCLine.QL_LabelApprovalIndicator = true;
			AssertHasMessageErrorContaining(eXDOCLine.QL_LabelApprovalIndicatorInfo, "Label approval indicator may only be set when produce type is Meat.");
			eXDOCLine.QL_LabelApprovalIndicator = false;
			AssertNoMessageErrorContaining(eXDOCLine.QL_LabelApprovalIndicatorInfo, "Label approval indicator may only be set when produce type is Meat.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_LabelApprovalIndicator = true;
			AssertNoMessageErrorContaining(eXDOCLine.QL_LabelApprovalIndicatorInfo, "Label approval indicator may only be set when produce type is Meat.");
		}

		public void TestCheckQL_CategoryCode()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var nprdd = refHelper.CreateNewOrGetExistingCusCodeType("NPRCD", "Dairy");
			var codeBUT = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NPRCD", "BUT", "BUTTER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NPRCD", "CHD", "CHEDDAR CHEESE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName("ProductType", "Desc.", nprdd.ZZK_CodeType, Core.Constants.CountryCodes.Australia);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(codeBUT.PK, "ProductType", "MIL");
			Factory.Save();

			AssertEquals(false, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);

			var msgAttributeSMK = string.Format(QuarantineExDocLineValidation.CategoryShouldHaveProductTypeAttribute, "SMK");
			eXDOCLine.QL_ProductType = "SMK";
			eXDOCLine.QL_Category = ZString.Empty;
			eXDOCLine.Validation.ValidateQL_Category();

			AssertNoMessageErrorContaining(eXDOCLine.QL_CategoryInfo, msgAttributeSMK);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);

			eXDOCLine.QL_ProductType = ZString.Empty;
			eXDOCLine.QL_Category = "CML";
			eXDOCLine.Validation.ValidateQL_Category();
			AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.ProductTypeMustBeEnteredBeforeCategoryCode);

			eXDOCLine.QL_Category = ZString.Empty;
			eXDOCLine.Validation.ValidateQL_Category();
			AssertNoMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.ProductTypeMustBeEnteredBeforeCategoryCode);

			eXDOCLine.QL_ProductType = "SMK";
			eXDOCLine.QL_Category = "BUT";
			eXDOCLine.Validation.ValidateQL_Category();
			AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, msgAttributeSMK);
		}

		public void TestCheckQL_CategoryWithAHECCAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("NPRCA", "NEXDOCS Product Category AHECC");

			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType, "DC0045", "DC0045", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType, "DC0275", "DC0275", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("AHECCC", "AHECC Code Desc.", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, "AHECCC", "040210");
			Factory.Save();

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);

			eXDOCLine.InvoiceLine.Declaration.JE_RL_NKFinalDestination = "DEHAM";

			void AssertMessageErrorWithMatchedAHECCAttribute(string tariff, string category, bool expectedHasMessageError)
			{
				eXDOCLine.InvoiceLine.JI_Tariff = tariff;
				eXDOCLine.QL_Category = category;

				eXDOCLine.Validation.ValidateQL_Category();

				if (expectedHasMessageError)
				{
					AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);
				}
				else
				{
					AssertNoMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);
				}
			}

			AssertMessageErrorWithMatchedAHECCAttribute(ZString.Empty, "DC0045", false);
			AssertMessageErrorWithMatchedAHECCAttribute("04021001", ZString.Empty, false);
			AssertMessageErrorWithMatchedAHECCAttribute("04021001", "DC0045", false);
			AssertMessageErrorWithMatchedAHECCAttribute("04021002", "DC0045", false);

			AssertMessageErrorWithMatchedAHECCAttribute("04061000", "DC0045", true);
			AssertMessageErrorWithMatchedAHECCAttribute("04021010", "DC0275", true);
		}

		public void TestCheckQL_CategoryWithPortsInEU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("NPRCA", "NEXDOCS Product Category AHECC");

			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType, "DC0045", "DC0045", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("AHECCC", "AHECC Code Desc.", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, "AHECCC", "040210");
			Factory.Save();

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);

			eXDOCLine.InvoiceLine.JI_Tariff = "04021010";
			eXDOCLine.QL_Category = "DC0275";

			var declaration = eXDOCLine.InvoiceLine.Declaration;
			declaration.JE_RL_NKFinalDestination = "DEHAM";

			eXDOCLine.Validation.ValidateQL_Category();
			AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);

			declaration.JE_RL_NKFinalDestination = string.Empty;
			declaration.Transports.RemoveAndDeleteAll();
			declaration.Transports.AddNew().JW_RL_NKLoadPort = "DEHAM";

			eXDOCLine.Validation.ValidateQL_Category();
			AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);

			declaration.JE_RL_NKFinalDestination = string.Empty;
			declaration.Transports.RemoveAndDeleteAll();
			declaration.Transports.AddNew().JW_RL_NKDiscPort = "DEHAM";

			eXDOCLine.Validation.ValidateQL_Category();
			AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);

			declaration.Transports.RemoveAndDeleteAll();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var consol = shipment.Consols.AddNew();
			consol.Transports.AddNew().JW_RL_NKDiscPort = "DEHAM";

			declaration.JE_JS = shipment.PK;

			eXDOCLine.Validation.ValidateQL_Category();
			AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);

			declaration.JE_JS = ZGuid.Empty;

			var newTransport = declaration.Transports.AddNew();
			newTransport.JW_RL_NKLoadPort = "NZAKL";
			newTransport.JW_RL_NKDiscPort = "AUSYD";

			declaration.JE_RL_NKFinalDestination = "AUBNE";

			eXDOCLine.Validation.ValidateQL_Category();
			AssertNoMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);
		}

		public void TestCheckQL_CategoryWithProductTypeDairy()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType("NPRCA", "NEXDOCS Product Category AHECC");

			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType, "DC0045", "DC0045", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("AHECCC", "AHECC Code Desc.", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Australia, codeType.ZZK_CodeType);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, "AHECCC", "040210");
			Factory.Save();

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);

			eXDOCLine.InvoiceLine.JI_Tariff = "04021010";
			eXDOCLine.QL_Category = "DC0275";

			eXDOCLine.Validation.ValidateQL_Category();
			AssertHasMessageErrorContaining(eXDOCLine.QL_CategoryInfo, QuarantineExDocLineValidation.DontHaveMatchedAHECCAttribute);
		}

		public void TestCheckQL_UngradedProductIndicator()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCLine.QL_UngradedProductIndicator = true;
			AssertHasMessageErrorContaining(eXDOCLine.QL_UngradedProductIndicatorInfo, "Ungraded Product Indicator may only be set when produce type is Meat.");
			eXDOCLine.QL_UngradedProductIndicator = false;
			AssertNoMessageErrorContaining(eXDOCLine.QL_UngradedProductIndicatorInfo, "Ungraded Product Indicator may only be set when produce type is Meat.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_UngradedProductIndicator = true;
			AssertNoMessageErrorContaining(eXDOCLine.QL_UngradedProductIndicatorInfo, "Ungraded Product Indicator may only be set when produce type is Meat.");
		}

		public void TestCheckCheckQL_ExtraCertificate()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCLine.QL_ExtraCertificate = "CERT";
			Assert("Extra Certificate is allowed for Skins", !eXDOCLine.QL_ExtraCertificateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCLine.QL_ExtraCertificate = "CERT";
			Assert("Extra Certificate is allowed for InedibleMeat", !eXDOCLine.QL_ExtraCertificateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCLine.QL_ExtraCertificate = "CERT";
			Assert("Extra Certificate is not allowed for Wool", eXDOCLine.QL_ExtraCertificateInfo.HasMessageErrors());
		}

		public void TestCheckQL_NatureOfCommodity()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_NatureOfCommodityInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCLine.QL_NatureOfCommodity = "CT";
			Assert("Valid nature of commodity", !eXDOCLine.QL_NatureOfCommodityInfo.HasMessageErrors());
			eXDOCLine.QL_NatureOfCommodity = "??";
			Assert("Invalid nature of commodity", eXDOCLine.QL_NatureOfCommodityInfo.HasMessageErrors());
			eXDOCLine.QL_NatureOfCommodity = ZString.Empty;
			Assert("Empty code is OK", !eXDOCLine.QL_NatureOfCommodityInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				AssertEquals(true, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(eXDOCHeader.QH_ProduceType));
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCLine.QL_NatureOfCommodity = "CT";
				AssertHasMessageErrorContaining(eXDOCLine.QL_NatureOfCommodityInfo, "Nature of Commodity must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckQL_UseByStart()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_UseByStartInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCLine.QL_UseByStart = ZDateTime.Now;
			AssertEquals("No Message Error", true, !eXDOCLine.QL_UseByStartInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				AssertEquals(true, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(eXDOCHeader.QH_ProduceType));
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCLine.QL_UseByStart = ZDateTime.Now;
				AssertHasMessageErrorContaining(eXDOCLine.QL_UseByStartInfo, "Durability Start Date must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckQL_UseByEnd()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_UseByEndInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCLine.QL_UseByEnd = ZDateTime.Now;
			AssertEquals("No Message Error", true, !eXDOCLine.QL_UseByEndInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				AssertEquals(true, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(eXDOCHeader.QH_ProduceType));
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCLine.QL_UseByEnd = ZDateTime.Now;
				AssertHasMessageErrorContaining(eXDOCLine.QL_UseByEndInfo, "Durability End Date must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckQL_TreatmentType()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("EXE34", "EXDOCS Code Set - E34 Treatment Type", "AU");
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "EXE34", "CHIL", "CHILLED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			AssertNoMessageErrors("Pre-Condition", eXDOCLine.QL_TreatmentTypeInfo);
			eXDOCLine.QL_TreatmentType = "CHIL";
			AssertNoMessageErrors("Valid Treatment Type", eXDOCLine.QL_TreatmentTypeInfo);
			eXDOCLine.QL_TreatmentType = "??";
			AssertHasMessageErrorContaining(eXDOCLine.QL_TreatmentTypeInfo, ListValidation.InvalidCodeMessageError);
			eXDOCLine.QL_TreatmentType = ZString.Empty;
			AssertNoMessageErrors("Empty code is OK", eXDOCLine.QL_TreatmentTypeInfo);
		}

		public void TestCheckQL_ProductType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("NPRDD", "Dairy");
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRDD", "BUT", "Butter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertNoMessageErrors("Pre-Condition", eXDOCLine.QL_ProductTypeInfo);

			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);
			eXDOCLine.QL_ProductType = "";
			AssertHasMessageErrorContaining(eXDOCLine.QL_ProductTypeInfo, "You have not entered");

			eXDOCLine.QL_ProductType = "B";
			AssertHasMessageError("Produce Type is invalid for any commodity", eXDOCLine.QL_ProductTypeInfo, QuarantineExDocLineValidation.ProductTypeMessage);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_ProductType = "BUT";
			AssertNoMessageError("Produce Type is valid for Dairy", eXDOCLine.QL_ProductTypeInfo, QuarantineExDocLineValidation.ProductTypeMessage);
		}

		public void TestCheckQL_SupplimentaryCode()
		{
			const string supplimentaryCode = "DM";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NSUPP", supplimentaryCode, ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("D", "H", "COB", "BB", supplimentaryCode);
			Factory.Save();

			eXDOCLine.QL_ProductType = "COB";
			Assert("Pre-Condition", !eXDOCLine.QL_SupplimentaryCodeInfo.HasMessageErrors());
			eXDOCLine.QL_SupplimentaryCode = "Z";
			Assert("Supplementary code is invalid for any commodity", eXDOCLine.QL_SupplimentaryCodeInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_SupplimentaryCode = supplimentaryCode;
			Assert("Supplementary code is valid for Dairy", !eXDOCLine.QL_SupplimentaryCodeInfo.HasMessageErrors());
		}

		public void TestCheckQL_PackType()
		{
			const string packTypeCode = "BB";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPCKT", packTypeCode, "desc", ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime);
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("E", "H", "COB", packTypeCode, "X");
			Factory.Save();

			Assert("Pre-Condition", !eXDOCLine.QL_PackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_ProductType = "COB";
			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);
			eXDOCLine.QL_PackType = "";
			AssertHasMessageErrorContaining(eXDOCLine.QL_PackTypeInfo, "You have not entered");

			eXDOCLine.QL_PackType = "Z";
			Assert("Pack type is invalid", eXDOCLine.QL_PackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_PackType = packTypeCode;
			Assert("Pack type is valid", !eXDOCLine.QL_PackTypeInfo.HasMessageErrors());
		}

		public void TestCheckQL_PreservationType()
		{
			const string preservationCode = "C";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRST", preservationCode, "desc", ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime);
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("E", preservationCode, "COB", "BB", "X");
			helper.CreateOrFindExistingRefCusAUNexdocECMCode("F", preservationCode, "COB", "BB", "X");
			Factory.Save();

			Assert("Pre-Condition", !eXDOCLine.QL_PreservationTypeInfo.HasMessageErrors());
			eXDOCLine.QL_ProductType = "COB";
			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);
			eXDOCLine.QL_PreservationType = "";
			AssertHasMessageErrorContaining(eXDOCLine.QL_PreservationTypeInfo, "You have not entered");
			eXDOCLine.QL_PreservationType = "Z";
			Assert("Preservation type is invalid", eXDOCLine.QL_PreservationTypeInfo.HasMessageErrors());
			eXDOCLine.QL_PreservationType = preservationCode;
			Assert("Preservation type is valid", !eXDOCLine.QL_PreservationTypeInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_PreservationType = ZString.Empty;
			Assert("Preservation type cannot be empty for Fish", eXDOCLine.QL_PreservationTypeInfo.HasMessageErrors());
			eXDOCLine.QL_PreservationType = preservationCode;
			Assert("Preservation type is not empty for Fish", !eXDOCLine.QL_PreservationTypeInfo.HasMessageErrors());
		}

		public void TestCheckQL_CutCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("NCUTC", "Dairy");
			helper.CreateNewOrGetExistingCusCodeList("AU", "NCUTC", "DC0215", "Jack Cheese", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertNoMessageErrors("Pre-Condition", eXDOCLine.QL_CutCodeInfo);

			eXDOCLine.QL_CutCode = "Z";
			AssertHasMessageError("Cut code is invalid", eXDOCLine.QL_CutCodeInfo, QuarantineExDocLineValidation.CutCodeMessage);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_CutCode = "DC0215";
			AssertNoMessageError("Cut code is valid", eXDOCLine.QL_CutCodeInfo, QuarantineExDocLineValidation.CutCodeMessage);
		}

		public void TestCheckQL_ProductDescriptionLocationQualifier()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			Assert("Pre-Condition", !eXDOCLine.QL_ProductDescriptionLocationQualifierInfo.HasMessageErrors());

			eXDOCLine.QL_ProductDescriptionLocationQualifier = "DARWIN";
			AssertHasMessageError("Product location is invalid", eXDOCLine.QL_ProductDescriptionLocationQualifierInfo, "The code you have selected is not in the list.");

			eXDOCLine.QL_ProductDescriptionLocationQualifier = EXDOCLocationQualifier.Codes.Australian;
			AssertNoMessageError(eXDOCLine.QL_ProductDescriptionLocationQualifierInfo, "The code you have selected is not in the list.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_ProductDescriptionLocationQualifier = EXDOCLocationQualifier.Codes.Tasmanian;
			AssertHasMessageError("Product location can only be entered for Meat", eXDOCLine.QL_ProductDescriptionLocationQualifierInfo, "Product location may only be present when produce type is Meat or Wool.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_ProductDescriptionLocationQualifier = EXDOCLocationQualifier.Codes.Australian;
			AssertNoMessageError(eXDOCLine.QL_ProductDescriptionLocationQualifierInfo, "Product location may only be present when produce type is Meat or Wool.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCLine.QL_ProductDescriptionLocationQualifier = EXDOCLocationQualifier.Codes.Tasmanian;
			AssertNoMessageError(eXDOCLine.QL_ProductDescriptionLocationQualifierInfo, "Product location may only be present when produce type is Meat or Wool.");
		}

		public void TestCheckQL_ProductDescriptionQualityQualifier()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_ProductDescriptionQualityQualifierInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_ProductDescriptionQualityQualifier = "10 per KG, U7";
			Assert("Product quality is invalid as commodity is Meat", eXDOCLine.QL_ProductDescriptionQualityQualifierInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_ProductDescriptionQualityQualifier = "10 per KG, U7";
			Assert("Product quality is valid as commodity is Dairy", !eXDOCLine.QL_ProductDescriptionQualityQualifierInfo.HasMessageErrors());
		}

		public void TestCheckQL_NetQuantity()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_NetQuantityInfo.HasMessageErrors());
			eXDOCLine.QL_NetQuantity = -12.3m;
			AssertHasMessageError("Net quantity is invalid (negative value)", eXDOCLine.QL_NetQuantityInfo, "Net quantity must be greater than 0.");
			eXDOCLine.QL_NetQuantity = 0.000m;
			AssertHasMessageError("Net quantity is invalid 0", eXDOCLine.QL_NetQuantityInfo, "Net quantity must be greater than 0.");
			eXDOCLine.QL_NetQuantity = 12.345m;
			AssertNoMessageError(eXDOCLine.QL_NetQuantityInfo, "Net quantity must be greater than 0.");
			eXDOCLine.QL_NetQuantityUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			eXDOCLine.QL_OuterPackCount = 12;
			eXDOCLine.QL_OuterPackWeight = 2;
			eXDOCLine.QL_OuterPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			AssertHasWarning("Product of Outer pack count and Weight must equal net quantity", eXDOCLine.QL_NetQuantityInfo, "Product of outer pack weight and count must be equal to net quantity.");
			eXDOCLine.QL_NetQuantity = 24m;
			AssertNoWarning(eXDOCLine.QL_NetQuantityInfo, "Product of outer pack weight and count must be equal to net quantity.");
		}

		[TestDate(2022, 1, 18)]
		public void TestCheckQL_NetQuantityUnit()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var startDate = new ZDateTime(2022, 1, 1);
			var endDate = new ZDateTime(2022, 1, 31);
			helper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "ONZ", "OUNCE", startDate, endDate);
			newFactory.Save();

			Assert("Pre-Condition", !eXDOCLine.QL_NetQuantityUnitInfo.HasMessageErrors());
			eXDOCLine.QL_NetQuantityUnit = ZString.Empty;
			Assert("Net quantity unit is invalid (empty)", eXDOCLine.QL_NetQuantityUnitInfo.HasMessageErrors());
			eXDOCLine.QL_NetQuantityUnit = "ZZZ";
			Assert("Net quantity unit is invalid code", eXDOCLine.QL_NetQuantityUnitInfo.HasMessageErrors());
			eXDOCLine.QL_NetQuantityUnit = "ONZ";
			Assert("Net quantity unit is valid code", !eXDOCLine.QL_NetQuantityUnitInfo.HasMessageErrors());
		}

		public void TestCheckQL_ImperialNetWeight()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_ImperialNetWeight = 12.3m;
			Assert("Imperial net weight is invalid for commodity Dairy", eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_ImperialNetWeight = -12.3m;
			Assert("Imperial net weight is invalid code", eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());

			eXDOCLine.QL_ImperialNetWeight = 12.3m;
			Assert("Imperial net weight is valid code", !eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_ImperialNetWeight = 1m;
			Assert("Imperial net weight is allowed for Meat", !eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCLine.QL_ImperialNetWeight = 1m;
			Assert("Imperial net weight is allowed for Eggs", !eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCLine.QL_ImperialNetWeight = 2m;
			Assert("Imperial net weight is allowed for Wool", !eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCLine.QL_ImperialNetWeight = 1m;
			Assert("Imperial net weight is NOT allowed for InedibleMeat", eXDOCLine.QL_ImperialNetWeightInfo.HasMessageErrors());
		}

		public void TestCheckQL_ImperialNetWeightUnit()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_ImperialNetWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_ImperialNetWeight = 12.3m;
			eXDOCLine.QL_ImperialNetWeightUnit = ZString.Empty;
			Assert("Imperial net weight unit is invalid (empty)", eXDOCLine.QL_ImperialNetWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_ImperialNetWeightUnit = "BLA";
			Assert("Imperial net weight unit is invalid code", eXDOCLine.QL_ImperialNetWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_ImperialNetWeightUnit = EXDOCImperialWeightUnitCodes.Codes.HundredWeightUk;
			Assert("Net quantity unit is valid code", !eXDOCLine.QL_ImperialNetWeightUnitInfo.HasMessageErrors());
		}

		public void TestCheckQL_GrossMetricWeight()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_GrossMetricWeightInfo.HasMessageErrors());
			eXDOCLine.QL_GrossMetricWeight = -12.3m;
			Assert("Gross metric weight is invalid", eXDOCLine.QL_GrossMetricWeightInfo.HasMessageErrors());
			eXDOCLine.QL_GrossMetricWeight = 2.3m;
			Assert("Gross metric weight is valid", !eXDOCLine.QL_GrossMetricWeightInfo.HasMessageErrors());
		}

		public void TestCheckQL_GrossMetricWeightIsMandatoryWhenNEXDOC()
		{
			AssertEquals(false, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);
			eXDOCLine.QL_GrossMetricWeight = 0.0m;
			Assert("Gross metric weight is no Mandatory", !eXDOCLine.QL_GrossMetricWeightInfo.HasMessageErrors());
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, "You have not entered a Gross Metric Weight.");

			eXDOCLine.QL_GrossMetricWeight = 1.0m;
			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals(eXDOCLine.QL_GrossMetricWeight, invoiceLine.JI_Weight);
			eXDOCLine.QL_GrossMetricWeight = 0.0m;
			AssertEquals(true, eXDOCLine.QuarantineExDocHeader.IsNEXDOCSActive);
			AssertHasMessageErrorContaining(eXDOCLine.QL_GrossMetricWeightInfo, "You have not entered a Gross Metric Weight.");
			invoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightInfo, "You have not entered a Gross Metric Weight.");
		}

		public void TestCheckQL_GrossMetricWeightUnit()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_GrossMetricWeightUnitInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_GrossMetricWeight = 23.4m;
			eXDOCLine.QL_GrossMetricWeightUnit = "ZZZ";
			Assert("Gross metric weight unit is invalid", eXDOCLine.QL_GrossMetricWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_GrossMetricWeight = ZDecimal.Zero;
			eXDOCLine.QL_GrossMetricWeightUnit = ZString.Empty;
			Assert("Gross metric weight unit is valid as Weight is empty", !eXDOCLine.QL_GrossMetricWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = ZBool.True;
			eXDOCLine.QL_GrossMetricWeight = 12.3m;
			eXDOCLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			Assert("Gross metric weight unit is valid code (Customs agent indicator set so has errors)", !eXDOCLine.QL_GrossMetricWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = ZBool.False;
			eXDOCLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTon;
			AssertHasMessageError(eXDOCLine.QL_GrossMetricWeightUnitInfo, "Metric tonne unit MTO can only be used when produce type is Grains and Plants.");
			eXDOCLine.QL_GrossMetricWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTonne;
			AssertNoMessageError(eXDOCLine.QL_GrossMetricWeightUnitInfo, "Metric tonne unit MTO can only be used when produce type is Grains and Plants.");
		}

		public void TestCheckQL_OuterPackCount()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackCount = 0;
			Assert("Outer pack count is invalid (0)", eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackCount = -19;
			Assert("Outer pack count is invalid (negative)", eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackCount = 5;
			Assert("Outer pack count is valid", !eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Bulk;
			Assert("Outer pack count must be zero for Bulk", eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackCount = 0;
			Assert("Outer pack count is zero for Bulk", !eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.MixedShipments;
			Assert("Outer pack count is zero for mixed shipments", !eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackCount = 7;
			Assert("Outer pack count must be zero for mixed shipments", eXDOCLine.QL_OuterPackCountInfo.HasMessageErrors());
		}

		public void TestCheckQL_OuterPackType()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_OuterPackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackType = ZString.Empty;
			Assert("Outer pack Type is invalid (ZString.Empty)", eXDOCLine.QL_OuterPackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackType = "ZZ";
			Assert("Outer pack type is an invalid code", eXDOCLine.QL_OuterPackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackType = EXDOCPacakgeTypeCodes.Codes.Bags;
			Assert("Outer pack type is valid", !eXDOCLine.QL_OuterPackTypeInfo.HasMessageErrors());
		}

		public void TestCheckQL_OuterPackAccuracy()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_OuterPackAccuracyInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_OuterPackAccuracy = ZString.Empty;
			Assert("Outer pack accuracy is invalid (ZString.Empty)", eXDOCLine.QL_OuterPackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackAccuracy = "Z";
			Assert("Outer pack accuracy is an invalid code", eXDOCLine.QL_OuterPackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			Assert("Outer pack accuracy is valid", !eXDOCLine.QL_OuterPackAccuracyInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_OuterPackAccuracy = "Z";
			Assert("Outer pack accuracy is an invalid code", eXDOCLine.QL_OuterPackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackAccuracy = ZString.Empty;
			Assert("Outer pack accuracy is valid", eXDOCLine.QL_OuterPackAccuracyInfo.HasMessageErrors());
		}

		public void TestCheckQL_OuterPackAccuracy_NEXDOCFish()
		{
			const string message = "Outer Pack accuracy is not allowed for this produce type.";
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_OuterPackAccuracy = ZString.Empty;
			AssertNoMessageErrors("QL_OuterPackAccuracy empty", eXDOCLine.QL_OuterPackAccuracyInfo);

			eXDOCLine.QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			AssertHasMessageError("QL_OuterPackAccuracy is not empty", eXDOCLine.QL_OuterPackAccuracyInfo, message);

			eXDOCLine.QL_OuterPackAccuracy = "Z";
			AssertHasMessageError("QL_OuterPackAccuracy is invalid", eXDOCLine.QL_OuterPackAccuracyInfo, message);
		}

		public void TestCheckQL_OuterPackWeight()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_OuterPackWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_OuterPackWeight = ZDecimal.Zero;
			Assert("Outer pack weight is invalid (Zint.Zero)", eXDOCLine.QL_OuterPackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackWeight = -12;
			Assert("Outer pack weight is invalid", eXDOCLine.QL_OuterPackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackWeight = 12;
			Assert("Outer pack weight is valid", !eXDOCLine.QL_OuterPackWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_OuterPackWeight = ZDecimal.Zero;
			Assert("Outer pack weight is valid", !eXDOCLine.QL_OuterPackWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ObtainExportCustomsPermit = true;
			eXDOCLine.QL_OuterPackWeight = 45;
			Assert("Outer pack weight is invalid when commodity is Meat and obtaining a customs permit.", eXDOCLine.QL_OuterPackWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ObtainExportCustomsPermit = false;
			eXDOCLine.QL_OuterPackWeight = 99999.999;
			AssertHasMessageError("Value is greater than the maximum allowed by EXDOC but less then the database field size", eXDOCLine.QL_OuterPackWeightInfo, "Outer pack weight cannot be greater than 9999.999");
			eXDOCLine.QL_OuterPackWeight = 9999.999;
			AssertNoMessageError(eXDOCLine.QL_OuterPackWeightInfo, "Outer pack weight cannot be greater than 9999.999");
		}

		[TestDate(2022, 1, 18)]
		public void TestCheckQL_OuterPackWeightUnit()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var startDate = new ZDateTime(2022, 1, 1);
			var endDate = new ZDateTime(2022, 1, 31);
			helper.CreateNewOrGetExistingCusCodeList("AU",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSUnitOfMeasurement, "ONZ", "OUNCE",
				startDate, endDate);
			newFactory.Save();

			Assert("Pre-Condition", !eXDOCLine.QL_OuterPackWeightUnitInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_OuterPackWeightUnit = ZString.Empty;
			Assert("Outer pack weight unit is invalid (Zstring.Empty)", eXDOCLine.QL_OuterPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackWeightUnit = "ZZZ";
			Assert("Outer pack weight unit is an invalid code", eXDOCLine.QL_OuterPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackWeightUnit = "ONZ";
			Assert("Outer pack weight unit is valid", !eXDOCLine.QL_OuterPackWeightUnitInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_OuterPackWeightUnit = ZString.Empty;
			Assert("Outer pack weight unit is valid", !eXDOCLine.QL_OuterPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_OuterPackWeight = 12;
			eXDOCLine.QL_OuterPackWeightUnit = ZString.Empty;
			Assert("Outer pack weight unit is invalid", eXDOCLine.QL_OuterPackWeightUnitInfo.HasMessageErrors());
		}

		public void TestCheckQL_IntermediatePackCount()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackCount = -12;
			Assert("Intermediate pack count is invalid (negative)", eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackCount = ZInt.Zero;
			Assert("Intermediate pack count is valid", !eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackCount = 14;
			Assert("Intermediate pack count is valid", !eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_IntermediatePackCount = 13;
			Assert("Intermediate pack count is invalid when exporter defined description is filled in.", eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = ZString.Empty;
			eXDOCLine.QL_IntermediatePackType = EXDOCPacakgeTypeCodes.Codes.Bulk;
			Assert("Intermediate pack count is invalid when pack type is bulk.", eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackCount = 0;
			Assert("Intermediate pack count is 0 (valid) when pack type is bulk.", !eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackType = EXDOCPacakgeTypeCodes.Codes.MixedShipments;
			Assert("Intermediate pack count is 0 (valid) when pack type is mixed shipments.", !eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackCount = 6;
			Assert("Intermediate pack count is invalid when pack type is mixed shipments.", eXDOCLine.QL_IntermediatePackCountInfo.HasMessageErrors());
		}

		public void TestCheckQL_IntermediatePackType()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_IntermediatePackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackType = ZString.Empty;
			Assert("Intermediate pack type is valid (ZString.Empty)", !eXDOCLine.QL_IntermediatePackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackType = "ZZ";
			Assert("Intermediate pack type is an invalid code", eXDOCLine.QL_IntermediatePackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackType = EXDOCPacakgeTypeCodes.Codes.Bags;
			Assert("Intermediate pack type is valid", !eXDOCLine.QL_IntermediatePackTypeInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_IntermediatePackType = EXDOCPacakgeTypeCodes.Codes.Dozens;
			Assert("Intermediate pack type is an invalid when exporter defined description is filled in.", eXDOCLine.QL_IntermediatePackTypeInfo.HasMessageErrors());
		}

		public void TestCheckQL_IntermediatePackAccuracy()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_IntermediatePackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackAccuracy = ZString.Empty;
			Assert("Intermediate pack accuracy is valid (ZString.Empty)", !eXDOCLine.QL_IntermediatePackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackAccuracy = "Z";
			Assert("Intermediate pack accuracy is an invalid code", eXDOCLine.QL_IntermediatePackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			Assert("Intermediate pack accuracy is valid", !eXDOCLine.QL_IntermediatePackAccuracyInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_IntermediatePackAccuracy = EXDOCPackAccuracyCodes.Codes.EqualTo;
			Assert("Intermediate pack accuracy is invalid when exporter defined description is filled in.", eXDOCLine.QL_IntermediatePackAccuracyInfo.HasMessageErrors());
		}

		public void TestCheckQL_IntermediatePackWeight()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_IntermediatePackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackWeight = ZDecimal.Zero;
			Assert("Intermediate pack weight is valid (Zint.Zero)", !eXDOCLine.QL_IntermediatePackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackWeight = -12;
			Assert("Intermediate pack weight is invalid", eXDOCLine.QL_IntermediatePackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackWeight = 12;
			Assert("Intermediate pack weight is valid", !eXDOCLine.QL_IntermediatePackWeightInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_IntermediatePackWeight = 13;
			Assert("Intermediate pack weight is invalid when exporter defined description is filled in.", eXDOCLine.QL_IntermediatePackWeightInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = ZString.Empty;
			eXDOCLine.QL_IntermediatePackWeight = 99999.999;
			AssertHasMessageError("Value is greater than the maximum allowed by EXDOC but less then the database field size", eXDOCLine.QL_IntermediatePackWeightInfo, "Intermediate pack weight cannot be greater than 9999.999");
			eXDOCLine.QL_IntermediatePackWeight = 9999.999;
			AssertNoMessageError(eXDOCLine.QL_IntermediatePackWeightInfo, "Intermediate pack weight cannot be greater than 9999.999");
		}

		public void TestCheckQL_IntermediatePackWeightUnit()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_IntermediatePackWeightUnitInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_IntermediatePackWeightUnit = ZString.Empty;
			Assert("Intermediate pack weight unit is invalid (Zstring.Empty)", !eXDOCLine.QL_IntermediatePackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackWeightUnit = "ZZZ";
			Assert("Intermediate pack weight unit is an invalid code", eXDOCLine.QL_IntermediatePackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTons;
			Assert("Intermediate pack weight unit is valid", !eXDOCLine.QL_IntermediatePackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackWeight = 12;
			eXDOCLine.QL_IntermediatePackWeightUnit = ZString.Empty;
			Assert("Intermediate pack weight unit is invalid", eXDOCLine.QL_IntermediatePackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_IntermediatePackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTons;
			Assert("Intermediate pack weight unit is valid", !eXDOCLine.QL_IntermediatePackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_IntermediatePackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTons;
			Assert("Intermediate pack weight unit is invalid when exporter defined description is filled in.", eXDOCLine.QL_IntermediatePackWeightUnitInfo.HasMessageErrors());
		}

		public void TestCheckQL_InnerPackCount()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackCount = -12;
			Assert("Inner pack count is invalid (negative)", eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackCount = ZInt.Zero;
			Assert("Inner pack count is valid", !eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackCount = 14;
			Assert("Inner pack count is valid", !eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_InnerPackCount = 15;
			Assert("Inner pack count is invalid when exporter defined description is filled in.", eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = ZString.Empty;
			eXDOCLine.QL_InnerPackType = EXDOCPacakgeTypeCodes.Codes.MixedShipments;
			Assert("Inner pack count is invalid when pack type is mixed shipments.", eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackCount = 0;
			Assert("Inner pack count is valid (0) when pack type is mixed shipments.", !eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackType = EXDOCPacakgeTypeCodes.Codes.Bulk;
			Assert("Inner pack count is valid (0) when pack type is bulk.", !eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackCount = 8;
			Assert("Inner pack count is invalid when pack type is bulk.", eXDOCLine.QL_InnerPackCountInfo.HasMessageErrors());
		}

		public void TestCheckQL_InnerPackType()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_InnerPackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackType = ZString.Empty;
			Assert("Inner pack type is valid (ZString.Empty)", !eXDOCLine.QL_InnerPackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackType = "ZZ";
			Assert("Inner pack type is an invalid code", eXDOCLine.QL_InnerPackTypeInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackType = EXDOCPacakgeTypeCodes.Codes.Bags;
			Assert("Inner pack type is valid", !eXDOCLine.QL_InnerPackTypeInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_InnerPackType = EXDOCPacakgeTypeCodes.Codes.Jars;
			Assert("Inner pack type is an invalid when exporter defined description is filled in.", eXDOCLine.QL_InnerPackTypeInfo.HasMessageErrors());
		}

		public void TestCheckQL_InnerPackAccuracy()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_InnerPackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackAccuracy = ZString.Empty;
			Assert("Inner pack accuracy is valid (ZString.Empty)", !eXDOCLine.QL_InnerPackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackAccuracy = "Z";
			Assert("Inner pack accuracy is an invalid code", eXDOCLine.QL_InnerPackAccuracyInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackAccuracy = EXDOCPackAccuracyCodes.Codes.EqualTo;
			Assert("Inner pack accuracy is valid", !eXDOCLine.QL_InnerPackAccuracyInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_InnerPackAccuracy = EXDOCPackAccuracyCodes.Codes.Approximate;
			Assert("Inner pack accuracy is an invalid when exporter defined description is filled in.", eXDOCLine.QL_InnerPackAccuracyInfo.HasMessageErrors());
		}

		public void TestCheckQL_InnerPackWeight()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_InnerPackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeight = ZDecimal.Zero;
			Assert("Inner pack weight is valid (Zint.Zero)", !eXDOCLine.QL_InnerPackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeight = -12;
			Assert("Inner pack weight is invalid", eXDOCLine.QL_InnerPackWeightInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeight = 12;
			Assert("Inner pack weight is valid", !eXDOCLine.QL_InnerPackWeightInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_InnerPackWeight = 13;
			Assert("Inner pack weight is invalid when exporter defined description is filled in.", eXDOCLine.QL_InnerPackWeightInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = ZString.Empty;
			eXDOCLine.QL_InnerPackWeight = 99999.999;
			AssertHasMessageError("Value is greater than the maximum allowed by EXDOC but less then the database field size", eXDOCLine.QL_InnerPackWeightInfo, "Inner pack weight cannot be greater than 9999.999");
			eXDOCLine.QL_InnerPackWeight = 9999.999;
			AssertNoMessageError(eXDOCLine.QL_InnerPackWeightInfo, "Inner pack weight cannot be greater than 9999.999");
		}

		public void TestCheckQL_InnerPackWeightUnit()
		{
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			Assert("Pre-Condition", !eXDOCLine.QL_InnerPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeightUnit = ZString.Empty;
			Assert("Inner pack weight unit is invalid (Zstring.Empty)", !eXDOCLine.QL_InnerPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeightUnit = "ZZZ";
			Assert("Inner pack weight unit is an invalid code", eXDOCLine.QL_InnerPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTons;
			Assert("Inner pack weight unit is valid", !eXDOCLine.QL_InnerPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeight = 12;
			eXDOCLine.QL_InnerPackWeightUnit = ZString.Empty;
			Assert("Inner pack weight unit is invalid", eXDOCLine.QL_InnerPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_InnerPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTons;
			Assert("Inner pack weight unit is valid", !eXDOCLine.QL_InnerPackWeightUnitInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_Description = "This is a description";
			eXDOCLine.QL_InnerPackWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			Assert("Inner pack weight unit is invalid when exporter defined description is filled in.", eXDOCLine.QL_InnerPackWeightUnitInfo.HasMessageErrors());
		}

		public void TestCheckQL_BeefVealWeightAmount()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("CUTCM", "CUTCM");
			var cut3940H = refHelper.CreateNewOrGetExistingCusCodeList("AU", "CUTCM", "3940H", "FORE & HIND CUTS MXD (EU HQB)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(cut3940H.PK, "BoneInIndicator", "O");
			refHelper.CreateCusCodeListAttribute(cut3940H.PK, "IsBeefVeal", "Y");
			refHelper.CreateCusCodeListAttribute(cut3940H.PK, "IsChemicalLean", "N");
			Factory.Save();

			Assert("Pre-Condition", !eXDOCLine.QL_BeefVealWeightAmountInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_BeefVealWeightAmount = 12m;
			Assert("Only valid when produce type is Meat", eXDOCLine.QL_BeefVealWeightAmountInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_CutCode = "3940H";
			eXDOCLine.QL_BeefVealWeightAmount = 13.4m;
			Assert("valid cut code and commodity", !eXDOCLine.QL_BeefVealWeightAmountInfo.HasMessageErrors());
			eXDOCLine.QL_CutCode = "3420";
			eXDOCLine.QL_BeefVealWeightAmount = 12.3m;
			Assert("invalid cut code and commodity", eXDOCLine.QL_BeefVealWeightAmountInfo.HasMessageErrors());
		}

		public void TestCheckQL_ChemicalLeanPercentage()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("CUTCM", "CUTCM");
			var cut3951 = refHelper.CreateNewOrGetExistingCusCodeList("AU", "CUTCM", "3951", "HINDQUARTER - 0 RIB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(cut3951.PK, "BoneInIndicator", "O");
			refHelper.CreateCusCodeListAttribute(cut3951.PK, "IsBeefVeal", "N");
			refHelper.CreateCusCodeListAttribute(cut3951.PK, "IsChemicalLean", "Y");
			var cut3420 = refHelper.CreateNewOrGetExistingCusCodeList("AU", "CUTCM", "3420", "BRISKET ROLLED ROAST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(cut3420.PK, "BoneInIndicator", "N");
			refHelper.CreateCusCodeListAttribute(cut3420.PK, "IsBeefVeal", "N");
			refHelper.CreateCusCodeListAttribute(cut3420.PK, "IsChemicalLean", "N");

			Factory.Save();

			Assert("Pre-Condition", !eXDOCLine.QL_ChemicalLeanPercentageInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_PackType = EXDOCPackTypeCodes.Codes.BulkPack;
			eXDOCLine.QL_ChemicalLeanPercentage = 13;
			Assert("Only valid when produce type is Meat", eXDOCLine.QL_ChemicalLeanPercentageInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_CutCode = "3951";
			eXDOCLine.QL_ChemicalLeanPercentage = 15;
			Assert("valid cut code and commodity", !eXDOCLine.QL_ChemicalLeanPercentageInfo.HasMessageErrors());
			eXDOCLine.QL_CutCode = "3420";
			eXDOCLine.QL_ChemicalLeanPercentage = 12;
			Assert("invalid cut code and commodity", eXDOCLine.QL_ChemicalLeanPercentageInfo.HasMessageErrors());
			eXDOCLine.QL_CutCode = "3951";
			eXDOCLine.QL_PackType = EXDOCPackTypeCodes.Codes.BagInABox;
			eXDOCLine.QL_ChemicalLeanPercentage = 14;
			Assert("valid cut code and commodity, invalid pack type", eXDOCLine.QL_ChemicalLeanPercentageInfo.HasMessageErrors());
		}

		public void TestCheckQL_ProduceType()
		{
			QuarantineExDocEstablishmentAndTime process = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("No processing process", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("No processing process", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is a processing process", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is a processing process", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			process.EE_ProcessingType = ZString.Empty;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("No requirement for processing type on Grains", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is no packing process", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Packing;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is a packing process", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			QuarantineExDocEstablishmentAndTime process2 = eXDOCLine.Processes.AddNew();
			eXDOCHeader.Declaration.JE_RL_NKFinalDestination = "JPAMX";
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is no slaughter process goods going to Japan", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Slaughter;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is a slaughter process goods going to Japan", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
		}

		public void TestCheckQL_ProduceType_Fish()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PRODF", "Fish");
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPRDF", "COB", "COBBLER - SILVER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var process = eXDOCLine.Processes.AddNew();
			Assert("Pre-Condition", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			Assert("Pre-Condition", eXDOCHeader.IsNEXDOCSActive);
			process.EE_ProcessingType = ZString.Empty;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("No processing & catcherVessel process", eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Processing;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is a processing process", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			Assert("There is a CatcherVessel process", !eXDOCLine.QL_ProduceTypeInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.Declaration.JE_RL_NKFinalDestination = "CNBJS";
			eXDOCLine.QL_SupplimentaryCode = "WO";
			eXDOCLine.Validation.ValidateQL_ProduceType();
			process.EE_ProcessingType = ZString.Empty;
			AssertHasMessageError(eXDOCLine.QL_ProduceTypeInfo, "A catcher vessel (CT) or boat (CB) process is required for wild origin Fish shipments to China.");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherVessel;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			AssertNoMessageError(eXDOCLine.QL_ProduceTypeInfo, "A catcher vessel (CT) or boat (CB) process is required for wild origin Fish shipments to China.");
			process.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.CatcherBoat;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			AssertNoMessageError(eXDOCLine.QL_ProduceTypeInfo, "A catcher vessel (CT) or boat (CB) process is required for wild origin Fish shipments to China.");
			eXDOCLine.QL_SupplimentaryCode = "AQ";
			eXDOCLine.QL_ProductType = "COB";
			eXDOCLine.Validation.ValidateQL_ProduceType();
			AssertHasMessageError(eXDOCLine.QL_ProduceTypeInfo, "Aquaculture farm process is required for aquaculture COBBLER - SILVER shipments to China.");
			var process2 = eXDOCLine.Processes.AddNew();
			process2.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.AquacultureFarm;
			eXDOCLine.Validation.ValidateQL_ProduceType();
			AssertNoMessageError(eXDOCLine.QL_ProduceTypeInfo, "Aquaculture farm process is required for aquaculture COBBLER - SILVER shipments to China.");
		}

		public void TestCheckQL_GrowerNumber()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_GrowerNumberInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_GrowerNumber = "123456";
			Assert("Grower Number not allowed for Dairy", eXDOCLine.QL_GrowerNumberInfo.HasMessageErrors());
			eXDOCLine.QL_GrowerNumber = ZString.Empty;
			Assert("Grower Number is empty for Dairy", !eXDOCLine.QL_GrowerNumberInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_GrowerNumber = "123456";
			Assert("Grower Number is allowed for Horticulture", !eXDOCLine.QL_GrowerNumberInfo.HasMessageErrors());
		}

		public void TestCheckQL_HCFormatRequested()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_HCFormatRequestedInfo.HasMessageErrors());
			eXDOCLine.QL_HCFormatRequested = "12345";
			Assert("HC template is less then 6", !eXDOCLine.QL_HCFormatRequestedInfo.HasMessageErrors());
			eXDOCLine.QL_HCFormatRequested = "123456";
			Assert("HC template is equal to 6", !eXDOCLine.QL_HCFormatRequestedInfo.HasMessageErrors());
			eXDOCLine.QL_HCFormatRequested = "1234567";
			Assert("HC template is equal to 7", eXDOCLine.QL_HCFormatRequestedInfo.HasMessageErrors());
			eXDOCLine.QL_HCFormatRequested = "123456/1234";
			Assert("HC template/endorsement is in the correct format", !eXDOCLine.QL_HCFormatRequestedInfo.HasMessageErrors());
			eXDOCLine.QL_HCFormatRequested = "1234567/123";
			Assert("HC template/endorsement is in the incorrect format", eXDOCLine.QL_HCFormatRequestedInfo.HasMessageErrors());
			eXDOCLine.QL_HCFormatRequested = "12345/12345";
			Assert("HC template/endorsement is in the incorrect format", eXDOCLine.QL_HCFormatRequestedInfo.HasMessageErrors());
		}

		public void TestCheckQL_StatementNumber1()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_StatementNumber1Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_StatementNumber1 = 1234;
			Assert("Statement Number is not valid for Fish", eXDOCLine.QL_StatementNumber1Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_StatementNumber1 = 4321;
			Assert("Statement Number is valid for Grains and Plants", !eXDOCLine.QL_StatementNumber1Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_StatementNumber1 = 9856;
			Assert("Statement Number is valid for Horticulture", !eXDOCLine.QL_StatementNumber1Info.HasMessageErrors());
		}

		public void TestCheckQL_StatementNumber2()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_StatementNumber2Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_StatementNumber2 = 1234;
			Assert("Statement Number is not valid for Fish", eXDOCLine.QL_StatementNumber2Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_StatementNumber2 = 4321;
			Assert("Statement Number is valid for Grains and Plants", !eXDOCLine.QL_StatementNumber2Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_StatementNumber2 = 9856;
			Assert("Statement Number is valid for Horticulture", !eXDOCLine.QL_StatementNumber2Info.HasMessageErrors());
		}

		public void TestCheckQL_StatementNumber3()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_StatementNumber3Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_StatementNumber3 = 1234;
			Assert("Statement Number is not valid for Fish", eXDOCLine.QL_StatementNumber3Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_StatementNumber3 = 4321;
			Assert("Statement Number is valid for Grains and Plants", !eXDOCLine.QL_StatementNumber3Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_StatementNumber3 = 9856;
			Assert("Statement Number is valid for Horticulture", !eXDOCLine.QL_StatementNumber3Info.HasMessageErrors());
		}

		public void TestCheckQL_StatementNumber4()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_StatementNumber4Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_StatementNumber4 = 1234;
			Assert("Statement Number is not valid for Fish", eXDOCLine.QL_StatementNumber4Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_StatementNumber4 = 4321;
			Assert("Statement Number is valid for Grains and Plants", !eXDOCLine.QL_StatementNumber4Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_StatementNumber4 = 9856;
			Assert("Statement Number is valid for Horticulture", !eXDOCLine.QL_StatementNumber4Info.HasMessageErrors());
		}

		public void TestCheckQL_StatementNumber5()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_StatementNumber5Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_StatementNumber5 = 1234;
			Assert("Statement Number is not valid for Fish", eXDOCLine.QL_StatementNumber5Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_StatementNumber5 = 4321;
			Assert("Statement Number is valid for Grains and Plants", !eXDOCLine.QL_StatementNumber5Info.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_StatementNumber5 = 9856;
			Assert("Statement Number is valid for Horticulture", !eXDOCLine.QL_StatementNumber5Info.HasMessageErrors());
		}

		public void TestCheckQL_StatementText()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_StatementTextInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_StatementText = "Free text statement";
			eXDOCLine.InvoiceLine.JI_TempImportNum = "12345";
			Assert("Statement text is not valid for Grains and Plants", eXDOCLine.QL_StatementTextInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_StatementText = "Free text statement2";
			Assert("Statement text is valid for Horticulture", !eXDOCLine.QL_StatementTextInfo.HasMessageErrors());
			eXDOCLine.InvoiceLine.JI_TempImportNum = ZString.Empty;
			eXDOCLine.QL_StatementText = "Free text statement3";
			Assert("Statement text is valid for Horticulture but Temporary import number is empty.", eXDOCLine.QL_StatementTextInfo.HasMessageErrors());
		}

		public void TestCheckQL_AdditionalDeclarationComments()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_AddtionalDeclarationCommentsInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_AddtionalDeclarationComments = "Free text declaration";
			Assert("Statement text is not valid for Dairy", eXDOCLine.QL_AddtionalDeclarationCommentsInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_AddtionalDeclarationComments = "Free text declaration23";
			Assert("Statement text is valid for Grains and Plants", !eXDOCLine.QL_AddtionalDeclarationCommentsInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_AddtionalDeclarationComments = "Free text statement345";
			Assert("Statement text is valid for Horticulture", !eXDOCLine.QL_AddtionalDeclarationCommentsInfo.HasMessageErrors());
		}

		public void TestCheckQL_DrainedWeight()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_DrainedWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_DrainedWeight = 123;
			Assert("Drained weight has errors not valid for Dairy", eXDOCLine.QL_DrainedWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_DrainedWeight = 12;
			Assert("Drained weight has no errors valid for Fish", !eXDOCLine.QL_DrainedWeightInfo.HasMessageErrors());
			eXDOCLine.QL_DrainedWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			eXDOCLine.QL_DrainedWeight = 0;
			Assert("Drained weight is empty for Fish and weight unit is valid", eXDOCLine.QL_DrainedWeightInfo.HasMessageErrors());
		}

		public void TestCheckQL_DrainedWeightUnit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSUnitOfMeasurement,
				EXDOCMetricWeightUnitCodes.Codes.Kilotonne, "Kilotonne",
				ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			Assert("Pre_Condition", !eXDOCLine.QL_DrainedWeightInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_DrainedWeightUnit = EXDOCMetricWeightUnitCodes.Codes.MetricTonne;
			Assert("Drained Weight unit is not allowed for anything but Fish", eXDOCLine.QL_DrainedWeightUnitInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCLine.QL_DrainedWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilotonne;
			Assert("Drained Weight unit is valid and for Fish", !eXDOCLine.QL_DrainedWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_DrainedWeightUnit = "VCX";
			Assert("Drained Weight unit is invalid and for Fish", eXDOCLine.QL_DrainedWeightUnitInfo.HasMessageErrors());
			eXDOCLine.QL_DrainedWeight = 12;
			eXDOCLine.QL_DrainedWeightUnit = ZString.Empty;
			Assert("Drained Weight unit is empty and for Fish while Drained weight is not empty", eXDOCLine.QL_DrainedWeightUnitInfo.HasMessageErrors());
		}

		public void TestCheckQL_PercentageOfMilkFat()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_PercentOfMilkFatInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_PercentOfMilkFat = 23.4;
			Assert("Percentage of milk fat not valid for Meat", eXDOCLine.QL_PercentOfMilkFatInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_PercentOfMilkFat = 24.55;
			Assert("Percentage of milk fat is valid for Dairy", !eXDOCLine.QL_PercentOfMilkFatInfo.HasMessageErrors());
			eXDOCLine.QL_PercentOfMilkFat = -45.3;
			Assert("Percentage of milk fat is valid for Dairy but cannot be negative", eXDOCLine.QL_PercentOfMilkFatInfo.HasMessageErrors());
			eXDOCLine.QL_PercentOfMilkFat = 134.45;
			Assert("Percentage of milk fat is valid for Dairy but cannot be greater than 100", eXDOCLine.QL_PercentOfMilkFatInfo.HasMessageErrors());
			eXDOCLine.QL_PercentOfMilkFat = 50.00;
			Assert("Percentage of milk fat is valid for Dairy and valid value", !eXDOCLine.QL_PercentOfMilkFatInfo.HasMessageErrors());
		}

		public void TestCheckQL_PercentageOfMilkProtein()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_PercentOfMilkProteinInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_PercentOfMilkProtein = 23.4;
			Assert("Percentage of milk protein not valid for Meat", eXDOCLine.QL_PercentOfMilkProteinInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_PercentOfMilkProtein = 24.55;
			Assert("Percentage of milk protein is valid for Dairy", !eXDOCLine.QL_PercentOfMilkProteinInfo.HasMessageErrors());
			eXDOCLine.QL_PercentOfMilkProtein = -45.3;
			Assert("Percentage of milk protein is valid for Dairy but cannot be negative", eXDOCLine.QL_PercentOfMilkProteinInfo.HasMessageErrors());
			eXDOCLine.QL_PercentOfMilkProtein = 134.45;
			Assert("Percentage of milk protein is valid for Dairy but cannot be greater than 100", eXDOCLine.QL_PercentOfMilkProteinInfo.HasMessageErrors());
			eXDOCLine.QL_PercentOfMilkProtein = 50.00;
			Assert("Percentage of milk protein is valid for Dairy and valid value", !eXDOCLine.QL_PercentOfMilkProteinInfo.HasMessageErrors());
		}

		public void TestCheckQL_TotalWeightOfMilkFatInMixtures()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_TotalWeightOfMilkFatInMixturesInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_TotalWeightOfMilkFatInMixtures = 234;
			Assert("Total Weight of milk fat in mixtures is not valid for Meat", eXDOCLine.QL_TotalWeightOfMilkFatInMixturesInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_TotalWeightOfMilkFatInMixtures = 435;
			Assert("Total Weight of milk fat in mixtures is valid for Dairy", !eXDOCLine.QL_TotalWeightOfMilkFatInMixturesInfo.HasMessageErrors());
		}

		public void TestCheckQL_TotalWeightOfMilkProteinInMixtures()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_TotalWeightOfMilkProteinInMixturesInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_TotalWeightOfMilkProteinInMixtures = 234;
			Assert("Total Weight of milk protein in mixtures is not valid for Meat", eXDOCLine.QL_TotalWeightOfMilkProteinInMixturesInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_TotalWeightOfMilkProteinInMixtures = 435;
			Assert("Total Weight of milk protein in mixtures is valid for Dairy", !eXDOCLine.QL_TotalWeightOfMilkProteinInMixturesInfo.HasMessageErrors());
		}

		public void TestCheckQL_SaltingDate()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_SaltingDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_SaltingDate = new ZDateTime(2006, 12, 12);
			Assert("Salting date is not valid for Horticulture", eXDOCLine.QL_SaltingDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCLine.QL_SaltingDate = new ZDateTime(2006, 12, 13);
			Assert("Salting date is valid for Skins and Hides", !eXDOCLine.QL_SaltingDateInfo.HasMessageErrors());
		}

		public void TestCheckQL_MeatInspectionDescription()
		{
			Assert("Pre-Condition", !eXDOCLine.QL_MeatInspectionDescriptionInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_MeatInspectionDescription = "Test for Horticulture";
			AssertHasMessageErrorContaining("Line Item Description is NOT valid for Horticulture", eXDOCLine.QL_MeatInspectionDescriptionInfo, "Line item description may only be supplied when produce type is Meat or Dairy.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCLine.QL_MeatInspectionDescription = "Test for Meat";
			AssertNoMessageErrorContaining("Line Item Description is valid for Meat", eXDOCLine.QL_MeatInspectionDescriptionInfo, "Line item description may only be supplied when produce type is Meat or Dairy.");
			Assert("Line Item Description is valid for Meat", !eXDOCLine.QL_MeatInspectionDescriptionInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCLine.QL_MeatInspectionDescription = "Test for Dairy";
			AssertNoMessageErrorContaining("Line Item Description is valid for Dairy", eXDOCLine.QL_MeatInspectionDescriptionInfo, "Line item description may only be supplied when produce type is Meat or Dairy.");
			Assert("Line Item Description is valid for Dairy", !eXDOCLine.QL_MeatInspectionDescriptionInfo.HasMessageErrors());
		}

		public void TestCheckQL_CatchStartDate()
		{
			TestCatchDate(eXDOCLine.QL_CatchStartDateInfo, eXDOCLine.QL_CatchEndDateInfo);
			eXDOCLine.QL_CatchEndDate = ZDateTime.Now.AddMonths(-1);
			eXDOCLine.QL_CatchStartDate = ZDateTime.Now;
			AssertHasMessageErrorContaining(eXDOCLine.QL_CatchStartDateInfo, QuarantineExDocLineValidation.errorCatchDateOrder);
			eXDOCLine.QL_CatchEndDate = ZDateTime.Now;
			eXDOCLine.QL_CatchStartDate = ZDateTime.Now.AddMonths(-1);
			AssertNoMessageErrorContaining(eXDOCLine.QL_CatchStartDateInfo, QuarantineExDocLineValidation.errorCatchDateOrder);
		}

		public void TestCheckQL_CatchEndDate()
		{
			TestCatchDate(eXDOCLine.QL_CatchEndDateInfo, eXDOCLine.QL_CatchStartDateInfo);
			eXDOCLine.QL_CatchStartDate = ZDateTime.Now;
			eXDOCLine.QL_CatchEndDate = ZDateTime.Now.AddMonths(-1);
			AssertHasMessageErrorContaining(eXDOCLine.QL_CatchEndDateInfo, QuarantineExDocLineValidation.errorCatchDateOrder);
			eXDOCLine.QL_CatchStartDate = ZDateTime.Now.AddMonths(-1);
			eXDOCLine.QL_CatchEndDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(eXDOCLine.QL_CatchEndDateInfo, QuarantineExDocLineValidation.errorCatchDateOrder);
		}

		public void TestCheckQL_FishWaterIndicator()
		{
			var targetInfo = eXDOCLine.QL_FishWaterIndicatorInfo;
			eXDOCLine.QL_FishWaterIndicator = "F";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);

			eXDOCLine.QL_FishWaterIndicator = "X";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);

			eXDOCLine.QL_FishWaterIndicator = "";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckQL_CombinedNomenclature()
		{
			eXDOCLine.QL_CombinedNomenclature = "F";
			AssertHasMessageError(eXDOCLine.QL_CombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
			AssertHasMessageError(eXDOCLine.QL_FormattedCombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");

			eXDOCLine.QL_CombinedNomenclature = "";
			AssertNoMessageError(eXDOCLine.QL_CombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
			AssertNoMessageError(eXDOCLine.QL_FormattedCombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");

			eXDOCLine.QL_CombinedNomenclature = "12345";
			AssertHasMessageError(eXDOCLine.QL_CombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
			AssertHasMessageError(eXDOCLine.QL_FormattedCombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");

			eXDOCLine.QL_CombinedNomenclature = "123456";
			AssertNoMessageError(eXDOCLine.QL_CombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
			AssertNoMessageError(eXDOCLine.QL_FormattedCombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");

			eXDOCLine.QL_CombinedNomenclature = "1234567";
			AssertHasMessageError(eXDOCLine.QL_CombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
			AssertHasMessageError(eXDOCLine.QL_FormattedCombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");

			eXDOCLine.QL_CombinedNomenclature = "12345678";
			AssertNoMessageError(eXDOCLine.QL_CombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
			AssertNoMessageError(eXDOCLine.QL_FormattedCombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");

			eXDOCLine.QL_CombinedNomenclature = "123456789";
			AssertHasMessageError(eXDOCLine.QL_CombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
			AssertHasMessageError(eXDOCLine.QL_FormattedCombinedNomenclatureInfo, "Combined Nomenclature must be 6 or 8 digits.");
		}

		public void TestCheckQL_ProductPart()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSProductPart, "EXDPP");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "EXDPP", "25", "Soil", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "EXDPP", "26", "Peat", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCLine.QL_ProductPart = "26";
			AssertHasMessageError(eXDOCLine.QL_ProductPartInfo, "Product Part may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCLine.QL_ProductPart = "";
			AssertNoMessageError(eXDOCLine.QL_ProductPartInfo, "Product Part may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCLine.QL_ProductPart = "26";
			AssertNoMessageError(eXDOCLine.QL_ProductPartInfo, "Product Part may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCLine.QL_ProductPart = "26";
			AssertNoMessageError(eXDOCLine.QL_ProductPartInfo, "Product Part may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			eXDOCLine.QL_ProductPart = "EGG";
			AssertHasMessageError(eXDOCLine.QL_ProductPartInfo, "Product Part may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCLine.QL_ProductPart = "25";
			AssertNoMessageErrorContaining(eXDOCLine.QL_ProductPartInfo, ListValidation.InvalidCodeMessageError);

			eXDOCLine.QL_ProductPart = "X";
			AssertHasMessageErrorContaining(eXDOCLine.QL_ProductPartInfo, ListValidation.InvalidCodeMessageError);

			eXDOCLine.QL_ProductPart = "";
			AssertNoMessageErrorContaining(eXDOCLine.QL_ProductPartInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			eXDOCHeader = invoiceHeader.QuarantineExDocHeader;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			eXDOCLine = invoiceLine.QuarantineExDocLine;
		}
		JobComInvoiceLine invoiceLine;
		QuarantineExDocHeader eXDOCHeader;
		QuarantineExDocLine eXDOCLine;

		void TestCatchDate(ZPropertyInfo prop, ZPropertyInfo brotherProp)
		{
			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			brotherProp.Value = ZDateTime.Now;
			prop.Value = ZDateTime.Empty;
			AssertHasMessageErrorContaining(prop, QuarantineExDocLineValidation.errorFishNeedBothCatchDates);
			prop.Value = ZDateTime.Now;
			AssertNoMessageErrorContaining(prop, QuarantineExDocLineValidation.errorFishNeedBothCatchDates);

			AssertNoMessageErrorContaining(prop, QuarantineExDocLineValidation.errorCatchDateOnyNeededForFish);
			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			prop.Value = ZDateTime.Now.AddSeconds(1);
			AssertHasMessageErrorContaining(prop, QuarantineExDocLineValidation.errorCatchDateOnyNeededForFish);
			eXDOCLine.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
		}
	}
}
