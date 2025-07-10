using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupplierBuyerLinkBCMChargeRule : ISupplierBuyerLinkChargeRule
	{
		ZString ISupplierBuyerLinkChargeRule.GetChargeCode(JobDeclaration declaration) => UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge;

		ZDecimal ISupplierBuyerLinkChargeRule.GetPercentage(OrgSupplierBuyerLink link) => link.OL_BuyingCommissionPercentage;

		ZBool ISupplierBuyerLinkChargeRule.ShouldApplyRule(JobDeclaration declaration) => declaration.IsImport;
	}
}
