namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSCommunicationProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSCommunicationProvider>
{
	public void TestIdentifier()
	{
		AssertEquals("ShaGou@163.com", provider.Identifier);
	}

	public void TestType()
	{
		AssertEquals("EM", provider.Type);
	}

	protected override PNTSCommunicationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		provider = new PNTSCommunicationProvider("ShaGou@163.com", "EM");
	}
	PNTSCommunicationProvider provider;
}
