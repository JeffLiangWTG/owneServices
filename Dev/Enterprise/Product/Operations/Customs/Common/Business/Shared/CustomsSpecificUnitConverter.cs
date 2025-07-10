using System.Collections.Generic;
using IUnitConverter = Enterprise.MasterFiles.Business.IUnitConverter;

namespace Enterprise.Customs.Common
{
	public static class CustomsSpecificUnitConverter
	{
		public static IEnumerable<IUnitConverter> GetUnitConversionFactorsFromProductUnits(this MasterFiles.Business.OrgSupplierPart part)
		{
			var netWeightGrossWeightRatio = Core.Constants.Weight.ContainsCode(part.OP_WeightUQ) && part.OP_Weight > 0m ?
				part.OP_NetWeight / part.OP_Weight : 1m;

			bool weightConversionReturned = false;

			foreach (IUnitConverter unitConverter in part.PartUnits)
			{
				if (Core.Constants.Weight.ContainsCode(unitConverter.ChildUnit) || Core.Constants.Weight.ContainsCode(unitConverter.ParentUnit))
				{
					if (netWeightGrossWeightRatio > 0m)
					{
						weightConversionReturned = true;
						yield return new MasterFiles.Business.UnitConversion(netWeightGrossWeightRatio * unitConverter.ConversionFactor, unitConverter.ChildUnit, unitConverter.ParentUnit);
					}
				}
				else
				{
					yield return unitConverter;
				}
			}

			if (!weightConversionReturned && part.OP_NetWeight > 0m && Core.Constants.Weight.ContainsCode(part.OP_WeightUQ))
			{
				yield return new MasterFiles.Business.UnitConversion(part.OP_NetWeight, part.OP_WeightUQ, part.OP_StockKeepingUnit);
			}
		}

		public static bool HasSpecificUnitConversions(this MasterFiles.Business.OrgSupplierPart part)
		{
			return part != null && (part.OP_NetWeight > 0m && Core.Constants.Weight.ContainsCode(part.OP_WeightUQ)) || part.PartUnits.Count > 0;
		}
	}
}

