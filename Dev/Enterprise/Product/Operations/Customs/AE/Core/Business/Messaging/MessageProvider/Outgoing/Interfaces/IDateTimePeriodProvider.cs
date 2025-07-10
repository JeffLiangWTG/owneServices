namespace Enterprise.Customs.AE.Business;

public interface IDateTimePeriodProvider
{
	string DateTimePeriodFunctionCode { get; }

	string DateTimePeriodText { get; }

	string DateTimePeriodFormat { get; }
}
