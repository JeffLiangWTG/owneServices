using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class ConsignorDataProviderTest : BasePassarDataProviderTest<ConsignorDataProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("JobDocAddress==null", ConsignorDataProvider.New(null));

			var address = Factory.New<JobDocAddress>();
			AssertNull("JobDocAddress is empty", ConsignorDataProvider.New(address, getContactByName: true));
			address.E2_AddressOverride = true;
			AssertNotNull("E2_AddressOverride = true", ConsignorDataProvider.New(address, getContactByName: true));
			address.E2_OA_Address = Factory.New<OrgAddress>().PK;
			AssertNotNull("E2_AddressOverride = true", ConsignorDataProvider.New(address, getContactByName: true));
		});
	}

	public void TestPrivatePerson() => CombineAssertions(() =>
	{
		Consignor.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		AssertEquals("PrivatePerson == NAT", true, DataProvider.PrivatePerson);

		ResetDataProvider();
		Consignor.Organisation.OH_Category = ZString.Empty;
		AssertEquals("PrivatePerson != NAT", false, DataProvider.PrivatePerson);
	});

	public void TestContactPerson() => CombineAssertions(() =>
	{
		AssertNull("Contact Person is null", DataProvider.ContactPerson);

		var contactName = "Contact Name";
		var contact = Consignor.Organisation.Contacts.AddNew();

		AssertNull("Contact Person is null", DataProvider.ContactPerson);

		Consignor.E2_Contact = "XXX";
		ResetDataProvider();
		AssertEquals("Contact not in the list, use E2_Contact Name", "XXX", DataProvider.ContactPerson.Name);

		contact.OC_ContactName = contactName;
		Consignor.E2_Contact = contactName;
		ResetDataProvider();
		AssertEquals("Contact in the list", contactName, DataProvider.ContactPerson.Name);
	});

	public void TestReferenceNumber() => AssertNull("ReferenceNumber not available", DataProvider.ReferenceNumber);

	JobDocAddress Consignor => docAddress ?? (docAddress = CreateDocAddress());
	JobDocAddress docAddress;

	JobDocAddress CreateDocAddress()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		var address = Factory.New<JobDocAddress>();
		address.E2_OA_Address = orgAddress.PK;
		return address;
	}

	protected override ConsignorDataProvider CreateDataProvider() => ConsignorDataProvider.New(Consignor, getContactByName: true);
}
