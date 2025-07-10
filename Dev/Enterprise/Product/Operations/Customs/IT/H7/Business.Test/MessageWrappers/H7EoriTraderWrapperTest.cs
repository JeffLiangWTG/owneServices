using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7EoriTraderWrapper))]
public sealed class H7EoriTraderWrapperTest : DataProviderTestCase<H7EoriTraderWrapper>
{
	public void TestEoriNumber()
	{
		AssertNull(nameof(Provider.EoriNumber), Provider.EoriNumber);
	}

	public void TestIdentificationNumber()
	{
		AssertEquals(nameof(Provider.IdentificationNumber), "EORICustomsRegNo3", Provider.IdentificationNumber);
	}

	public void TestIdentificationNumber_NoExceptionWhenOrgHasNoEORI()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var address = orgHeader.Addresses.AddNew();

		var provider = new H7EoriTraderWrapper(address);
		AssertNoExceptionThrown("No exception when org has no EORI", () => AssertNullOrEmpty(provider.IdentificationNumber));
	}

	public void TestAddress()
	{
		AssertNotNull(Provider.Address);
		CombineAssertions(nameof(Provider.Address), () =>
		{
			AssertEquals(nameof(Provider.Address.Name), "WiseTech", Provider.Address.Name);
			AssertEquals(nameof(Provider.Address.StreetAndNumber), "25 Bourke Road Alexandria", Provider.Address.StreetAndNumber);
			AssertEquals(nameof(Provider.Address.Country), "AU", Provider.Address.Country);
			AssertEquals(nameof(Provider.Address.ZipCode), "2015", Provider.Address.ZipCode);
			AssertEquals(nameof(Provider.Address.City), "Australia", Provider.Address.City);
			AssertNotNull(nameof(Provider.Address.Contact), Provider.Address.Contact);
		});
	}

	protected override H7EoriTraderWrapper GetProvider()
	{
		var orgHeader = Factory.New<OrgHeader>();

		var address = orgHeader.Addresses.AddNew();
		address.CompanyName = "WiseTech";
		address.OA_Address1 = "25 Bourke Road";
		address.OA_Address2 = "Alexandria";
		address.OA_RN_NKCountryCode = "AU";
		address.OA_PostCode = "2015";
		address.OA_City = "Australia";

		var orgCusCode = address.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		orgCusCode.OK_CustomsRegNo = "EORICustomsRegNo3";

		var contact = orgHeader.Contacts.AddNew();
		contact.OC_ContactName = "Hugh Mungus";
		contact.OC_Mobile = "123456789";
		contact.OC_Email = "HughMungus@test.org";
		contact.OC_IsActive = true;

		var contactAllocation = contact.Allocations.AddNew();
		contactAllocation.PC_Type = OrgConstants.ContactAllocationType.CUS;

		return new H7EoriTraderWrapper(address);
	}
}
