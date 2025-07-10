using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.GUI.Test.WorkflowManagement
{
	class WorkflowUserControlTest : BMSTestCaseWithFactory
	{
		public void TestWorkflowOnlyLayout_BufferManagmentEnabled()
		{
			var system = CreateSystem("ORG");
			var header = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new TestForm(header))
			using (var control = new ZWorkflowUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertEquals("Only 6 tabs", 6, control.MainTabControl.TabPages.Count);
				AssertEquals("Management should be visible", true, control.MainTabControl.TabPages[0] is IWorkflowManagementTabPage);
				AssertEquals("Tasks should be visible", true, control.MainTabControl.TabPages[1] is TaskWithDetailsAndFilterTab);
				AssertEquals("Management tab selected by default", true, control.MainTabControl.SelectedTab is IWorkflowManagementTabPage);
			}
		}

		public void TestWorkflowOnlyLayout_BufferManagmentDisabled()
		{
			BMSTestHelper.DisableBMSInRegistry();

			var system = CreateSystem("ORG");
			var header = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new TestForm(header))
			using (var control = new ZWorkflowUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertEquals("Only 5 tabs", 5, control.MainTabControl.TabPages.Count);
				AssertEquals("Tasks should be visible", true, control.MainTabControl.TabPages[0] is TaskWithDetailsAndFilterTab);
			}
		}

		public void TestWorkflowAndTrackingLayout_BufferManagementEnabled()
		{
			var system = CreateSystem("ORG");
			var header = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new TestForm(header))
			using (var control = new ZWorkflowUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertEquals("MainTabControl visible", true, control.MainTabControl.Visible);
				AssertEquals(6, control.MainTabControl.TabPages.Count);
				AssertEquals("Management tab selected by default", true, control.MainTabControl.SelectedTab is IWorkflowManagementTabPage);
			}
		}

		public void TestWorkflowAndTrackingLayout_BufferManagementDisabled()
		{
			BMSTestHelper.DisableBMSInRegistry();

			var system = CreateSystem("ORG");
			var header = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new TestForm(header))
			using (var control = new ZWorkflowUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertEquals("MainTabControl visible", true, control.MainTabControl.Visible);
				AssertEquals(5, control.MainTabControl.TabPages.Count);
			}
		}

		public void TestWorkflowAndTrackingLayout_WorkflowManagementTab_ShouldExist()
		{
			WorkflowAndTrackingLayout_WorkflowManagementTab<WorkflowManagementTabPage>(true, true, 6);
		}

		public void TestWorkflowAndTrackingLayout_WorkflowManagementTab_ShouldExist_BecauseBufferManagementIsEnabled()
		{
			WorkflowAndTrackingLayout_WorkflowManagementTab<WorkflowManagementTabPage>(true, false, 6);
		}

		public void TestWorkflowAndTrackingLayout_WorkflowManagementTab_ShouldExist_BecauseUserHasPermission()
		{
			WorkflowAndTrackingLayout_WorkflowManagementTab<WorkflowManagementTabPage>(false, true, 6);
		}

		public void TestWorkflowAndTrackingLayout_WorkflowManagementTab_ShouldNotExist_BecauseUserHasNoPermission()
		{
			WorkflowAndTrackingLayout_WorkflowManagementTab<TaskWithDetailsAndFilterTab>(false, false, 5);
		}

		public void TestWorkflowAndTrackingLayout_WorkflowManagementTab_ShouldNotExist_BecauseSystemHasNoSuchWorkflowType()
		{
			WorkflowAndTrackingLayout_WorkflowManagementTab<TaskWithDetailsAndFilterTab>(true, true, 5, "ABC");
		}

		public void WorkflowAndTrackingLayout_WorkflowManagementTab<T>(bool alwaysViewWorkflowManagementTab, bool viewWorkflowManagementTabAllowed, int tabsExpected, string systemType = "ORG")
		{
			BMSRegistry.Instance.AlwaysViewWorkflowManagementTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, alwaysViewWorkflowManagementTab);
			Env.Security.ViewWorkflowManagementTab.IsAllowed = viewWorkflowManagementTabAllowed;

			var system = CreateSystem(systemType);
			var header = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new TestForm(header))
			using (var control = new ZWorkflowUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertEquals("MainTabControl visible", true, control.MainTabControl.Visible);
				AssertEquals(tabsExpected, control.MainTabControl.TabPages.Count);
				AssertEquals("Management tab selected by default", true, control.MainTabControl.SelectedTab is T);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.AlwaysViewWorkflowManagementTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}

	class TestForm : ZChildForm
	{
		public TestForm(OrgHeader header)
			: base(header)
		{
			this.ControllerID = DummyControllerIDs.Dummy;
			ZTemplateTabControl tabControl = new ZTemplateTabControl();
			ZWorkflowTabPage tabPage = new ZWorkflowTabPage();
			Controls.Add(tabControl);
			tabControl.Controls.Add(tabPage);
			tabPage.Initialize(header);
		}
	}
}
