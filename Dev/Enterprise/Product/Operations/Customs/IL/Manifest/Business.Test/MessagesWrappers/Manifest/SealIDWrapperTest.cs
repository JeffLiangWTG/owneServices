using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class SealIDWrapperTest : DataProviderTestCase<SealIDWrapper>
	{
		public void TestValue()
		{
			AssertEquals("Value must have expected value", "Seal1", Provider.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull(SealIDWrapper.NewOrNull(null));
			AssertNotNull(SealIDWrapper.NewOrNull("Seal"));
		}

		protected override SealIDWrapper GetProvider()
		{
			return SealIDWrapper.NewOrNull("Seal1");
		}
	}
}
