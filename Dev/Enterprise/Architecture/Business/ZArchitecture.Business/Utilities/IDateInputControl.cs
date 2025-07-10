namespace Enterprise.Core.Forms
{
	public interface IDateInputControl
	{
		bool AutoCompleteYear { get; }
		int AutoCompleteMonthThreshold { get; }
		string FormatString { get; }
	}
}
