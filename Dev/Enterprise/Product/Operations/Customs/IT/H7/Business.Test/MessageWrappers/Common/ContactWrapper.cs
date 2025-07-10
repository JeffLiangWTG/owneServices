using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(ContactWrapper))]
public sealed class ContactWrapperTest : DataProviderTestCase<ContactWrapper>
{
	public void TestName()
	{
		AssertEquals(nameof(Provider.Name), "Hugh Mungus", Provider.Name);
	}

	public void TestPhoneNumber()
	{
		AssertEquals(nameof(Provider.PhoneNumber), "123456789", Provider.PhoneNumber);
	}

	public void TestEmailAddress()
	{
		AssertEquals(nameof(Provider.EmailAddress), "HughMungus@test.org", Provider.EmailAddress);
	}

	protected override ContactWrapper GetProvider()
	{
		var orgHeader = Factory.New<OrgHeader>();

		var contact = orgHeader.AllocatedContacts.AddNew();
		contact.OC_ContactName = "Hugh Mungus";
		contact.OC_Mobile = "123456789";
		contact.OC_Email = "HughMungus@test.org";

		return new ContactWrapper(contact);
	}
}
