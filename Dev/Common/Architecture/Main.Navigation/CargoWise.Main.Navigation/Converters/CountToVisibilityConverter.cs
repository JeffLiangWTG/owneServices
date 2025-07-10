using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;

public class CountToVisibilityConverter : IValueConverter
{
	public bool ShouldInvertResult { get; set; }
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var cutoff = 1; // Default to 1, so that if the parameter is not provided, it will hide the element if there are no items
		if (parameter is string paramAsString && int.TryParse(paramAsString, out var cutOffAsParameter))
		{
			cutoff = cutOffAsParameter;
		}

		if (value is int count)
		{
			return CountToVisibility(count, cutoff);
		}
		else if (value is ICollection valueAsCollection)
		{
			return CountToVisibility(valueAsCollection.Count, cutoff);
		}
		else if (value is Array valueAsArray)
		{
			return CountToVisibility(valueAsArray.Length, cutoff);
		}

		return Visibility.Collapsed;
	}

	Visibility CountToVisibility(int count, int cutoff)
	{
		var shouldCollapse = count < cutoff;
		if (ShouldInvertResult)
		{
			shouldCollapse = !shouldCollapse;
		}

		return shouldCollapse ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
