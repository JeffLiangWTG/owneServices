using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSAdditionalSupplyChainActorProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSAdditionalSupplyChainActorProvider>
{
	public void TestRole()
	{
		supplyChainActor.CFR_Code = "Cod";
		AssertEquals("Cod", provider.Role);
	}

	public void TestIdentificationNumber()
	{
		supplyChainActor.CFR_Reference = "Reference";
		AssertEquals("Reference", provider.IdentificationNumber);
	}

	protected override PNTSAdditionalSupplyChainActorProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		supplyChainActor = Factory.New<CusSupplyChainActorReference>();
		provider = new PNTSAdditionalSupplyChainActorProvider(supplyChainActor);
	}

	CusSupplyChainActorReference supplyChainActor;
	PNTSAdditionalSupplyChainActorProvider provider;
}
