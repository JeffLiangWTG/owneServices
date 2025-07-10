using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsCustomsPorts()
		{
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("MX manifests supports CustomsPorts", true, new FeatureProvider().SupportsCustomsPorts(header));
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("MX manifests supports CustomsPorts", false, new FeatureProvider().SupportsCustomsPorts(header));
		}

		public void TestSupportsAsycudaPacks() => AssertEquals("Should have Pack tab. ", true, new FeatureProvider().SupportsAsycudaPacks);

		protected override void SetUp()
		{
			header = Factory.New<AsycudaManifestHeader>();
			header.FillWithValidTestData();
		}

		AsycudaManifestHeader header;
	}
}
