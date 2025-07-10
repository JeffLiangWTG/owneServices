using System;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;
public class InvertVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is Visibility)
		{
			return (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		}

		return DependencyProperty.UnsetValue;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return Convert(value, targetType, parameter, culture);
	}
}
