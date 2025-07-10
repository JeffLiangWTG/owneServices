using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Interfaces
{
	public interface ISupplierBuyerLinkChargeRule
	{
		ZBool ShouldApplyRule(JobDeclaration declaration);

		ZDecimal GetPercentage(OrgSupplierBuyerLink link);

		ZString GetChargeCode(JobDeclaration declaration);
	}
}
