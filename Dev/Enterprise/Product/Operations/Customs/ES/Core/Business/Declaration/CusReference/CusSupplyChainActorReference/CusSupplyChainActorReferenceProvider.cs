using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusSupplyChainActorReferenceProvider : EU.Business.Declaration.CusSupplyChainActorReferenceProvider
	{
		protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
		{
		}

		protected override ZString GetReferenceFromOwnerCore(OrgHeader ownerOrg) => ownerOrg?.GetIDCode() ?? ZString.Empty;
	}
}
