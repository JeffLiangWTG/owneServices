using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentUnloadingLocationWrapperTest : DataProviderTestCase<DeclarationConsignmentUnloadingLocationWrapper>
	{
		public void TestArrivalDateTime()
		{
			AssertNull("ArrivalDateTime", Provider.ArrivalDateTime);
		}

		public void TestId()
		{
			AssertNotNull("ID", Provider.Id);
			AssertEquals("ID should be equal to the expected value", "PORT1", Provider.Id.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull("When asycudaBill is null", DeclarationConsignmentUnloadingLocationWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill is not null", DeclarationConsignmentUnloadingLocationWrapper.NewOrNull(Factory.New<AsycudaBill>()));
		}

		protected override DeclarationConsignmentUnloadingLocationWrapper GetProvider()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			asycudaBill.ABL_RL_NKPortOfDischarge = "PORT1";

			return DeclarationConsignmentUnloadingLocationWrapper.NewOrNull(asycudaBill);
		}
	}
}
