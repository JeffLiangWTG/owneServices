using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLReports))]
	public class GLReportsTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GLReports;
		}

		public void AddReport(string businessContext, string menuName = "")
		{
			var menuItem = Factory.New<ReportCommand>();
			menuItem.SU_BusinessContext = businessContext;
			menuItem.SU_GS_NKStaffCode = "";
			menuItem.SU_MenuName = menuName;
			menuItem.SU_DocumentDirection = "";
		}

		public void TestGetNewEmbeddedControl()
		{
			using (var module = new GLReports())
			{
				var accGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
				accGLHeader.AG_Column = ZString.Empty;

				AddReport(module.BusinessContext, "menu");
				Factory.Save();

				using (var testForm = (ZForm)module.ShowPopup())
				{
					Assert("Form should be shown", testForm.Visible);

					var reportUserControl = testForm.Controls.OfType<ReportUserControl>().FirstOrDefault();
					AssertNotNull("Pre-condition: a ReportUserControl was found", reportUserControl);

					var commandCollection = reportUserControl.BindingSource.DataSource as ReportCommandCollection;

					AssertNotNull("There should have been a binding to this control", commandCollection);
					Assert("Rows should be visible", commandCollection.Count > 0);
				}
			}
		}
	}
}
