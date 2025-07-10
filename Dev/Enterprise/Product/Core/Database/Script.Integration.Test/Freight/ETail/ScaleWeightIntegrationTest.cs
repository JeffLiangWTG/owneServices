using System;
using System.Globalization;
using CargoWise.Data;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	internal class ScaleWeightIntegrationTest : TransactionedTestCase
	{
		public void TestMetricWeightConversions()
		{
			AssertEquals(new Tuple<decimal, string>(100m, "G"), ScaleWeight(100m, "G"));
			AssertEquals(new Tuple<decimal, string>(999999.999m, "G"), ScaleWeight(999999.999m, "G"));
			AssertEquals(new Tuple<decimal, string>(1000m, "KG"), ScaleWeight(1000000m, "G"));
			AssertEquals(new Tuple<decimal, string>(1000m, "KG"), ScaleWeight(1000000000m, "MG"));
			AssertEquals(new Tuple<decimal, string>(1000m, "T"), ScaleWeight(1000000000000m, "MG"));
			AssertEquals(new Tuple<decimal, string>(1000m, "KT"), ScaleWeight(1000000000000000m, "MG"));
			AssertEquals(new Tuple<decimal, string>(123456.789m, "T"), ScaleWeight(123456789m, "KG"));
		}
		public void TestImperialWeightConversions()
		{
			AssertEquals(new Tuple<decimal, string>(100m, "OZ"), ScaleWeight(100m, "OZ"));
			AssertEquals(new Tuple<decimal, string>(999999.999m, "OZ"), ScaleWeight(999999.999m, "OZ"));
			AssertEquals(new Tuple<decimal, string>(62500m, "LB"), ScaleWeight(1000000m, "OZ"));
			AssertEquals(new Tuple<decimal, string>(31250m, "TN"), ScaleWeight(1000000000m, "OZ"));
			AssertEquals(new Tuple<decimal, string>(4232.804m, "TN"), ScaleWeight(123456789m, "OT"));
		}

		public void TestWeightOutOfRange()
		{
			AssertEquals(new Tuple<decimal, string>(100000000m, "KT"), ScaleWeight(100000000m, "KT"));
		}

		public void TestMetricWeightConversionSequences()
		{
			AssertUnitSequence(
				Constants.Weight.Milligrams,
				Constants.Weight.Grams,
				Constants.Weight.Kilograms,
				Constants.Weight.Tonnes,
				Constants.Weight.Kilotonnes
			);

			AssertUnitSequence(
				Constants.Weight.Hectograms,
				Constants.Weight.Kilograms,
				Constants.Weight.Tonnes,
				Constants.Weight.Kilotonnes
			);

			AssertUnitSequence(
				Constants.Weight.Decitons,
				Constants.Weight.Tonnes,
				Constants.Weight.Kilotonnes
			);
		}

		public void TestImperialWeightConversionSequences()
		{
			/* Can't use AssertUnitSequence due to rounding errors of Constants.Weight.Convert, Manually added equivalent tests
			AssertUnitSequence(
				Constants.Weight.Ounces,
				Constants.Weight.Pounds,
				Constants.Weight.ShortTons,
				Constants.Weight.LongTons
			);
			*/

			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "OZ"), ScaleWeight(999999.999m, "OZ"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(62500.000m, "LB"), ScaleWeight(1000000m, "OZ"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "LB"), ScaleWeight(15999999.984m, "OZ"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(500.000m, "TN"), ScaleWeight(16000000m, "OZ"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TN"), ScaleWeight(31999999968m, "OZ"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(892857.143m, "TL"), ScaleWeight(32000000000m, "OZ"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TL"), ScaleWeight(35839999964.1m, "OZ"));
			AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(1000000m, "TL"), ScaleWeight(35840000000m, "OZ"));

			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "LB"), ScaleWeight(999999.999m, "LB"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(500.000m, "TN"), ScaleWeight(1000000m, "LB"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TN"), ScaleWeight(1999999998m, "LB"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(892857.143m, "TL"), ScaleWeight(2000000000m, "LB"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TL"), ScaleWeight(2239999997.76m, "LB"));
			AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(1000000m, "TL"), ScaleWeight(2240000000m, "LB"));

			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TN"), ScaleWeight(999999.999m, "TN"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(892857.143m, "TL"), ScaleWeight(1000000m, "TN"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TL"), ScaleWeight(1119999.99888m, "TN"));
			AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(1000000m, "TL"), ScaleWeight(1120000m, "TN"));

			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TL"), ScaleWeight(999999.999m, "TL"));
			AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(1000000m, "TL"), ScaleWeight(1000000m, "TL"));

			//AssertUnitSequence(
			//	Constants.Weight.OuncesTroy,
			//	Constants.Weight.PoundsTroy,
			//	Constants.Weight.ShortTons,
			//	Constants.Weight.LongTons
			//);

			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "OT"), ScaleWeight(999999.999m, "OT"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(83333.333m, "LT"), ScaleWeight(1000000m, "OT"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "LT"), ScaleWeight(11999999.988m, "OT"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(411.429m, "TN"), ScaleWeight(12000000m, "OT"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TN"), ScaleWeight(29166669970, "OT"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(892857.154m, "TL"), ScaleWeight(29166670000, "OT"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TL"), ScaleWeight(32666669967m, "OT"));
			AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(1000000m, "TL"), ScaleWeight(32666670000m, "OT"));

			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "LT"), ScaleWeight(999999.999m, "LT"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(411.428m, "TN"), ScaleWeight(1000000m, "LT"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TN"), ScaleWeight(2430555997m, "LT"));
			AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(892857.379m, "TL"), ScaleWeight(2430556000m, "LT"));
			AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(999999.999m, "TL"), ScaleWeight(2722221997m, "LT"));
			AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(1000000m, "TL"), ScaleWeight(2722222000m, "LT"));
		}

		#region Implementation

		void AssertUnitSequence(params string[] units)
		{
			for (int i = 0; i < units.Length; i++)
			{
				var fromUnit = units[i];
				for (int j = i; j < units.Length; j++)
				{
					var toUnit = units[j];
					//get 999999.999 of target unit in from units to test upper limit
					var toWeight = 999999.999m;
					var fromWeight = Constants.Weight.Convert(toWeight, toUnit, fromUnit);
					AssertEquals("Should return 999999.999 of expected unit", new Tuple<decimal, string>(toWeight, toUnit), ScaleWeight(fromWeight, fromUnit));
					//get 1000000 of target unit in from units to test low limit or overflow 
					toWeight = 1000000m;
					fromWeight = Constants.Weight.Convert(toWeight, toUnit, fromUnit);
					if (j == units.Length - 1)
					{
						AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(toWeight, toUnit), ScaleWeight(fromWeight, fromUnit));
					}
					else
					{
						var nextUnit = units[j + 1];
						var nextWeight = decimal.Round(Constants.Weight.Convert(toWeight, toUnit, nextUnit), 3);
						AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(nextWeight, nextUnit), ScaleWeight(fromWeight, fromUnit));
					}
				}
			}
		}

		Tuple<decimal, string> ScaleWeight(decimal value, string fromUnit)
		{
			string commandText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.ScaleWeight({0}, '{1}')", value, fromUnit);
			var table = DataUtils.GetDataTableFromQuery(TestConnection, commandText);

			AssertEquals("Number of Rows", 1, table.Rows.Count);
			return new Tuple<decimal, string>(decimal.Round(Convert.ToDecimal(table.Rows[0]["Value"]), 3), table.Rows[0]["ToUnit"].ToString());
		}
		#endregion
	}
}
