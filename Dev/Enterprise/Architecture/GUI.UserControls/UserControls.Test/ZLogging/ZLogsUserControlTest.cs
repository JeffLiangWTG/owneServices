using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZLogsUserControlTest : TestCaseWithFactory
	{
		public void TestControl()
		{
			DataRegistry.Instance.UserEventTrackingEnterprise = true;

			LogsControl.LogsToShow = LogsToShow.All;
			Form.Controls.Add(LogsControl);
			LogsControl.SetDataBinding(Dummy, "");
			Form.Show();
			Application.DoEvents();

			ZTabControl tabControl = (ZTabControl)LogsControl.Controls[0];
			AssertEquals(typeof(KSplitContainer), tabControl.TabPages[0].Controls[0].GetType());
			AssertEquals(typeof(ZActivityLoggingUserControl), tabControl.TabPages[1].Controls[0].GetType());
			var kSplitContainer = ((KSplitContainer)LogsControl.ChangeLogsTabPage.Controls[0]);
			AssertEquals(LogsToShow.All, ((ZStmALogUserControl)kSplitContainer.Panel1.Controls[0]).LogsToShow);
		}

		public void TestLogsToShow()
		{
			Form.Controls.Add(LogsControl);
			Form.Show();
			Application.DoEvents();

			LogsControl.LogsToShow = LogsToShow.All;
			AssertEquals(LogsToShow.All, LogsControl.ChangeLogsTabPage.LogsToShow);

			LogsControl.LogsToShow = LogsToShow.ChangeLogs;
			AssertEquals(LogsToShow.ChangeLogs, LogsControl.ChangeLogsTabPage.LogsToShow);

			LogsControl.LogsToShow = LogsToShow.Operations;
			AssertEquals(LogsToShow.Operations, LogsControl.ChangeLogsTabPage.LogsToShow);
		}

		#region TestActivityLogsTabPageVisibility

		public void TestActivityLogsTabPageVisibility()
		{
			AssertActivityLogsTabPageVisibility(true, true, true);
			AssertActivityLogsTabPageVisibility(true, false, true);
			AssertActivityLogsTabPageVisibility(false, true, true);
			AssertActivityLogsTabPageVisibility(false, false, false);
		}

		void AssertActivityLogsTabPageVisibility(bool log1, bool log2, bool expectedVisibility)
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = log1;
			EnvProxy.Instance.Registry.UserEventTrackingExternal = log2;
			using (ZLogsUserControl logsUserControl = new ZLogsUserControl())
			{
				AssertEquals(expectedVisibility, logsUserControl.ActivityLogsTabPage.TabVisible);
			}
		}

		#endregion

		#region Implementation

		DummyEnterpriseBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>()); }
		}
		DummyEnterpriseBusinessObject dummy;

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

		ZLogsUserControl LogsControl
		{
			get
			{
				if (logsControl == null)
				{
					logsControl = new ZLogsUserControl();
				}
				return logsControl;
			}
		}
		ZLogsUserControl logsControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (logsControl != null)
			{
				logsControl.Dispose();
			}
		}

		#endregion
	}
}
