using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.ManifestBase.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RequestHeaderCollection))]
	sealed class RequestHeaderCollectionTest : EUMemberStateCommunicationCollectionTest<RequestHeader>
	{
		public void TestMaximumCollectionCount()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var collection = new RequestHeaderCollection(header);
			AssertEquals("Maximum count for collection is 99", 99, collection.MaxCount);
		}

		protected override EUMemberStateCommunicationCollection<RequestHeader> GetEUMemberStateCommunicationCollection() => new RequestHeaderCollection(Factory.New<AsycudaManifestHeader>());
	}
}
