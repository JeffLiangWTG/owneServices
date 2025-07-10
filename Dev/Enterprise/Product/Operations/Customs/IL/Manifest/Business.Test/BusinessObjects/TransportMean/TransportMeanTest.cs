using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(TransportMean))]
	sealed class TransportMeanTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var transport = Factory.NewWithValidTestData<TransportMean>();
			AssertType<TransportMeanLookups>(transport.Lookups);
		}
		
		public void TestValidation()
		{
			var transport = GetNewBusinessObject() as TransportMean;
			AssertNotNull(transport.Validation);
			AssertType<TransportMeanValidation>(transport.Validation);
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(typeof(TransportMeanRoadValidation), transport.Validation.GetType());
		}
		
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			asycudaManifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			return asycudaManifestHeader.TransportMeans.AddNew();
		}

		AsycudaManifestHeader asycudaManifestHeader;
	}
}
