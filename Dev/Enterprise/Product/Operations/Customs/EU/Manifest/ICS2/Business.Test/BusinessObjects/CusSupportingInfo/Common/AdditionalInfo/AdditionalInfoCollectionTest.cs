using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AdditionalInfoCollection))]
	sealed class AdditionalInfoCollectionTest : CusSupportingInfoCollectionTest<AdditionalInfo>
	{
		public void TestMaximumCollectionCount()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var additionalInfoCollection = new AdditionalInfoCollection(header);
			AssertEquals("Maximum count for collection is 99", 99, additionalInfoCollection.MaxCount);
		}

		protected override CusSupportingInfoCollection<AdditionalInfo> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new AdditionalInfoCollection(header);
		}
	}
}
