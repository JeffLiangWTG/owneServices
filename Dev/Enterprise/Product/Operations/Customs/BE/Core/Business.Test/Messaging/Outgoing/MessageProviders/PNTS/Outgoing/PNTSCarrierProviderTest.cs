using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSCarrierProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSCarrierProvider>
{
	public void TestName()
	{
		orgHeader.OH_FullName = "FN";
		AssertEquals("FN", provider.Name);
	}

	public void TestIdentificationNumber()
	{
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Belgium);
		AssertEquals("BE12345", provider.IdentificationNumber);
	}

	protected override PNTSCarrierProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
		orgAddress.OA_OH = orgHeader.PK;
		provider = new PNTSCarrierProvider(orgAddress);
	}
	OrgAddress orgAddress;
	OrgHeader orgHeader;
	PNTSCarrierProvider provider;
}
