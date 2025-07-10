using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool AllowDefaultingOfNatureCore => false;
	}
}
