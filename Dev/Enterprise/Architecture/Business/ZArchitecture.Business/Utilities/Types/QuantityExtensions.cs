
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture
{
	using Business;

	public static class QuantityExtensions
	{
		public static IQuantity Convert(this IQuantity quantity, string targetUnit, ConversionFactor[] conversionFactors = null, bool roundResult = false)
		{
			if (quantity == null || quantity.IsEmpty || quantity.Amount.IsEmpty)
			{
				return new Quantity(0m, targetUnit);
			}

			var converter = new UnitsConverter(conversionFactors);
			return converter.Convert(quantity, targetUnit, roundResult);
		}

		public static bool IsWeight(this IQuantity quantity)
		{
			return IsWeight(quantity.Unit);
		}

		public static bool IsWeight(ZString unit)
		{
			return Constants.Weight.ContainsCode(unit);
		}

		public static bool IsVolume(this IQuantity quantity)
		{
			return IsVolume(quantity.Unit);
		}

		public static bool IsVolume(ZString unit)
		{
			return Constants.Volume.ContainsCode(unit);
		}

		public static Quantity AmountFor(this Quantity quantity, ZString targetUnit)
		{
			if (quantity.IsEmpty)
			{
				return quantity;
			}

			if (quantity.Unit == targetUnit)
			{
				return quantity;
			}

			if (quantity.IsWeight() && IsWeight(targetUnit))
			{
				return new Quantity(
					Constants.Weight.Convert(quantity.Amount, quantity.Unit, targetUnit),
					targetUnit,
					quantity.Source,
					quantity.Reference,
					quantity.Description);
			}

			if (quantity.IsVolume() && IsVolume(targetUnit))
			{
				return new Quantity(
					Constants.Volume.Convert(quantity.Amount, quantity.Unit, targetUnit),
					targetUnit,
					quantity.Source,
					quantity.Reference,
					quantity.Description);
			}

			return new Quantity(0, targetUnit);
		}

		public static Quantity Round(this Quantity quantity, int decimalPlaces)
		{
			return new Quantity(Utilities.Round(quantity.Amount, decimalPlaces), quantity.Unit);
		}
	}
}
