using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZLogsTabPageTest : ZTabPageControlTest
	{
		public void TestAddAdditionalTab_WithExcludeFromBindingOnSave()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var tabControl = new ZTemplateTabControl())
			using (var logsTabPage = new TestLogsTabPage())
			using (var someControl = new ZUserControl())
			{
				tabControl.TabPages.Add(logsTabPage);
				form.Controls.Add(tabControl);
				form.Show();

				logsTabPage.AddAdditionalTab("TabExcludeBindingOnSave", someControl, true);
				logsTabPage.SetSelectedTab("TabExcludeBindingOnSave");
				AssertEquals(true, logsTabPage.LogsUserControl.MainTabControl.SelectedTab.ExcludeFromBindingOnSave);
			}
		}

		public void TestAddAdditionalTab()
		{
			DataRegistry.Instance.UserEventTrackingEnterprise = true;

			DummyEnterpriseBusinessObject dummy = Factory.New<DummyEnterpriseBusinessObject>();
			using (ZForm form = new ZForm(dummy))
			using (ZTemplateTabControl tabControl = new ZTemplateTabControl())
			using (TestLogsTabPage logsTabPage = new TestLogsTabPage())
			using (ZUserControl someControl = new ZUserControl())
			{
				tabControl.TabPages.Add(logsTabPage);
				form.Controls.Add(tabControl);
				form.Show();

				AssertEquals(2, logsTabPage.LogsUserControl.MainTabControl.TabPages.Count);
				logsTabPage.AddAdditionalTab("Zubin", someControl);
				AssertEquals(3, logsTabPage.LogsUserControl.MainTabControl.TabPages.Count);
				AssertEquals("Zubin", logsTabPage.LogsUserControl.MainTabControl.TabPages[2].Text);
				AssertEquals(1, logsTabPage.LogsUserControl.MainTabControl.TabPages[2].Controls.Count);
				AssertEquals(someControl, logsTabPage.LogsUserControl.MainTabControl.TabPages[2].Controls[0]);
				AssertEquals(DockStyle.Fill, logsTabPage.LogsUserControl.MainTabControl.TabPages[2].Controls[0].Dock);
				AssertEquals(false, ((ZTabPage)logsTabPage.LogsUserControl.MainTabControl.TabPages[2]).ExcludeFromBindingOnSave);
			}
		}

		public void TestSetSelectedlTab()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var tabControl = new ZTemplateTabControl())
			using (var logsTabPage = new TestLogsTabPage())
			using (var someControl = new ZUserControl())
			{
				tabControl.TabPages.Add(logsTabPage);
				form.Controls.Add(tabControl);
				form.Show();

				logsTabPage.AddAdditionalTab("LOL", someControl);
				AssertNotEquals("LOL", logsTabPage.LogsUserControl.MainTabControl.SelectedTab.Text);

				logsTabPage.SetSelectedTab("LOL");
				AssertEquals("LOL", logsTabPage.LogsUserControl.MainTabControl.SelectedTab.Text);

				logsTabPage.SetSelectedTab("DUMMY NOT EXISTS");
				AssertEquals("LOL", logsTabPage.LogsUserControl.MainTabControl.SelectedTab.Text);
			}
		}

		public void TestText()
		{
			DummyEnterpriseBusinessObject dummy = Factory.New<DummyEnterpriseBusinessObject>();
			using (ZForm form = new ZForm(dummy))
			using (ZTemplateTabControl tabControl = new ZTemplateTabControl())
			using (TestLogsTabPage logsTabPage = new TestLogsTabPage())
			{
				tabControl.TabPages.Add(logsTabPage);
				form.Controls.Add(tabControl);
				form.Show();
				Application.DoEvents();
				AssertEquals("Logs", logsTabPage.Text);
			}
		}

		public void TestExcludeFromBindingOnSave()
		{
			AssertEquals("For performance don't bind on save", true, TestTabPage.ExcludeFromBindingOnSave);
		}

		public void TestHostedLogsControl()
		{
			Form.Controls.Add(TestTabControl);
#if !WINZOR
			TestTabControl.TabPages.Add(TestTabPage);
#endif
			TestTabPage.TabVisible = true;
			Form.Show();
			Application.DoEvents();

			AssertEquals(1, TestTabPage.Controls.Count);
			AssertEquals(typeof(ZLogsUserControl), TestTabPage.Controls[0].GetType());
		}

		public void TestWithControlStillAddsPlugin()
		{
			Form.Controls.Add(TestTabControl);
#if !WINZOR
			TestTabControl.TabPages.Add(TestTabPage);
#endif
			TestTabPage.Controls.Add(new ZPanel());
			TestTabPage.TabVisible = true;
			Form.Show();
			Application.DoEvents();

			AssertEquals(2, TestTabPage.Controls.Count);
			AssertEquals(typeof(ZLogsUserControl), TestTabPage.Controls[0].GetType());
		}

		#region Determining log types to show

		public void TestLogsToShow_ChangeLogsVisibleOnlyWhenWorkflowTabExists()
		{
			using (TestLogsTabPage logsTabPage = new TestLogsTabPage())
			{
				TestTabControl.TabPages.Clear();

				WorkflowTabPage.SupportsEventTracking = true;

				TestTabControl.TabPages.Add(WorkflowTabPage);
				TestTabControl.TabPages.Add(logsTabPage);
				Form.Controls.Add(TestTabControl);
				Form.Show();
				TestTabControl.SelectedTab = logsTabPage;

				ZLogsUserControl events = (ZLogsUserControl)logsTabPage.Controls[0];
				AssertEquals("Only change logs shown when tracking tab available", LogsToShow.ChangeLogs, events.LogsToShow);
			}
		}

		public void TestLogsToShow_HandleInitialized()
		{
			using (TestLogsTabPage logsTabPage = new TestLogsTabPage())
			{
				TestTabControl.TabPages.Clear();

				WorkflowTabPage.SupportsEventTracking = false;
				WorkflowTabPage.Initialized = false;

				TestTabControl.TabPages.Add(WorkflowTabPage);
				TestTabControl.TabPages.Add(logsTabPage);
				Form.Controls.Add(TestTabControl);
				Form.Show();
				TestTabControl.SelectedTab = logsTabPage;

				ZLogsUserControl events = (ZLogsUserControl)logsTabPage.Controls[0];
				AssertEquals("Only change logs shown when tracking tab available", LogsToShow.All, events.LogsToShow);

				TestTabControl.SelectedTab = WorkflowTabPage;
				TestTabControl.SelectedTab = logsTabPage;
				events = (ZLogsUserControl)logsTabPage.Controls[0];
				AssertEquals("Only change logs shown when tracking tab available", LogsToShow.All, events.LogsToShow);

				WorkflowTabPage.SupportsEventTracking = true;
				WorkflowTabPage.Initialized = true;
				TestTabControl.SelectedTab = WorkflowTabPage;
				TestTabControl.SelectedTab = logsTabPage;
				events = (ZLogsUserControl)logsTabPage.Controls[0];
				AssertEquals("Only change logs shown when tracking tab available", LogsToShow.ChangeLogs, events.LogsToShow);
			}
		}

		public void TestLogsToShow_AllLogTypesVisibleWhenWorkflowTabDoesntSupportEventTracking()
		{
			WorkflowTabPage.SupportsEventTracking = false;
#if !WINZOR
			TestTabControl.TabPages.Add(LogsTabPage);
#endif
			Form.Controls.Add(TestTabControl);
			Form.Show();

			ZLogsUserControl logsUserControl = (ZLogsUserControl)LogsTabPage.Controls[0];
			AssertEquals("All log types shown when no tracking tab", LogsToShow.All, logsUserControl.LogsToShow);
		}

		public void TestLogsToShow_AllLogTypesVisibleWhenWorkflowTabNotExists()
		{
#if !WINZOR
			TestTabControl.TabPages.Add(LogsTabPage);
#endif
			Form.Controls.Add(TestTabControl);
			Form.Show();

			ZLogsUserControl logsUserControl = (ZLogsUserControl)LogsTabPage.Controls[0];
			AssertEquals("All log types shown when no tracking tab", LogsToShow.All, logsUserControl.LogsToShow);
		}

		#endregion

		#region Test Classes

		class TestLogsTabPage : ZLogsTabPage
		{
		}

		class TestWorkflowTabPage : ZTabPage, IWorkflowTabPage
		{
			public bool SupportsEventTracking
			{
				get { return supportsEventTracking; }
				set { supportsEventTracking = value; }
			}
			bool supportsEventTracking;

			bool IWorkflowTabPage.SupportsEventTracking
			{
				get { return SupportsEventTracking; }
			}

			[DefaultValue(true)]
			public bool Initialized { get; set; }
		}

		#endregion

		#region Implementation

		protected override int ExpectedDefaultTabIconIndex
		{
			get { return Icons.GetImageIndex(IconTypes.Events); }
		}

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyEnterpriseBusinessObject); }
		}

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(Dummy);
				}
				return form;
			}
		}
		ZForm form;

		TestWorkflowTabPage WorkflowTabPage
		{
			get
			{
				if (workflowTabPage == null)
				{
					workflowTabPage = new TestWorkflowTabPage();
				}
				return workflowTabPage;
			}
		}
		TestWorkflowTabPage workflowTabPage;

		TestLogsTabPage LogsTabPage
		{
			get { return TestTabPage; }
		}

		new TestLogsTabPage TestTabPage
		{
			get { return (TestLogsTabPage)base.TestTabPage; }
		}

		protected override ZTabPage NewTabPage()
		{
			return new TestLogsTabPage();
		}

		protected override ZTabControl NewTabControl()
		{
			return new ZTemplateTabControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (workflowTabPage != null)
			{
				workflowTabPage.Dispose();
			}
		}

		#endregion
	}
}
