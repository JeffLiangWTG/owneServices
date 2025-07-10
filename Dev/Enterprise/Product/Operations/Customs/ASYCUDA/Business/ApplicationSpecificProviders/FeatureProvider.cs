using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract class FeatureProvider
	{
		public ZBool SupportsCustomsPorts(AsycudaManifestHeader header) => SupportsCustomsPortsCore(header);
		protected virtual ZBool SupportsCustomsPortsCore(AsycudaManifestHeader header) => false;

		public ZBool SupportArrivalInformation(AsycudaManifestHeader header) => SupportArrivalInformationCore(header);
		protected virtual ZBool SupportArrivalInformationCore(AsycudaManifestHeader header) => false;

		public ZBool SupportArrivalTransfers => SupportArrivalTransfersCore;
		protected virtual ZBool SupportArrivalTransfersCore => false;

		public ZBool SupportsAsycudaPacks => SupportsAsycudaPacksCore;
		protected virtual ZBool SupportsAsycudaPacksCore => false;

		public ZBool AllowDefaultingOfNature => AllowDefaultingOfNatureCore;
		protected virtual ZBool AllowDefaultingOfNatureCore => true;
	}
}
