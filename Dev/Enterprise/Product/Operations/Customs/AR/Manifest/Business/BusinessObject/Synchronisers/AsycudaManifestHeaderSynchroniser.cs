using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser((AsycudaManifestHeader)Destination);
		}
	}
}
