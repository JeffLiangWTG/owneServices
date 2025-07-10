using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	class TextFormatterTest : TestCase
	{
		public void TestFormatHoursToTimeString()
		{
			AssertEquals("1:00", 1m.FormatHoursToTimeString());
			AssertEquals("0:00", 0m.FormatHoursToTimeString());
			AssertEquals("0:06", .1m.FormatHoursToTimeString());
			AssertEquals("2:30", 2.5m.FormatHoursToTimeString());
			AssertEquals("10:15", 10.25m.FormatHoursToTimeString());
		}
	}
}
