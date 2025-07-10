using CargoWise.Types;

namespace Enterprise.Customs.GB.GVMS
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool AllowDefaultingOfNatureCore => false;
	}
}
