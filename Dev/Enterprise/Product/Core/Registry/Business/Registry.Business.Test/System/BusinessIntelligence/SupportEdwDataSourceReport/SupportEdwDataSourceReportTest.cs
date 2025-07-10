using System.Text;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupportEdwDataSourceReport))]
	public class SupportEdwDataSourceReportTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestSerializeDeserialize()
		{
			var report1 = Factory.New<IStmMenuItem>();
			report1.SU_IsPublished = true;
			report1.SU_BusinessContext = "RepAReport";
			report1.SU_MenuName = "Test Report";
			report1.SU_IsSystemDefined = true;

			Factory.Save();

			var newReport = new SupportEdwDataSourceReport();
			newReport.ReportName = report1.SU_MenuName;
			newReport.BusinessContext = report1.SU_BusinessContext;

			byte[] serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(newReport);

			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-16\"?><SupportEdwDataSourceReport><ReportName>Test Report</ReportName><BusinessContext>RepAReport</BusinessContext></SupportEdwDataSourceReport>",
				Encoding.Unicode.GetString(serializedValue).Trim());

			var deserializedSupportEdwDataSourceReport = RegistryBusinessObjectTemplateTestCase.Deserialize<SupportEdwDataSourceReport>(serializedValue);

			AssertEquals("Test Report", deserializedSupportEdwDataSourceReport.ReportName);
			AssertEquals("RepAReport", deserializedSupportEdwDataSourceReport.BusinessContext);
		}

		public void TestReportNameAndBusinessContext()
		{
			var report1 = Factory.New<IStmMenuItem>();
			report1.SU_IsPublished = true;
			report1.SU_BusinessContext = "RepAReport";
			report1.SU_MenuName = "Test Report";
			report1.SU_IsSystemDefined = true;

			var report2 = Factory.New<IStmMenuItem>();
			report2.SU_IsPublished = true;
			report2.SU_BusinessContext = "RepAReport";
			report2.SU_MenuName = "Test Report2";
			report2.SU_IsSystemDefined = true;

			Factory.Save();

			var collection = new SupportEdwDataSourceReportCollection();
			var supportEdwDataSourceReport1 = collection.AddNew();
			supportEdwDataSourceReport1.ReportName = report1.SU_MenuName;
			supportEdwDataSourceReport1.BusinessContext = report1.SU_BusinessContext;

			var supportEdwDataSourceReport2 = collection.AddNew();
			supportEdwDataSourceReport2.ReportName = report2.SU_MenuName;
			supportEdwDataSourceReport2.BusinessContext = report2.SU_BusinessContext;

			AssertNoErrors("Ledger should not have errors.", supportEdwDataSourceReport1.ReportNameInfo);
			AssertNoErrors("Ledger should not have errors.", supportEdwDataSourceReport1.BusinessContextInfo);

			supportEdwDataSourceReport1.ReportName = "";
			AssertHasError(supportEdwDataSourceReport1.ReportNameInfo, "Please enter a value.");
			supportEdwDataSourceReport1.BusinessContext = "";
			AssertHasError(supportEdwDataSourceReport1.BusinessContextInfo, "Please enter a value.");

			supportEdwDataSourceReport1.BusinessContext = report1.SU_BusinessContext;
			supportEdwDataSourceReport1.ReportName = report1.SU_MenuName.ToLower();
			AssertHasError(supportEdwDataSourceReport1.ReportNameInfo,string.Format("Please enter the correct name: Report Name:'{0}' Business Context:'{1}'", report1.SU_MenuName, report1.SU_BusinessContext));

			supportEdwDataSourceReport1.ReportName = report1.SU_MenuName;
			supportEdwDataSourceReport1.BusinessContext = report1.SU_BusinessContext.ToLower();
			AssertHasError(supportEdwDataSourceReport1.BusinessContextInfo, string.Format("Please enter the correct name: Report Name:'{0}' Business Context:'{1}'", report1.SU_MenuName, report1.SU_BusinessContext));

			supportEdwDataSourceReport1.BusinessContext = report1.SU_BusinessContext;
			supportEdwDataSourceReport1.ReportName = "XX";
			AssertHasError(supportEdwDataSourceReport1.ReportNameInfo, "Report Name and Business Context don't match any report.");

			supportEdwDataSourceReport1.ReportName = report1.SU_MenuName;
			supportEdwDataSourceReport1.BusinessContext = "XXX";
			AssertHasError(supportEdwDataSourceReport1.BusinessContextInfo, "Report Name and Business Context don't match any report.");

			supportEdwDataSourceReport1.ReportName = report2.SU_MenuName;
			supportEdwDataSourceReport1.BusinessContext = report2.SU_BusinessContext;
			supportEdwDataSourceReport1.ValidateReportName();
			AssertHasError(supportEdwDataSourceReport1.ReportNameInfo, "Report Name and Business Context must be unique.");
			AssertHasError(supportEdwDataSourceReport1.BusinessContextInfo, "Report Name and Business Context must be unique.");
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var report1 = Factory.New<IStmMenuItem>();
			report1.SU_IsPublished = true;
			report1.SU_BusinessContext = "RepAReport";
			report1.SU_MenuName = "Test Report";
			report1.SU_IsSystemDefined = true;

			Factory.Save();

			var supportEdwDataSourceReport = new SupportEdwDataSourceReport();
			supportEdwDataSourceReport.ReportName = report1.SU_MenuName;
			supportEdwDataSourceReport.BusinessContext = report1.SU_BusinessContext;

			return supportEdwDataSourceReport;
		}
	}
}
