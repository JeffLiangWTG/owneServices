using System.Collections.Generic;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Core.Testing
{
	sealed class DimensionTest : TestCase
	{
		public void TestCodes()
		{
			AssertContainsExactElementsInAnyOrder("expected Dimension units",
				new[]
				{
					"MM",
					"CM",
					"M",
					"IN",
					"FT",
					"YD"
				},
				Dimension.Codes);
		}

		public void TestContainsCode()
		{
			CombineAssertions(() =>
			{
				foreach (var code in Dimension.Codes)
				{
					Assert($"expected Dimension to contain {code}", Dimension.ContainsCode(code));
				}
			});

			var invalidCodes = new[]
			{
				Distance.NauticalMiles,
				Area.SquareFoot,
				Volume.CubicFeet,
				Weight.Kilograms
			};

			CombineAssertions(() =>
			{
				foreach (var code in invalidCodes)
				{
					Assert($"expected Dimension not to contain {code}", !Dimension.ContainsCode(code));
				}
			});
		}

		public void TestConvert()
		{
			CombineAssertions(() =>
			{
				AssertConvert(1000, Dimension.Millimetres, Dimension.Metres, 1m);
				AssertConvert(1, Dimension.Metres, Dimension.Millimetres, 1000m);

				AssertConvert(10, Dimension.Metres, Dimension.Feet, 32.808399m);
				AssertConvert(10, Dimension.Feet, Dimension.Metres, 3.048m);

				AssertConvert(10, Dimension.Yards, Dimension.Feet, 30m);
				AssertConvert(10, Dimension.Yards, Dimension.Metres, 9.144m);
			});

			AssertEquals("ConvertSafe 10M to KG", 0m, Dimension.ConvertSafe(10, Dimension.Metres, Weight.Kilograms));

			AssertExceptionThrown<KeyNotFoundException>("Convert 10M to KG", null,
				() => Dimension.Convert(10, Dimension.Metres, Weight.Kilograms));
		}

		void AssertConvert(decimal fromValue, string fromUnit, string toUnit, decimal expectedValue)
		{
			var convertedValue = Dimension.Convert(fromValue, fromUnit, toUnit);
			AssertEquals($"expected conversion of {fromValue}{fromUnit} to {toUnit}", expectedValue, convertedValue);
		}

		public void TestConvertSafe()
		{
			AssertNoExceptionThrown(() =>
				Dimension.ConvertSafe(10, Dimension.Yards, Weight.Kilograms));
		}

		public void TestDescription_NonPlural()
		{
			AssertEquals("Meter", Distance.GetDescription(Dimension.Metres, PluralState.NonPlural));
			AssertEquals("Foot", Distance.GetDescription(Dimension.Feet, PluralState.NonPlural));
			AssertEquals("Yard", Distance.GetDescription(Dimension.Yards, PluralState.NonPlural));
		}

		public void TestDescription_Plural()
		{
			AssertEquals("Meters", Distance.GetDescription(Dimension.Metres, PluralState.Plural));
			AssertEquals("Feet", Distance.GetDescription(Dimension.Feet, PluralState.Plural));
			AssertEquals("Yards", Distance.GetDescription(Dimension.Yards, PluralState.Plural));
		}

		public void TestDescription_PluralOrNonPlural()
		{
			AssertEquals("Meter(s)", Distance.GetDescription(Dimension.Metres, PluralState.PluralOrNonPlural));
			AssertEquals("Feet", Distance.GetDescription(Dimension.Feet, PluralState.PluralOrNonPlural));
			AssertEquals("Yard(s)", Distance.GetDescription(Dimension.Yards, PluralState.PluralOrNonPlural));
		}
	}
}
