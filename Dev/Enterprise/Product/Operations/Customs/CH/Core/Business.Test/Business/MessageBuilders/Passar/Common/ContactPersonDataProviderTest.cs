using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class ContactPersonDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("JobDocAddress==null", ContactPersonDataProvider.New(null));

			var contact = Factory.New<JobDocAddress>();
			AssertNotNull("JobDocAddress != null", ContactPersonDataProvider.New(contact));
		});
	}

	public void TestOverride() => CombineAssertions(() =>
	{
		JobDocAddress.E2_AddressOverride = ZBool.True;
		JobDocAddress.E2_Contact = "name";
		JobDocAddress.E2_Email = "email";
		JobDocAddress.E2_Phone = "phone";

		AssertEquals("Name", "name", DataProvider.Name);
		AssertEquals("Email", "email", DataProvider.EmailAddress);
		AssertEquals("Phone", "phone", DataProvider.PhoneNumber);
	});

	public void TestNoOverride() => CombineAssertions(() =>
	{
		var orgHeader = JobDocAddress.Organisation;
		orgHeader.MainAddress.OA_Email = "a@example.org";
		orgHeader.MainAddress.OA_Phone = "000";
		var orgContact = orgHeader.Contacts.AddNew();
		orgContact.OC_ContactName = "C1";
		orgContact.OC_Email = "c1@example.org";
		orgContact.OC_Phone = "111";

		JobDocAddress.E2_Contact = "C1";
		AssertEquals("Name", "C1", DataProvider.Name);
		AssertEquals("Email", "c1@example.org", DataProvider.EmailAddress);
		AssertEquals("Phone", "111", DataProvider.PhoneNumber);
	});

	public void TestProviderNullableProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Email", null, DataProvider.EmailAddress);
			AssertEquals("Phone number", null, DataProvider.PhoneNumber);
		});
	}

	JobDocAddress JobDocAddress => jobDocAddress ?? (jobDocAddress = CreateDocAddress());
	JobDocAddress jobDocAddress;
	JobDocAddress CreateDocAddress()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var address = Factory.New<JobDocAddress>();
		address.E2_OA_Address = orgHeader.MainAddress.PK;
		return address;
	}

	ContactPersonDataProvider DataProvider => dataProvider ?? (dataProvider = ContactPersonDataProvider.New(JobDocAddress));
	ContactPersonDataProvider dataProvider;
}
