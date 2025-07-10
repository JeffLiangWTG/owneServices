using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;

public class StringToVisibilityConverter : IValueConverter
{
	public bool IsCollapsed { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType != typeof(Visibility))
		{
			throw new InvalidOperationException("The target must be a Visibility");
		}

		if ((value ?? string.Empty) is not string strValue)
		{
			throw new InvalidOperationException("The source must be a string");
		}

		if (!string.IsNullOrEmpty(strValue))
		{
			return Visibility.Visible;
		}

		return IsCollapsed ? Visibility.Collapsed : Visibility.Hidden;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
