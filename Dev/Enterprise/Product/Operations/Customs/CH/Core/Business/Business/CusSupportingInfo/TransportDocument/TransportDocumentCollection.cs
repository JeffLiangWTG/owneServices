using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business;

public class TransportDocumentCollection : Customs.Business.CusSupportingInfoCollection<TransportDocument>
{
	public TransportDocumentCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.TransportDocument)
	{
	}
}
