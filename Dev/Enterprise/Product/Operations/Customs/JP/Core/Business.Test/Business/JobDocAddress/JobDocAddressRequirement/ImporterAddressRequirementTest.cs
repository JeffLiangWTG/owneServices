using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ImporterAddressRequirement))]
	sealed class ImporterAddressRequirementTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOrganisationPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;

			var targetInfo = importerDocumentaryAddress.OrganisationPKInfo;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "AIR";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_MessageType = "EXP";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckE2_CompanyName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;

			var targetInfo = importerDocumentaryAddress.E2_CompanyNameInfo;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "AIR";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_MessageType = "EXP";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}
	}
}
