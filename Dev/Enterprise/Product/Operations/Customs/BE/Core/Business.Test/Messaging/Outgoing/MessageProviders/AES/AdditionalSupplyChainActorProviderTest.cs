using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class AdditionalSupplyChainActorProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalSupplyChainActorProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals(1, Provider.SequenceNumber);
	}

	public void TestRole()
	{
		reference.CFR_Code = "A";
		AssertEquals("A", Provider.Role);
	}

	public void TestIdentificationNumber()
	{
		reference.CFR_Reference = "Reference";
		AssertEquals("Reference", Provider.IdentificationNumber);
	}

	protected override AdditionalSupplyChainActorProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		reference = Factory.New<CusReference>();
		provider = new AdditionalSupplyChainActorProvider(reference, 1);
	}

	CusReference reference;

	AdditionalSupplyChainActorProvider provider;
}
