using System;
using System.Globalization;
using CargoWise.Data;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	internal class ScaleVolumeIntegrationTest : TransactionedTestCase
	{
		public void TestMetricVolumeConversions()
		{
			AssertEquals(new Tuple<decimal, string>(100m, "CC"), ScaleVolume(100m, "CC"));
			AssertEquals(new Tuple<decimal, string>(999999.999m, "M3"), ScaleVolume(999999.999m, "M3"));
			AssertEquals(new Tuple<decimal, string>(1000m, "D3"), ScaleVolume(1000000m, "CC"));
			AssertEquals(new Tuple<decimal, string>(1000m, "M3"), ScaleVolume(1000000m, "D3"));
			AssertEquals(new Tuple<decimal, string>(1000m, "M3"), ScaleVolume(1000000000m, "CC"));
			AssertEquals(new Tuple<decimal, string>(1000m, "ML"), ScaleVolume(1000000000000m, "CC"));
			AssertEquals(new Tuple<decimal, string>(123456.789m, "ML"), ScaleVolume(123456789m, "M3"));
			AssertEquals(new Tuple<decimal, string>(decimal.Round(Constants.Volume.Convert(1000000m, "TE", "M3"), 3), "M3"), ScaleVolume(1000000m, "TE"));
		}
		public void TestImperialVolumeConversions()
		{
			AssertEquals(new Tuple<decimal, string>(100m, "CI"), ScaleVolume(100m, "CI"));
			AssertEquals(new Tuple<decimal, string>(999999.999m, "CI"), ScaleVolume(999999.999m, "CI"));
			AssertEquals(new Tuple<decimal, string>(decimal.Round(Constants.Volume.Convert(1000000m, "CI", "CF"), 3), "CF"), ScaleVolume(1000000m, "CI"));
			AssertEquals(new Tuple<decimal, string>(decimal.Round(Constants.Volume.Convert(10000000000m, "CI", "CY"), 3), "CY"), ScaleVolume(10000000000m, "CI"));
			AssertEquals(new Tuple<decimal, string>(decimal.Round(Constants.Volume.Convert(1000000m, "CF", "CY"), 3), "CY"), ScaleVolume(1000000m, "CF"));
		}

		public void TestVolumeOutOfRange()
		{
			AssertEquals(new Tuple<decimal, string>(100000000.000m, "ML"), ScaleVolume(100000000m, "ML"));
		}

		public void TestMetricVolumeConversionSequence()
		{
			AssertUnitSequence(
				Constants.Volume.Litre,
				Constants.Volume.CubicMetres,
				Constants.Volume.MegaLitre
			);

			AssertUnitSequence(
				Constants.Volume.CubicCentimeters,
				Constants.Volume.CubicDecimetres,
				Constants.Volume.CubicMetres,
				Constants.Volume.MegaLitre
			);
		}

		public void TestImperialVolumeConversionSequence()
		{
			AssertUnitSequence(
				Constants.Volume.CubicInches,
				Constants.Volume.CubicFeet,
				Constants.Volume.CubicYards
			);
		}

		public void TestMiscVolumeConversionSequence()
		{
			AssertUnitSequence(
				Constants.Volume.TeaChest,
				Constants.Volume.CubicMetres,
				Constants.Volume.MegaLitre
			);
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
					var toVolume = 999999.999m;
					var fromVolume = Constants.Volume.Convert(toVolume, toUnit, fromUnit);
					AssertEquals(new Tuple<decimal, string>(toVolume, toUnit), ScaleVolume(fromVolume, fromUnit));

					//get 1000000 of target unit in from units to test low limit or overflow 
					toVolume = 1000000m;
					fromVolume = Constants.Volume.Convert(toVolume, toUnit, fromUnit);
					if (j == units.Length - 1)
					{
						AssertEquals("overflow - return overflow on max units", new Tuple<decimal, string>(toVolume, toUnit), ScaleVolume(fromVolume, fromUnit));
					}
					else
					{
						var nextUnit = units[j + 1];
						var nextVolume = decimal.Round(Constants.Volume.Convert(toVolume, toUnit, nextUnit), 3);
						AssertEquals("overflow - but can return next higher units", new Tuple<decimal, string>(nextVolume, nextUnit), ScaleVolume(fromVolume, fromUnit));
					}
				}
			}
		}

		Tuple<decimal, string> ScaleVolume(decimal value, string fromUnit)
		{
			string commandText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.ScaleVolume({0}, '{1}')", value, fromUnit);
			var table = DataUtils.GetDataTableFromQuery(TestConnection, commandText);

			AssertEquals("Number of Rows", 1, table.Rows.Count);
			return new Tuple<decimal, string>(decimal.Round(Convert.ToDecimal(table.Rows[0]["Value"]), 3), table.Rows[0]["ToUnit"].ToString());
		}
		#endregion
	}
}
