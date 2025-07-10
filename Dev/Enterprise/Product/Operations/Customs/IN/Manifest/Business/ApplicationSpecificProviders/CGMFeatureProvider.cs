using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMFeatureProvider : ASYCUDA.Business.FeatureProvider
{
	protected override ZBool SupportsAsycudaPacksCore => true;
}
