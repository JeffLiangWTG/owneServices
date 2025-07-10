using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business 
{
	public class AsycudaPackCollectionSynchroniser : ASYCUDA.Business.AsycudaPackCollectionSynchroniser
	{
		public AsycudaPackCollectionSynchroniser(ForwardingShipment source, AsycudaBill destination) : base(source, destination)
		{
		}

		protected override ASYCUDA.Business.AsycudaPackSynchroniser GetAsycudaPackSynchroniser(ASYCUDA.Business.AsycudaPack asycudaPack, PackLine sourcePack) => new AsycudaPackSynchroniser((AsycudaPack)asycudaPack, sourcePack);	
	}
}
