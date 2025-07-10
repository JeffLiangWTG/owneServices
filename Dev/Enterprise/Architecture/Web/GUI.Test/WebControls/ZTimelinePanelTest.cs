using System.Data;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTimelinePanelTest : WebControlTest
	{
		#region setup

		protected override Control GetNewControl()
		{
			return new ZTimelinePanel();
		}

		#endregion

		public void TestBindTo()
		{
			ZTimelinePanel testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";
			AssertEquals("CssClass should be empty", "", testPanel.CssClass);

			//no estimate and actual
			TestBizO.Z0_SmallDateTime = ZDateTime.Empty;
			TestBizO.Z0_Date = ZDateTime.Empty;
			testPanel.Bind(TestBizO);

			AssertEquals("&nbsp;", testPanel.ContentsLabel.Text);
			AssertEquals("Estimated time not set", testPanel.ToolTip);
			AssertEquals(System.Drawing.Color.Empty, testPanel.BackColor);

			//no actual, estimate set to the future, Pending
			testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";

			var pendingDate = ZDateTime.Now.AddDays(1);
			TestBizO.Z0_SmallDateTime = pendingDate;
			TestBizO.Z0_Date = ZDateTime.Empty;
			testPanel.Bind(TestBizO);

			var expectedDate = SuppressUtil.GetFormattedDate(pendingDate, testPanel.DateTimeFormat);
			AssertEquals(expectedDate, testPanel.ContentsLabel.Text);
			AssertEquals(
			$@"Estimated: {expectedDate}
Pending", testPanel.ToolTip);
			AssertEquals(TimelineHelper.PendingClass, testPanel.CssClass);

			//no estimate, actual is set, Completed
			testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";

			TestBizO.Z0_SmallDateTime = ZDateTime.Empty;
			TestBizO.Z0_Date = new ZDateTime(2005, 12, 05);
			testPanel.Bind(TestBizO);

			AssertEquals("05-Dec-05", testPanel.ContentsLabel.Text);
			AssertEquals(
			@"Estimated time not set
Actual: 05-Dec-05
Completed", testPanel.ToolTip);
			AssertEquals(TimelineHelper.CompletedClass, testPanel.CssClass);

			//estimate > actual, Completed
			testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";

			TestBizO.Z0_SmallDateTime = new ZDateTime(2005, 12, 05);
			TestBizO.Z0_Date = TestBizO.Z0_SmallDateTime.AddDays(-1);
			testPanel.Bind(TestBizO);

			AssertEquals("04-Dec-05", testPanel.ContentsLabel.Text);
			AssertEquals(
			@"Estimated: 05-Dec-05
Actual: 04-Dec-05
Completed", testPanel.ToolTip);
			AssertEquals(TimelineHelper.CompletedClass, testPanel.CssClass);

			//estimate = actual, Completed
			testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";

			TestBizO.Z0_SmallDateTime = new ZDateTime(2005, 12, 05);
			TestBizO.Z0_Date = new ZDateTime(2005, 12, 05);
			testPanel.Bind(TestBizO);

			AssertEquals("05-Dec-05", testPanel.ContentsLabel.Text);
			AssertEquals(
			@"Estimated: 05-Dec-05
Actual: 05-Dec-05
Completed", testPanel.ToolTip);
			AssertEquals(TimelineHelper.CompletedClass, testPanel.CssClass);

			//both estimate and actual set, actual > cestimated,  Completed Late
			testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";

			TestBizO.Z0_SmallDateTime = new ZDateTime(2005, 12, 05);
			TestBizO.Z0_Date = TestBizO.Z0_SmallDateTime.AddDays(1);
			testPanel.Bind(TestBizO);

			AssertEquals("06-Dec-05", testPanel.ContentsLabel.Text);
			AssertEquals(
			@"Estimated: 05-Dec-05
Actual: 06-Dec-05
Completed Late", testPanel.ToolTip);
			AssertEquals(TimelineHelper.CompletedLateClass, testPanel.CssClass);

			//estimate is set in the past, actual is not set, Overdue
			testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";

			TestBizO.Z0_SmallDateTime = new ZDateTime(2005, 12, 05);
			TestBizO.Z0_Date = ZDateTime.Empty;
			testPanel.Bind(TestBizO);

			AssertEquals("05-Dec-05", testPanel.ContentsLabel.Text);
			AssertEquals(
			@"Estimated: 05-Dec-05
Overdue", testPanel.ToolTip);
			AssertEquals(TimelineHelper.OverdueClass, testPanel.CssClass);

			//suppressed
			testPanel = GetNewControl() as ZTimelinePanel;
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToEstimated = "Z0_SmallDateTime";

			TestBizOWrapper wrapper = Factory.New<TestBizOWrapper>();
			wrapper.Z0_SmallDateTime = SuppressUtil.SuppressedDateTime;
			wrapper.Z0_Date = SuppressUtil.SuppressedDateTime;
			testPanel.Bind(wrapper);

			AssertEquals("*SUPPRESSED*", testPanel.ContentsLabel.Text);
			AssertEquals(
			@"Estimated: *SUPPRESSED*
Actual: *SUPPRESSED*", testPanel.ToolTip);
			AssertEquals(TimelineHelper.NoTimeLineEstimateClass, testPanel.CssClass);
		}

		class TestBizOWrapper : DummyBusinessObject
		{
			public TestBizOWrapper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZDateTime Z0_Date
			{
				get { return fZ0_Date; }
				set { fZ0_Date = value; }
			}
			ZDateTime fZ0_Date;

			public override ZDateTime Z0_SmallDateTime
			{
				get { return fZ0_SmallDateTime; }
				set { fZ0_SmallDateTime = value; }
			}
			ZDateTime fZ0_SmallDateTime;
		}
	}
}
