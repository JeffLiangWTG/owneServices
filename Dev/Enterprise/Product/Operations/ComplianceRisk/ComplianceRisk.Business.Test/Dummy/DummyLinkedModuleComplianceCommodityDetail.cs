using System;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class DummyLinkedModuleComplianceCommodityDetail : NonPersistentBusinessObject, ILinkedModuleComplianceCommodityDetail
	{
		public DummyLinkedModuleComplianceCommodityDetail()
		{
			SupportInteractionWithCommodities = new DummySupportInteractionWithComplianceWiseCommodities();
		}

		public IComplianceCommodityRiskStatusProvider CommodityRiskStatusProvider { get; }
		public ISupportInteractionWithComplianceWiseCommodities SupportInteractionWithCommodities { get; }
		public ZString RiskStatusDescription { get; set; }
		public ZPropertyInfo RiskStatusDescriptionInfo { get; }
		public ZString AssessmentNotes { get; set; }
		public ZPropertyInfo AssessmentNotesInfo => GetZPropertyInfo(nameof(AssessmentNotes));
		public ZString HarmonizedBorderWiseTextual { get; set; }
		public ZPropertyInfo HarmonizedBorderWiseTextualInfo { get; set; }

		public Task ViewBorderWisePortalIfAvailable()
		{
			ViewBorderWisePortalCount++;
			return Task.CompletedTask;
		}
		public int ViewBorderWisePortalCount { get; set; }

		public void InitializeIfNeeded()
		{
			return;
		}

		public void BatchInitialize(ComplianceResultFromCpw? commodityInfo)
		{
			return;
		}

		public bool CommodityExists { get; set; }
	}

	public class DummySupportInteractionWithComplianceWiseCommodities : ISupportInteractionWithComplianceWiseCommodities
	{
		public DummySupportInteractionWithComplianceWiseCommodities()
		{
			Helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			Enabled = true;
		}

		public bool Enabled { get; }
		public IInteractionWithComplianceWiseCommoditiesHelper Helper { get; set; }
	}

	public class DummyInteractionWithComplianceWiseCommoditiesHelper : IInteractionWithComplianceWiseCommoditiesHelper
	{
		public DummyInteractionWithComplianceWiseCommoditiesHelper()
		{
			SourceSideCommodities = new SourceSideCommodities() { AssessmentInitialized = AssessmentInitialized, InitializeAssessment = InitializeAssessment };
			CpwSideCommodities = new CpwSideCommodities()
			{
				AssessmentStatusChanged = delegate { AssessmentStatusChangedCount++; },
				CommoditiesChanged = CommoditiesChanged,
			};
		}

		void CommoditiesChanged(ComplianceResultFromCpw[] commodities)
		{
			CpwSideCommoditiesChanged = commodities;
		}

		public ComplianceResultFromCpw[] CpwSideCommoditiesChanged { get; set; } = Array.Empty<ComplianceResultFromCpw>();

		public int AssessmentStatusChangedCount { get; set; }

		public bool AssessmentInitialized()
		{
			return AssessmentInitializedForTest;
		}

		void InitializeAssessment()
		{
			InitializeCout++;
		}

		public int InitializeCout { get; set; }

		public bool AssessmentInitializedForTest { get; set; }

		public ISourceSideCommodities SourceSideCommodities { get; }
		public ICpwSideCommodities CpwSideCommodities { get; }
	}
}
