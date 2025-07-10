using System;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;
public class DrawingToMediaColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value != null && value is System.Drawing.Color)
		{
			var color = (System.Drawing.Color)value;
			return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		return System.Windows.Media.Colors.White;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value != null && value is System.Windows.Media.Color)
		{
			var color = (System.Windows.Media.Color)value;
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		return System.Drawing.Color.White;
	}
}
