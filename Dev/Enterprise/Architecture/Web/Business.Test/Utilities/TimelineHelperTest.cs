using System;
using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Business.Utilities.Testing
{
	sealed class TimelineHelperTest : TestCaseWithFactory
	{
		void AssertGetProcessTaskWebStatus(ZDateTimeOffset actualDate, ZDateTimeOffset estimatedDate, string expectedStatus, string expectedModifiedStatus = null)
		{
			AssertEquals(expectedModifiedStatus ?? expectedStatus, TimelineHelper.GetProcessTaskWebStatus(actualDate, estimatedDate));
			AssertEquals(expectedStatus, TimelineHelper.GetProcessTaskWebStatus(actualDate, estimatedDate, false));
		}

		public void TestGetProcessTaskWebStatus()
		{
			var date1 = new ZDateTimeOffset(new ZDateTime(2009, 1, 1));
			var date2 = new ZDateTimeOffset(new ZDateTime(2009, 2, 2));

			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.All))
			{
				AssertGetProcessTaskWebStatus(date1, date2, "Completed");
				AssertGetProcessTaskWebStatus(date2, date1, "Completed Late");
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), "Overdue");
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), "Pending");
			}

			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedAndPendingOnly))
			{
				AssertGetProcessTaskWebStatus(date1, date2, "Completed");
				AssertGetProcessTaskWebStatus(date2, date1, "Completed Late", "Completed");
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), "Overdue", "Pending");
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), "Pending");
			}

			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedOnly))
			{
				AssertGetProcessTaskWebStatus(date1, date2, "Completed");
				AssertGetProcessTaskWebStatus(date2, date1, "Completed Late", "Completed");
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), "Overdue", ZString.Empty);
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), "Pending", ZString.Empty);
			}

			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None))
			{
				AssertGetProcessTaskWebStatus(date1, date2, "Completed", ZString.Empty);
				AssertGetProcessTaskWebStatus(date2, date1, "Completed Late", ZString.Empty);
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), "Overdue", ZString.Empty);
				AssertGetProcessTaskWebStatus(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), "Pending", ZString.Empty);
			}
		}

		public void TestGetComment()
		{
			AssertEquals("Precondition: default registry value", MilestoneStatusVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneStatusVisibility.Value);

			var date1 = new ZDateTimeOffset(new ZDateTime(2009, 1, 1));
			var date2 = new ZDateTimeOffset(new ZDateTime(2009, 2, 2));

			AssertEquals(
			@"Estimated: 02-Feb-09
Actual: 01-Jan-09
Completed", TimelineHelper.GetComment(date1, date2, ZDateTimePickerFormat.Short));
			AssertEquals(
			@"Estimated: 01-Jan-09
Actual: 02-Feb-09
Completed Late", TimelineHelper.GetComment(date2, date1, ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(
			@"Estimated: {0}
Overdue", ZDateTime.Now.AddDays(-1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(
			@"Estimated: {0}
Pending", ZDateTime.Now.AddDays(1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), ZDateTimePickerFormat.Short));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedAndPendingOnly);

			AssertEquals(
			@"Estimated: 02-Feb-09
Actual: 01-Jan-09
Completed", TimelineHelper.GetComment(date1, date2, ZDateTimePickerFormat.Short));
			AssertEquals(
			@"Estimated: 01-Jan-09
Actual: 02-Feb-09
Completed", TimelineHelper.GetComment(date2, date1, ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(
			@"Estimated: {0}
Pending", ZDateTime.Now.AddDays(-1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(
			@"Estimated: {0}
Pending", ZDateTime.Now.AddDays(1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), ZDateTimePickerFormat.Short));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedOnly);

			AssertEquals(
			@"Estimated: 02-Feb-09
Actual: 01-Jan-09
Completed", TimelineHelper.GetComment(date1, date2, ZDateTimePickerFormat.Short));
			AssertEquals(
			@"Estimated: 01-Jan-09
Actual: 02-Feb-09
Completed", TimelineHelper.GetComment(date2, date1, ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(@"Estimated: {0}", ZDateTime.Now.AddDays(-1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(@"Estimated: {0}", ZDateTime.Now.AddDays(1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), ZDateTimePickerFormat.Short));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None);

			AssertEquals(
			@"Estimated: 02-Feb-09
Actual: 01-Jan-09", TimelineHelper.GetComment(date1, date2, ZDateTimePickerFormat.Short));
			AssertEquals(
			@"Estimated: 01-Jan-09
Actual: 02-Feb-09", TimelineHelper.GetComment(date2, date1, ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(@"Estimated: {0}", ZDateTime.Now.AddDays(-1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1), ZDateTimePickerFormat.Short));
			AssertEquals(string.Format(@"Estimated: {0}", ZDateTime.Now.AddDays(1).ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture)), TimelineHelper.GetComment(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1), ZDateTimePickerFormat.Short));
		}

		public void TestGetColor()
		{
			AssertEquals("Precondition: default registry value", MilestoneStatusVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneStatusVisibility.Value);

			var date1 = new ZDateTimeOffset(new ZDateTime(2009, 1, 1));
			var date2 = new ZDateTimeOffset(new ZDateTime(2009, 2, 2));

			AssertEquals(Color.FromArgb(255, 204, 204, 255), TimelineHelper.GetColor(date1, date2));
			AssertEquals(Color.Pink, TimelineHelper.GetColor(date2, date1));
			AssertEquals(Color.LightGray, TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals(Color.FromArgb(255, 204, 255, 204), TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedAndPendingOnly);

			AssertEquals(Color.FromArgb(255, 204, 204, 255), TimelineHelper.GetColor(date1, date2));
			AssertEquals(Color.FromArgb(255, 204, 204, 255), TimelineHelper.GetColor(date2, date1));
			AssertEquals(Color.FromArgb(255, 204, 255, 204), TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals(Color.FromArgb(255, 204, 255, 204), TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedOnly);

			AssertEquals(Color.FromArgb(255, 204, 204, 255), TimelineHelper.GetColor(date1, date2));
			AssertEquals(Color.FromArgb(255, 204, 204, 255), TimelineHelper.GetColor(date2, date1));
			AssertEquals(Color.White, TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals(Color.White, TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None);

			AssertEquals(Color.White, TimelineHelper.GetColor(date1, date2));
			AssertEquals(Color.White, TimelineHelper.GetColor(date2, date1));
			AssertEquals(Color.White, TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals(Color.White, TimelineHelper.GetColor(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));
		}

		public void TestGetCSS()
		{
			AssertEquals("Precondition: default registry value", MilestoneStatusVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneStatusVisibility.Value);

			var date1 = new ZDateTimeOffset(new ZDateTime(2009, 1, 1));
			var date2 = new ZDateTimeOffset(new ZDateTime(2009, 2, 2));

			AssertEquals("TimeLineCompleted", TimelineHelper.GetCSS(date1, date2));
			AssertEquals("TimeLineCompletedLate", TimelineHelper.GetCSS(date2, date1));
			AssertEquals("TimeLineOverdue", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals("TimeLinePending", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedAndPendingOnly);

			AssertEquals("TimeLineCompleted", TimelineHelper.GetCSS(date1, date2));
			AssertEquals("TimeLineCompleted", TimelineHelper.GetCSS(date2, date1));
			AssertEquals("TimeLinePending", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals("TimeLinePending", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedOnly);

			AssertEquals("TimeLineCompleted", TimelineHelper.GetCSS(date1, date2));
			AssertEquals("TimeLineCompleted", TimelineHelper.GetCSS(date2, date1));
			AssertEquals("NoTimeLineEstimate", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals("NoTimeLineEstimate", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None);

			AssertEquals("NoTimeLineEstimate", TimelineHelper.GetCSS(date1, date2));
			AssertEquals("NoTimeLineEstimate", TimelineHelper.GetCSS(date2, date1));
			AssertEquals("NoTimeLineEstimate", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(-1)));
			AssertEquals("NoTimeLineEstimate", TimelineHelper.GetCSS(ZDateTimeOffset.Empty, ZDateTimeOffset.Now.AddDays(1)));
		}
	}
}
