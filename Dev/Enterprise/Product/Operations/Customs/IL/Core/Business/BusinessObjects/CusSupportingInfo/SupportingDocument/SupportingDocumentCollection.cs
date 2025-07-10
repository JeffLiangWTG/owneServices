using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocumentCollection : CusSupportingInfoCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(BusinessObject parent)
			: base(parent, Common.IL.CusSupportingInfoTypeList.Codes.SupportingDocument)
		{
		}
	}
}
