using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	[ValueConversion(typeof(Location), typeof(Point))]
	public class LocationToPointConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var location = (Location)value;
			return new Point(location.X, location.Y);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var point = (Point)value;
			return new Location(point.X, point.Y);
		}
	}
}
