using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7AddressWrapper))]
public sealed class H7AddressWrapperTest : DataProviderTestCase<H7AddressWrapper>
{
	public void TestName()
	{
		AssertEquals(nameof(Provider.Name), "WiseTech", Provider.Name);
	}

	public void TestStreetAndNumber()
	{
		AssertEquals(nameof(Provider.StreetAndNumber), "25 Bourke Road Alexandria", Provider.StreetAndNumber);
	}

	public void TestCountry()
	{
		AssertEquals(nameof(Provider.Country), "AU", Provider.Country);
	}

	public void TestZipCode()
	{
		AssertEquals(nameof(Provider.ZipCode), "2015", Provider.ZipCode);
	}

	public void TestCity()
	{
		AssertEquals(nameof(Provider.City), "Sydney", Provider.City);
	}

	public void TestContact()
	{
		AssertNotNull(nameof(Provider.Contact), Provider.Contact);
		CombineAssertions(nameof(Provider.Contact), () =>
		{
			AssertEquals(nameof(Provider.Contact.Name), "Hugh Mungus", Provider.Contact.Name);
			AssertEquals(nameof(Provider.Contact.PhoneNumber), "123456789", Provider.Contact.PhoneNumber);
			AssertEquals(nameof(Provider.Contact.EmailAddress), "HughMungus@test.org", Provider.Contact.EmailAddress);
		});
	}

	protected override H7AddressWrapper GetProvider()
	{
		var orgHeader = Factory.New<OrgHeader>();

		var address = orgHeader.Addresses.AddNew();
		address.CompanyName = "WiseTech";
		address.OA_Address1 = "25 Bourke Road";
		address.OA_Address2 = "Alexandria";
		address.OA_RN_NKCountryCode = "AU";
		address.OA_PostCode = "2015";
		address.OA_City = "Sydney";

		var contact = orgHeader.Contacts.AddNew();
		contact.OC_ContactName = "Hugh Mungus";
		contact.OC_Mobile = "123456789";
		contact.OC_Email = "HughMungus@test.org";
		contact.OC_IsActive = true;

		var contactAllocation = contact.Allocations.AddNew();
		contactAllocation.PC_Type = OrgConstants.ContactAllocationType.CUS;

		return new H7AddressWrapper(address);
	}
}
