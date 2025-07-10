using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.ZArchitecture
{
	public static class TotalCalculation
	{
		#region Weight

		public static decimal GetTotalWeight(IEnumerable<BusinessObject> parentCollection, string valuePropertyName, string unitCodePropertyName, ZString targetUnitCode)
		{
			return GetTotalWeight(parentCollection, x => x[valuePropertyName], x => x[unitCodePropertyName], targetUnitCode);
		}

		public static decimal GetTotalWeight<T>(IEnumerable<T> parentCollection, Func<T, object> getMeasurementQuantity, Func<T, object> getMeasurementUnit, ZString targetUnitCode)
		{
			return GetTotalMeasurement(parentCollection, getMeasurementQuantity, getMeasurementUnit, targetUnitCode, IsValidWeightUnit, (val, sourceUnit, targetUnit) => Constants.Weight.Convert(val, sourceUnit, targetUnitCode));
		}

		#endregion

		#region Volume

		public static decimal GetTotalVolume(IEnumerable<BusinessObject> parentCollection, string valuePropertyName, string unitCodePropertyName, ZString targetUnitCode)
		{
			return GetTotalVolume(parentCollection, x => x[valuePropertyName], x => x[unitCodePropertyName], targetUnitCode);
		}

		public static decimal GetTotalVolume<T>(IEnumerable<T> parentCollection, Func<T, object> getMeasurementQuantity, Func<T, object> getMeasurementUnit, ZString targetUnitCode)
		{
			return GetTotalMeasurement(parentCollection, getMeasurementQuantity, getMeasurementUnit, targetUnitCode, IsValidVolumeUnit, (val, sourceUnit, targetUnit) => Constants.Volume.Convert(val, sourceUnit, targetUnitCode));
		}

		#endregion

		#region Length

		public static decimal GetTotalLength(IEnumerable<BusinessObject> parentCollection, string valuePropertyName, string unitCodePropertyName, ZString targetUnitCode)
		{
			return GetTotalLength(parentCollection, x => x[valuePropertyName], x => x[unitCodePropertyName], targetUnitCode);
		}

		public static decimal GetTotalLength<T>(IEnumerable<T> parentCollection, Func<T, object> getMeasurementQuantity, Func<T, object> getMeasurementUnit, ZString targetUnitCode)
		{
			return GetTotalMeasurement(parentCollection, getMeasurementQuantity, getMeasurementUnit, targetUnitCode, IsValidLengthUnit, (val, sourceUnit, targetUnit) => Constants.Length.Convert(val, sourceUnit, targetUnit));
		}

		#endregion

		#region No unit

		public static decimal GetTotal(IEnumerable<BusinessObject> parentCollection, string valuePropertyName)
		{
			decimal result = 0m;
			foreach (BusinessObject child in parentCollection)
			{
				result += GetDecimalFromObject(child[valuePropertyName]);
			}

			return result;
		}

		#endregion

		#region Implementation

		static decimal GetTotalMeasurement<T>(IEnumerable<T> parentCollection, Func<T, object> getMeasurementQuantity, Func<T, object> getMeasurementUnit, ZString targetUnitCode, Func<ZString, bool> isValidUnit, Func<decimal, string, string, decimal> convert)
		{
			return isValidUnit(targetUnitCode)
				? parentCollection.Select(x => new { Value = GetDecimalFromObject(getMeasurementQuantity(x)), Unit = new ZString(getMeasurementUnit(x)) }).Where(x => isValidUnit(x.Unit)).Sum(x => convert(x.Value, x.Unit, targetUnitCode))
				: 0;
		}

		static decimal GetDecimalFromObject(object value)
		{
			decimal result;
			if (value is IZType)
			{
				result = new ZDecimal(((IZTypeInternals)value).GetValueForLogicalDataLayer(false));
			}
			else // assume it's just a .NET primitive type (decimal, int, float etc.)
			{
				result = Convert.ToDecimal(value);
			}

			return result;
		}

		static bool IsValidWeightUnit(ZString unitCode)
		{
			return Constants.Weight.ContainsCode(unitCode.ToString());
		}

		static bool IsValidVolumeUnit(ZString unitCode)
		{
			return Constants.Volume.ContainsCode(unitCode.ToString());
		}

		static bool IsValidLengthUnit(ZString unitCode)
		{
			return Constants.Length.ContainsCode(unitCode.ToString());
		}

		#endregion
	}
}
