using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCAddInfoJobComInvoicelineValidationTest : AUAddInfoValidationTest
	{
		[TestDate(2006, 12, 12)]
		public void CheckZA_TemporaryImportDate_Hidden()
		{
			Assert("Pre-Condition", !testInvoiceLine.JI_TempImportDateInfo.HasMessageErrors());
			testInvoiceLine.JI_TempImportDate = new ZDateTime(2006, 12, 12);
			Assert("Import Number is empty cannot have a date", testInvoiceLine.JI_TempImportDateInfo.HasMessageErrors());
			testInvoiceLine.JI_TempImportNum = "12334";
			testInvoiceLine.JI_TempImportDate = new ZDateTime(2006, 12, 11);
			Assert("Import Number is not empty can have a date", !testInvoiceLine.JI_TempImportDateInfo.HasMessageErrors());
			testInvoiceLine.JI_TempImportDate = new ZDateTime(2006, 12, 14);
			Assert("Import date is to late", testInvoiceLine.JI_TempImportDateInfo.HasMessageErrors());
		}

		public void TestCheckZA_RelatedExportPermitNumber_Hidden()
		{
			Assert("Pre-Condition", !testInvoiceLine.JI_RelatedExportPermitNumberInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitAuthority = EXDOCPermitTypeCodes.Codes.AustralianFisheriesManagementAuthority;
			testInvoiceLine.JI_RelatedExportPermitNumber = "1";
			testInvoiceLine.JI_RelatedExportPermitNumber = ZString.Empty;
			Assert("Related Export Permit Number has errors as it is not entered", testInvoiceLine.JI_RelatedExportPermitNumberInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitAuthority = ZString.Empty;
			testInvoiceLine.JI_RelatedExportPermitDate = ZDateTime.Now;
			testInvoiceLine.JI_RelatedExportPermitNumber = "1";
			testInvoiceLine.JI_RelatedExportPermitNumber = ZString.Empty;
			Assert("Related Export Permit Number has errors as it is not entered", testInvoiceLine.JI_RelatedExportPermitNumberInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitAuthority = EXDOCPermitTypeCodes.Codes.AustralianFisheriesManagementAuthority;
			testInvoiceLine.JI_RelatedExportPermitNumber = "TEST123456";
			Assert("Related Export Permit Number has has been entered", !testInvoiceLine.JI_RelatedExportPermitNumberInfo.HasMessageErrors());
		}

		public void TestCheckZA_RelatedExportPermitAuthority_Hidden()
		{
			Assert("Pre-Condition", !testInvoiceLine.JI_RelatedExportPermitAuthorityInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitAuthority = "ZZZ";
			Assert("Related Export Authority has errors as the code entered is invalid", testInvoiceLine.JI_RelatedExportPermitAuthorityInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitAuthority = EXDOCPermitTypeCodes.Codes.AustralianHoneyBoard;
			Assert("Related Export Authority has no errors as the code entered is valid", !testInvoiceLine.JI_RelatedExportPermitAuthorityInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitNumber = "TEST43534";
			testInvoiceLine.JI_RelatedExportPermitAuthority = ZString.Empty;
			Assert("Related Export Authority cannot be blank when a number is entered.", testInvoiceLine.JI_RelatedExportPermitAuthorityInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitNumber = ZString.Empty;
			testInvoiceLine.JI_RelatedExportPermitDate = ZDateTime.Now;
			Assert("Related Export Authority cannot be blank when a date is entered.", testInvoiceLine.JI_RelatedExportPermitAuthorityInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitAuthority = EXDOCPermitTypeCodes.Codes.AustralianHoneyBoard;
			Assert("Related Export Authority is entered.", !testInvoiceLine.JI_RelatedExportPermitAuthorityInfo.HasMessageErrors());
		}

		public void TestCheckZA_RelatedExportPermitDate_Hidden()
		{
			Assert("Pre-Condition", !testInvoiceLine.JI_RelatedExportPermitDateInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitNumber = "TEST987924";
			testInvoiceLine.JI_RelatedExportPermitDate = ZDateTime.Now;
			testInvoiceLine.JI_RelatedExportPermitDate = ZDateTime.Empty;
			Assert("Related Export Date cannot be empty as a export number has been entered", testInvoiceLine.JI_RelatedExportPermitDateInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitNumber = ZString.Empty;
			testInvoiceLine.JI_RelatedExportPermitAuthority = EXDOCPermitTypeCodes.Codes.AustralianHorticulturalCorporation;
			testInvoiceLine.JI_RelatedExportPermitDate = ZDateTime.Now;
			testInvoiceLine.JI_RelatedExportPermitDate = ZDateTime.Empty;
			Assert("Related Export Date cannot be empty as a export authority has been entered", testInvoiceLine.JI_RelatedExportPermitDateInfo.HasMessageErrors());
			testInvoiceLine.JI_RelatedExportPermitDate = ZDateTime.Now;
			Assert("Related Export Date is not empty", !testInvoiceLine.JI_RelatedExportPermitDateInfo.HasMessageErrors());
		}

		public void TestCheckZA_TemporaryImportNumbers_Hidden()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertNoMessageError(testInvoiceLine.JI_TempImportNumInfo, EXDOCAddInfoJobComInvoicelineValidation.TemporaryImportNumberMaximumLength);
			testInvoiceLine.JI_TempImportNum = "This is greater than 20 characters";
			AssertHasMessageError("Cannot be greater than 20 characters for non dairy & GrainsAndPlants", testInvoiceLine.JI_TempImportNumInfo, EXDOCAddInfoJobComInvoicelineValidation.TemporaryImportNumberMaximumLength);
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			testInvoiceLine.JI_TempImportNum = "This is greater than 20 characters2";
			AssertNoMessageError("Can be greater than 20 for GrainsAndPlants", testInvoiceLine.JI_TempImportNumInfo, EXDOCAddInfoJobComInvoicelineValidation.TemporaryImportNumberMaximumLength);
		}

		public void TestCheckZA_TemporaryImportNumbers_HiddenForDairy()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertNoMessageError(testInvoiceLine.JI_TempImportNumInfo, EXDOCAddInfoJobComInvoicelineValidation.TemporaryImportNumberMaxLengthForDairy);
			testInvoiceLine.JI_TempImportNum = "This can be 25 characters";
			AssertNoMessageError("Dairy can now have a permit number upto 25 characters", testInvoiceLine.JI_TempImportNumInfo, EXDOCAddInfoJobComInvoicelineValidation.TemporaryImportNumberMaxLengthForDairy);
			testInvoiceLine.JI_TempImportNum = "This is greater than 25 characters";
			AssertHasMessageError("Permit No. cannot be greater than 25 characters for Dairy", testInvoiceLine.JI_TempImportNumInfo, EXDOCAddInfoJobComInvoicelineValidation.TemporaryImportNumberMaxLengthForDairy);
		}

		public void TestCheckZA_AQISDominantProduct_Hidden()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			testInvoiceLine.QuarantineExDocLine.QL_DominantProduct = "VEAL";
			AssertHasMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_DominantProductInfo, MandatoryValidation.DoNotEntered + " a Dominant Product");
			testInvoiceLine.QuarantineExDocLine.QL_DominantProduct = ZString.Empty;
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_DominantProductInfo, MandatoryValidation.DoNotEntered + " a Dominant Product");
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			testInvoiceLine.QuarantineExDocLine.QL_DominantProduct = "VEAL";
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_DominantProductInfo, MandatoryValidation.DoNotEntered + " a Dominant Product");
			testInvoiceLine.QuarantineExDocLine.QL_DominantProduct = "VEALXXX";
			AssertHasMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_DominantProductInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZA_AQISAdditionalProducts_Hidden()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("DOMP", "DOMP");

			refHelper.CreateNewOrGetExistingCusCodeList("AU", "DOMP", "BEEF", "BEEF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "DOMP", "PORK", "PORK", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList("AU", "DOMP", "VEAL", "VEAL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			testInvoiceLine.QuarantineExDocLine.QL_DominantProduct = "BEEF";
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			testInvoiceLine.QuarantineExDocLine.QL_AdditionalProducts = "VEAL";
			AssertHasMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, MandatoryValidation.DoNotEntered + " an Additional Products");
			testInvoiceLine.QuarantineExDocLine.QL_AdditionalProducts = ZString.Empty;
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, MandatoryValidation.DoNotEntered + " an Additional Products");
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			testInvoiceLine.QuarantineExDocLine.QL_AdditionalProducts = "VEAL";
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, MandatoryValidation.DoNotEntered + " an Additional Products");
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, EXDOCAddInfoJobComInvoicelineValidation.FirstEnterDominentProduct);
			testInvoiceLine.QuarantineExDocLine.QL_DominantProduct = ZString.Empty;
			AssertHasMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, EXDOCAddInfoJobComInvoicelineValidation.FirstEnterDominentProduct);
			testInvoiceLine.QuarantineExDocLine.QL_DominantProduct = "BEEF";
			testInvoiceLine.QuarantineExDocLine.QL_AdditionalProducts = "VEAL,PORKXX,BEEF";
			AssertHasMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, ListValidation.InvalidCodeMessageError);
			testInvoiceLine.QuarantineExDocLine.QL_AdditionalProducts = "VEAL,PORK,BEEF";
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, ListValidation.InvalidCodeMessageError);

			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, EXDOCAddInfoJobComInvoicelineValidation.AdditionalProductsHasSpaces);
			testInvoiceLine.QuarantineExDocLine.QL_AdditionalProducts = "VEAL,PORK, BEEF";
			AssertHasMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_AdditionalProductsInfo, EXDOCAddInfoJobComInvoicelineValidation.AdditionalProductsHasSpaces);
		}

		public void TestCheckZA_AQISHalal_Hidden()
		{
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			testInvoiceLine.QuarantineExDocLine.QL_HalalProductIndicator = true;
			AssertHasMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_HalalProductIndicatorInfo, "Halal Product Indicator may only be set when produce type is meat.");
			testInvoiceLine.QuarantineExDocLine.QL_HalalProductIndicator = false;
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_HalalProductIndicatorInfo, "Halal Product Indicator may only be set when produce type is meat.");
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			testInvoiceLine.QuarantineExDocLine.QL_HalalProductIndicator = true;
			AssertNoMessageErrorContaining(testInvoiceLine.QuarantineExDocLine.QL_HalalProductIndicatorInfo, "Halal Product Indicator may only be set when produce type is meat.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.PopulateSimpleExportDeclaration();
			JobDeclaration declaration = helper.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			invoiceHeader = declaration.Invoices.AddNew();
			testInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine testInvoiceLine;
	}
}
