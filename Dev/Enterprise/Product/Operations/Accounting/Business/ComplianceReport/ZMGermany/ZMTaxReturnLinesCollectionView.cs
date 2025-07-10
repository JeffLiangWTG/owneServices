using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.ZMGermany
{
	public class ZMTaxReturnLinesCollectionView : BusinessObjectCollectionView<AccTaxReturnLine>
	{
		public ZMTaxReturnLinesCollectionView(BusinessObjectCollection collectionToFilter, Predicate<AccTaxReturnLine> isReportLinePartOfTheCollection)
			: base(collectionToFilter)
		{
			IsReportLinePartOfTheCollection = isReportLinePartOfTheCollection;
			Rebuild();
		}

		readonly Predicate<AccTaxReturnLine> IsReportLinePartOfTheCollection;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var reportLine = element as AccTaxReturnLine;
			return reportLine != null && IsReportLinePartOfTheCollection != null &&
				IsReportLinePartOfTheCollection(reportLine);
		}
	}
}
