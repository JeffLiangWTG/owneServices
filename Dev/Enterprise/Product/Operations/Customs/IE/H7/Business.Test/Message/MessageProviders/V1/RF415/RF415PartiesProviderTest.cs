using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class RF415PartiesProviderTest : DataProviderTestCase<RF415PartiesProvider>
	{
		public void TestApplicant()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE08970987987");
			Header.AMA_OA_Declarant = declarant.MainAddress.PK;

			AssertEquals("IE08970987987", Provider.Applicant);
		}

		public void TestRepresentativeIdentification()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE08970987987");
			Header.AMA_OA_Representative = declarant.MainAddress.PK;

			AssertEquals("IE08970987987", Provider.RepresentativeIdentification);
		}

		public void TestContactPerson()
		{
			AssertNull("No Declarant CUS Contact", Provider.ContactPerson);

			var declarant = Factory.New<OrgHeader>();
			var cusContact = declarant.Contacts.AddNew();
			cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			cusContact.OC_ContactName = "CUS Contact";

			var vatContact = declarant.Contacts.AddNew();
			vatContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;
			vatContact.OC_ContactName = "VAT Contact";

			Header.AMA_OA_Declarant = declarant.MainAddress.PK;
			AssertEquals("Has Declarant CUS Contact", "CUS Contact", Provider.ContactPerson.Name);
		}

		protected override RF415PartiesProvider GetProvider() => new RF415PartiesProvider(Header);

		AsycudaManifestHeader Header => header ?? (header = Factory.New<AsycudaManifestHeader>());
		AsycudaManifestHeader header;
	}
}
