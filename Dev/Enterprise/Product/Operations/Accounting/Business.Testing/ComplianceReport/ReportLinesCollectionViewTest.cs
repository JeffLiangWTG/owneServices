using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport
{
	[TestedType(typeof(ReportLinesCollectionView))]
	class ReportLinesCollectionViewTest : BusinessObjectCollectionViewTestCase<ReportLinesCollectionView>
	{
		public void TestFiltering()
		{
			var dummyCollection = new DummyBusinessObjectCollection(Factory);
			dummyCollection.AddNew();
			dummyCollection.AddNew();

			var reportLinesCollectionView = new ReportLinesCollectionView(dummyCollection, (reportLine) => reportLine.PostDate.IsValid);
			AssertContainsExactElementsInAnyOrder("Not a reportLine collection to filter", Array.Empty<AccComplianceReportLine>(), reportLinesCollectionView);

			var line1 = createReportLine(new DateTime(2019, 3, 20));
			var line2 = createReportLine(new DateTime(2019, 3, 15));
			var line3 = createReportLine(new DateTime(2019, 3, 10));
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var lineCollection = new AccComplianceReportLineCollectionBase<AccComplianceReportLine>(report);
			lineCollection.Add(line1);
			lineCollection.Add(line2);
			lineCollection.Add(line3);

			reportLinesCollectionView = new ReportLinesCollectionView(lineCollection, null);
			AssertContainsExactElementsInAnyOrder("No predicate => no report line", Array.Empty<AccComplianceReportLine>(), reportLinesCollectionView);

			reportLinesCollectionView = new ReportLinesCollectionView(lineCollection, (line) => line.PostDate > new DateTime(2019, 3, 12));
			AssertContainsExactElementsInAnyOrder(new AccComplianceReportLine[] { line1, line2 }, reportLinesCollectionView);

			var line4 = createReportLine(new DateTime(2019, 3, 25));
			lineCollection.Add(line4);
			AssertContainsExactElementsInAnyOrder(new AccComplianceReportLine[] { line1, line2, line4 }, reportLinesCollectionView);
		}

		AccComplianceReportLine createReportLine(DateTime postDate)
		{
			var row = AccComplianceReportLineTest.GetDataRow(Factory);
			row[AccComplianceReportLine.Schema.PostDate] = postDate;
			return new AccComplianceReportLine(Factory, row);
		}

		protected override ReportLinesCollectionView GetCollectionToTest()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var reportLines = new AccComplianceReportLineCollectionBase<AccComplianceReportLine>(report);
			return new ReportLinesCollectionView(reportLines, (reportLine) => true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AccComplianceReportLine(Factory, AccComplianceReportLineTest.GetDataRow(Factory));
		}
	}
}
