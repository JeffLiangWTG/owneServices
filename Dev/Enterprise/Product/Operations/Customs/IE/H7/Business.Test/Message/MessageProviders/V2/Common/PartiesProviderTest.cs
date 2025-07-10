using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class PartiesProviderTest : DataProviderTestCase<PartiesProvider>
	{
		public void TestApplicant()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE08970987987");
			header.AMA_OA_Declarant = declarant.MainAddress.PK;

			AssertEquals("Applicant", "IE08970987987", Provider.Applicant);
		}

		public void TestRepresentativeIdentification()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE08970987987");
			header.AMA_OA_Representative = declarant.MainAddress.PK;

			AssertEquals("RepresentativeIdentification", "IE08970987987", Provider.RepresentativeIdentification);
		}

		public void TestContactPerson()
		{
			AssertNull("No Declarant CUS Contact", Provider.ContactPerson);

			var declarant = Factory.New<OrgHeader>();
			var cusContact = declarant.Contacts.AddNew();
			cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			cusContact.OC_ContactName = "CUS Contact";

			header.AMA_OA_Declarant = declarant.MainAddress.PK;
			AssertEquals("Has Declarant CUS Contact", "CUS Contact", Provider.ContactPerson.Name);
		}

		protected sealed override PartiesProvider GetProvider()
		{
			return new PartiesProvider(header);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader header;
	}
}

