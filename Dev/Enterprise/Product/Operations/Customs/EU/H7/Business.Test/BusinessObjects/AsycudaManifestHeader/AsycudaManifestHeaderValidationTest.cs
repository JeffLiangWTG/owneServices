using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRepresentative()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError("Representative is mandatory", header.AMA_OA_RepresentativeInfo, "You have not entered a Representative.");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			header.AMA_OA_Representative = orgAddress.PK;
			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError("Contact info is required", header.AMA_OA_RepresentativeInfo, "'CUS - Customs' type contact is required when Representative is provided. It can be configured within the Organization > Contact > Allocated Contact.");

			var contact = orgHeader.Contacts.AddNew();
			var personalAlloc = contact.Allocations.AddNew();
			personalAlloc.PC_Type = OrgConstants.ContactAllocationType.CUS;
			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError("Identification number is required", header.AMA_OA_RepresentativeInfo, "Identification Number is required");
			AssertHasMessageError("Contact phone number is required", header.AMA_OA_RepresentativeInfo, "Phone number is required for 'CUS - Customs' type contact.");
			AssertHasMessageError("Contact email address is required", header.AMA_OA_RepresentativeInfo, "Email address is required for 'CUS - Customs' type contact.");

			contact.OC_IsActive = false;
			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError("Contact info is still required if the only qualified contact is not active", header.AMA_OA_RepresentativeInfo, "'CUS - Customs' type contact is required when Representative is provided. It can be configured within the Organization > Contact > Allocated Contact.");
		}

		public void TestValidateDeclarant()
		{
			var headerWithDeclarantEmpty = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			headerWithDeclarantEmpty.AMA_OA_Declarant = ZGuid.Empty;
			headerWithDeclarantEmpty.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(headerWithDeclarantEmpty.AMA_OA_DeclarantInfo, "You have not entered a Declarant.");

			var headerWithNoIdNumber = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			headerWithNoIdNumber.Declarant.OA_PostCode = ZString.Empty;
			headerWithNoIdNumber.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(headerWithNoIdNumber.AMA_OA_DeclarantInfo, "You have not entered an EORI number for the Declarant.");
			AssertHasMessageError(headerWithNoIdNumber.AMA_OA_DeclarantInfo, "You have not entered an Postcode for the Declarant.");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgCusCode.OK_CustomsRegNo = "RegNum";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var headerWithIdNumber = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			headerWithIdNumber.AMA_OA_Declarant = orgAddress.PK;
			headerWithIdNumber.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(headerWithIdNumber.AMA_OA_DeclarantInfo, "'CUS - Customs' type contact is required when Declarant is provided. It can be configured within the Organization > Contact > Allocated Contact.");

			var contact = orgHeader.Contacts.AddNew();
			var personalAlloc = contact.Allocations.AddNew();
			personalAlloc.PC_Type = OrgConstants.ContactAllocationType.CUS;
			headerWithIdNumber.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError(headerWithIdNumber.AMA_OA_DeclarantInfo, "Phone number is required for 'CUS - Customs' type contact.");
			AssertHasMessageError(headerWithIdNumber.AMA_OA_DeclarantInfo, "Email address is required for 'CUS - Customs' type contact.");

			contact.OC_IsActive = false;
			headerWithIdNumber.Validation.ValidateAMA_OA_Declarant();
			AssertHasMessageError("Contact info is still required if the only qualified contact is not active", headerWithIdNumber.AMA_OA_DeclarantInfo, "'CUS - Customs' type contact is required when Declarant is provided. It can be configured within the Organization > Contact > Allocated Contact.");
		}
	}
}
