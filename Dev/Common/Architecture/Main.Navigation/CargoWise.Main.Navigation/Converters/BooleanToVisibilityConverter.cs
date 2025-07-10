using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
	public bool IsCollapsed { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType != typeof(Visibility))
		{
			throw new InvalidOperationException("The target must be a Visibility");
		}

		if (value is not bool bValue)
		{
			throw new InvalidOperationException("The source must be a bool");
		}

		if (bValue)
		{
			return Visibility.Visible;
		}

		return IsCollapsed ? Visibility.Collapsed : Visibility.Hidden;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType != typeof(bool))
		{
			throw new InvalidOperationException("The target must be a bool");
		}

		if (value is Visibility visibility)
		{
			return visibility == Visibility.Visible;
		}

		throw new InvalidOperationException("The source must be a Visibility");
	}
}
