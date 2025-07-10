
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSPartyWithNameAndCommunicationsAndAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSPartyWithNameAndCommunicationsAndAddressProvider>
{
	public void TestTypeOfPerson()
	{
		provider = new PNTSPartyWithNameAndCommunicationsAndAddressProvider(address, "WiseTech Global", "NAT", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertEquals(1, provider.TypeOfPerson);

		provider = new PNTSPartyWithNameAndCommunicationsAndAddressProvider(address, "WiseTech Global", "BUS", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertEquals(2, provider.TypeOfPerson);

		provider = new PNTSPartyWithNameAndCommunicationsAndAddressProvider(address, "WiseTech Global", "GOV", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertEquals(2, provider.TypeOfPerson);

		provider = new PNTSPartyWithNameAndCommunicationsAndAddressProvider(address, "WiseTech Global", "NGO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertEquals(3, provider.TypeOfPerson);
	}

	public void TestAddress()
	{
		AssertNotNull(provider.Address);
	}

	public void TestCommunications()
	{
		var contact = address.Header.AllocatedContacts.AddNew();
		contact.OC_ContactName = "Name";
		var allocatedContact = contact.Allocations.AddNew();
		allocatedContact.PC_Type = "CUS";

		contact.OC_Phone = "123456789";
		AssertEquals(1, provider.Communications.Count);

		contact.OC_Email = "123@test.org";
		provider = new PNTSPartyWithNameAndCommunicationsAndAddressProvider(address, "WiseTech Global", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertEquals(2, provider.Communications.Count);

		contact.OC_Phone = ZString.Empty;
		provider = new PNTSPartyWithNameAndCommunicationsAndAddressProvider(address, "WiseTech Global", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertEquals(1, provider.Communications.Count);
	}

	public void TestName()
	{
		AssertEquals("WiseTech Global", provider.Name);
	}

	public void TestIdentificationNumber()
	{
		address.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Belgium);
		AssertEquals("BE12345", provider.IdentificationNumber);
	}

	protected override PNTSPartyWithNameAndCommunicationsAndAddressProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "TestOrg";
		address = orgHeader.Addresses.AddNew();

		provider = new PNTSPartyWithNameAndCommunicationsAndAddressProvider(address, "WiseTech Global", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
	}

	OrgAddress address;
	PNTSPartyWithNameAndCommunicationsAndAddressProvider provider;
}
