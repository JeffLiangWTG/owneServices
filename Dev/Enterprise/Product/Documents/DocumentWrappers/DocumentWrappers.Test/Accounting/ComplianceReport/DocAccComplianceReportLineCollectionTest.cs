using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccComplianceReportLineCollection))]
	sealed class DocAccComplianceReportLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocAccComplianceReportLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var reportLine = new AccComplianceReportLine(Factory, DocAccComplianceReportLineTest.GetDataRow(Report));
			var result = DocAccComplianceReportLine.New(reportLine, Factory);
			result.ReportUniqueID = ReportWrapper.UniqueReportID;
			return result;
		}

		protected override DocAccComplianceReportLineCollection GetCollectionToTest()
		{
			return new DocAccComplianceReportLineCollection(Factory);
		}

		TestObjectCreator Creator;
		AccComplianceReport Report;
		DocAccComplianceReport ReportWrapper;

		protected override void SetUp()
		{
			Creator = new TestObjectCreator(Factory);
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateConfigurationForComplianceReport(Report);
			ReportWrapper = DocAccComplianceReport.New(Report, Factory);

			base.SetUp();
		}
	}
}
