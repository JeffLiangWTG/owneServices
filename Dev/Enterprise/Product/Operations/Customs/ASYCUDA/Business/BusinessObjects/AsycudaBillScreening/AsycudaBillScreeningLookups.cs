using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillScreeningLookups : ManifestBase.AsycudaBillScreeningLookups
	{
		public AsycudaBillScreeningLookups(AutoAsycudaBillScreening parent)
			: base(parent)
		{
		}

		protected new AsycudaBillScreening Parent => (AsycudaBillScreening)base.Parent;
	}
}
