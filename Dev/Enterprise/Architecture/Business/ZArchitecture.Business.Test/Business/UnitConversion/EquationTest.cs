using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EquationTest : TestCaseWithFactory
	{
		public void TestZeroFactorsDoNotThrowException()
		{
			var converter = new ConversionFactor(0m, Constants.Volume.Litre, Constants.Weight.Kilograms);
			AssertNoExceptionThrown(() => converter.Convert(new ZVolume(10m, Constants.Volume.Litre)));
			AssertNoExceptionThrown(() => converter.Convert(new ZWeight(10m, Constants.Weight.Kilograms)));
		}

		public void TestIsValid()
		{
			AssertEquals(false, new ConversionFactor(0, "KG", "M3").IsValid);
			AssertEquals(false, new ConversionFactor(-5, "KG", "M3").IsValid);
			AssertEquals(false, new ConversionFactor(100m, "KG", "").IsValid);
			AssertEquals(false, new ConversionFactor(100m, "", "M3").IsValid);
			AssertEquals(false, new ConversionFactor(10m, "M3", "M3").IsValid);
			AssertEquals(true, new ConversionFactor(100m, "KG", "M3").IsValid);

			AssertEquals(true, new ConversionFactor(0, "KG", "LM").IsValid);
			AssertEquals(false, new ConversionFactor(-5, "KG", "LM").IsValid);
			AssertEquals(false, new ConversionFactor(100m, "", "LM").IsValid);
			AssertEquals(false, new ConversionFactor(10m, "LM", "LM").IsValid);
			AssertEquals(true, new ConversionFactor(100m, "KG", "LM").IsValid);

			AssertEquals(false, new ConversionFactor(1, "", "").IsValid);
			AssertEquals(false, new ConversionFactor(1, null, "").IsValid);
			AssertEquals(false, new ConversionFactor(1, "", null).IsValid);
			AssertEquals(false, new ConversionFactor(1, null, null).IsValid);
		}

		public void TestUnitsSystem()
		{
			AssertEquals(UnitsSystem.Metric, new ConversionFactor(150m, "KG", "M3").UnitsSystem);
			AssertEquals(UnitsSystem.Metric, new ConversionFactor(150m, "M3", "KG").UnitsSystem);
			AssertEquals(UnitsSystem.Imperial, new ConversionFactor(150m, "KG", "CI").UnitsSystem);
			AssertEquals(UnitsSystem.Imperial, new ConversionFactor(150m, "KG", "LB").UnitsSystem);
		}

		public void TestToShortString_ReturnShortStringRepresentation()
		{
			AssertEquals("150 KG/M3", new ConversionFactor(150m, "KG", "M3").ToShortString());
			AssertEquals("150.6 LB/CI", new ConversionFactor(150.6m, "LB", "CI").ToShortString());
			AssertEquals("150.66 CC/KG", new ConversionFactor(150.66m, "CC", "KG").ToShortString());
			AssertEquals("150.666 KG/M", new ConversionFactor(150.666m, "KG", "M").ToShortString());
			AssertEquals("150.666666 KG/M", new ConversionFactor(150.666666m, "KG", "M").ToShortString());
			AssertEquals("150.0001 KG/M", new ConversionFactor(150.0001m, "KG", "M").ToShortString());
			AssertEquals("150 KG/M", new ConversionFactor(150m, "KG", "M").ToShortString());
		}

		public void TestToString_ReturnShortStringRepresentation()
		{
			AssertEquals("150 KG/M3", new ConversionFactor(150m, "KG", "M3").ToShortString());
			AssertEquals("150.6 LB/CI", new ConversionFactor(150.6m, "LB", "CI").ToShortString());
			AssertEquals("150.66 CC/KG", new ConversionFactor(150.66m, "CC", "KG").ToShortString());
			AssertEquals("150.666 KG/M", new ConversionFactor(150.666m, "KG", "M").ToShortString());
			AssertEquals("150.666666 KG/M", new ConversionFactor(150.666666m, "KG", "M").ToShortString());
			AssertEquals("150.0001 KG/M", new ConversionFactor(150.0001m, "KG", "M").ToShortString());
			AssertEquals("150 KG/M", new ConversionFactor(150m, "KG", "M").ToShortString());
		}

		public void TestToLongString_ReturnLongStringRepresentation()
		{
			AssertEquals("1 M3 = 150 KG", new ConversionFactor(150m, "KG", "M3").ToLongString());
			AssertEquals("1 CI = 150.6 LB", new ConversionFactor(150.6m, "LB", "CI").ToLongString());
			AssertEquals("1 KG = 150.66 CC", new ConversionFactor(150.66m, "CC", "KG").ToLongString());
			AssertEquals("1 M = 150.666 KG", new ConversionFactor(150.666m, "KG", "M").ToLongString());
			AssertEquals("1 M = 150.666666 KG", new ConversionFactor(150.666666m, "KG", "M").ToLongString());
			AssertEquals("1 M = 150.0001 KG", new ConversionFactor(150.0001m, "KG", "M").ToLongString());
			AssertEquals("1 M = 150 KG", new ConversionFactor(150m, "KG", "M").ToLongString());
		}

		public void TestTryParse()
		{
			ConversionFactor equation;

			AssertEquals("100", false, ConversionFactor.TryParse("100", out equation));
			AssertEquals("100 KG", false, ConversionFactor.TryParse("100 KG", out equation));
			AssertEquals("100 KG M3", false, ConversionFactor.TryParse("100 KG M3", out equation));
			AssertEquals("100 KG/", false, ConversionFactor.TryParse("100 KG/", out equation));
			AssertEquals("100 /KG", false, ConversionFactor.TryParse("100 /KG", out equation));
			AssertEquals("KG/M3", false, ConversionFactor.TryParse("KG/M3", out equation));
			AssertEquals("KG 100 KG/M3", false, ConversionFactor.TryParse("KG 100 KG/M3", out equation));
			AssertEquals("100 KG/M3/", false, ConversionFactor.TryParse("100 KG/M3/", out equation));
			AssertEquals("100 KG/M3 KG", false, ConversionFactor.TryParse("100 KG/M3 KG", out equation));

			AssertEquals("100 KG/M3", true, ConversionFactor.TryParse("100 KG/M3", out equation));
			AssertEquation(100, "KG", "M3", equation);

			AssertEquals("  100   KG /   M3  ", true, ConversionFactor.TryParse("  100   KG /   M3  ", out equation));
			AssertEquation(100, "KG", "M3", equation);

			AssertEquals("100.000 KG/M3", true, ConversionFactor.TryParse("100.000 KG/M3", out equation));
			AssertEquation(100.000m, "KG", "M3", equation);

			AssertEquals("100.001 KG/M3", true, ConversionFactor.TryParse("100.001 KG/M3", out equation));
			AssertEquation(100.001m, "KG", "M3", equation);
		}

		static void AssertEquation(decimal expectedFactor, string expectedNumerator, string expectedDenominator, ConversionFactor equation)
		{
			var expected = string.Format("{0} {1}/{2}", expectedFactor, expectedNumerator, expectedDenominator);
			var actual = string.Format("{0} {1}/{2}", equation.Factor, equation.NumeratorUnit, equation.DenominatorUnit);

			AssertEquals("Equation", expected, actual);
		}

		public void TestConversion()
		{
			var equation = new ConversionFactor(1m, Constants.Volume.Litre, Constants.Weight.Kilograms);
			AssertEquals(new ZWeight(10m, Constants.Weight.Kilograms), equation.Convert(new ZVolume(10m, Constants.Volume.Litre)));
			AssertEquals(new ZVolume(10m, Constants.Volume.Litre), equation.Convert(new ZWeight(10m, Constants.Weight.Kilograms)));

			AssertEquals(new ZWeight(2m, Constants.Weight.Kilograms), equation.Convert(new ZVolume(2m, Constants.Volume.CubicDecimetres)));
			AssertEquals(new ZVolume(0.907185m, Constants.Volume.Litre), equation.Convert(new ZWeight(2m, Constants.Weight.Pounds)));

			equation = new ConversionFactor(450m, Constants.Volume.CubicInches, Constants.Weight.Pounds);
			AssertEquals(new ZWeight(16.896000004444444444444444444m, Constants.Weight.Pounds), equation.Convert(new ZVolume(4.4m, Constants.Volume.CubicFeet)));
			AssertEquals(new ZVolume(900.015300m, Constants.Volume.CubicInches), equation.Convert(new ZWeight(907.2m, Constants.Weight.Grams)));

			AssertEquals(new ZWeight(6.6666406298691777777777777778m, Constants.Weight.Pounds), equation.Convert(new ZVolume(49161m, Constants.Volume.CubicCentimeters)));
			AssertEquals(new ZVolume(1350m, Constants.Volume.CubicInches), equation.Convert(new ZWeight(48m, Constants.Weight.Ounces)));
		}

		public void TestConversion_LoadingMeters()
		{
			var equation = new ConversionFactor(2m, Weight.Kilograms, LoadingLength.LoadingMeters);
			AssertConversion(equation, 10m, Volume.Litre, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, Weight.Pounds, expectedQuantity: new Quantity(2.26796185m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Weight.Kilograms, expectedQuantity: new Quantity(5m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Volume.CubicMetres, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, LoadingLength.LoadingMeters, expectedQuantity: new ZWeight(20m, Weight.Kilograms));

			equation = new ConversionFactor(2m, Weight.Pounds, LoadingLength.LoadingMeters);
			AssertConversion(equation, 10m, Volume.Litre, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, Weight.Pounds, expectedQuantity: new Quantity(5m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Weight.Kilograms, expectedQuantity: new Quantity(11.02311310924390m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Volume.CubicMetres, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, LoadingLength.LoadingMeters, expectedQuantity: new ZWeight(20m, Weight.Pounds));

			equation = new ConversionFactor(0m, Weight.Kilograms, LoadingLength.LoadingMeters);
			AssertConversion(equation, 10m, Volume.Litre, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, Weight.Pounds, expectedQuantity: new Quantity(0m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Weight.Kilograms, expectedQuantity: new Quantity(0m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Volume.CubicMetres, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, LoadingLength.LoadingMeters, expectedQuantity: new ZWeight(0m, Weight.Kilograms));

			equation = new ConversionFactor(2m, Volume.CubicMetres, LoadingLength.LoadingMeters);
			AssertConversion(equation, 10m, Volume.Litre, expectedQuantity: new Quantity(0.005m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Weight.Pounds, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, Weight.Kilograms, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, Volume.CubicMetres, expectedQuantity: new Quantity(5.0m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, LoadingLength.LoadingMeters, expectedQuantity: new ZVolume(20m, Volume.CubicMetres));

			equation = new ConversionFactor(0m, Volume.CubicMetres, LoadingLength.LoadingMeters);
			AssertConversion(equation, 10m, Volume.Litre, expectedQuantity: new Quantity(0m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, Weight.Pounds, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, Weight.Kilograms, expectedQuantity: new Quantity());
			AssertConversion(equation, 10m, Volume.CubicMetres, expectedQuantity: new Quantity(0m, LoadingLength.LoadingMeters));
			AssertConversion(equation, 10m, LoadingLength.LoadingMeters, expectedQuantity: new ZVolume(0m, Volume.CubicMetres));
		}

		public void TestConversion_LoadingMeters_InvalidFactor_ShouldReturnEmpty()
		{
			var equations = new ConversionFactor[]
			{
				new ConversionFactor(-1m, Weight.Kilograms, LoadingLength.LoadingMeters),
				new ConversionFactor(-1m, Volume.CubicMetres, LoadingLength.LoadingMeters),
				new ConversionFactor(1m, "", LoadingLength.LoadingMeters),
				new ConversionFactor(0m, "", LoadingLength.LoadingMeters),
				new ConversionFactor(-1m, "", LoadingLength.LoadingMeters)
			};

			foreach (var equation in equations)
			{
				AssertConversion(equation, 10m, Volume.Litre, expectedQuantity: new Quantity());
				AssertConversion(equation, 10m, Weight.Pounds, expectedQuantity: new Quantity());
				AssertConversion(equation, 10m, Weight.Kilograms, expectedQuantity: new Quantity());
				AssertConversion(equation, 10m, Volume.CubicMetres, expectedQuantity: new Quantity());
				AssertConversion(equation, 10m, LoadingLength.LoadingMeters, expectedQuantity: new Quantity());
			}
		}

		static void AssertConversion(ConversionFactor conversionFactor, decimal quantityAmount, string quantityUnit, IQuantity expectedQuantity)
		{
			var input = new Quantity(quantityAmount, quantityUnit);
			var actual = conversionFactor.Convert(input);
			AssertEquals(expectedQuantity, actual);
		}
	}
}
