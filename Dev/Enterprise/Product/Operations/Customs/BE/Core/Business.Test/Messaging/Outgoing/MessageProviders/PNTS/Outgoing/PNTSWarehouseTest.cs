namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSWarehouseTest : Customs.Business.Testing.DataProviderTestCase<PNTSWarehouseProvider>
{
	public void TestType()
	{
		AssertEquals("V", provider.Type);
	}

	public void TestIdentifier()
	{
		authorizationUsage.AGC_Number = "ID";
		AssertEquals("ID", provider.Identifier);
	}

	protected override PNTSWarehouseProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		authorizationUsage = Factory.New<CusAuthorizationUsage>();
		provider = new PNTSWarehouseProvider(authorizationUsage);
	}

	CusAuthorizationUsage authorizationUsage;
	PNTSWarehouseProvider provider;
}
