using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusEntryInstructionJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestE2_Contact()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var organization = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress address = organization.MainAddress;
			address.Address1 = "testAddress";

			Factory.Save();

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.JustificationContactDetailAddress.OrganisationPK = organization.PK;
			instruction.JustificationContactDetailAddress.E2_AddressType = DocAddressTypes.Codes.JustificationContactDetailAddress;

			instruction.JustificationContactDetailAddress.E2_Contact = "Justify Name Contact Justify Name Contact Justify Name Contact Justify Name Contact Justify Name Contact";
			AssertHasMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "Contact Name should have a maximum length of 100 characters.");

			instruction.JustificationContactDetailAddress.E2_Contact = "Justify Name Contact";
			AssertNoMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "Contact Name should have a maximum length of 100 characters.");

			address.OA_Email = "joaquimjose@emailcompanyprovedor.com.br";
			instruction.JustificationContactDetailAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "Contact E-mail should have a maximum length of 77 characters.");

			address.OA_Email = "joaquimjosedasilvafranscicoxavierpereiramattosleite@emailcompanyprovedor.com.br";
			instruction.JustificationContactDetailAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "Contact E-mail should have a maximum length of 77 characters.");

			address.OA_Email = ZString.Empty;
			instruction.JustificationContactDetailAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "You have entered a Justification Organization. Contact's Email is required");

			address.OA_Phone = "+55 11 2345-6789";
			address.OA_Mobile = ZString.Empty;
			instruction.JustificationContactDetailAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "You have entered a Justification Organization. Contact's Mobile or Contact Phone is required");

			address.OA_Phone = ZString.Empty;
			address.OA_Mobile = "+55 11 2345-6789";
			instruction.JustificationContactDetailAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "You have entered a Justification Organization. Contact's Mobile or Contact Phone is required");

			address.OA_Phone = "+55 11 2345-6789";
			address.OA_Mobile = "+55 11 2345-6789";
			instruction.JustificationContactDetailAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "You have entered a Justification Organization. Contact's Mobile or Contact Phone is required");

			address.OA_Phone = ZString.Empty;
			address.OA_Mobile = ZString.Empty;
			instruction.JustificationContactDetailAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining(instruction.JustificationContactDetailAddress.E2_ContactInfo, "You have entered a Justification Organization. Contact's Mobile or Contact Phone is required");
		}
	}
}
