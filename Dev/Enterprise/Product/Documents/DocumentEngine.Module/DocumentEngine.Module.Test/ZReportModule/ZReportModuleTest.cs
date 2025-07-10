using System.IO;
using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.Module.Testing
{
	sealed class ZReportModuleTest : TestCaseWithFactory
	{
		class DummyReportModule : ZReportModule
		{
			public DummyReportModule()
			{
			}

			public override ModuleIdentifier ID
			{
				get { return DummyModuleIDs.Dummy; }
			}
		}

		[CodeAlive("Instance is created by GetTypeByName in ReportUrlHandler unit test TestHandle_ForClientModule")]
		public class DummyClientReportModule : ZReportModule
		{
			public DummyClientReportModule()
			{
			}

			public override ModuleIdentifier ID
			{
				get { return new ClientModuleIdentifier(TestClientModuleId.ClientModuleID, "Reports", (NoResString)"Test Client Reports"); }
			}
		}

		public void TestEmbeddedControl()
		{
			TestData.CreateDocEngineTestTable();
			TestData.CreateJobTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateHeaderTestTable();

			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly))
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));

				StmTemplateBase template = Factory.New<StmTemplateBase>();
				template.SO_Name = "TestTemplate";
				template.SO_IsSystemDefined = true;
				template.SO_Template = new ZBlob(excelTemplate.GetAsByteArray());

				ReportCommand command1 = Factory.New<ReportCommand>();
				command1.SU_MenuName = "Test Report 99";
				command1.SU_IsSystemDefined = true;
				command1.SU_IsPublished = true;
				command1.SU_BusinessContext = "Rep" + DummyModuleIDs.Dummy.ToString();

				StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SU = command1.PK;
				pivot.SI_SO = template.PK;
				pivot.SI_DocumentTitle = "TestReport";

				Factory.Save();

				using (DummyReportModule module = new DummyReportModule())
				{
					ReportUserControl reportControl = module.EmbeddedControl as ReportUserControl;

					using (ZForm testForm = new ZForm(Dummy))
					{
						testForm.Controls.Add(reportControl);
						testForm.Show();

						AssertEquals("Row Count on grid", 1, reportControl.ReportGrid.ListManager.Count);

						AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "Run"));
						AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "Schedule"));
						AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "Customize Reports"));
						AssertNotNull(FindMenuItem(reportControl.ReportGrid.ContextMenu.MenuItems, "&Customize Columns"));

						reportControl.ReportGrid.ListManager.Position = 0;
					}
				}
			}
		}

		MenuItem FindMenuItem(Menu.MenuItemCollection items, string text)
		{
			foreach (MenuItem item in items)
			{
				if (item.Text == text)
				{
					return item;
				}
			}
			return null;
		}

		public void TestNotShowingReportsThatBelongToAnotherCountry()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Liechtenstein);

			ReportCommand unFilteredReport = Factory.New<ReportCommand>();
			unFilteredReport.SU_MenuName = "UnFiltered Report";
			unFilteredReport.SU_BusinessContext = "Rep" + DummyModuleIDs.Dummy.ToString();
			unFilteredReport.SU_IsPublished = true;
			unFilteredReport.SU_IsSystemDefined = true;

			ReportCommand sGReport = Factory.New<ReportCommand>();
			sGReport.SU_FilterList = DocumentFilters.CTY + "=SG";
			sGReport.SU_MenuName = "SG Report";
			sGReport.SU_BusinessContext = "Rep" + DummyModuleIDs.Dummy.ToString();
			sGReport.SU_IsPublished = true;
			sGReport.SU_IsSystemDefined = true;

			Factory.Save();

			using (DummyReportModule module = new DummyReportModule())
			using (ReportUserControl reportControl = module.EmbeddedControl as ReportUserControl)
			using (ZForm testForm = new ZForm(Dummy))
			{
				testForm.Controls.Add(reportControl);
				testForm.Show();

				AssertEquals("Row Count on grid", 1, reportControl.ReportGrid.ListManager.Count);
			}
		}

		#region Implementation

		DummyBusinessObject Dummy;

		protected override void SetUp()
		{
			base.SetUp();

			MockSourceControl.Setup();
			CustomisationMenusMaker<MenuItem>.ShouldAddDebugOnlyMenuItemsForTesting = true;
			Dummy = Factory.New<DummyBusinessObject>();
			AssertNotNull("Dummy should be created!", Dummy);
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
			CustomisationMenusMaker<MenuItem>.ShouldAddDebugOnlyMenuItemsForTesting = false;
		}

		#endregion
	}
}
