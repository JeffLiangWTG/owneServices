using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class NonCustomsLawCollection : CusSupportingInfoCollection<NonCustomsLaw>
{
	public NonCustomsLawCollection(BusinessObject parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.NonCustomsLaw) { }
}
