using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;

public class EqualityToCollapsedConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values == null || values.Length != 2)
		{
			return Visibility.Visible;
		}

		var value1 = values[0];
		var value2 = values[1];

		if (value1 == null && value2 == null)
		{
			return Visibility.Collapsed;
		}

		if (value1 == null || value2 == null)
		{
			return Visibility.Visible;
		}

		if (value1 is string str1 && value2 is string str2)
		{
			return string.Equals(str1, str2, StringComparison.CurrentCultureIgnoreCase)
				? Visibility.Collapsed
				: Visibility.Visible;
		}

		return Visibility.Visible;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
