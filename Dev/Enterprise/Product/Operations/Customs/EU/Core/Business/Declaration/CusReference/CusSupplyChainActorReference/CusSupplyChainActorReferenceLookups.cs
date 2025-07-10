using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusSupplyChainActorReferenceLookups : CommonCusReferenceLookups
	{
		public CusSupplyChainActorReferenceLookups(CusSupplyChainActorReference parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<SupplyChainActorRoleList>();
	}
}
