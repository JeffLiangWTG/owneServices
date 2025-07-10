using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class AsycudaBillCollectionSynchroniser(ASYCUDA.Business.AsycudaManifestHeader header) : ASYCUDA.Business.AsycudaBillCollectionSynchroniser(header)
{
	protected override ManifestBillSynchroniser<ASYCUDA.Business.AsycudaBill> GetNewManifestBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment source) => new AsycudaBillSynchroniser(destination, source);
}
