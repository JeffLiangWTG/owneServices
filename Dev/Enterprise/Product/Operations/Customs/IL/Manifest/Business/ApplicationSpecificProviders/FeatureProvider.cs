using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public sealed class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
