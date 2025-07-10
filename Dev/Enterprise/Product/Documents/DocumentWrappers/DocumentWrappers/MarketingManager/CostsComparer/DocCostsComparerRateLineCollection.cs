using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public sealed class DocCostsComparerRateLineCollection : DocumentWrapperCollection<DocCostsComparerRateLine>
	{
		public DocCostsComparerRateLineCollection(CostsComparer comparer)
			: base(comparer.Factory)
		{
			const QuotationLineType Type = QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription;

			CostsComparer = DocCostsComparer.New(comparer, comparer.Factory);

			foreach (CostsComparerEntry cost in comparer.Costs)
			{
				foreach (RateLine line in cost.RateLines)
				{
					Add(DocCostsComparerRateLine.New(QuotationLine.Header(line, Type), DocCostsComparerEntry.New(cost, Factory), Factory));
				}
			}
		}

		public DocCostsComparer CostsComparer { get; private set; }
	}
}
