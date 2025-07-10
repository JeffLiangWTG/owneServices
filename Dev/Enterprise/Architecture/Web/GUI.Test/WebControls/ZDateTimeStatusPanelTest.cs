using System.Web.UI;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDateTimeStatusPanelTest : WebControlTest
	{
		#region setup

		protected override Control GetNewControl()
		{
			return new ZDateTimeStatusPanel();
		}

		#endregion

		public void TestBindTo()
		{
			AssertDateTimeStatusPanel(ZDateTime.Empty, string.Empty, TimelineHelper.NoTimeLineEstimateClass);

			AssertDateTimeStatusPanel(ZDateTime.Empty, Constants.DateTimeStatus.Overdue, TimelineHelper.OverdueClass);
			AssertDateTimeStatusPanel(ZDateTime.Empty, Constants.DateTimeStatus.Late, TimelineHelper.CompletedLateClass);
			AssertDateTimeStatusPanel(ZDateTime.Empty, Constants.DateTimeStatus.OnTime, TimelineHelper.CompletedClass);

			AssertDateTimeStatusPanel(ZDateTime.Now, string.Empty, TimelineHelper.NoTimeLineEstimateClass);

			AssertDateTimeStatusPanel(ZDateTime.Now, Constants.DateTimeStatus.Overdue, TimelineHelper.OverdueClass);
			AssertDateTimeStatusPanel(ZDateTime.Now, Constants.DateTimeStatus.Late, TimelineHelper.CompletedLateClass);
			AssertDateTimeStatusPanel(ZDateTime.Now, Constants.DateTimeStatus.OnTime, TimelineHelper.CompletedClass);
		}

		void AssertDateTimeStatusPanel(ZDateTime date, string status, string cssClass)
		{
			var testPanel = (ZDateTimeStatusPanel)GetNewControl();
			testPanel.BindTo = "Z0_Date";
			testPanel.BindToStatus = "Z0_Description";
			AssertEquals("CssClass should be empty", "", testPanel.CssClass);

			TestBizO.Z0_Date = date;
			TestBizO.Z0_Description = status;
			testPanel.Bind(TestBizO);

			var expectedText = date.IsEmpty ? "&nbsp;" : date.ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture);
			var expectedToolTip = string.IsNullOrEmpty(status) ? date.ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture) : status;

			AssertEquals("Should be expected text", expectedText, testPanel.ContentsLabel.Text);
			AssertEquals("ToolTip should be as expected", expectedToolTip, testPanel.ToolTip);
			if (string.IsNullOrEmpty(cssClass) || cssClass == TimelineHelper.NoTimeLineEstimateClass)
			{
				AssertEquals("Should be no background color", System.Drawing.Color.Empty, testPanel.BackColor);
			}
			AssertEquals("CssClass should be as expected", cssClass, testPanel.CssClass);
		}
	}
}
