using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class AdditionalSupplyChainActorWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalSupplyChainActorWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("Wrapper IdentificationNumber should equal CFR_Reference.", "Reference", Provider.IdentificationNumber);
		}

		public void TestRole()
		{
			AssertEquals("Wrapper Role should equal CFR_Code.", "COD", Provider.Role);
		}

		protected override AdditionalSupplyChainActorWrapper GetProvider()
		{
			var cusReference = Factory.New<CusSupplyChainActorReference>();
			cusReference.CFR_Reference = "Reference";
			cusReference.CFR_Code = "COD";
			return AdditionalSupplyChainActorWrapper.New(cusReference);
		}
	}
}
