using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;

		protected override ZBool SupportsCustomsPortsCore(ASYCUDA.Business.AsycudaManifestHeader header) => header.IsSea;
	}
}
