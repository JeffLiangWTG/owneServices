using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class UOMDefaulter
	{
		public static void DefaultUOMIfApplicable(RateView currentRateView, NctsCommonCargoDesc goodsItem)
		{
			if (goodsItem != null && currentRateView != null)
			{
				var rateFormula = currentRateView.ZZ2_RateFormula;
				var unit = rateFormula.SubstringSafe(rateFormula.IndexOf('[') + 1, rateFormula.IndexOf(']') - rateFormula.IndexOf('[') - 1);
				if (!unit.IsEmpty && rateFormula.IndexOf('[') != -1)
				{
					if (!UnitIsDeclared(unit, goodsItem))
					{
						if (goodsItem.BY_CustomsThirdUnitQty.IsEmpty)
						{
							goodsItem.BY_CustomsThirdUnitQty = unit;
						}
						else if (goodsItem.BY_CustomsFourthUnitQty.IsEmpty)
						{
							goodsItem.BY_CustomsFourthUnitQty = unit;
						}
					}
				}
			}
		}

		static bool UnitIsDeclared(ZString unit, NctsCommonCargoDesc goodsItem)
		{
			var unitAndConversionsList = GetContainingList(unit, goodsItem.ConvertibleUnitsOfMeasureSets);
			var customsUnitsList = new List<string>() { goodsItem.BY_CustomsSecondUnitQty, goodsItem.BY_CustomsThirdUnitQty, goodsItem.BY_CustomsFourthUnitQty };

			if (goodsItem is NctsDepartureCargoDesc departureGoodItem)
			{
				customsUnitsList.Add(departureGoodItem.CustomsFirstUnitQtyKilograms);
			}

			return customsUnitsList.Intersect(unitAndConversionsList).Any();
		}

		static IReadOnlyCollection<string> GetContainingList(string unit, IReadOnlyCollection<HashSet<string>> lists)
		{
			foreach (var list in lists)
			{
				if (list.Contains(unit))
				{
					return list;
				}
			}

			return new List<string>() { unit };
		}
	}
}
