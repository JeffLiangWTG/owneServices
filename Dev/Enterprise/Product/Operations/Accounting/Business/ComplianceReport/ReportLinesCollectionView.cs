using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class ReportLinesCollectionView : BusinessObjectCollectionView<AccComplianceReportLine>
	{
		public ReportLinesCollectionView(BusinessObjectCollection collectionToFilter, Predicate<AccComplianceReportLine> isReportLinePartOfTheCollection)
			: base(collectionToFilter)
		{
			IsReportLinePartOfTheCollection = isReportLinePartOfTheCollection;
			Rebuild();
		}

		readonly Predicate<AccComplianceReportLine> IsReportLinePartOfTheCollection;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var reportLine = element as AccComplianceReportLine;
			return reportLine != null && IsReportLinePartOfTheCollection != null &&
				IsReportLinePartOfTheCollection(reportLine);
		}
	}
}
