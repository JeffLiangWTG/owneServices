namespace Enterprise.Customs.AE.Business;

public class DateTimePeriodProvider : IDateTimePeriodProvider
{
	public DateTimePeriodProvider(string functionCode, string dateTime, string dateTimeFormat)
	{
		DateTimePeriodFunctionCode = functionCode;
		DateTimePeriodText = dateTime;
		DateTimePeriodFormat = dateTimeFormat;
	}

	public string DateTimePeriodFunctionCode { get; }

	public string DateTimePeriodText { get; }

	public string DateTimePeriodFormat { get; }
}
