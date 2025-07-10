using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmALogTabPageTest : ZTabPageControlTest
	{
		public void TestExcludeFromBindingOnSave()
		{
			AssertEquals("For performance don't bind on save", true, TestTabPage.ExcludeFromBindingOnSave);
		}

		public void TestHostedStmALogUserControl()
		{
			TestTabPage.SetDataBindingCore(Dummy, "");
			AssertEquals(1, TestTabPage.Controls.Count);
			AssertEquals(typeof(KSplitContainer), TestTabPage.Controls[0].GetType());
		}

		#region Test Classes

		class TestStmALogTabPage : ZStmALogTabPage
		{
			public new void SetDataBindingCore(object dataSource, string dataMember)
			{
				base.SetDataBindingCore(dataSource, dataMember);
			}
		}

		#endregion

		#region Implementation

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyEnterpriseBusinessObject); }
		}

		new TestStmALogTabPage TestTabPage
		{
			get { return (TestStmALogTabPage)base.TestTabPage; }
		}

		protected override ZTabPage NewTabPage()
		{
			return new TestStmALogTabPage();
		}

		protected override ZTabControl NewTabControl()
		{
			return new ZTemplateTabControl();
		}

		#endregion
	}
}
