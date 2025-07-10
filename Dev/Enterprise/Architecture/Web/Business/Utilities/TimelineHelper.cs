using System.Drawing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public static class TimelineHelper
	{
		public const string TimeLineLegendClass = "TimeLineLegend";
		public const string TimeLineEstimateClass = "TimeLineEstimate";
		public const string CompletedLateClass = "TimeLineCompletedLate";
		public const string OverdueClass = "TimeLineOverdue";
		public const string CompletedClass = "TimeLineCompleted";
		public const string PendingClass = "TimeLinePending";
		public const string NoTimeLineEstimateClass = "NoTimeLineEstimate";

		public static ZString GetProcessTaskWebStatus(ZDateTimeOffset actualDate, ZDateTimeOffset estimatedDate, bool accordingToRegistry = true)
		{
			if (actualDate != SuppressUtil.SuppressedDateTimeOffset && estimatedDate != SuppressUtil.SuppressedDateTimeOffset)
			{
				var processTaskStatus = ProcessTask.GetStatus(actualDate.ToUtcZDateTime(), estimatedDate.ToUtcZDateTime());

				return accordingToRegistry ? GetStatusAccordingToRegistry(processTaskStatus) : processTaskStatus;
			}

			return ZString.Empty;
		}

		public static ZString GetStatusAccordingToRegistry(string status)
		{
			switch (WebDataRegistry.Instance.MilestoneStatusVisibility.Value)
			{
				case MilestoneStatusVisibilityList.Codes.All:
					return status;

				case MilestoneStatusVisibilityList.Codes.CompletedOnly:
					switch (status)
					{
						case ProcessTask.Completed:
						case ProcessTask.CompletedLate:
							return ProcessTask.Completed;
					}
					break;

				case MilestoneStatusVisibilityList.Codes.CompletedAndPendingOnly:
					switch (status)
					{
						case ProcessTask.Completed:
						case ProcessTask.CompletedLate:
							return ProcessTask.Completed;

						case ProcessTask.Pending:
						case ProcessTask.Overdue:
							return ProcessTask.Pending;
					}
					break;
			}
			return ZString.Empty;
		}

		public static ZString GetComment(ZDateTimeOffset actualValue, ZDateTimeOffset estimatedValue, ZDateTimePickerFormat dateTimeFormat)
		{
			ZStringBuilder comment = new ZStringBuilder();

			if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value == MilestoneDatesVisibilityList.Codes.All)
			{
				comment.AppendIfNotEmpty(estimatedValue.IsValid ? Res.GetString("570ecf74-828e-417c-b525-22fa6f7199d1", "Estimated:") + " " + SuppressUtil.GetFormattedDate(estimatedValue, dateTimeFormat) : Res.GetString("4e5b6575-1bb6-4359-9763-bf2a12a0dbab", "Estimated time not set"));
				comment.AppendIfNotEmpty(actualValue.IsValid ? Res.GetString("68e7b411-9fb9-4cca-aec4-398dabca94b5", "Actual:") + " " + SuppressUtil.GetFormattedDate(actualValue, dateTimeFormat) : string.Empty);
			}
			if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value == MilestoneDatesVisibilityList.Codes.ShowActualDateWithFallbackToEstimated)
			{
				comment.AppendIfNotEmpty(actualValue.IsValid ? Res.GetString("e31467ea-9e9b-4474-9f72-111a13145ef8", "Date:") + " " + SuppressUtil.GetFormattedDate(actualValue, dateTimeFormat) : string.Empty);
			}

			switch (GetProcessTaskWebStatus(actualValue, estimatedValue))
			{
				case ProcessTask.Completed:
					comment.Append(Res.GetString("b4de6627-fedb-41e3-960d-a211f7fd4042", "Completed"));
					break;
				case ProcessTask.CompletedLate:
					comment.Append(Res.GetString("acf6a013-7728-4780-bb44-efd099b8252f", "Completed Late"));
					break;
				case ProcessTask.Pending:
					comment.Append(Res.GetString("930c4131-26ef-49ed-9a62-58074732ce86", "Pending"));
					break;
				case ProcessTask.Overdue:
					comment.Append(Res.GetString("25936a95-8f50-49f1-a642-9505ebdc6120", "Overdue"));
					break;
			}

			return comment.ToStringWithNewLineBetweenAppends();
		}

		public static Color? GetColor(ZDateTimeOffset actualValue, ZDateTimeOffset estimatedValue)
		{
			switch (GetProcessTaskWebStatus(actualValue, estimatedValue))
			{
				case ProcessTask.Completed:
					return Color.FromArgb(255, 204, 204, 255);//.LightCyan;//.LightSkyBlue; //.FromArgb(230,230,250);
				case ProcessTask.CompletedLate:
					return Color.Pink;//.FromArgb(255,182,193);
				case ProcessTask.Pending:
					return Color.FromArgb(255, 204, 255, 204);//Color.Chartreuse;//.FromArgb(152,251,152);
				case ProcessTask.Overdue:
					return Color.LightGray;
			}

			return Color.White;
		}

		public static string GetCSS(ZDateTimeOffset actualValue, ZDateTimeOffset estimatedValue)
		{
			switch (GetProcessTaskWebStatus(actualValue, estimatedValue))
			{
				case ProcessTask.Overdue:
					return OverdueClass;
				case ProcessTask.CompletedLate:
					return CompletedLateClass;
				case ProcessTask.Completed:
					return CompletedClass;
				case ProcessTask.Pending:
					return PendingClass;
			}

			return NoTimeLineEstimateClass;
		}
	}
}
