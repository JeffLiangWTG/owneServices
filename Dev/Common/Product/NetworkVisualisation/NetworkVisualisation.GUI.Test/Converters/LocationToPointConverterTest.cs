using System.Windows;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test.Converters
{
	[TestedType(typeof(LocationToPointConverter))]
	public class LocationToPointConverterTest : ValueConverterTestCase<LocationToPointConverter>
	{
		public override void TestConvert()
		{
			var converter = new LocationToPointConverter();
			var location = new Location(14, 7);

			var point = (Point)converter.Convert(location, null, null, null);

			AssertEquals(location.X, point.X);
			AssertEquals(location.Y, point.Y);
		}

		public void TestConvertBack()
		{
			var converter = new LocationToPointConverter();
			var point = new Point(14, 7);

			var location = (Location)converter.ConvertBack(point, null, null, null);

			AssertEquals(point.X, location.X);
			AssertEquals(point.Y, location.Y);
		}
	}
}
