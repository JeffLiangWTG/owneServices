using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZActivityLoggingUserControlTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestShowControl()
		{
			Form.Controls.Add(ActivityLoggingControl);
			Form.Show();
			Application.DoEvents();
		}

		#region Test Classes

		class TestActivityLoggingUserControl : ZActivityLoggingUserControl
		{
			public new ZButton FindButton
			{
				get { return base.FindButton; }
			}

			public new ZButton ClearButton
			{
				get { return base.ClearButton; }
			}
		}

		#endregion

		#region Implementation

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(ActivityLogFilterProvider);
				}
				return form;
			}
		}
		ZForm form;

		StmActivityLogCollectionByParent ActivityLogs
		{
			get
			{
				if (activityLogs == null)
				{
					activityLogs = new StmActivityLogCollectionByParent(Dummy);
					activityLogs.Load();
				}
				return activityLogs;
			}
		}
		StmActivityLogCollectionByParent activityLogs;

		StmActivityLogFilterProvider ActivityLogFilterProvider
		{
			get
			{
				if (activityLogFilterProvider == null)
				{
					activityLogFilterProvider = new StmActivityLogFilterProvider(ActivityLogs);
				}
				return activityLogFilterProvider;
			}
		}
		StmActivityLogFilterProvider activityLogFilterProvider;

		TestActivityLoggingUserControl ActivityLoggingControl
		{
			get
			{
				if (activityLoggingControl == null)
				{
					activityLoggingControl = new TestActivityLoggingUserControl();
				}
				return activityLoggingControl;
			}
		}
		TestActivityLoggingUserControl activityLoggingControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (activityLoggingControl != null)
			{
				activityLoggingControl.Dispose();
			}
		}

		#endregion
	}
}
