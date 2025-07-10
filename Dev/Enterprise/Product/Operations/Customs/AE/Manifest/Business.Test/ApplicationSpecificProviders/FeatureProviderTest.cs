using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(FeatureProvider))]
sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
{
	[ExpectNoExceptions]
	public void TestSupportsAsycudaPacks() => NUnit.Framework.Assert.That(new FeatureProvider().SupportsAsycudaPacks, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Should have Pack tab. ");
}
