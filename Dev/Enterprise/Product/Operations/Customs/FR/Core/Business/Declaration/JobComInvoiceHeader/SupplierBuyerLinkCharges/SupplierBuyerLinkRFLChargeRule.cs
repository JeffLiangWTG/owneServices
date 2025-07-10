using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupplierBuyerLinkRFLChargeRule : ISupplierBuyerLinkChargeRule
	{
		ZString ISupplierBuyerLinkChargeRule.GetChargeCode(JobDeclaration declaration) => UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge;

		ZDecimal ISupplierBuyerLinkChargeRule.GetPercentage(OrgSupplierBuyerLink link) => link.OL_RoyaltyPercentage;

		ZBool ISupplierBuyerLinkChargeRule.ShouldApplyRule(JobDeclaration declaration) => true;
	}
}
