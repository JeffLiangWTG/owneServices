using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollectionSynchroniser))]
	sealed class AsycudaBillCollectionSynchroniserTest : ManifestBillCollectionSynchroniserTest
	{
		protected override BusinessObjectCollectionSynchroniser GetManifestBillCollectionSynchroniser(IManifestHeaderForSynchroniser header) => new AsycudaBillCollectionSynchroniser((AsycudaManifestHeader)header);

		protected override IManifestHeaderForSynchroniser GetManifestBillHeader(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			return header;
		}
	}
}
