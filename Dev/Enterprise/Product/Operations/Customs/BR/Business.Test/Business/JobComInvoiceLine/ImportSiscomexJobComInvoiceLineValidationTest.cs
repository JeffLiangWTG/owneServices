using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportSiscomexJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckJI_Tariff()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m, preference: RatePreferenceType.Normal);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ExTariff;
			invoiceLine.DutyRateIsOverridden = true;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "There is no applicable DTY rate");

			invoiceLine.DutyRateIsOverridden = false;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "There is no applicable DTY rate");
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, "XX", ContainerTypeList.Codes.ReturnableGlassBottle);
		}

		public void TestCheckJI_CustomsThirdUnitQty()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsThirdUnitQtyInfo, "X", CapacityUnitList.Codes.L);
		}

		public void TestCheckMercosulForeignDeclarationType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.MercosulForeignDeclarationTypeInfo, "XXXXX", CertificateTypeList.Codes.CCPTC);

			invoiceLine.MercosulForeignDeclarations.AddNew();
			invoiceLine.MercosulForeignDeclarationType = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.MercosulForeignDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.MercosulForeignDeclarationType = CertificateTypeList.Codes.CCPTC;
			AssertNoMessageErrorContaining(invoiceLine.MercosulForeignDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckDutyTaxRegime()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.DutyTaxRegimeInfo, "You have not entered a Tax Regime.");
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.DutyTaxRegimeInfo, "X", "1");
		}

		public void TestCheckDutyLegalBase()
		{
			var taxRegimeCode = "2";
			ReferenceTestDataHelper.CreateReferenceDataForPISLegalBaseList(Factory, taxRegimeCode);
			ReferenceTestDataHelper.CreateReferenceDataForDutyLegalBaseList(Factory, taxRegimeCode);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			invoiceLine.DutyTaxRegime = taxRegimeCode;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.DutyLegalBaseInfo, "You have not entered a Legal Base.");
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.DutyLegalBaseInfo, "XX", "01");
		}

		public void TestCheckPISTaxRegime()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.PisCofinsTaxRegimeInfo, "X", "1");
		}

		public void TestCheckJI_PrimaryPreference()
		{
			Factory.ClearCachedValue<CodeDescriptionPairList>("BR_EN_ZZRefCusPreference");
			ReferenceTestDataHelper.CreatePreferenceViewForPrimaryPreferenceList(Factory);
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_PrimaryPreferenceInfo, "XX", Constants.RatePreferenceType.Normal);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_OA_ManufacturerAddress()
		{
			var manufacturer = OrgHeader.New(Factory);
			manufacturer.MainAddress.CompanyName = "MANUFACTURER";
			manufacturer.MainAddress.OA_Email = "MANUFACTURER@TEST.COM";
			manufacturer.MainAddress.OA_Address1 = "MANUFACTURER ADDRESS 1";
			manufacturer.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "MANUFACTURER ADDITIONAL ADDRESS";
			manufacturer.MainAddress.OA_City = "MANUFACTURER CITY";
			manufacturer.MainAddress.OA_State = "MS";
			manufacturer.OH_Code = "XXX";

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;

			invoiceLine.InvoiceHeader.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");

			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");
		}

		public void TestCheckJI_OA_ManufacturerAddress_StatusAndStreetNumber()
		{
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			AddressValidationHelperTest.TestCheckAddressStatusAndStreetNumber(Factory, invoiceLine.JI_OA_ManufacturerAddressInfo, true);
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			AddressValidationHelperTest.TestCheckAddressStatusAndStreetNumber(Factory, invoiceLine.JI_OA_ManufacturerAddressInfo, false);
		}

		public void TestCheckPisRateIsOverridden()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 50m, rateType: Constants.RateTypes.PIS, rateCode: Constants.RateCodes.PIS);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceLine.JI_Tariff = "03024100";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.Validation.ValidatePisRateIsOverridden();
			AssertNoMessageError("PisRateIsOverridden must NOT contain message error", invoiceLine.PisRateIsOverriddenInfo, "There is no applicable PIS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.Validation.ValidatePisRateIsOverridden();
			AssertNoMessageError("PisRateIsOverridden must NOT contain message error", invoiceLine.PisRateIsOverriddenInfo, "There is no applicable PIS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.Validation.ValidatePisRateIsOverridden();
			AssertHasMessageError("PisRateIsOverridden must contain message error", invoiceLine.PisRateIsOverriddenInfo, "There is no applicable PIS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.PisRateIsOverridden = true;
			AssertNoMessageError("PisRateIsOverridden must NOT contain message error", invoiceLine.PisRateIsOverriddenInfo, "There is no applicable PIS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Immunity;
			invoiceLine.PisRateIsOverridden = false;
			AssertNoMessageError("PisRateIsOverridden must NOT contain message error", invoiceLine.PisRateIsOverriddenInfo, "There is no applicable PIS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			var specialCase = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCase.TaxGroup = Constants.RateCodes.PIS;
			specialCase.RateOrUnitValue = 10m;
			AssertNoErrorContaining("PisRateIsOverridden must NOT contain message error", invoiceLine.PisRateIsOverriddenInfo, "Special Cases Ad Valorem has been entered. You cannot override the rate.");

			invoiceLine.PisRateIsOverridden = true;
			invoiceLine.Validation.ValidatePisRateIsOverridden();
			AssertHasErrorContaining("PisRateIsOverridden must NOT contain message error", invoiceLine.PisRateIsOverriddenInfo, "Special Cases Ad Valorem has been entered. You cannot override the rate.");
		}

		public void TestCheckCofinsRateIsOverridden()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 50m, rateType: Constants.RateTypes.Cofins, rateCode: Constants.RateCodes.Cofins);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceLine.JI_Tariff = "03024100";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.Validation.ValidateCofinsRateIsOverridden();
			AssertNoMessageError("CofinsRateIsOverridden must NOT contain message error", invoiceLine.CofinsRateIsOverriddenInfo, "There is no applicable COFINS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.JI_CountryOfOrigin = "";
			invoiceLine.Validation.ValidateCofinsRateIsOverridden();
			AssertNoMessageError("CofinsRateIsOverridden must contain message error", invoiceLine.CofinsRateIsOverriddenInfo, "There is no applicable COFINS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.Validation.ValidateCofinsRateIsOverridden();
			AssertHasMessageError("CofinsRateIsOverridden must contain message error", invoiceLine.CofinsRateIsOverriddenInfo, "There is no applicable COFINS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.CofinsRateIsOverridden = true;
			AssertNoMessageError("CofinsRateIsOverridden must NOT contain message error", invoiceLine.CofinsRateIsOverriddenInfo, "There is no applicable COFINS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			invoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Immunity;
			invoiceLine.CofinsRateIsOverridden = false;
			AssertNoMessageError("CofinsRateIsOverridden must NOT contain message error", invoiceLine.CofinsRateIsOverriddenInfo, "There is no applicable COFINS Ad Valorem Rate (%) for the Tariff. Override must be used.");

			var specialCase = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCase.TaxGroup = Constants.RateCodes.Cofins;
			specialCase.RateOrUnitValue = 10m;
			AssertNoErrorContaining("CofinsRateIsOverridden must NOT contain message error", invoiceLine.CofinsRateIsOverriddenInfo, "Special Cases Ad Valorem has been entered. You cannot override the rate.");

			invoiceLine.CofinsRateIsOverridden = true;
			invoiceLine.Validation.ValidateCofinsRateIsOverridden();
			AssertHasErrorContaining("CofinsRateIsOverridden must contain message error", invoiceLine.CofinsRateIsOverriddenInfo, "Special Cases Ad Valorem has been entered. You cannot override the rate.");
		}

		public void TestCheckIPIRateIsOverriddenForIPITaxRegime()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 50m, rateType: Constants.RateTypes.IPI, rateCode: Constants.RateCodes.IPI);

			invoiceLine.JI_Tariff = "03024100";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.NonTaxable;
			invoiceLine.Validation.ValidateIPIRateIsOverridden();
			AssertNoMessageError("IPIRateIsOverridden must NOT contain message error", invoiceLine.IPIRateIsOverriddenInfo, "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used.");

			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.JI_CountryOfOrigin = "";
			invoiceLine.Validation.ValidateIPIRateIsOverridden();
			AssertNoMessageError("IPIRateIsOverridden must contain message error", invoiceLine.IPIRateIsOverriddenInfo, "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used.");

			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.Validation.ValidateIPIRateIsOverridden();
			AssertHasMessageError("IPIRateIsOverridden must contain message error", invoiceLine.IPIRateIsOverriddenInfo, "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used.");

			invoiceLine.JI_Tariff = "03024100";
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine.Validation.ValidateIPIRateIsOverridden();
			AssertNoMessageError("IPIRateIsOverridden must NOT contain message error", invoiceLine.IPIRateIsOverriddenInfo, "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used.");

			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			invoiceLine.Validation.ValidateIPIRateIsOverridden();
			AssertHasMessageError("IPIRateIsOverridden must contain message error", invoiceLine.IPIRateIsOverriddenInfo, "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used.");

			invoiceLine.IPIRateIsOverridden = true;
			AssertNoMessageError("IPIRateIsOverridden must NOT contain message error", invoiceLine.IPIRateIsOverriddenInfo, "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used.");

			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Reduction;
			invoiceLine.Validation.ValidateIPIRateIsOverridden();
			AssertNoMessageError("IPIRateIsOverridden must contain message error", invoiceLine.IPIRateIsOverriddenInfo, "There is no applicable IPI Ad Valorem (%) for the Tariff. Override must be used.");

			invoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.FullCollection;
			var specialCase = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCase.TaxGroup = Constants.RateCodes.IPI;
			specialCase.RateOrUnitValue = 10m;
			AssertNoErrorContaining("IPIRateIsOverridden must NOT contain message error", invoiceLine.IPIRateIsOverriddenInfo, "Special Cases Ad Valorem has been entered. You cannot override the rate.");

			invoiceLine.IPIRateIsOverridden = true;
			invoiceLine.Validation.ValidateIPIRateIsOverridden();
			AssertHasErrorContaining("IPIRateIsOverridden must contain message error", invoiceLine.IPIRateIsOverriddenInfo, "Special Cases Ad Valorem has been entered. You cannot override the rate.");
		}

		public void TestCheckIPIRateIsOverriddenWhenDeletedIPITariff()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024100", 50m, rateType: Constants.RateTypes.IPI, rateCode: Constants.RateCodes.IPI);

			invoiceLine.JI_Tariff = "03024100";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageError("IPIRateIsOverridden must NOT contain message error", invoiceLine.IPIRateIsOverriddenInfo, "Enter Ex IPI Tariff in Additional Tariff Grid");

			invoiceLine.IPIRateIsOverridden = true;
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageError("IPIRateIsOverridden must NOT contain message error", invoiceLine.IPIRateIsOverriddenInfo, "Enter Ex IPI Tariff in Additional Tariff Grid");

			invoiceLine.AdditionalTariffs.RemoveAndDeleteAll();
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageError("IPIRateIsOverridden must contain message error", invoiceLine.IPIRateIsOverriddenInfo, "Enter Ex IPI Tariff in Additional Tariff Grid");
		}

		public void TestCheckIPIVigentRateValue()
		{
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.IPIVigentRateValueInfo);
		}

		public void TestCheckDutyVigentRateValue()
		{
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			invoiceLine.DutyRateIsOverridden = true;
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.DutyVigentRateValueInfo);
		}

		public void TestCheckFTAMarginRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024200", 50m);
			invoiceLine.JI_Tariff = "03024200";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;
			invoiceLine.DutyRateIsOverridden = true;
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.FTAMarginRateValueInfo);

			invoiceLine.FTAMarginRateValue = 0;
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(invoiceLine.FTAMarginRateValueInfo);

			invoiceLine.DutyRateIsOverridden = false;
			AssertNoMessageErrorContaining(invoiceLine.FTAMarginRateValueInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckReductionMarginRateValue()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "03024200", 50m);
			invoiceLine.JI_Tariff = "03024200";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReductionMargin;
			invoiceLine.DutyRateIsOverridden = true;
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.ReductionMarginRateValueInfo);

			invoiceLine.ReductionMarginRateValue = 0;
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(invoiceLine.ReductionMarginRateValueInfo);

			invoiceLine.DutyRateIsOverridden = false;
			AssertNoMessageErrorContaining(invoiceLine.ReductionMarginRateValueInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckReducedDutyRateValue()
		{
			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ReducedRate;
			invoiceLine.DutyRateIsOverridden = true;
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.ReducedDutyRateValueInfo);
		}

		public void TestCheckPisVigentRateValue()
		{
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.PisVigentRateValueInfo);
		}

		public void TestCheckCofinsVigentRateValue()
		{
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.CofinsVigentRateValueInfo);
		}

		public void TestCheckICMSFCPRateValue()
		{
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.ICMSFCPRateValueInfo);
		}

		public void TestCheckJI_CGC_Catalog()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_CGC_CatalogInfo);
		}

		public void TestCheckJI_GoodsApplication()
		{
			var codesNotRequireGoodsApplication = new[]
			{
				MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04,
				MessageSubTypeList.Codes._05, MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07,
				MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10,
			};

			foreach (var code in new MessageSubTypeList().GetAllCodes())
			{
				declaration.JE_MessageSubType = code;
				CombineAssertions($"JE_MessageSubType = {code}", () =>
				{
					if (codesNotRequireGoodsApplication.Contains(code))
					{
						ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_GoodsApplicationInfo);
						ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_GoodsApplicationInfo, "X", GoodsApplicationTypeList.Codes.Consumption);
					}
					else
					{
						ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_GoodsApplicationInfo, "X", GoodsApplicationTypeList.Codes.Consumption);
					}
				});
			}
		}

		public void TestCheckJI_GoodsCondition()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_GoodsConditionInfo, "X", GoodsConditionTypeList.Codes.UsedMaterial);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_GoodsConditionInfo);
		}

		public void TestCheckJI_ICMSRate()
		{
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(invoiceLine.JI_ICMSRateInfo);
		}

		public void TestCheckJI_ICMSBaseValueReductionPercentage()
		{
			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = ZDecimal.Zero;
			AssertNoErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.JI_ICMSRate = 10m;
			AssertHasMessageErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo);

			invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
			AssertNoErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
		}

		public void TestCheckJI_ICMSTotalAmountReductionPercentage()
		{
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.FullCollection;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = 150m;
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "Percentage should be a value between 0 and 100.");
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			AssertHasMessageErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo);

			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = ZDecimal.Zero;
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
		}

		public void TestCheckJI_ICMSFormula()
		{
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.JI_ICMSFormula = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_ICMSFormulaInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_ICMSFormulaInfo, "X", ICMSFormulaList.Codes.BC);
		}

		public void TestCheckJI_ManufacturerAuthorityIdentifier()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_ManufacturerAuthorityIdentifierInfo);
		}

		public void TestCheckJI_ManufacturerAuthorityVersion()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_ManufacturerAuthorityVersionInfo);
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.ImportSiscomex;
	}
}
