namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public enum DocEngineDatePickerFormats { Long, Short, YearAndMonth }

	internal interface IDateFormatSupport
	{
		DocEngineDatePickerFormats PickerFormat { get; set; }
	}
}
