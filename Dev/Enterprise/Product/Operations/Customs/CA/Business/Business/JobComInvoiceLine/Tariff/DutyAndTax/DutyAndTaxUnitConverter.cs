using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class DutyAndTaxUnitConverter
	{
		#region Weight Converter

		public static ZDecimal GetConvertedQuantity(ZDecimal qty, ZString unit1, ZString unit2)
		{
			var mappedUnit1 = GetMappedUnit(unit1);
			var mappedUnit2 = GetMappedUnit(unit2);
			if (mappedUnit1.IsEmpty || mappedUnit2.IsEmpty)
			{
				return ZDecimal.Zero;
			}
			return new ZDecimal(Core.Constants.Weight.Convert(qty, mappedUnit1, mappedUnit2)).Round(4);
		}

		public static ZString GetMappedUnit(ZString unit)
		{
			var result = ZString.Empty;
			switch (unit)
			{
				case CustomsUnitOfMeasureList.Codes.MetricCarat:
					result = Core.Constants.Weight.MetricCarat;
					break;
				case CustomsUnitOfMeasureList.Codes.Milligram:
					result = Core.Constants.Weight.Milligrams;
					break;
				case CustomsUnitOfMeasureList.Codes.Gram:
					result = Core.Constants.Weight.Grams;
					break;
				case CustomsUnitOfMeasureList.Codes.Hectogram:
					result = Core.Constants.Weight.Hectograms;
					break;
				case CustomsUnitOfMeasureList.Codes.Kilogram:
					result = Core.Constants.Weight.Kilograms;
					break;
				case CustomsUnitOfMeasureList.Codes.Pound:
					result = Core.Constants.Weight.Pounds;
					break;
				case CustomsUnitOfMeasureList.Codes.OunceONZ:
					result = Core.Constants.Weight.Ounces;
					break;
				case CustomsUnitOfMeasureList.Codes.PoundsTroy:
					result = Core.Constants.Weight.PoundsTroy;
					break;
				case CustomsUnitOfMeasureList.Codes.OuncesTroy:
					result = Core.Constants.Weight.OuncesTroy;
					break;
				case CustomsUnitOfMeasureList.Codes.MetricTon:
					result = Core.Constants.Weight.Tonnes;
					break;
				case CustomsUnitOfMeasureList.Codes.LongTons:
					result = Core.Constants.Weight.LongTons;
					break;
				case CustomsUnitOfMeasureList.Codes.ShortTons:
					result = Core.Constants.Weight.ShortTons;
					break;
				case CustomsUnitOfMeasureList.Codes.Deciton:
					result = Core.Constants.Weight.Decitons;
					break;
				default:
					break;
			}
			return result;
		}

		#endregion

		#region Number Converter

		public ZDecimal ConvertSafeToNumber(ZDecimal sourceValue, ZString sourceUnitCode)
		{
			var result = ZDecimal.Zero;

			if (string.IsNullOrWhiteSpace(sourceUnitCode))
			{
				return result;
			}

			switch (sourceUnitCode)
			{
				case CustomsUnitOfMeasureList.Codes.Number:
					result = sourceValue;
					break;
				case CustomsUnitOfMeasureList.Codes.Pair:
					result = sourceValue * 2;
					break;
				case CustomsUnitOfMeasureList.Codes.Dozen:
					result = sourceValue * 12;
					break;
				case CustomsUnitOfMeasureList.Codes.Score:
					result = sourceValue * 20;
					break;
				case CustomsUnitOfMeasureList.Codes.DozenPairs:
					result = sourceValue * 24;
					break;
				case CustomsUnitOfMeasureList.Codes.Gross:
					result = sourceValue * 144;
					break;
				case CustomsUnitOfMeasureList.Codes.GreatGross:
					result = sourceValue * 1728;
					break;
				case CustomsUnitOfMeasureList.Codes.Hundred:
					result = sourceValue * 100;
					break;
				case CustomsUnitOfMeasureList.Codes.Thousand:
					result = sourceValue * 1000;
					break;
				case CustomsUnitOfMeasureList.Codes.Million:
					result = sourceValue * 1000000;
					break;
			}

			return result;
		}

		public ZDecimal ConvertFromNumberToTargetUnit(ZDecimal valueInNumber, ZString targetUnitCode)
		{
			var result = ZDecimal.Zero;
			if (targetUnitCode.IsEmpty)
			{
				return result;
			}

			switch (targetUnitCode)
			{
				case CustomsUnitOfMeasureList.Codes.Number:
					result = valueInNumber;
					break;
				case CustomsUnitOfMeasureList.Codes.Pair:
					result = valueInNumber / 2;
					break;
				case CustomsUnitOfMeasureList.Codes.Dozen:
					result = valueInNumber / 12;
					break;
				case CustomsUnitOfMeasureList.Codes.Score:
					result = valueInNumber / 20;
					break;
				case CustomsUnitOfMeasureList.Codes.DozenPairs:
					result = valueInNumber / 24;
					break;
				case CustomsUnitOfMeasureList.Codes.Gross:
					result = valueInNumber / 144;
					break;
				case CustomsUnitOfMeasureList.Codes.GreatGross:
					result = valueInNumber / 1728;
					break;
				case CustomsUnitOfMeasureList.Codes.Hundred:
					result = valueInNumber / 100;
					break;
				case CustomsUnitOfMeasureList.Codes.Thousand:
					result = valueInNumber / 1000;
					break;
				case CustomsUnitOfMeasureList.Codes.Million:
					result = valueInNumber / 1000000;
					break;
			}

			return result;
		}

		public ZBool IsNumberUnit(ZString unit)
		{
			return unit == CustomsUnitOfMeasureList.Codes.Number
				|| unit == CustomsUnitOfMeasureList.Codes.Pair
				|| unit == CustomsUnitOfMeasureList.Codes.Dozen
				|| unit == CustomsUnitOfMeasureList.Codes.Score
				|| unit == CustomsUnitOfMeasureList.Codes.DozenPairs
				|| unit == CustomsUnitOfMeasureList.Codes.Gross
				|| unit == CustomsUnitOfMeasureList.Codes.GreatGross
				|| unit == CustomsUnitOfMeasureList.Codes.Hundred
				|| unit == CustomsUnitOfMeasureList.Codes.Thousand
				|| unit == CustomsUnitOfMeasureList.Codes.Million;
		}

		#endregion

		#region Volumn Converter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZDecimal ConvertSafeToLitre(ZDecimal sourceValue, ZString sourceUnitCode)
		{
			var result = ZDecimal.Zero;

			if (string.IsNullOrWhiteSpace(sourceUnitCode))
			{
				return result;
			}

			switch (sourceUnitCode)
			{
				case CustomsUnitOfMeasureList.Codes.CubicMillimetre:
					result = sourceValue * 1e-6m;
					break;
				case CustomsUnitOfMeasureList.Codes.CubicCentimetre:
				case CustomsUnitOfMeasureList.Codes.Millilitre:
					result = sourceValue * 0.001m;
					break;
				case CustomsUnitOfMeasureList.Codes.Centilitre:
					result = sourceValue * 0.01m;
					break;
				case CustomsUnitOfMeasureList.Codes.Decilitre:
					result = sourceValue * 0.1m;
					break;
				case CustomsUnitOfMeasureList.Codes.CubicDecimetre:
				case CustomsUnitOfMeasureList.Codes.Litre:
					result = sourceValue;
					break;
				case CustomsUnitOfMeasureList.Codes.Hectolitre:
					result = sourceValue * 100;
					break;
				case CustomsUnitOfMeasureList.Codes.CubicMetre:
					result = sourceValue * 1000;
					break;
				case CustomsUnitOfMeasureList.Codes.ThousandCubicMetres:
				case CustomsUnitOfMeasureList.Codes.Megalitre:
					result = sourceValue * 1e6m;
					break;
				case CustomsUnitOfMeasureList.Codes.MillionCubicMetres:
					result = sourceValue * 1e9m;
					break;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZDecimal ConvertFromLiterToTargetUnit(ZDecimal valueInVolumn, ZString targetUnitCode)
		{
			var result = ZDecimal.Zero;
			if (targetUnitCode.IsEmpty)
			{
				return result;
			}

			switch (targetUnitCode)
			{
				case CustomsUnitOfMeasureList.Codes.CubicMillimetre:
					result = valueInVolumn / 1e-6m;
					break;
				case CustomsUnitOfMeasureList.Codes.CubicCentimetre:
				case CustomsUnitOfMeasureList.Codes.Millilitre:
					result = valueInVolumn * 1000;
					break;
				case CustomsUnitOfMeasureList.Codes.Centilitre:
					result = valueInVolumn * 100m;
					break;
				case CustomsUnitOfMeasureList.Codes.Decilitre:
					result = valueInVolumn * 10m;
					break;
				case CustomsUnitOfMeasureList.Codes.CubicDecimetre:
				case CustomsUnitOfMeasureList.Codes.Litre:
					result = valueInVolumn;
					break;
				case CustomsUnitOfMeasureList.Codes.Hectolitre:
					result = valueInVolumn / 100;
					break;
				case CustomsUnitOfMeasureList.Codes.CubicMetre:
					result = valueInVolumn / 1000;
					break;
				case CustomsUnitOfMeasureList.Codes.ThousandCubicMetres:
				case CustomsUnitOfMeasureList.Codes.Megalitre:
					result = valueInVolumn / 1e6m;
					break;
				case CustomsUnitOfMeasureList.Codes.MillionCubicMetres:
					result = valueInVolumn / 1e9m;
					break;
			}

			return result;
		}

		public ZDecimal ConvertSafeToOunce(ZDecimal sourceValue, ZString sourceUnitCode)
		{
			var result = sourceValue;

			if (sourceUnitCode != CustomsUnitOfMeasureList.Codes.Ounce)
			{
				result = ConvertSafeToLitre(sourceValue, sourceUnitCode) * 35.1950652m;
			}

			return result;
		}

		public ZBool IsVolumnUnit(ZString unit)
		{
			return unit == CustomsUnitOfMeasureList.Codes.CubicMillimetre
				|| unit == CustomsUnitOfMeasureList.Codes.CubicCentimetre
				|| unit == CustomsUnitOfMeasureList.Codes.Millilitre
				|| unit == CustomsUnitOfMeasureList.Codes.Centilitre
				|| unit == CustomsUnitOfMeasureList.Codes.Decilitre
				|| unit == CustomsUnitOfMeasureList.Codes.CubicDecimetre
				|| unit == CustomsUnitOfMeasureList.Codes.Litre
				|| unit == CustomsUnitOfMeasureList.Codes.Hectolitre
				|| unit == CustomsUnitOfMeasureList.Codes.CubicMetre
				|| unit == CustomsUnitOfMeasureList.Codes.ThousandCubicMetres
				|| unit == CustomsUnitOfMeasureList.Codes.Megalitre
				|| unit == CustomsUnitOfMeasureList.Codes.MillionCubicMetres
				|| unit == CustomsUnitOfMeasureList.Codes.Ounce;
		}

		#endregion
	}
}
