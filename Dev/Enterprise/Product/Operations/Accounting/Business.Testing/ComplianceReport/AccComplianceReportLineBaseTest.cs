using System;

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Business.ComplianceReport;
	using NUnit.Framework;

	[TestedType(typeof(AccComplianceReportLineBase))]
	public class AccComplianceReportLineBaseTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var table = AccComplianceReportLineBase.GetDataTable(report);
			var row = table.NewRow();

			row[AccComplianceReportLineBase.Schema.PK] = Guid.NewGuid();
			table.Rows.Add(row);
			row.AcceptChanges();

			return new AccComplianceReportLineBase(Factory, row);
		}
	}
}

