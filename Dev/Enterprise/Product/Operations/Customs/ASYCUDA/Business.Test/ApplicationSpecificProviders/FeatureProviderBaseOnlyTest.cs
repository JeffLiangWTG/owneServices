using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(FeatureProviderForTest))]
	sealed class FeatureProviderBaseOnlyTest : FeatureProviderAbstractTest<FeatureProviderForTest>
	{
		public void TestSupportsCustomsPortsIsDisabledByDefault()
			=> AssertEquals("SupportsCustomsPorts should be Disabled by default", false, new FeatureProviderForTest().SupportsCustomsPorts(header));

		public void TestSupportArrivalInformationIsDisabledByDefault()
			=> AssertEquals("SupportArrivalInformation should be Disabled by default", false, new FeatureProviderForTest().SupportArrivalInformation(header));

		public void TestSupportArrivalTransfersIsDisabledByDefault()
			=> AssertEquals("SupportArrivalTransfers should be Disabled by default", false, new FeatureProviderForTest().SupportArrivalTransfers);

		public void TestSupportsAsycudaPacksIsDisabledByDefault()
			=> AssertEquals("SupportsAsycudaPacks should be Disabled by default", false, new FeatureProviderForTest().SupportsAsycudaPacks);

		public void TestAllowDefaultingOfNatureIsEnabledByDefault()
			=> AssertEquals("AllowDefaultingOfNature should be Enabled by default", true, new FeatureProviderForTest().AllowDefaultingOfNature);

		protected override void SetUp()
		{
			header = Factory.New<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader header;
	}

	sealed class FeatureProviderForTest : FeatureProvider
	{
	}
}
