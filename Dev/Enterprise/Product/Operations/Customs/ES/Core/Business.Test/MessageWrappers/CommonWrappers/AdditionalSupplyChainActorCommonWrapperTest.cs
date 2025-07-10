using System;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AdditionalSupplyChainActorCommonWrapperTest : WrapperHelperTest<AdditionalSupplyChainActorCommonWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if supplyChainActorRef is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","supplyChainActorRef"), () => new AdditionalSupplyChainActorCommonWrapper(null));
		}

		public void TestRole()
		{
			supplyChainActorRef.CFR_Code = "CS";
			AssertEquals("Expected filled Role", "CS", wrapper.Role);
		}

		public void TestId()
		{
			supplyChainActorRef.CFR_Reference = "reference";
			AssertEquals("Expected filled Id", "reference", wrapper.Id);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supplyChainActorRef = Factory.New<CusSupplyChainActorReference>();

			wrapper = new AdditionalSupplyChainActorCommonWrapper(supplyChainActorRef);
		}

		CusSupplyChainActorReference supplyChainActorRef;
		AdditionalSupplyChainActorCommonWrapper wrapper;

		protected override AdditionalSupplyChainActorCommonWrapper GetProvider() => wrapper;
	}
}
