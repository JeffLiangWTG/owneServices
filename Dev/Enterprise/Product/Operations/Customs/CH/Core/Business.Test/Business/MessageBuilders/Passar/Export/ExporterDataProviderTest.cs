using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class ExporterDataProviderTest : BasePassarDataProviderTest<ExporterDataProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("OrgAddress==null", ExporterDataProvider.New(null));
			AssertNotNull("Argument != null", ExporterDataProvider.New(JobDocAddress));
		});
	}

	public void TestPrivatePerson() => CombineAssertions(() =>
	{
		AssertPrivatePerson(true, OrgConstants.Category.NaturalPersonIndividual);
		AssertPrivatePerson(false, OrgConstants.Category.Business);
		AssertPrivatePerson(false, OrgConstants.Category.NonGovernmentOrganisation);
		AssertPrivatePerson(false, OrgConstants.Category.Government);

		void AssertPrivatePerson(bool expecteddPrivatePerson, string organisationCategory)
		{
			JobDocAddress.Organisation.OH_Category = organisationCategory;
			AssertEquals($"OH_Category={organisationCategory}", expecteddPrivatePerson, DataProvider.PrivatePerson);
		}
	});

	public void TestContactPerson() => CombineAssertions(() =>
	{
		AssertNull("Contact Person is null", DataProvider.ContactPerson);

		var contactName = "Contact Name";
		var contact = JobDocAddress.Organisation.Contacts.AddNew();

		AssertNull("Contact Person is null", DataProvider.ContactPerson);

		JobDocAddress.E2_Contact = "Contact Name";
		ResetDataProvider();
		AssertEquals("Contact not in the list, use E2_Contact Name", "Contact Name", DataProvider.ContactPerson.Name);

		contact.OC_ContactName = contactName;
		JobDocAddress.E2_Contact = contactName;
		ResetDataProvider();
		AssertEquals("Contact in the list", contactName, DataProvider.ContactPerson.Name);
	});

	JobDocAddress CreateDocAddress()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		var address = Factory.New<JobDocAddress>();
		address.E2_OA_Address = orgAddress.PK;
		return address;
	}

	protected override ExporterDataProvider CreateDataProvider() => ExporterDataProvider.New(JobDocAddress);

	JobDocAddress JobDocAddress => docAddress ?? (docAddress = CreateDocAddress());
	JobDocAddress docAddress;
}
