using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class RestrictionCollection : CusSupportingInfoCollection<Restriction>
{
	public RestrictionCollection(BusinessObject parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.Restriction)
	{
	}
}
