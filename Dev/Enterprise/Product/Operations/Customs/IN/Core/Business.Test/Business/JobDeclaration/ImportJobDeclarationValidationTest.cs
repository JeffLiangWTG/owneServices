using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ImportJobDeclarationValidation))]
sealed class ImportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
{
	public void TestCheckIECCode()
	{
		var message = "You have not entered an IEC for Selected Importer.";

		var importer = Factory.New<OrgHeader>();
		Declaration.JE_OH_Importer = importer.PK;
		var cusCode = importer.CustomsCodes.AddNew();
		cusCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;

		CombineAssertions(() =>
		{
			Declaration.Validation.ValidateIECCode();
			AssertHasMessageError("No IEC RegNo", Declaration.IECCodeInfo, message);
			cusCode.OK_CustomsRegNo = "1234567890";
			Declaration.Validation.ValidateIECCode();
			AssertNoMessageError("Has IEC RegNo", Declaration.IECCodeInfo, message);
			cusCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.UIN;
			Declaration.Validation.ValidateIECCode();
			AssertHasMessageError("Has RegNo but not IEC", Declaration.IECCodeInfo, message);
		});
	}

	public void TestCheckBranchSerialNumber()
	{
		var expectedMessageError = "You have not entered a BSN – Branch Serial Number for Selected Importer.";

		var importer = Factory.New<OrgHeader>();
		var address = importer.Addresses.AddNew();
		var cusCode = importer.CustomsCodes.AddNew();
		cusCode.OK_OA_PremisesAddress = address.PK;
		var importerDocumentaryAddress = Declaration.ImporterDocumentaryAddress;
		cusCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.BSN;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;

		CombineAssertions(() =>
		{
			importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.Validation.ValidateBranchSerialNumber();
			AssertHasMessageError(Declaration.BranchSerialNumberInfo, expectedMessageError);

			importerDocumentaryAddress.E2_OA_Address = address.PK;
			cusCode.OK_CustomsRegNo = "111";
			Declaration.Validation.ValidateBranchSerialNumber();
			AssertNoMessageError(Declaration.BranchSerialNumberInfo, expectedMessageError);
		});
	}

	protected override string MessageType => JobMessageTypeList.Codes.Import;
}
