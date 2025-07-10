using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class PreviousDocumentCollection : Customs.Business.CusSupportingInfoCollection<PreviousDocument>
{
	public PreviousDocumentCollection(BusinessObject parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.PreviousDocument) { }
}
