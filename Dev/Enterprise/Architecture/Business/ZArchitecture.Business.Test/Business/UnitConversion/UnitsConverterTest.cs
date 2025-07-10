using System;
using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class UnitsConverterTest : TestCaseWithFactory
	{
		public void TestConvert_QuantityIsNull_ThrowException()
		{
			var converter = new UnitsConverter();
			AssertExceptionThrown<ArgumentNullException>(() => converter.Convert(null, Weight.Kilograms));
		}

		public void TestConvert_UnitIsUnknown_ThrowException()
		{
			var converter = new UnitsConverter();

			AssertExceptionThrown<UnitConversionException>(() => converter.Convert(new Quantity(10, Weight.Grams), Temperature.Fahrenheit));
			AssertExceptionThrown<UnitConversionException>(() => converter.Convert(new Quantity(10, Weight.Grams), "AA"));
			AssertExceptionThrown<UnitConversionException>(() => converter.Convert(new Quantity(10, "AA"), Weight.Kilograms));
		}

		public void TestConvert_RequiredConversionFactorsProvided_ReturnConvertedValue()
		{
			var factors = new[]
			{
				new ConversionFactor(10, Weight.Kilograms, Volume.CubicMetres),
				new ConversionFactor(100, Weight.Kilograms, LoadingLength.LoadingMeters),
			};

			var converter = new UnitsConverter(factors);

			var actualValue = converter.Convert(new Quantity(2000, Weight.Grams), Weight.Kilograms);
			AssertEquals("2000 G -> KG", new Quantity(2, Weight.Kilograms), actualValue);

			actualValue = converter.Convert(new Quantity(50000, Weight.Grams), Volume.CubicMetres);
			AssertEquals("5000 G -> M3", new Quantity(5, Volume.CubicMetres), actualValue);

			actualValue = converter.Convert(new Quantity(50000, Weight.Grams), Volume.Litre);
			AssertEquals("5000 G -> L", new Quantity(5000, Volume.Litre), actualValue);

			actualValue = converter.Convert(new Quantity(6 * 1000 * 1000, Volume.CubicCentimeters), LoadingLength.LoadingMeters);
			AssertEquals("6000000 CC -> LM", new Quantity(0.6, LoadingLength.LoadingMeters), actualValue);
		}

		public void TestConvert_NoRequiredConversionFactors_ReturnQuantityEmpty()
		{
			var factors = new[]
			{
				new ConversionFactor(10, Weight.Kilograms, Volume.CubicMetres),
			};

			var converter = new UnitsConverter(factors);
			var actualValue = converter.Convert(new Quantity(2000, Weight.Grams), LoadingLength.LoadingMeters);

			Assert("Expected an empty value rather null", actualValue.IsEmpty);
		}
	}
}
