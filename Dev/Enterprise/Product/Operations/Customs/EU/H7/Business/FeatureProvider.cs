using CargoWise.Types;

namespace Enterprise.Customs.EU.H7.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
