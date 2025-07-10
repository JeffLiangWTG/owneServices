using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class AdditionalSupplyChainActorProviderTest : DataProviderTestCase<AdditionalSupplyChainActorProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestRole()
		{
			AssertEquals("FR1", Provider.Role);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("AddSupID", Provider.IdentificationNumber);
		}

		protected override AdditionalSupplyChainActorProvider GetProvider()
		{
			var data = Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "FIS", "AddSupID");
			return new AdditionalSupplyChainActorProvider(data, 1);
		}
	}
}
