using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentLoadingLocationWrapperTest : DataProviderTestCase<DeclarationConsignmentLoadingLocationWrapper>
	{
		public void TestNewOrNull()
		{
			var factory = Factory;
			AssertNull("When asycudaBill is null", DeclarationConsignmentLoadingLocationWrapper.NewOrNull(null));
			AssertNotNull("When asycudaBill with Header,", DeclarationConsignmentLoadingLocationWrapper.NewOrNull(factory.New<AsycudaManifestHeader>().Bills.AddNew()));
		}

		public void TestId()
		{
			AssertNotNull("ID", Provider.Id);
			AssertEquals("ID should be equal to the expected value", "XYZ", Provider.Id.Value);
		}

		protected override DeclarationConsignmentLoadingLocationWrapper GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var asycudaBill = header.MasterBill;
			asycudaBill.ABL_RL_NKPortOfLoading = "XYZ";

			return DeclarationConsignmentLoadingLocationWrapper.NewOrNull((AsycudaBill)asycudaBill);
		}
	}
}
