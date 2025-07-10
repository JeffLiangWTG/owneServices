using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillScreeningValidation : ManifestBase.AsycudaBillScreeningValidation
	{
		public AsycudaBillScreeningValidation(AutoAsycudaBillScreening parent)
			: base(parent)
		{
		}

		protected new AsycudaBillScreening Parent => (AsycudaBillScreening)base.Parent;
	}
}
