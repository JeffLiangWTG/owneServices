using System;

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Business.ComplianceReport;
	using NUnit.Framework;

	[TestedType(typeof(AccComplianceReportSummaryLine))]
	public class AccComplianceReportSummaryLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var table = AccComplianceReportSummaryLine.GetDataTable(report);
			var row = table.NewRow();

			row[AccComplianceReportSummaryLine.Schema.PK] = Guid.NewGuid();
			table.Rows.Add(row);
			row.AcceptChanges();

			return new AccComplianceReportSummaryLine(Factory, row);
		}
	}
}
