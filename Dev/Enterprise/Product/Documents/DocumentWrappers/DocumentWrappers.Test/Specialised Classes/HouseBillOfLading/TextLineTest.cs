using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	sealed class TextLineTest : TestCase
	{
		public void TestTextLine()
		{
			var line = new TextLine("CargoWise!");
			AssertEquals(1, line.Height);
			AssertEquals(0, line.MaxLineLength);
			AssertEquals("CargoWise!", line.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise!" }, line.ToStringArray());

			line.MaxLineLength = 5;
			AssertEquals(2, line.Height);
			AssertEquals(5, line.MaxLineLength);
			AssertEquals("CargoWise!", line.ToString());
			AssertArrayEqualsByElements(new ZString[] { "Cargo", "Wise!" }, line.ToStringArray());

			line = new TextLine("CargoWise is a world class provider of high value supply chain management software for Logistics Service Providers");
			AssertEquals(1, line.Height);
			AssertEquals(0, line.MaxLineLength);
			AssertEquals("CargoWise is a world class provider of high value supply chain management software for Logistics Service Providers", line.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise is a world class provider of high value supply chain management software for Logistics Service Providers" }, line.ToStringArray());

			line.MaxLineLength = 20;
			AssertEquals(7, line.Height);
			AssertEquals(20, line.MaxLineLength);
			AssertEquals("CargoWise is a world class provider of high value supply chain management software for Logistics Service Providers", line.ToString());
			AssertArrayEqualsByElements(new ZString[]
			{
				"CargoWise is a world",
				"class provider of",
				"high value supply",
				"chain management",
				"software for",
				"Logistics Service",
				"Providers"
			}, line.ToStringArray());

			line = new TextLine("CargoWise          ");
			line.MaxLineLength = 10;
			AssertEquals(1, line.Height);
			AssertEquals("CargoWise          ", line.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise " }, line.ToStringArray());
		}
	}
}
