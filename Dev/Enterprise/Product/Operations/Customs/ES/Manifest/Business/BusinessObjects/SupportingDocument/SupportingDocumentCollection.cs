using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class SupportingDocumentCollection : CusSupportingInfoCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(AsycudaBill parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument)
		{
		}
	}
}
