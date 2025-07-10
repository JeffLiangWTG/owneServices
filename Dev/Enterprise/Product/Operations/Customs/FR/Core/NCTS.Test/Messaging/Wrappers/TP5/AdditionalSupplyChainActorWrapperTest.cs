using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class AdditionalSupplyChainActorWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalSupplyChainActorWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal CFR_Reference.", "REF1", Provider.IdentificationNumber);
		}

		public void TestRole()
		{
			AssertEquals("Role should equal CFR_Code.", "A", Provider.Role);
		}

		protected override AdditionalSupplyChainActorWrapper GetProvider()
		{
			var supplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
			supplyChainActorReference.CFR_Reference = "REF1";
			supplyChainActorReference.CFR_Code = "A";
			return AdditionalSupplyChainActorWrapper.New(supplyChainActorReference);
		}
	}
}
