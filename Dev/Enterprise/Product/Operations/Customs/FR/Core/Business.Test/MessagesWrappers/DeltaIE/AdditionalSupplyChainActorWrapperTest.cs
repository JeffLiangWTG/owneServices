using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class AdditionalSupplyChainActorWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalSupplyChainActorWrapper>
	{
		protected override AdditionalSupplyChainActorWrapper GetProvider()
		{
			var supplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
			supplyChainActorReference.CFR_Reference = "ref";
			supplyChainActorReference.CFR_Code = "A";
			return AdditionalSupplyChainActorWrapper.New(supplyChainActorReference);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should be equal to CFR_Reference.", "ref", Provider.IdentificationNumber);
		}

		public void TestRole()
		{
			AssertEquals("Role should be equal to CFR_Code.", "A", Provider.Role);
		}
	}
}
