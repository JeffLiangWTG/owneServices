using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CommonAdditionalSupplyChainActorSeqNumWrapperTest : WrapperHelperTest<CommonAdditionalSupplyChainActorSeqNumWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supplyChainActorRef = Factory.New<CusSupplyChainActorReference>();

			wrapper = new CommonAdditionalSupplyChainActorSeqNumWrapper(supplyChainActorRef, 1);
		}

		CusSupplyChainActorReference supplyChainActorRef;
		CommonAdditionalSupplyChainActorSeqNumWrapper wrapper;

		protected override CommonAdditionalSupplyChainActorSeqNumWrapper GetProvider() => wrapper;
	}
}
