using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;

		protected override ZBool SupportArrivalInformationCore(ASYCUDA.Business.AsycudaManifestHeader header) => header.IsAir;
	}
}
