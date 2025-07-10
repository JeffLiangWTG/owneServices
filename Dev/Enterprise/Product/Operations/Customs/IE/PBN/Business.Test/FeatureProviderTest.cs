using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestAllowDefaultingOfNatureIsDisabledByDefault()
		=> AssertEquals("AllowDefaultingOfNature should be Enabled by default", false, new FeatureProvider().AllowDefaultingOfNature);
	}
}
