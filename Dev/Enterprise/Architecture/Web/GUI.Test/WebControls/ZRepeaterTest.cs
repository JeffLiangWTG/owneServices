using System;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZRepeaterTest : WebControlTest
	{
		#region TestOnPreRender

		public void TestOnPreRenderAddsNoDataLabel()
		{
			AssertEquals("PreCondition: Controls should be empty", 0, TestRepeater.Controls.Count);
			TestRepeater.Bind(TestBizO.Collection);
			TestRepeater.OnPreRenderForTesting(EventArgs.Empty);
			AssertEquals("No Data Found Control should have been added", 1, TestRepeater.Controls.Count);
			AssertSame("StatusPanel", TestRepeater.StatusPanel, TestRepeater.Controls[0]);
			AssertEquals("StatusLabel", "No Data Found", TestRepeater.StatusLabel.Text);
		}

		public void TestOnPreRenderDoesNotAddNoDataLabel()
		{
			AssertEquals("PreCondition: Controls should be empty", 0, TestRepeater.Controls.Count);
			TestRepeater.Bind(TestBizO.Collection);

			TestRepeater.HideStatusLabel = true;
			TestRepeater.OnPreRenderForTesting(EventArgs.Empty);
			AssertEquals("StatusLabel should not be added", 0, TestRepeater.Controls.Count);
		}

		#endregion TestOnPreRender

		#region Binding

		public virtual void TestIsBindable()
		{
			AssertEquals("PreCondition: Empty BindTo", "", TestRepeater.BindTo);
			AssertEquals("IsBindable (Empty BindTo and null datasource)", false, TestRepeater.IsBindable(null));
			AssertEquals("IsBindable (Empty BindTo and Not IBusinessObjectCollection)", false, TestRepeater.IsBindable(TestBizO.Z0_Bool));
			AssertEquals("IsBindable (Empty BindTo and IBusinessObjectCollection)", true, TestRepeater.IsBindable(TestBizO.Collection));
			TestRepeater.BindTo = "Collection";
			AssertEquals("IsBindable (BindTo set and non IBusinessObjectCollection)", true, TestRepeater.IsBindable(TestBizO));
		}

		public void TestUnBind()
		{
			AssertNull("PreCondition: null datasource", TestRepeater.DataSource);
			TestRepeater.DataSource = TestBizO.Collection;
			TestRepeater.UnBind();
			AssertNull("Datasource should be nulled", TestRepeater.DataSource);
		}

		#endregion Binding

		ZRepeaterForTest TestRepeater
		{
			get { return Control as ZRepeaterForTest; }
		}

		protected override Control GetNewControl()
		{
			return new ZRepeaterForTest();
		}

		class ZRepeaterForTest : ZRepeater
		{
			#region Test Properties

			public void OnPreRenderForTesting(EventArgs e) => OnPreRender(e);

			#endregion
		}
	}
}
