using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaConsolContainerCollectionSynchroniser : ASYCUDA.Business.AsycudaConsolContainerCollectionSynchroniser
	{
		public AsycudaConsolContainerCollectionSynchroniser(ForwardingConsol source, ASYCUDA.Business.AsycudaManifestHeader destination) : base(source, destination)
		{
		}

		protected override ASYCUDA.Business.AsycudaContainerSynchroniser GetNewContainerSynchroniser(ASYCUDA.Business.AsycudaContainer cusContainer, ForwardingContainer sourceContainer)
		{
			return new AsycudaContainerSynchroniser(cusContainer, sourceContainer);
		}
	}
}

