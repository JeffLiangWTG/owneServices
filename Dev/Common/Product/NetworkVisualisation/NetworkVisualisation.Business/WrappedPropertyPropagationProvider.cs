using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class WrappedPropertyPropagationProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a property name")]
		public static Dictionary<string, string[]> GetNodeViewModelPropertyMap()
		{
			return new Dictionary<string, string[]>
			{
				{ "AppliedAffinities", new [] { "AppliedAttributesReadableText", "StatusColors" } },
				{ "CanDelete", new [] { "CanDeleteUnderlyingEntity" } },
				{ "CanHaveChildren", new [] { "StatusTextWeight", "TextAlignment" } },
				{ "CanUnlinkEntity", new [] { "DeleteTooltip" } },
				{ "EstimateSummary", new [] { "Estimate" } },
				{ "ExplicitDurationMinutes", new [] { "DurationReadableText" } },
				{ "IsDiagramScaled", new [] { "ShowScheduleDetails" } },
				{ "IsLeafEntity", new [] { "ShowScheduleDetails" }  },
				{ "IsSelected", new [] { "AdditionalDetail" } },
				{ "JobName_ReadOnly", new [] { "JobNameEnabled" } },
				{ "JobNumber", new [] { "JobNumberReadableText" }  },
				{ "Name", new [] { "ToolTip", "DiagramName" } },
				{ "ShowScheduleDetails", new [] { "StartDateReadableText", "StartDateToolTip", "FinishDateReadableText", "FinishDateToolTip" } },
				{ "SupportedActions", new [] { "CanHide", "SupportsDiagramVisualStyles", "SupportsEditEntity", "SupportsShowItems", "SupportsChildEntities" } },
				{ "Status", new [] { "StatusColors", "StatusTextWeight" } },
				{ "StatusDescription", new [] { "StatusTooltip" } },
				{ "LowerBar", new [] { "LowerBarHeight" } },
				{ "Height", new [] { "LowerBarHeight" } }
			};
		}
	}
}
