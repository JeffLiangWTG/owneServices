using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusSupplyChainActorReferenceValidation : EU.Business.Declaration.CusSupplyChainActorReferenceValidation
{
	public CusSupplyChainActorReferenceValidation(EU.Business.Declaration.CusSupplyChainActorReference parent) : base(parent)
	{
	}

	protected override ZString GetAEORegNumber(OrgHeader ownerOrg) => ownerOrg.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, ZString.Empty);
}

