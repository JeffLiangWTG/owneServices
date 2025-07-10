using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(ReportCommandCollection))]
	sealed class ReportCommandCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			const string TestReportContext = "TestReports";
			return new ReportCommandCollection(Factory, TestReportContext);
		}

		public void TestBusinessContext()
		{
			ReportCommandCollection reports = new ReportCommandCollection(Factory, "Grapefruit");
			AssertEquals("BusinessContext", "Grapefruit", reports.BusinessContext);
		}

		public void TestReportCollection()
		{
			const string TestReportContext = "TestReports";

			#region Templates

			StmTemplateBase sysBalanceSheetTemplate = Factory.New<StmTemplateBase>();
			sysBalanceSheetTemplate.SO_DataContext = "";
			sysBalanceSheetTemplate.SO_IsSystemDefined = true;
			sysBalanceSheetTemplate.SO_Name = "System BalanceSheet Template";

			StmTemplateBase userBalanceSheetTemplate = Factory.New<StmTemplateBase>();
			userBalanceSheetTemplate.SO_DataContext = "";
			userBalanceSheetTemplate.SO_IsSystemDefined = false;
			userBalanceSheetTemplate.SO_Name = "User BalanceSheet Template";

			StmTemplateBase sysIncidentsTemplate = Factory.New<StmTemplateBase>();
			sysIncidentsTemplate.SO_DataContext = "";
			sysIncidentsTemplate.SO_IsSystemDefined = true;
			sysIncidentsTemplate.SO_Name = "System Incidents Template";

			#endregion

			#region Published System BalanceSheet

			ReportCommand pubSysBalanceSheetMenu = Factory.New<ReportCommand>();
			pubSysBalanceSheetMenu.SU_BusinessContext = TestReportContext;
			pubSysBalanceSheetMenu.SU_IsPublished = true;
			pubSysBalanceSheetMenu.SU_IsSystemDefined = true;
			pubSysBalanceSheetMenu.SU_MenuName = "Pub System BalanceSheet Report";
			pubSysBalanceSheetMenu.SU_MenuIndex = 1;
			pubSysBalanceSheetMenu.SU_MenuPath = "";
			pubSysBalanceSheetMenu.SU_MenuShortcut = "";

			StmMenuTemplatePivotBase pubSysBalanceSheetPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysBalanceSheetPivot1.SI_SO = sysBalanceSheetTemplate.PK;
			pubSysBalanceSheetPivot1.SI_SU = pubSysBalanceSheetMenu.PK;
			pubSysBalanceSheetPivot1.SI_DocumentTitle = "Pub System Balance Sheet Document";

			#endregion

			#region Published System IncidentReport
			// this report is not meant to be loaded.
			ReportCommand pubSysIncidentMenu = Factory.New<ReportCommand>();
			pubSysIncidentMenu.SU_BusinessContext = "Another Context";
			pubSysIncidentMenu.SU_IsPublished = true;
			pubSysIncidentMenu.SU_IsSystemDefined = true;
			pubSysIncidentMenu.SU_MenuName = "Pub System Incident Report";
			pubSysIncidentMenu.SU_MenuIndex = 1;
			pubSysIncidentMenu.SU_MenuPath = "";
			pubSysIncidentMenu.SU_MenuShortcut = "";

			StmMenuTemplatePivotBase pubSysIncidentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysIncidentPivot1.SI_SO = sysIncidentsTemplate.PK;
			pubSysIncidentPivot1.SI_SU = pubSysIncidentMenu.PK;
			pubSysIncidentPivot1.SI_DocumentTitle = "Pub System Incident Document";

			#endregion

			Factory.Save();

			ReportCommandCollection collection = new ReportCommandCollection(Factory, TestReportContext);
			collection.Load();

			AssertEquals("Count", 1, collection.Count);
			AssertEquals("PubSysBalanceSheet Name", pubSysBalanceSheetMenu.SU_MenuName, collection[0].SU_MenuName);
		}

		public void TestLoadApplicableReports()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Liechtenstein);

			const string TestReportContext = "TestReports";

			ReportCommand unFilteredReport = Factory.New<ReportCommand>();
			unFilteredReport.SU_MenuName = "UnFiltered Report";
			unFilteredReport.SU_BusinessContext = TestReportContext;
			unFilteredReport.SU_IsPublished = true;
			unFilteredReport.SU_IsSystemDefined = true;

			ReportCommand sGReport = Factory.New<ReportCommand>();
			sGReport.SU_MenuName = "SG Report";
			sGReport.SU_BusinessContext = TestReportContext;
			sGReport.SU_FilterList = DocumentFilters.CTY + "=SG";
			sGReport.SU_IsPublished = true;
			sGReport.SU_IsSystemDefined = true;

			Factory.Save();

			ReportCommandCollection collection = new ReportCommandCollection(Factory, TestReportContext);
			collection.LoadApplicableReports();

			AssertEquals("Collection.Count", 1, collection.Count);
			AssertEquals("MenuName", unFilteredReport.SU_MenuName, collection[0].SU_MenuName);
		}

		public void TestAddReport()
		{
			const string TestReportContext = "TestReports";

			ReportCommand testReport = Factory.New<ReportCommand>();
			testReport.SU_MenuName = "Test Report Add";
			testReport.SU_IsPublished = true;
			testReport.SU_IsSystemDefined = true;

			ReportCommandCollection collection = new ReportCommandCollection(Factory, TestReportContext);
			collection.Add(testReport);

			AssertEquals("BusinessContext", TestReportContext, testReport.SU_BusinessContext);
		}
	}
}
