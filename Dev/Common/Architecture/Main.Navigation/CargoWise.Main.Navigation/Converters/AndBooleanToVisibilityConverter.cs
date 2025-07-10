using System;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;

public class AndBooleanToVisibilityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (values != null && values.Length == 2)
		{
			if (values[0] is bool && values[1] is bool)
			{
				return (bool)values[0] && (bool)values[1] ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		return Visibility.Collapsed;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
	{
		return new[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
	}
}
