using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class DeleteExpiredRatesEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited => typeof(WebPrintNudgeRegistryDataType);
	}

	public static class DeleteExpiredRatesDefaultValue
	{
		public const int ExpiredRatesPeriodInYears = 0;
		public const int BatchSize = 100;
	}

	[Serializable]
	public class DeleteExpiredRates
	{
		public int ExpiredRatesPeriodInYears { get; set; }
		public int BatchSize { get; set; }
	}
}
