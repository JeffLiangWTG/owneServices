using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSActorProviderTest : Customs.Business.Testing.DataProviderTestCase<INCTSActor>
	{
		public void TestRole()
		{
			AssertEquals("ABC", Provider.Role);
		}

		public void TestEoriNumber()
		{
			AssertEquals("DE1234567", Provider.EoriNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			actor = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
			actor.CFR_Code = "ABC";
			actor.CFR_Reference = "DE1234567";
		}

		protected override INCTSActor GetProvider() => NCTSActorProvider.NewOrNull(actor);

		CusSupplyChainActorReference actor;
	}
}
