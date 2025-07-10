using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillCollectionSynchroniser : ASYCUDA.Business.AsycudaBillCollectionSynchroniser
	{
		public AsycudaBillCollectionSynchroniser(ASYCUDA.Business.AsycudaManifestHeader header) : base(header)
		{
		}

		protected override Customs.Business.ManifestBillSynchroniser<ASYCUDA.Business.AsycudaBill> GetNewManifestBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment source)
		{
			return new AsycudaBillSynchroniser((AsycudaBill)destination, source);
		}
	}
}
