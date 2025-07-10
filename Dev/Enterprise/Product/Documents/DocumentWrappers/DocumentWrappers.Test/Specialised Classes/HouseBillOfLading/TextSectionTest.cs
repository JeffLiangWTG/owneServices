using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	sealed class TextSectionTest : TestCase
	{
		public void TestTextSection()
		{
			TextSection section = new TextSection(20);
			AssertEquals(0, section.Height);
			AssertEquals(0, section.Count);
			AssertEquals(20, section.MaxLineLength);

			section.Add("CargoWise is a world class provider");
			AssertEquals(1, section.Count);
			AssertEquals(2, section.Height);
			AssertEquals("CargoWise is a world class provider", section.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise is a world", "class provider" }, section.ToStringArray());

			section.Add(new TextLine("of high value supply chain management"));
			AssertEquals(2, section.Count);
			AssertEquals(4, section.Height);
			AssertEquals("CargoWise is a world class provider\nof high value supply chain management", section.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise is a world", "class provider", "of high value supply", "chain management" }, section.ToStringArray());

			TextSection section2 = new TextSection(20);
			section2.AddRange(new[] { new TextLine("software for Logistics Service Providers") });
			section.Merge(section2);

			AssertEquals(3, section.Count);
			AssertEquals(7, section.Height);
			AssertEquals("CargoWise is a world class provider\nof high value supply chain management\nsoftware for Logistics Service Providers", section.ToString());
			AssertArrayEqualsByElements(new ZString[]
			{
				"CargoWise is a world",
				"class provider",
				"of high value supply",
				"chain management",
				"software for",
				"Logistics Service",
				"Providers"
			}, section.ToStringArray());
		}

		public void TestMaxLineLength()
		{
			TextSection section = new TextSection(10);
			section.Add("CargoWise is a world class provider");
			AssertEquals(1, section.Count);
			AssertEquals(4, section.Height);
			AssertEquals("CargoWise is a world class provider", section.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise", "is a world", "class", "provider" }, section.ToStringArray());

			section.MaxLineLength = 20;
			AssertEquals(1, section.Count);
			AssertEquals(2, section.Height);
			AssertEquals("CargoWise is a world class provider", section.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise is a world", "class provider" }, section.ToStringArray());

			section.MaxLineLength = 40;
			AssertEquals(1, section.Count);
			AssertEquals(1, section.Height);
			AssertEquals("CargoWise is a world class provider", section.ToString());
			AssertArrayEqualsByElements(new ZString[] { "CargoWise is a world class provider" }, section.ToStringArray());
		}
	}
}
