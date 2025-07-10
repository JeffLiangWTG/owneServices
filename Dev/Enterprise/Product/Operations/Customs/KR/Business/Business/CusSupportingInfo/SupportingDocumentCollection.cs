using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class SupportingDocumentCollection : CusSupportingInfoCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(ISupportingDocumentParent parent)
			: base((BusinessObject)parent, CusSupportingInfoTypeList.Codes.SupportingDoc)
		{
		}
	}
}
