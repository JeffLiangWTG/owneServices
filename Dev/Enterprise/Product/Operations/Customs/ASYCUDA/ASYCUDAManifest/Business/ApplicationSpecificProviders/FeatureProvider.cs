using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
