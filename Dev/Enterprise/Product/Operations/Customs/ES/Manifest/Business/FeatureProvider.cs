using CargoWise.Types;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public sealed class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
