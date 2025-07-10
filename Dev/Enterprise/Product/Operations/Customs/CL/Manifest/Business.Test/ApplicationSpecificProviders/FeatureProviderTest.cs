using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class FeatureProviderTest : FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportsAsycudaPacks()
			=> AssertEquals("Should have Pack tab. ", true, new FeatureProvider().SupportsAsycudaPacks);

		public void TestSupportArrivalInformation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var featureProvider = new FeatureProvider();

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("CL Manifest should not have Arrival Information enabled for Sea Mode. ", false, featureProvider.SupportArrivalInformation(header));

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("CL Manifest should have Arrival Information enabled for Air Mode. ", true, featureProvider.SupportArrivalInformation(header));
		}
	}
}
