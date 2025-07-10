using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillScreeningLookups : ASYCUDA.Business.AsycudaBillScreeningLookups
	{
		public AsycudaBillScreeningLookups(AutoAsycudaBillScreening parent)
			: base(parent)
		{
		}

		public new AsycudaBillScreening Parent => (AsycudaBillScreening)base.Parent;

		public CodeDescriptionPairList ScreeningResultList => Factory.GetCachedValue<EUICS2ScreeningResultList>();

		public CodeDescriptionPairList ScreeningAuthorizedPersonTypes => Factory.GetCachedValue<EUICS2ScreeningAuthorizedPersonTypes>();

		public CodeDescriptionPairList TransportDocumentTypeList => Factory.GetCachedValue<ASYCUDA.Business.TransportDocumentTypes>();
	}
}
