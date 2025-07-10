using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalDocumentLookups : BaseAdditionalInfoLookups
	{
		public AdditionalDocumentLookups(AdditionalDocument parent)
			: base(parent)
		{
		}

		protected new AdditionalDocument Parent => (AdditionalDocument)base.Parent;
	}
}
