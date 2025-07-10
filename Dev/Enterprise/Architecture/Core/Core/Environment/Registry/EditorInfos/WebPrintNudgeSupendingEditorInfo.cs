using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebPrintNudgeSuspendingEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited => typeof(WebPrintNudgeSuspendingRegistryDataType);
	}

	[Serializable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public class WebPrintNudgeSuspending
	{
		public int MaxErrorsInMinutes { get; set; }
		public int IntervalMinutes { get; set; }
		public int SuspendMinutes { get; set; }

		public int MaxErrorsInHours { get; set; }
		public int IntervalHours { get; set; }
		public int SuspendHours { get; set; }
	}
}
