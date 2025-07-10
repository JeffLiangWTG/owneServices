using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMFeatureProvider))]
sealed class CGMFeatureProviderTest : FeatureProviderAbstractTest<CGMFeatureProvider>
{
	public void TestSupportsAsycudaPacks() => AssertEquals("Should have Pack tab. ", true, new CGMFeatureProvider().SupportsAsycudaPacks);
}
