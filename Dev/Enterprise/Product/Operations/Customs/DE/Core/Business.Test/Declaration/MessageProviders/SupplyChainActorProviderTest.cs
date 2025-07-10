using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class SupplyChainActorProviderTest : Customs.Business.Testing.DataProviderTestCase<SupplyChainActorProvider>
	{
		public void TestRole()
		{
			AssertEquals("ABC", dataProvider.Role);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("REFERENCE", dataProvider.IdentificationNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			supplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
			supplyChainActorReference.CFR_Code = "ABC";
			supplyChainActorReference.CFR_Reference = "REFERENCE";
			dataProvider = SupplyChainActorProvider.NewOrNull(supplyChainActorReference);
		}
		CusSupplyChainActorReference supplyChainActorReference;
		ISupplyChainActor dataProvider;

		protected override SupplyChainActorProvider GetProvider() => (SupplyChainActorProvider)dataProvider;
	}
}
