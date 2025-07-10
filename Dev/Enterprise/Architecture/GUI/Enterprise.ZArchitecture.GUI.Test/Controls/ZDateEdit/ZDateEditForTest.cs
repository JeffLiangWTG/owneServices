using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateEditForTest : ZDateEdit
	{
		internal bool AreSizeAndFormatOnSync()
		{
			return ((FormatString == DateTimeFormatStrings.ShortDateFormat && Width == ControlDpiScalingHelper.ScaleToCurrentDpiX(DateFormatWidth))
				|| (FormatString == DateTimeFormatStrings.ShortTimeFormat && Width == ControlDpiScalingHelper.ScaleToCurrentDpiX(TimeFormatWidth))
				|| (FormatString == DateTimeFormatStrings.LongTimeFormat && Width == ControlDpiScalingHelper.ScaleToCurrentDpiX(DateTimeFormatWidth))
				|| (FormatString == DateTimeFormatStrings.LongTimeIncludingSecondsFormat && Width == ControlDpiScalingHelper.ScaleToCurrentDpiX(DateTimeFormatIncludingSecondsWidth))
				|| (FormatString == DateTimeFormatStrings.ShortTimeIncludingSecondsFormat && Width == ControlDpiScalingHelper.ScaleToCurrentDpiX(TimeFormatIncludingSecondsWidth)));
		}

		internal static int DateTimeFormatWidthExposed
		{
			get { return DateTimeFormatWidth; }
		}
	}
}
