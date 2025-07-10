using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test.FilterRule
{
	public abstract class FilterTabPageControlTestCase : BMSTestCaseWithFactory
	{
		public void TestFilterTab_InViewMode_ShouldOnlyEnablePreviewButton()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var bizo = GetBusinessObjectForBinding(config);
			Factory.Save();
			var controller = ZControllerFactory.Instance.GetControllerForBizo(bizo);

			using (var form = (ZForm)controller.ShowViewForm(bizo))
			{
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>(x => x.Name == TabControlName);
				tabControl.SelectTab(TabPageControlName);
				Application.DoEvents();

				var filterControl = tabControl.SelectedTab.FindSingle<BMFilterStripWrapperControl>();
				var toolStrip = filterControl.FindSingle<ZToolStrip>(x => x.Name == "ToolStrip");

				foreach (ToolStripItem item in toolStrip.Items)
				{
					if (item.Text == "Preview")
					{
						AssertEquals("The preview button should be enabled even in view mode, and yet...", true, item.Enabled);
					}
					else
					{
						AssertEquals("All buttons should be disabled except the preview button, and yet... Button: " + item.Text, true, !item.Enabled || !item.Visible);
					}
				}

				var toolStripHelp = filterControl.FindSingle<ZToolStrip>(x => x.Name == "ToolStripHelp");

				foreach (ToolStripItem item in toolStripHelp.Items)
				{
					AssertEquals("All buttons should be disabled except the preview button, and yet... Button: " + item.Text, true, !item.Enabled || !item.Visible);
				}
			}
		}

		public void TestFilterTab_InViewMode_ShouldNoBeDisabled()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var bizo = GetBusinessObjectForBinding(config);
			Factory.Save();
			var controller = ZControllerFactory.Instance.GetControllerForBizo(bizo);

			using (var form = (ZForm)controller.ShowViewForm(bizo))
			{
				Application.DoEvents();

				var tabPage = form.FindSingle<ZTabPage>(x => x.Name == TabPageControlName);
				AssertEquals("Opening the form in view mode should not disable the tab page itself, and yet...", true, tabPage.Enabled);

				tabPage.SetReadOnly(true);
				AssertEquals("Explicitly setting the tab page read only should not disable the tab page itself, and yet...", true, tabPage.Enabled);
			}
		}

		protected abstract string TabPageControlName { get; }
		protected abstract string TabControlName { get; }
		protected abstract BusinessObject GetBusinessObjectForBinding(VisualBoardTestConfig config);
	}
}
