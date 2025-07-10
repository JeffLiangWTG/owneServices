using System.Threading.Tasks;

namespace Enterprise.ComplianceRisk.Business
{
	public interface ISupportCheckCommodityRiskStatus
	{
		Task CheckCommoditiesRiskStatus(params ComplianceCommodityDetail[] complianceCommodityDetails);

		Task ViewBorderWisePortal(ComplianceCommodityDetail commodityDetail);

		Task CheckAllCommoditiesRiskStatus(bool needValidation);

		Task GetSupportedCountriesAndAssignStatusIfNeeded();
	}
}
