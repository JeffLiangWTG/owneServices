using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccComplianceReportLineCollectionBase<AccComplianceReportLine>))]
	public class AccComplianceReportLineCollectionBaseTest : NonPersistentBusinessObjectCollectionTestCase<AccComplianceReportLineCollectionBase<AccComplianceReportLine>>
	{
		protected override AccComplianceReportLineCollectionBase<AccComplianceReportLine> GetCollectionToTest()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			return new AccComplianceReportLineCollectionBase<AccComplianceReportLine>(report);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AccComplianceReportLine(Factory, AccComplianceReportLineTest.GetDataRow(Factory));
		}
	}
}
