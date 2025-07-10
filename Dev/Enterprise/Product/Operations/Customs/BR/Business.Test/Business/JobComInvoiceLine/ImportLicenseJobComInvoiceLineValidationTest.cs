using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckJI_Tariff()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m, preference: RatePreferenceType.Normal);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoNotifications(invoiceLine.JI_TariffInfo);
		}

		public void TestCheckJI_Model()
		{
			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.TemporaryAdmission;
			invoiceLine.Validation.ValidateJI_Model();
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			invoiceLine.JI_Model = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Model = "XXX";
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Model = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoiceLine.JI_Model = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Brand()
		{
			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.TemporaryAdmission;
			invoiceLine.Validation.ValidateJI_BrandName();
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			invoiceLine.JI_BrandName = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_BrandName = "XXX";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine.JI_BrandName = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoiceLine.JI_BrandName = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckDutyTaxRegime()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory, BRJobMessageTypeList.Codes.ImportSiscomex);

			invoiceLine.Declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.DutyTaxRegimeInfo, "You have not entered a Tax Regime.");
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.DutyTaxRegimeInfo, "X", "1");
		}

		public void TestCheckDutyLegalBase()
		{
			var taxRegimeCode = "2";
			ReferenceTestDataHelper.CreateReferenceDataForPISLegalBaseList(Factory, taxRegimeCode);
			ReferenceTestDataHelper.CreateReferenceDataForDutyLegalBaseList(Factory, taxRegimeCode);

			invoiceLine.Declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			invoiceLine.DutyTaxRegime = taxRegimeCode;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.DutyLegalBaseInfo, "You have not entered a Legal Base.");
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.DutyLegalBaseInfo, "XX", "01");
		}

		public void TestCheckJI_SecondaryPreference()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_SecondaryPreferenceInfo, "XX", "AR99");
		}

		public void TestCheckJI_ManufacturerIndicator()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "TEST1";
			supplier.Addresses.AddNew();

			invoiceLine.JI_ManufacturerIndicator = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_ManufacturerIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.InvoiceHeader.SupplierDocAddressPK = supplier.Addresses[0].PK;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_ManufacturerIndicatorInfo, "X", ManufacturerIndicatorList.Codes._2);
		}

		public void TestCheckJI_UsedMaterialRegime()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_UsedMaterialRegimeInfo, "X", UsedMaterialRegimeList.Codes.Nationalization);
		}

		public void TestCheckJI_UsedMaterialOperationType()
		{
			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_UsedMaterialOperationTypeInfo, "X", GoodsConditionOperationTypeList.Codes.ExTariff);
		}

		public void TestCheckJI_UsedMaterialSerialNumber()
		{
			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.TemporaryAdmission;
			invoiceLine.Validation.ValidateJI_UsedMaterialSerialNumber();
			AssertNoMessageErrorContaining(invoiceLine.JI_UsedMaterialSerialNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			invoiceLine.JI_UsedMaterialSerialNumber = "X";
			AssertNoMessageErrorContaining(invoiceLine.JI_UsedMaterialSerialNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_UsedMaterialSerialNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_UsedMaterialSerialNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_UsedMaterialManufactureYear()
		{
			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.TemporaryAdmission;
			invoiceLine.Validation.ValidateJI_UsedMaterialManufactureYear();
			AssertNoMessageErrorContaining(invoiceLine.JI_UsedMaterialManufactureYearInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			invoiceLine.JI_UsedMaterialManufactureYear = "2022";
			AssertNoMessageErrorContaining(invoiceLine.JI_UsedMaterialManufactureYearInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_UsedMaterialManufactureYear = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_UsedMaterialManufactureYearInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_UsedMaterialManufactureYear = "XXXX";
			AssertHasErrorContaining(invoiceLine.JI_UsedMaterialManufactureYearInfo, "Incorrect format (YYYY).");

			invoiceLine.JI_UsedMaterialManufactureYear = "0159";
			AssertHasErrorContaining(invoiceLine.JI_UsedMaterialManufactureYearInfo, "Incorrect format (YYYY).");

			invoiceLine.JI_UsedMaterialManufactureYear = "123";
			AssertHasErrorContaining(invoiceLine.JI_UsedMaterialManufactureYearInfo, "Incorrect format (YYYY).");
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.ImportLicense;
	}
}
