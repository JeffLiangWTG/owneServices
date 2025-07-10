using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ExportRestrictionAdditionalInformationValidation))]
sealed class ExportRestrictionAdditionalInformationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code()
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeRestrictionCodes: true);
		Factory.Save();

		Restriction.CSI_Code = RefCusCodeTestHelper.AdditionalInformationRestrictionCode;

		RestrictionAdditionalInformation.CY_Code = ZString.Empty;
		AssertHasMessageErrorContaining("CY_Code empty", RestrictionAdditionalInformation.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		AssertNoMessageErrorContaining("CY_Code empty", RestrictionAdditionalInformation.CY_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());

		RestrictionAdditionalInformation.CY_Code = "Z~Z";
		AssertNoMessageErrorContaining("CY_Code invalid code", RestrictionAdditionalInformation.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		AssertHasMessageErrorContaining("CY_Code invalid code", RestrictionAdditionalInformation.CY_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;
		AssertNoMessageErrorContaining("CY_Code valid code", RestrictionAdditionalInformation.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		AssertNoMessageErrorContaining("CY_Code valid code", RestrictionAdditionalInformation.CY_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckCY_Data_NS30110_Mandatory()
	{
		const string messageError = $"[NS30110] {MandatoryValidation.YouHaveNotEntered}";

		AssertNoMessageErrorContaining("CY_Code and CY_Data empty", RestrictionAdditionalInformation.CY_DataInfo, messageError);

		RestrictionAdditionalInformation.CY_Code = "AB";
		RestrictionAdditionalInformation.Validation.ValidateCY_Data();
		AssertHasMessageErrorContaining("CY_Code not empty and CY_Data empty", RestrictionAdditionalInformation.CY_DataInfo, messageError);

		RestrictionAdditionalInformation.CY_Code = ZString.Empty;
		RestrictionAdditionalInformation.CY_Data = "BA";
		AssertNoMessageErrorContaining("CY_Code empty and CY_Data not empty", RestrictionAdditionalInformation.CY_DataInfo, messageError);

		RestrictionAdditionalInformation.CY_Code = "AB";
		AssertNoMessageErrorContaining("CY_Code and CY_Data not empty", RestrictionAdditionalInformation.CY_DataInfo, messageError);
	}

	public void TestCheckCY_Data_NS30110_NotApplicable()
	{
		AssertNoMessageError("CY_Code and CY_Data empty", RestrictionAdditionalInformation.CY_DataInfo, PassarValidationMessages.MessageNS30110);

		RestrictionAdditionalInformation.CY_Code = "AB";
		RestrictionAdditionalInformation.Validation.ValidateCY_Data();
		AssertNoMessageError("CY_Code not empty and CY_Data empty", RestrictionAdditionalInformation.CY_DataInfo, PassarValidationMessages.MessageNS30110);

		RestrictionAdditionalInformation.CY_Code = ZString.Empty;
		RestrictionAdditionalInformation.CY_Data = "BA";
		AssertHasMessageError("CY_Code empty and CY_Data not empty", RestrictionAdditionalInformation.CY_DataInfo, PassarValidationMessages.MessageNS30110);

		RestrictionAdditionalInformation.CY_Code = "AB";
		RestrictionAdditionalInformation.Validation.ValidateCY_Data();
		AssertNoMessageError("CY_Code and CY_Data not empty", RestrictionAdditionalInformation.CY_DataInfo, PassarValidationMessages.MessageNS30110);
	}

	public void TestCheckCY_Data_NP70229() => CombineAssertions(() =>
	{
		const string messageError = "[NP70229] The code you have selected is not in the list.";

		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeLinkedCodeTypes: true);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;
		RestrictionAdditionalInformation.CY_Data = RefCusCodeTestHelper.AdditionalInformationLinkedCode_N5004_1;
		AssertNoMessageError("Valid code", RestrictionAdditionalInformation.CY_DataInfo, messageError);
		RestrictionAdditionalInformation.CY_Data = RefCusCodeTestHelper.AdditionalInformationLinkedCodeType_Invalid;
		AssertHasMessageError("Invalid code", RestrictionAdditionalInformation.CY_DataInfo, messageError);
		RestrictionAdditionalInformation.CY_Data = ZString.Empty;
		AssertNoMessageError("Empty code", RestrictionAdditionalInformation.CY_DataInfo, messageError);

		RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_B1001;
		RestrictionAdditionalInformation.CY_Data = RefCusCodeTestHelper.AdditionalInformationLinkedCodeType_Invalid;
		AssertNoMessageError("Free text allowed", RestrictionAdditionalInformation.CY_DataInfo, messageError);
	});

	public void TestCheckCY_Code_NP70199()
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeRestrictionCodes: true);

		Restriction.CSI_Code = RefCusCodeTestHelper.AdditionalInformationRestrictionCode;
		string expectedMessage_NoPermitNumber = PassarValidationMessages.MessageNP70199_NoPermitNumber(RestrictionAdditionalInformation.Lookups.CY_CodeList.CodesAsString, Restriction.CSI_Code);
		var restrictionAdditionalInformation2 = Restriction.AdditionalInformations.AddNew();
		restrictionAdditionalInformation2.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_B1001;
		CombineAssertions(() =>
		{
			RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;
			AssertNoRowMessageErrorContaining(RestrictionAdditionalInformation, expectedMessage_NoPermitNumber);
			RestrictionAdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithoutRestrictionCode_B1004;
			AssertHasRowMessageErrorContaining(RestrictionAdditionalInformation, expectedMessage_NoPermitNumber);

			Restriction.CSI_ReferenceNumber = "123";
			string expectedMessage_PermitNumber = PassarValidationMessages.MessageNP70199_PermitNumber(RestrictionAdditionalInformation.Lookups.CY_CodeList.CodesAsString, Restriction.CSI_Code);
			RestrictionAdditionalInformation.Validation.ValidateCY_Code();
			AssertHasRowMessageErrorContaining(RestrictionAdditionalInformation, expectedMessage_PermitNumber);
		});
	}

	Restriction Restriction => restriction ??= CreateRestriction();
	Restriction restriction;

	RestrictionAdditionalInformation RestrictionAdditionalInformation => restrictionAdditionalInformation ??= Restriction.AdditionalInformations.AddNew();
	RestrictionAdditionalInformation restrictionAdditionalInformation;

	Restriction CreateRestriction()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		return invoiceLine.Restrictions.AddNew();
	}
}
