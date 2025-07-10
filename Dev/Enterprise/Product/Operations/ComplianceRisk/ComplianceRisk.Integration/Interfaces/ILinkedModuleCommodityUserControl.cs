using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface ILinkedModuleCommodityUserControl
	{
	}

	public interface ILinkedModuleComplianceCommodityDetail
	{
		public IComplianceCommodityRiskStatusProvider CommodityRiskStatusProvider { get; }
		public ISupportInteractionWithComplianceWiseCommodities SupportInteractionWithCommodities { get; }
		public ZString RiskStatusDescription { get; set; }
		public ZPropertyInfo RiskStatusDescriptionInfo { get; }
		public ZString AssessmentNotes { get; set; }
		public ZPropertyInfo AssessmentNotesInfo { get; }
		public ZString HarmonizedBorderWiseTextual { get; set; }
		public ZPropertyInfo HarmonizedBorderWiseTextualInfo { get; }
		public Task ViewBorderWisePortalIfAvailable();
		public void InitializeIfNeeded();
		public void BatchInitialize(ComplianceResultFromCpw? commodityInfo);
		public bool CommodityExists { get; }
	}
}
