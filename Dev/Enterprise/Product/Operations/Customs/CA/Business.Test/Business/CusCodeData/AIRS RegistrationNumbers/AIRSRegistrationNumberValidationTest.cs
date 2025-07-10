using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AIRSRegistrationNumberValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var startDate = ZDateTime.UtcToday.AddDays(-1);
			var endDate = ZDateTime.UtcToday.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, dataGrouping.ZZZ_DataGrouping);
			_ = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "2", "2 DESC", startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			var cfia = invoiceLine.CFIAPGAHeader;
			var airsRegNum = cfia.AIRSRegistrationNumbers.AddNew();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(airsRegNum.CY_CodeInfo, "???", "2");
		}

		public void TestCheckCY_DataForCFIA()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			var cfia = invoiceLine.CFIAPGAHeader;
			var airs = cfia.AIRSRegistrationNumbers.AddNew();
			var validation = airs.Validation;

			validation.ValidateCY_Data();
			AssertNoNotifications(airs.CY_DataInfo);

			airs.CY_Code = "A00";
			validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(airs.CY_DataInfo, "You have not entered");
			AssertNoWarningContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeNumericOnly.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldIndicateY.ToString());

			airs.CY_Code = "A01";
			validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(airs.CY_DataInfo, "You have not entered");
			AssertNoWarningContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeNumericOnly.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldIndicateY.ToString());

			airs.CY_Data = "!!";
			validation.ValidateCY_Data();
			AssertHasWarningContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());

			airs.CY_Data = "123-456.s";
			validation.ValidateCY_Data();
			AssertNoWarningContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());

			airs.CY_Data = "A12";
			validation.ValidateCY_Data();
			AssertNoNotifications(airs.CY_DataInfo);

			airs.CY_Code = "A02";
			airs.CY_Data = ZString.Empty;
			validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(airs.CY_DataInfo, "You have not entered");
			AssertNoWarningContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeNumericOnly.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldIndicateY.ToString());

			airs.CY_Data = "A12";
			validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeNumericOnly.ToString());

			airs.CY_Data = "1.2";
			validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeNumericOnly.ToString());

			airs.CY_Data = "112";
			validation.ValidateCY_Data();
			AssertNoNotifications(airs.CY_DataInfo);

			airs.CY_Code = "A03";
			airs.CY_Data = ZString.Empty;
			validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(airs.CY_DataInfo, "You have not entered");
			AssertNoWarningContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldBeNumericOnly.ToString());
			AssertNoMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldIndicateY.ToString());

			airs.CY_Data = "A12";
			validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(airs.CY_DataInfo, RegistrationNumberHelper.ShouldIndicateY.ToString());

			airs.CY_Data = YesNoList.Codes.Yes;
			validation.ValidateCY_Data();
			AssertNoNotifications(airs.CY_DataInfo);
		}

		public void TestCheckCY_Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			var cfia = invoiceLine.CFIAPGAHeader;
			var airsRegNum = cfia.AIRSRegistrationNumbers.AddNew();
			airsRegNum.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(airsRegNum.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			airsRegNum.CY_Code = "001";
			airsRegNum.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(airsRegNum.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			airsRegNum.CY_Data = "1234567";
			airsRegNum.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(airsRegNum.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMPORTER";
			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			var license = orgImpAddInfo.SafeFoodLicenses.AddNew();
			license.CY_Code = "SFC";
			license.CY_Data = "SafeFoodLicense";
			declaration.JE_OH_Importer = importer.PK;

			airsRegNum.CY_Code = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			airsRegNum.CY_Data = "KJ";
			AssertHasWarning(airsRegNum.CY_DataInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());

			airsRegNum.CY_Data = "SFC";
			AssertNoWarning(airsRegNum.CY_DataInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());
		}
	}
}
