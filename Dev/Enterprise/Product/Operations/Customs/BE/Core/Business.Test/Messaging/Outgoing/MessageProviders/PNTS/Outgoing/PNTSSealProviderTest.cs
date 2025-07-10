using CargoWise.Types;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSSealProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSSealProvider>
{
	public void TestIdentifier()
	{
		AssertEquals("Seal", provider.Identifier);
	}

	protected override PNTSSealProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		seal = "Seal";
		provider = new PNTSSealProvider(seal);
	}

	ZString seal;
	PNTSSealProvider provider;
}
