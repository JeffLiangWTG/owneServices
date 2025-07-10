using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSDeclarantProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSDeclarantProvider>
{
	public void TestIdentificationNumber()
	{
		AssertEquals("BE12345", provider.IdentificationNumber);
	}

	public void TestName()
	{
		AssertEquals("SBCompany", provider.Name);
	}

	public void TestCommunications()
	{
		var cusContact = orgHeader.Contacts.AddNew();
		cusContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		cusContact.OC_Email = "ShaGou@163.com";
		cusContact.OC_Phone = "1234567890";
		CombineAssertions(() =>
		{
			AssertEquals(2, provider.Communications.Count);
			AssertEquals("ShaGou@163.com", provider.Communications.First().Identifier);
			AssertEquals("EM", provider.Communications.First().Type);
		});
	}

	protected override PNTSDeclarantProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Belgium);
		orgHeader.OH_FullName = "SBCompany";
		orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		provider = new PNTSDeclarantProvider(orgAddress);
	}
	OrgAddress orgAddress;
	OrgHeader orgHeader;
	PNTSDeclarantProvider provider;
}
