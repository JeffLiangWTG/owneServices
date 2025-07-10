using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestZTabPagePlugIn : TestCaseWithDummy
	{
		public void TestAddPlugInUserControl()
		{
			using (var testForm = new TestPlugInForm2(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy3);
				var plugIn = testForm.PlugIns.Instances[1] as DummyPlugIn3;
				var userControl = (DummyPlugIn3.TestUserControl)plugIn.UserControl;
				var tabPage = plugIn.TabPage;

				AssertEquals("Precondition: TabPage.Controls.Contains(UserControl)", false, tabPage.Controls.Contains(userControl));
				AssertEquals("Precondition: TabPage.Controls.Count", 0, tabPage.Controls.Count);
				Assert("Precondition: PlugIn.UserControl.Dock != DockStyle.Fill", plugIn.UserControl.Dock != DockStyle.Fill);

				tabPage.AddPlugInUserControl();

				AssertEquals("TabPage.Controls.Contains(UserControl)", true, tabPage.Controls.Contains(userControl));
				AssertEquals("TabPage.Controls.Count", 1, tabPage.Controls.Count);
				Assert("PlugIn.UserControl.Dock != DockStyle.Fill", plugIn.UserControl.Dock == DockStyle.Fill);

				tabPage.AddPlugInUserControl();

				AssertEquals("TabPage.Controls.Contains(UserControl)", true, tabPage.Controls.Contains(userControl));
				AssertEquals("TabPage.Controls.Count", 1, tabPage.Controls.Count);
				Assert("PlugIn.UserControl.Dock != DockStyle.Fill", plugIn.UserControl.Dock == DockStyle.Fill);
			}
		}

		public void TestIDataGridLayoutIdentifierRoot()
		{
			using (var form = new TestPlugInForm2(Dummy))
			{
				form.Show();

				var plugIn = (DummyPlugIn1)form.ExposedPlugIns[0];
				plugIn.Enabled = true;

				IDataGridLayoutIdentifierRoot idRoot = plugIn.TabPage;
				AssertEquals("DataGrid layout to be different for each form a plug-in is plugged into", Dummy.GetType().Name, idRoot.ID);
			}
		}

		public void TestAddPlugInUserControl_NullUserControl()
		{
			using (var testForm = new TestPlugInForm2(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy3);
				var dummyPlugIn3 = testForm.PlugIns.Instances[1] as DummyPlugIn3;
				dummyPlugIn3.DisableUserControl = true;
				AssertNull("PreCondition: UserControl is null", dummyPlugIn3.UserControl);

				var tabPage = dummyPlugIn3.TabPage;
				var initialTabPageControlCount = tabPage.Controls.Count;
				tabPage.AddPlugInUserControl();

				AssertEquals("No Controls were added.", initialTabPageControlCount, tabPage.Controls.Count);
			}
		}

		public void TestRemovePlugInUserControl()
		{
			using (var testForm = new TestPlugInForm2(Dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy3);
				var plugIn = testForm.PlugIns.Instances[1] as DummyPlugIn3;
				var userControl = (DummyPlugIn3.TestUserControl)plugIn.UserControl;
				var tabPage = plugIn.TabPage;

				tabPage.AddPlugInUserControl();

				AssertEquals("Precondition: TabPage.Controls.Contains(UserControl)", true, tabPage.Controls.Contains(userControl));
				AssertEquals("Precondition: TabPage.Controls.Count", 1, tabPage.Controls.Count);
				AssertEquals("Precondition: UserControl.Visible", true, userControl.Visible);

				tabPage.RemovePlugInUserControl(userControl);

				AssertEquals("TabPage.Controls.Contains(UserControl)", false, tabPage.Controls.Contains(userControl));
				AssertEquals("TabPage.Controls.Count", 0, tabPage.Controls.Count);
				AssertEquals("UserControl.Visible", false, userControl.Visible);

				tabPage.RemovePlugInUserControl(userControl);

				AssertEquals("TabPage.Controls.Contains(UserControl)", false, tabPage.Controls.Contains(userControl));
				AssertEquals("TabPage.Controls.Count", 0, tabPage.Controls.Count);
				AssertEquals("UserControl.Visible", false, userControl.Visible);
			}
		}

		public void TestTextAndName()
		{
			using (var plugIn = new DummyPlugIn2(Dummy))
			{
				plugIn.SetName("Hello Zubin");
				using (var form = new ZForm())
				using (var tabControl = new ZTabControl())
				using (var tabPage = new ZTabPagePlugIn(plugIn))
				{
					form.Controls.Add(tabControl);
					tabControl.Controls.Add(tabPage);

					form.Show();
					Application.DoEvents();

					AssertEquals("Hello Zubin", tabPage.Text);
					AssertEquals("HelloZubinTabPage", tabPage.Name);
				}
			}
		}

		public void TestResourceStringsEnabled()
		{
			using (var plugIn = new DummyPlugIn2(Dummy))
			{
				plugIn.SetName("Hello Zubin");
				using (var form = new ZForm())
				using (var tabControl = new ZTabControl())
				using (var tabPage = new ZTabPagePlugIn(plugIn))
				{
					form.CaptionRenderingEnabled = true;
					form.Controls.Add(tabControl);
					tabControl.TabPages.Add(tabPage);
					AssertEquals("TabPage.Text is not overridden by ZPlugIn.Name when CaptionRenderingEnabled", false, ((IVariableLengthCaptionRenderer)tabPage).IsCaptionOverridden);
				}
			}
		}

		public void TestDelayBinding()
		{
			using (var plugIn = new DummyPlugIn2(Dummy))
			using (var tabPage = new ZTabPagePlugIn(plugIn))
			{
				plugIn.OverrideDelayBinding = true;
				plugIn.DelayBindingExposed = false;
				AssertEquals(false, tabPage.DelayBinding);

				plugIn.DelayBindingExposed = true;
				AssertEquals(true, tabPage.DelayBinding);
			}
		}
	}
}
