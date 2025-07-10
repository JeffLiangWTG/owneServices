using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
