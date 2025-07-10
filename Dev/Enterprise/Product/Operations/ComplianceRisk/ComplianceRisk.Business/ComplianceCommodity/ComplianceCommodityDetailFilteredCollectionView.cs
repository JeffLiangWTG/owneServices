using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityDetailFilteredCollectionView : BusinessObjectCollectionView<ComplianceCommodityDetail>
	{
		public ComplianceCommodityDetailFilteredCollectionView(ComplianceCommodityDetailCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return element is ComplianceCommodityDetail commodityDetail && (!hasRiskStatusFilter || commodityDetail.AssessmentInitialized) && commodityDetail.MatchesFilter(Filter);
		}

		bool hasRiskStatusFilter;
		ZQuery filter;

		public ZQuery Filter
		{
			get => filter;
			set
			{
				filter = value;
				hasRiskStatusFilter = filter != null && filter.LiteralTextADO.IndexOf(nameof(ComplianceCommodityDetailSchema.CCD_RiskStatus), System.StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}
	}
}
