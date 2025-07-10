using System;
using System.Globalization;
using System.Windows.Data;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class TooltipTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var stringValue = value as string;
			return string.IsNullOrEmpty(stringValue) ? null : stringValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value as string;
		}
	}
}
