using System.Collections.Generic;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Core.Testing
{
	sealed class DistanceTest : TestCase
	{
		public void TestCodes()
		{
			AssertContainsExactElementsInAnyOrder("expected Distance units",
				new[]
				{
					"KM",
					"MI",
					"NM"
				},
				Distance.Codes);
		}

		public void TestContainsCode()
		{
			CombineAssertions(() =>
			{
				foreach (var code in Distance.Codes)
				{
					Assert($"expected Distance to contain {code}", Distance.ContainsCode(code));
				}
			});

			var invalidCodes = new[]
			{
				Length.Millimetres,
				Area.SquareFoot,
				Volume.CubicFeet,
				Weight.Kilograms
			};

			CombineAssertions(() =>
			{
				foreach (var code in invalidCodes)
				{
					Assert($"expected Distance not to contain {code}", !Distance.ContainsCode(code));
				}
			});
		}

		public void TestConvert()
		{
			CombineAssertions(() =>
			{
				AssertConvert(10, Distance.Kilometres, Distance.Miles, 6.213712m);
				AssertConvert(10, Distance.Kilometres, Distance.NauticalMiles, 5.399568m);

				AssertConvert(10, Distance.Miles, Distance.Kilometres, 16.09344m);
				AssertConvert(10, Distance.Miles, Distance.NauticalMiles, 8.689762m);

				AssertConvert(10, Distance.NauticalMiles, Distance.Kilometres, 18.52m);
				AssertConvert(10, Distance.NauticalMiles, Distance.Miles, 11.507794m);
			});

			AssertEquals("ConvertSafe 10NM to KG", 0m, Distance.ConvertSafe(10, Distance.NauticalMiles, Weight.Kilograms));

			AssertExceptionThrown<KeyNotFoundException>("Convert 10NM to KG",
				() => Distance.Convert(10, Distance.NauticalMiles, Weight.Kilograms));
		}

		void AssertConvert(decimal fromValue, string fromUnit, string toUnit, decimal expectedValue)
		{
			var convertedValue = Distance.Convert(fromValue, fromUnit, toUnit);
			AssertEquals($"expected conversion of {fromValue}{fromUnit} to {toUnit}", expectedValue, convertedValue);
		}

		public void TestConvertSafe()
		{
			AssertNoExceptionThrown(() =>
				Distance.ConvertSafe(10, Distance.NauticalMiles, Weight.Kilograms));
		}

		public void TestDescription_NonPlural()
		{
			AssertEquals("Kilometer", Distance.GetDescription(Distance.Kilometres, PluralState.NonPlural));
			AssertEquals("Mile", Distance.GetDescription(Distance.Miles, PluralState.NonPlural));
			AssertEquals("Nautical Mile", Distance.GetDescription(Distance.NauticalMiles, PluralState.NonPlural));
		}

		public void TestDescription_Plural()
		{
			AssertEquals("Kilometers", Distance.GetDescription(Distance.Kilometres, PluralState.Plural));
			AssertEquals("Miles", Distance.GetDescription(Distance.Miles, PluralState.Plural));
			AssertEquals("Nautical Miles", Distance.GetDescription(Distance.NauticalMiles, PluralState.Plural));
		}

		public void TestDescription_PluralOrNonPlural()
		{
			AssertEquals("Kilometer(s)", Distance.GetDescription(Distance.Kilometres, PluralState.PluralOrNonPlural));
			AssertEquals("Mile(s)", Distance.GetDescription(Distance.Miles, PluralState.PluralOrNonPlural));
			AssertEquals("Nautical Mile(s)", Distance.GetDescription(Distance.NauticalMiles, PluralState.PluralOrNonPlural));
		}
	}
}
