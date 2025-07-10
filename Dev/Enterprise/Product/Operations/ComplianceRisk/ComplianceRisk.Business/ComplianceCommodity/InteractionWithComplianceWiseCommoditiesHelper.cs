using System;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.ComplianceRisk.Business
{
	public class InteractionWithComplianceWiseCommoditiesHelper : IInteractionWithComplianceWiseCommoditiesHelper
	{
		public InteractionWithComplianceWiseCommoditiesHelper(IBusiness hostBusinessEntity)
		{
			if (hostBusinessEntity is IComplianceCommodityRiskStatusProvider complianceCommodityRiskStatusProvider)
			{
				ComplianceCommodityRiskStatusProvider = complianceCommodityRiskStatusProvider;
			}
			else
			{
				throw new ArgumentException($"Host business entity must implement {nameof(IComplianceCommodityRiskStatusProvider)}");
			}

			SourceSideCommodities = new SourceSideCommodities();
			CpwSideCommodities = new CpwSideCommodities();
		}

		public ISourceSideCommodities SourceSideCommodities { get; }
		public ICpwSideCommodities CpwSideCommodities { get; }
		public IComplianceCommodityRiskStatusProvider ComplianceCommodityRiskStatusProvider { get; }
	}

	public class SourceSideCommodities : ISourceSideCommodities
	{
		public Action<IComplianceCommodity[]> CommoditiesChanged { get; set; }
		public Action<ComplianceCommodityFromSource[]> CommoditiesAssessmentChanged { get; set; }
		public Action InitializeAssessment { get; set; }
		public Func<bool> AssessmentInitialized { get; set; }
		public Func<ComplianceResultFromCpw[]> GetCommoditiesStatusFromCpw { get; set; }
		public Func<ComplianceCommodityFromSource, ComplianceResultFromCpw?> GetCommodityStatusFromCpw { get; set; }
		public Func<ComplianceCommodityFromSource, Task> ViewBorderWisePortalIfAvailable { get; set; }
	}

	public class CpwSideCommodities : ICpwSideCommodities
	{
		public Action<ComplianceResultFromCpw[]> CommoditiesChanged { get; set; }
		public Action AssessmentStatusChanged { get; set; }
	}
}
