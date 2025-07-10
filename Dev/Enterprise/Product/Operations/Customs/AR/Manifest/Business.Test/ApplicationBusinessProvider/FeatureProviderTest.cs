using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsAsycudaPacks() => AssertEquals("Should have Pack tab. ", true, new FeatureProvider().SupportsAsycudaPacks);

		public void TestSupportsCustomsPorts()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("AR manifests supports CustomsPorts", true, new FeatureProvider().SupportsCustomsPorts(header));
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AR manifests supports CustomsPorts", false, new FeatureProvider().SupportsCustomsPorts(header));
		}
	}
}
