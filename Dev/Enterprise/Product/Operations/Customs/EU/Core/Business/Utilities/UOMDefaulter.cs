using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public static class UOMDefaulter
	{
		public static void DefaultUOMIfApplicable(RateView rateView, TemporaryStoragePackedItem packedItem)
		{
			if (rateView == null || packedItem == null)
			{
				return;
			}

			var rateFormula = rateView.ZZ2_RateFormula;
			var unit = rateFormula.SubstringSafe(rateFormula.IndexOf('[') + 1, rateFormula.IndexOf(']') - rateFormula.IndexOf('[') - 1);
			if (!unit.IsEmpty && rateFormula.IndexOf('[') != -1)
			{
				if (!IsUnitDeclared(unit, packedItem))
				{
					if (packedItem.API_CustomsUQ2.IsEmpty)
					{
						packedItem.API_CustomsUQ2 = unit;
					}
					else if (packedItem.API_CustomsUQ3.IsEmpty)
					{
						packedItem.API_CustomsUQ3 = unit;
					}
				}
			}
		}

		static bool IsUnitDeclared(ZString unit, TemporaryStoragePackedItem packedItem)
		{
			var convertibleUnitsOfMeasure = new List<HashSet<string>>()
			{
				ConvertibleUnitsOfMeasure.WeightConversionDictionary.Keys.ToHashSet(),
				ConvertibleUnitsOfMeasure.VolumeConversionDictionary.Keys.ToHashSet(),
				ConvertibleUnitsOfMeasure.AlcoholConversionDictionary.Keys.ToHashSet(),
			};

			var units = GetContainingList(unit, convertibleUnitsOfMeasure);
			var currentUnits = new List<string>() { packedItem.API_CustomsUQ, packedItem.API_CustomsUQ2, packedItem.API_CustomsUQ3 };
			return currentUnits.Intersect(units).Any();
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
