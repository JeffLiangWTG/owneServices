using System;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;
public class MenuIndexVisibilityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (values != null && values.Length == 3)
		{
			if (values[1] is int)
			{
				var sectionIndex = (int)values[1];
				if (sectionIndex > -1)
				{
					if (values[0] is int selectedIndex && values[2] is int numberOfColumns)
					{
						if (selectedIndex > -1
							&& numberOfColumns > 0)
							{
								return ((selectedIndex - sectionIndex) / numberOfColumns == sectionIndex) || ((sectionIndex + 1) * numberOfColumns + sectionIndex == selectedIndex)
									? Visibility.Visible
									: Visibility.Collapsed;
							}
					}
				}
				else
				{
					return Visibility.Visible;
				}
			}
		}

		return Visibility.Collapsed;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
