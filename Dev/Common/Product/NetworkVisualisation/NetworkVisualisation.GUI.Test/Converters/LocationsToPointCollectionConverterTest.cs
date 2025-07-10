using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test.Converters
{
	[TestedType(typeof(LocationsToPointCollectionConverter))]
	public class LocationsToPointCollectionConverterTest : ValueConverterTestCase<LocationsToPointCollectionConverter>
	{
		public override void TestConvert()
		{
			var converter = new LocationsToPointCollectionConverter();
			var locations = new List<Location>()
			{
				new Location(14, 1),
				new Location(21, 9)
			};

			var collection = (PointCollection)converter.Convert(locations, null, null, null);

			AssertEquals(locations[0].X, collection[0].X);
			AssertEquals(locations[0].Y, collection[0].Y);
			AssertEquals(locations[1].X, collection[1].X);
			AssertEquals(locations[1].Y, collection[1].Y);
		}

		public void TestConvertBack()
		{
			var converter = new LocationsToPointCollectionConverter();
			var collection = new PointCollection()
			{
				new Point(12, 34),
				new Point(56, 78)
			};

			var locations = (IEnumerable<Location>)converter.ConvertBack(collection, null, null, null);
			var locationsList = locations.ToList();

			AssertEquals(collection[0].X, locationsList[0].X);
			AssertEquals(collection[0].Y, locationsList[0].Y);
			AssertEquals(collection[1].X, locationsList[1].X);
			AssertEquals(collection[1].Y, locationsList[1].Y);
		}
	}
}
