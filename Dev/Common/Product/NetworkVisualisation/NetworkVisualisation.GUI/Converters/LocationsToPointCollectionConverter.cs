using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	[ValueConversion(typeof(IEnumerable<Location>), typeof(PointCollection))]
	public class LocationsToPointCollectionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var locations = (IEnumerable<Location>)value;
			var collection = new PointCollection();
			locations.ForEach(location =>
			{
				collection.Add(new Point(location.X, location.Y));
			});
			return collection;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var collection = (PointCollection)value;
			return collection.Select(p => new Location(p.X, p.Y));
		}
	}
}
