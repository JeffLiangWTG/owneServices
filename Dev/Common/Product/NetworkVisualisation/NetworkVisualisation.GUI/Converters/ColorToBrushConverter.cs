using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class ColorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return new SolidColorBrush();
			}

			var color = (System.Drawing.Color)value;
			if (color.IsEmpty)
			{
				return new SolidColorBrush();
			}

			return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
