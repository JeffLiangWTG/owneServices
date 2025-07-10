using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DutyAndTaxUnitConverterTest : TestCaseWithFactory
	{
		public void TestGetMappedUnit()
		{
			AssertEquals(Core.Constants.Weight.MetricCarat, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.MetricCarat));
			AssertEquals(Core.Constants.Weight.Milligrams, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.Milligram));
			AssertEquals(Core.Constants.Weight.Grams, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.Gram));
			AssertEquals(Core.Constants.Weight.Hectograms, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.Hectogram));
			AssertEquals(Core.Constants.Weight.Kilograms, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.Kilogram));
			AssertEquals(Core.Constants.Weight.Pounds, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.Pound));
			AssertEquals(Core.Constants.Weight.Ounces, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.OunceONZ));
			AssertEquals(Core.Constants.Weight.PoundsTroy, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.PoundsTroy));
			AssertEquals(Core.Constants.Weight.OuncesTroy, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.OuncesTroy));
			AssertEquals(Core.Constants.Weight.Tonnes, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.MetricTon));
			AssertEquals(Core.Constants.Weight.LongTons, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.LongTons));
			AssertEquals(Core.Constants.Weight.ShortTons, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.ShortTons));
			AssertEquals(Core.Constants.Weight.Decitons, DutyAndTaxUnitConverter.GetMappedUnit(CustomsUnitOfMeasureList.Codes.Deciton));
		}

		public void TestConvertSafeToNumber()
		{
			AssertEquals(100m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Number));
			AssertEquals(200m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Pair));
			AssertEquals(1200m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Dozen));
			AssertEquals(2000m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Score));
			AssertEquals(2400m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.DozenPairs));
			AssertEquals(14400m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Gross));
			AssertEquals(172800m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.GreatGross));
			AssertEquals(10000m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Hundred));
			AssertEquals(100000m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Thousand));
			AssertEquals(100000000m, UnitConverter.ConvertSafeToNumber(100m, CustomsUnitOfMeasureList.Codes.Million));
		}

		public void TestConvertFromNumberToTargetUnit()
		{
			AssertEquals(100m, UnitConverter.ConvertFromNumberToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.Number));
			AssertEquals(50m, UnitConverter.ConvertFromNumberToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.Pair));
			AssertEquals(10m, UnitConverter.ConvertFromNumberToTargetUnit(120m, CustomsUnitOfMeasureList.Codes.Dozen));
			AssertEquals(5m, UnitConverter.ConvertFromNumberToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.Score));
			AssertEquals(6m, UnitConverter.ConvertFromNumberToTargetUnit(144m, CustomsUnitOfMeasureList.Codes.DozenPairs));
			AssertEquals(1m, UnitConverter.ConvertFromNumberToTargetUnit(144m, CustomsUnitOfMeasureList.Codes.Gross));
			AssertEquals(0.1m, UnitConverter.ConvertFromNumberToTargetUnit(172.8m, CustomsUnitOfMeasureList.Codes.GreatGross));
			AssertEquals(1m, UnitConverter.ConvertFromNumberToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.Hundred));
			AssertEquals(0.1m, UnitConverter.ConvertFromNumberToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.Thousand));
			AssertEquals(0.0001m, UnitConverter.ConvertFromNumberToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.Million));
		}

		public void TestIsNumberUnit()
		{
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Number));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Pair));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Dozen));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Score));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.DozenPairs));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Gross));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.GreatGross));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Hundred));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Thousand));
			AssertEquals(true, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Million));
			AssertEquals(false, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Gram));
			AssertEquals(false, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Deciton));
			AssertEquals(false, UnitConverter.IsNumberUnit(CustomsUnitOfMeasureList.Codes.Ounce));
		}

		public void TestConvertSafeToLitre()
		{
			AssertEquals(0.0001m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.CubicMillimetre));
			AssertEquals(0.1m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.CubicCentimetre));
			AssertEquals(0.1m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.Millilitre));
			AssertEquals(1m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.Centilitre));
			AssertEquals(10m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.Decilitre));
			AssertEquals(100m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.CubicDecimetre));
			AssertEquals(100m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.Litre));
			AssertEquals(10000m, UnitConverter.ConvertSafeToLitre(100m, CustomsUnitOfMeasureList.Codes.Hectolitre));
			AssertEquals(10000m, UnitConverter.ConvertSafeToLitre(10m, CustomsUnitOfMeasureList.Codes.CubicMetre));
			AssertEquals(1000000m, UnitConverter.ConvertSafeToLitre(1m, CustomsUnitOfMeasureList.Codes.ThousandCubicMetres));
			AssertEquals(1000000m, UnitConverter.ConvertSafeToLitre(1m, CustomsUnitOfMeasureList.Codes.Megalitre));
			AssertEquals(1000000000m, UnitConverter.ConvertSafeToLitre(1m, CustomsUnitOfMeasureList.Codes.MillionCubicMetres));
		}

		public void TestConvertFromLiterToTargetUnit()
		{
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(0.0001m, CustomsUnitOfMeasureList.Codes.CubicMillimetre));
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(0.1m, CustomsUnitOfMeasureList.Codes.CubicCentimetre));
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(0.1m, CustomsUnitOfMeasureList.Codes.Millilitre));
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(1m, CustomsUnitOfMeasureList.Codes.Centilitre));
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(10m, CustomsUnitOfMeasureList.Codes.Decilitre));
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.CubicDecimetre));
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(100m, CustomsUnitOfMeasureList.Codes.Litre));
			AssertEquals(100m, UnitConverter.ConvertFromLiterToTargetUnit(10000m, CustomsUnitOfMeasureList.Codes.Hectolitre));
			AssertEquals(10m, UnitConverter.ConvertFromLiterToTargetUnit(10000m, CustomsUnitOfMeasureList.Codes.CubicMetre));
			AssertEquals(1m, UnitConverter.ConvertFromLiterToTargetUnit(1000000m, CustomsUnitOfMeasureList.Codes.ThousandCubicMetres));
			AssertEquals(1m, UnitConverter.ConvertFromLiterToTargetUnit(1000000m, CustomsUnitOfMeasureList.Codes.Megalitre));
			AssertEquals(1m, UnitConverter.ConvertFromLiterToTargetUnit(1000000000m, CustomsUnitOfMeasureList.Codes.MillionCubicMetres));
		}

		public void TestIsVolumnUnit()
		{
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.CubicMillimetre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.CubicCentimetre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Millilitre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Centilitre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Decilitre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.CubicDecimetre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Litre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Hectolitre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.CubicMetre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.ThousandCubicMetres));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Megalitre));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.MillionCubicMetres));
			AssertEquals(true, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Ounce));
			AssertEquals(false, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Gram));
			AssertEquals(false, UnitConverter.IsVolumnUnit(CustomsUnitOfMeasureList.Codes.Number));
		}

		#region Implementation

		DutyAndTaxUnitConverter UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new DutyAndTaxUnitConverter()); }
		}
		DutyAndTaxUnitConverter fUnitConverter;

		#endregion
	}
}
