using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business.Declaration;

public class FiscalReferenceCollection : Customs.Business.CusSupportingInfoCollection<FiscalReference>
{
	public FiscalReferenceCollection(BusinessObject parent) : base(parent, Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.FiscalReference)
	{
		MaxCountValidationEnable(99);
	}
}
