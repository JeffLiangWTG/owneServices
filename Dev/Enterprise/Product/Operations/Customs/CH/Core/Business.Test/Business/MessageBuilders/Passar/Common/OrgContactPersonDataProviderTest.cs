using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class OrgContactPersonDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("OrgContact==null", OrgContactPersonDataProvider.New(null));

			var contact = Factory.New<OrgContact>();
			AssertNotNull("OrgContact != null", OrgContactPersonDataProvider.New(contact));
		});
	}

	public void TestProvider()
	{
		Contact.OC_Email = "email";
		Contact.OC_ContactName = "name";
		Contact.OC_Phone = "phone";

		CombineAssertions(() =>
		{
			AssertEquals("Email", "email", DataProvider.EmailAddress);
			AssertEquals("Name", "name", DataProvider.Name);
			AssertEquals("Phone", "phone", DataProvider.PhoneNumber);
		});
	}

	public void TestProviderNullableProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Email", null, DataProvider.EmailAddress);
			AssertEquals("Phone number", null, DataProvider.PhoneNumber);
		});
	}

	OrgContact Contact => contact ?? (contact = Factory.New<OrgContact>());
	OrgContact contact;

	OrgContactPersonDataProvider DataProvider => dataProvider ?? (dataProvider = OrgContactPersonDataProvider.New(Contact));
	OrgContactPersonDataProvider dataProvider;
}
