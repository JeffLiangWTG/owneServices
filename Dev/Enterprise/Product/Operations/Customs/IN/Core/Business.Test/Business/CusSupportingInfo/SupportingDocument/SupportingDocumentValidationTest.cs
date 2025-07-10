using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SupportingDocumentValidation))]
sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_ReferenceNumber2()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingInfo.CSI_ReferenceNumber2Info);
	}

	public void TestCSI_Code()
	{
		RefDataSetupTestHelper.SetupSupportDocumentTypeData(Factory);
		ValidationTestHelper.AssertWarningIfNotEntered(SupportingInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		ValidationTestHelper.AssertInvalidCodeMessageError(SupportingInfo.CSI_CodeInfo, new ZString[] { "ACB", "XSD" }, new ZString[] { "DOC001", "DOC002" });
	}

	public void TestCheckCSI_IssuerType()
	{
		var (_, address) = RefDataSetupTestHelper.SetupCusCodes(Factory);
		SupportingInfo.OrganizationAddress.E2_OA_Address = address.PK;
		ValidationTestHelper.AssertInvalidCodeMessageError(SupportingInfo.CSI_IssuerTypeInfo, new ZString[] { "ACB", "XSD" }, new ZString[] { "IEC" });
	}

	SupportingDocument SupportingInfo => supportingInfo ??= Factory.New<SupportingDocument>();
	SupportingDocument supportingInfo;
}
