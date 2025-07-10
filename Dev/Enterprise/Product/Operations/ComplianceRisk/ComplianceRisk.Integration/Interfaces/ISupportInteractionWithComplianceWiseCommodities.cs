using System;
using System.Threading.Tasks;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface ISupportInteractionWithComplianceWiseCommodities
	{
		bool Enabled { get; }
		IInteractionWithComplianceWiseCommoditiesHelper Helper { get; set; }
	}

	public interface IInteractionWithComplianceWiseCommoditiesHelper
	{
		ISourceSideCommodities SourceSideCommodities { get; }
		ICpwSideCommodities CpwSideCommodities { get; }
	}

	public interface ISourceSideCommodities
	{
		Action<IComplianceCommodity[]> CommoditiesChanged { get; set; }
		Action<ComplianceCommodityFromSource[]> CommoditiesAssessmentChanged { get; set; }
		Action InitializeAssessment { get; set; }
		Func<bool> AssessmentInitialized { get; set; }
		Func<ComplianceResultFromCpw[]> GetCommoditiesStatusFromCpw { get; set; }
		Func<ComplianceCommodityFromSource, ComplianceResultFromCpw?> GetCommodityStatusFromCpw { get; set; }
		Func<ComplianceCommodityFromSource, Task> ViewBorderWisePortalIfAvailable { get; set; }
	}

	public interface ICpwSideCommodities
	{
		Action<ComplianceResultFromCpw[]> CommoditiesChanged { get; set; }
		Action AssessmentStatusChanged { get; set; }
	}

	public struct ComplianceResultFromCpw
	{
		public ZString HarmonizedCode { get; set; }
		public ZString GroupingOrCountry { get; set; }
		public ZString GoodsDescription { get; set; }
		public ZString OriginOfGoods { get; set; }
		public bool LinkVisible { get; set; }
		public ZString HarmonizedBorderWiseTextual { get; set; }
		public ZString RiskStatus { get; set; }
		public ZString ImportAlertStatus { get; set; }
		public ZString RiskNotes { get; set; }
		public bool AssessmentInitialized { get; set; }
	}

	public struct ComplianceCommodityFromSource
	{
		public ZString HarmonizedCode { get; set; }
		public ZString GroupingOrCountry { get; set; }
		public ZString GoodsDescription { get; set; }
		public ZString OriginOfGoods { get; set; }
		public ZString RiskStatus { get; set; }
		public ZString RiskNotes { get; set; }
	}
}
