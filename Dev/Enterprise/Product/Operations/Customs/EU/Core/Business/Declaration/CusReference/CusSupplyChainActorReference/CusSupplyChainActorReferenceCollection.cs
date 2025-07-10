using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusSupplyChainActorReferenceCollection<TCusSupplyChainActorReference> : CommonCusReferenceCollection<TCusSupplyChainActorReference>, ICusSupplyChainActorReferenceCollection<TCusSupplyChainActorReference>
		where TCusSupplyChainActorReference : CusSupplyChainActorReference
	{
		public CusSupplyChainActorReferenceCollection(BusinessObject parent) : base(parent, CusReferenceTypeList.Codes.SupplyChainActor)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}
		const int MaxCountForValidation = 99;
	}
}
