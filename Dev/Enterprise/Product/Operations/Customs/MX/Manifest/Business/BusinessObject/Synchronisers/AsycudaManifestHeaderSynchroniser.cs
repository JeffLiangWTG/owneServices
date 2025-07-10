using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected new AsycudaManifestHeader Destination => (AsycudaManifestHeader)base.Destination;

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser(Destination);
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.LastForeignPortInfo, Source.JK_RL_NKLastForeignPortInfo));
		}
	}
}
