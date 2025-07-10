using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business;

public class SupportingDocumentCollection : Customs.Business.CusSupportingInfoCollection<SupportingDocument>
{
	public SupportingDocumentCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.SupportingDocument)
	{
	}
}
