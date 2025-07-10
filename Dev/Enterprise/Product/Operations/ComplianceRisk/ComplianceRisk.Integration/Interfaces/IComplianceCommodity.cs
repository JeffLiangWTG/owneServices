using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface IComplianceCommodity
	{
		public ZString HarmonizedCode { get; }

		public ZString GroupingOrCountry { get; }

		public ZString Source { get; }

		public ZString CommoditySource { get; set; }

		public ZString GoodsDescription { get; }

		public ZGuid ParentJobID { get; }

		public ZString? RiskStatus { get; }

		public ZString AssessmentNotes { get; }

		public ZString Origin { get; set; }

		public ZDateTime DateAddedUtc { get; set; }
	}
}
