namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZActivityLoggingTabPageTest : ZTabPageControlTest
	{
		public void TestText()
		{
			AssertEquals("Activity Logs", "Activity Logs");
		}

		public void TestExcludeFromBindingOnSave()
		{
			AssertEquals("For performance don't bind on save", true, TestTabPage.ExcludeFromBindingOnSave);
		}

		public void TestHostedActivityLogsControl()
		{
			TestTabPage.SetDataBindingCore(Dummy, "");
			AssertEquals(1, TestTabPage.Controls.Count);
			AssertEquals(typeof(ZActivityLoggingUserControl), TestTabPage.Controls[0].GetType());
		}

		#region Test Classes

		class TestActivityLoggingTabPage : ZActivityLoggingTabPage
		{
			public new void SetDataBindingCore(object dataSource, string dataMember)
			{
				base.SetDataBindingCore(dataSource, dataMember);
			}
		}

		#endregion
		#region Implementation
		readonly ZForm form;

		new TestActivityLoggingTabPage TestTabPage
		{
			get { return (TestActivityLoggingTabPage)base.TestTabPage; }
		}

		protected override ZTabPage NewTabPage()
		{
			return new TestActivityLoggingTabPage();
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
		}

		#endregion
	}
}
