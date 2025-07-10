
using System;

namespace CargoWise.Types
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public interface IZDate : IZType
	{
		IZDate Invalid { get; }
		IZDate Now { get; }
		IZDate Today { get; }
		int Year { get; }
		string SqlFormat { get; }
		IZDate FirstDayOfLastCalendarYear { get; }
		IZDate LastDayOfLastCalendarYear { get; }
		IZDate AddDays(int days);
		IZDate AddMonths(int months);
		IZDate AddYears(int years);
		string ToISO8601String();
		string ToString(string format, IFormatProvider formatProvider);
		bool TryParse(string unparsedValue, out IZDate result);
		object ToBaseDate();
		IZDate EndOfDay();
	}
}
