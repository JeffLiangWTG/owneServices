using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSupplyChainActorReferenceProvider : EU.Business.Declaration.CusSupplyChainActorReferenceProvider
{
	protected CusSupplyChainActorReferenceProvider(ZString dataGroupingCode) : base(dataGroupingCode)
	{
	}

	protected override ZString GetReferenceFromOwnerCore(OrgHeader ownerOrg)
	{
		return ownerOrg.GetIdentificationNumberForCH(false);
	}
}
