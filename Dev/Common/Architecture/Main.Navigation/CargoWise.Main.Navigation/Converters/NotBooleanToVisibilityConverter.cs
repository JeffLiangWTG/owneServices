using System;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;
public class NotBooleanToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is bool notVisible)
		{
			return notVisible ? Visibility.Collapsed : Visibility.Visible;
		}

		return Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is Visibility)
		{
			return (Visibility)value != Visibility.Visible;
		}

		return false;
	}
}
