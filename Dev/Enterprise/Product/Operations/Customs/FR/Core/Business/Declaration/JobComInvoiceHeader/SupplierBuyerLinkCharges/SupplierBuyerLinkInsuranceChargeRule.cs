using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupplierBuyerLinkInsuranceChargeRule : ISupplierBuyerLinkChargeRule
	{
		ZString ISupplierBuyerLinkChargeRule.GetChargeCode(JobDeclaration declaration) => declaration.IsAir ? FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge : FRCustomsChargeTypeList.Codes.InsuranceCostsCharge;

		ZDecimal ISupplierBuyerLinkChargeRule.GetPercentage(OrgSupplierBuyerLink link) => link?.OL_InsuranceUplift ?? ZDecimal.Zero;

		ZBool ISupplierBuyerLinkChargeRule.ShouldApplyRule(JobDeclaration declaration) => true;
	}
}
