using System;
using System.Globalization;
using System.Windows.Data;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class BooleanInverterConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value);
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value);
		}

		static bool Convert(object value)
		{
			return !System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
		}
	}
}
