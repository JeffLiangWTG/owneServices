using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SupportingDocumentJobDocAddressValidation))]
sealed class SupportingDocumentJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOrganisationPK_MandatoryAddressValidation()
	{
		var expectedMessage = "You have not entered a Document Issuing Party/Organization.";
		var supportingDocument = Factory.New<SupportingDocument>();
		var supportingDocAddress = supportingDocument.OrganizationAddress;

		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(supportingDocAddress.OrganisationPKInfo, expectedMessage);
	}
}

