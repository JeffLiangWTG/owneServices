using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusSupplyChainActorReferenceCollection : CommonCusReferenceCollection<CusSupplyChainActorReference>
{
	public CusSupplyChainActorReferenceCollection(BusinessObject parent) : base(parent, CusReferenceTypeList.Codes.SupplyChainActor)
	{
		MaxCountValidationEnable(MaxCountForValidation);
	}
	const int MaxCountForValidation = 99;
}
