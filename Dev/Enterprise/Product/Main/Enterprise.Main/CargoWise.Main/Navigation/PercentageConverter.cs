#if !WINZOR
using System;
using System.Globalization;
using System.Windows.Data;

namespace CargoWise.Main.Navigation;

internal class PercentageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is double actualHeight && parameter is string percentageString && double.TryParse(percentageString, out double percentage))
		{
			if (percentage <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(parameter), "Percentage cannot be negative.");
			}
			return actualHeight * percentage;
		}
		return 0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
#endif
