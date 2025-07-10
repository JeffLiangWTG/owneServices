using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupportingDocumentCollection : CusSupportingInfoCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument)
		{
		}
	}
}
