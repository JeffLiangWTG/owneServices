using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class AuthorizationProviderTest : Customs.Business.Testing.DataProviderTestCase<AuthorizationProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals("1", Provider.SequenceNumber);
	}

	public void TestType()
	{
		cusAuthorizationUsage.AGC_Code = "ABC";
		AssertEquals("ABC", Provider.Type);
	}

	public void TestReferenceNumber()
	{
		cusAuthorizationUsage.AGC_Number = "123456";
		AssertEquals("123456", Provider.ReferenceNumber);
	}

	public void TestHolderOfAuthorisation()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_FullName = "AUTHCOMP";
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		MessageProviderDataHelper.SetupEORI(orgHeader, "ASD123");

		AssertEquals("ASD123", Provider.HolderOfAuthorisation);
	}

	protected override AuthorizationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		cusAuthorizationUsage = Factory.NewWithValidTestData<CusAuthorizationUsage>();
		provider = new AuthorizationProvider(cusAuthorizationUsage, 1);
	}

	CusAuthorizationUsage cusAuthorizationUsage;

	AuthorizationProvider provider;
}
