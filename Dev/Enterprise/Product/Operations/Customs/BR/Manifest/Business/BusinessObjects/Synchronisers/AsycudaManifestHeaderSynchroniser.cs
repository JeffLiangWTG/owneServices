using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected override void SynchronisersShippingAgent()
		{
			if (((AsycudaManifestHeader)Destination).IsMercante)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_OA_ShippingAgentInfo, Source.JK_OA_SendingForwarderAddressInfo));
			}
			else
			{
				base.SynchronisersShippingAgent();
			}
		}
	}
}
