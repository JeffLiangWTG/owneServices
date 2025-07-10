using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebPrintNudgeEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited => typeof(WebPrintNudgeRegistryDataType);
	}

	public static class WebPrintNudgeDefaultValue
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int SwtichBackToIPAddressIntervalInHours = 24;
	}

	[Serializable]
	public class WebPrintNudge
	{
		public DateTime ChangingToUrlAddressDateTimeUtc { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int SwtichBackToIPAddressIntervalInHours { get; set; }

		public bool EnableIPAddress { get; set; }
	}
}
