using System;
using System.Globalization;
using System.Windows.Data;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class StatusBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var colors = (NodeColors)value;
			var brush = BrushHelper.ColorsToBrush(colors.colors, colors.angle);
			brush.Opacity = colors.opacity;
			return brush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
