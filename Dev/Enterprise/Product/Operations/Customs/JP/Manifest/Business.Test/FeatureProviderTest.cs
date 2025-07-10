using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsAsycudaPacks()
		{
			AssertEquals("SupportsAsycudaPacks should be true", true, new FeatureProvider().SupportsAsycudaPacks);
		}
	}
}
