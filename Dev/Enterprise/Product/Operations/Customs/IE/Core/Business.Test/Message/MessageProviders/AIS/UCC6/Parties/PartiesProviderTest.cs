using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class PartiesProviderTest : DataProviderTestCase<PartiesProvider>
	{
		public void TestApplicant()
		{
			AssertEquals("No Declarant EORI", ZString.Empty, Provider.Applicant);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "293847584930295");
			Declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			AssertEquals("Has Declarant EORI", "IE293847584930295", Provider.Applicant);
		}

		public void TestRepresentativeIdentification()
		{
			AssertEquals("No Representative EORI", ZString.Empty, Provider.RepresentativeIdentification);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "293847584930295");
			Declaration.JE_OA_Representative = orgAddress.PK;
			AssertEquals("Has Representative EORI", "IE293847584930295", Provider.RepresentativeIdentification);
		}

		public void TestContactPerson()
		{
			AssertNull("No Declarant CUS Contact", Provider.ContactPerson);

			var orgHeader = Factory.New<OrgHeader>();
			var cusContact = orgHeader.Contacts.AddNew();
			cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			cusContact.OC_ContactName = "CUS Contact";

			var vatContact = orgHeader.Contacts.AddNew();
			vatContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;
			vatContact.OC_ContactName = "VAT Contact";

			Declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			AssertEquals("Has Declarant CUS Contact", "CUS Contact", Provider.ContactPerson.Name);
		}

		protected override PartiesProvider GetProvider()
		{
			return new PartiesProvider(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}
