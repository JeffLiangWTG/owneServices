using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Common
{
	public static class NACCSUnitConverter
	{
		public static decimal CalculateWeightInCustomsWeightUnit(ZDecimal amount, string unit, string procedureCode = "")
		{
			return CalculateValueInCustomsUnit(amount, unit, ConvertWeightToCustomsWeight, procedureCode);
		}

		public static decimal CalculateCustomsWeightInCW1Unit(ZDecimal amount, string unit, string procedureCode = "")
		{
			return CalculateValueInCustomsUnit(amount, unit, ConvertCustomsWeightToCW1Weight, procedureCode);
		}

		public static decimal CalculateVolumeInCustomsVolumeUnit(ZDecimal amount, string unit, string procedureCode = "")
		{
			return CalculateValueInCustomsUnit(amount, unit, ConvertVolumeToCustomsVolume, procedureCode);
		}

		public static decimal CalculateCustomsVolumeInCW1Unit(ZDecimal amount, string unit, string procedureCode = "")
		{
			return CalculateValueInCustomsUnit(amount, unit, ConvertCustomsVolumeToCW1Volume, procedureCode);
		}

		public static decimal CalculateValueInCustomsUnit(ZDecimal amount, string unit, Func<decimal, string, string, decimal> convertToCustomsValue, string procedureCode = "")
		{
			var res = new ZDecimal(-1);
			var scale = GetScale(procedureCode);

			res = convertToCustomsValue(amount, unit, procedureCode);

			if (res.DecimalPlaces > scale && scale > 0)
			{
				try
				{
					res = RoundToExpectedScaleNeeded(res, scale);
				}
				catch (OverflowException)
				{
					res = -1;
				}
			}

			return res;
		}

		public static ZDecimal RoundToExpectedScaleNeeded(decimal decimalInput, int expectedScale)
		{
			return Enterprise.ZArchitecture.Core.Utilities.Round(decimalInput, expectedScale);
		}

		public static bool IsCustomsWeightUnit(string unit, string procedureCode = "")
		{
			switch (procedureCode)
			{
				case JPProcedureCodeList.Codes.HCH01:
					return unit == Weight.Kilograms || unit == Weight.Pounds;
				case JPProcedureCodeList.Codes.HDF01:
					return unit == Weight.Kilograms;
				case JPProcedureCodeList.Codes.NVC01:
				case JPProcedureCodeList.Codes.VAE:
				case JPProcedureCodeList.Codes.VAN:
					return unit == Weight.Kilograms || unit == Weight.Tonnes || unit == Weight.Pounds;
				default:
					return unit == Weight.Kilograms || unit == Weight.Tonnes || unit == Weight.Pounds;
			}
		}

		public static bool IsCustomsVolumeUnit(string unit)
		{
			return unit == VolumeList.Codes.BoardFoot || unit == Volume.CubicMetres || unit == Volume.CubicFeet;
		}

		public static string ConvertToCustomsVolumeUnit(string unit)
		{
			if (IsCustomsVolumeUnit(unit))
			{
				return CargoWiseUnitToCustomsUnitVolumeDict.GetValueSafe(unit);
			}
			else
			{
				return CustomsVolumeUnitList.Codes.CubicMeters;
			}
		}

		public static string ConvertJPCustomsVolumeUnitToCW1VolumeUnit(ZString customsVolumeUnit)
		{
			return CargoWiseUnitToCustomsUnitVolumeDict.FirstOrDefault(x => x.Value == customsVolumeUnit).Key ?? string.Empty;
		}

		public static string ConvertToCustomsWeightUnit(string unit, string procedureCode = "")
		{
			if (IsCustomsWeightUnit(unit, procedureCode))
			{
				return CargoWiseUnitToCustomsUnitWeightDict.GetValueSafe(unit);
			}
			else
			{
				return CustomsWeightUnitList.Codes.Kilograms;
			}
		}

		public static string ConvertJPCustomsWeightUnitToCW1WeightUnit(ZString customsWeightUnit)
		{
			return CargoWiseUnitToCustomsUnitWeightDict.FirstOrDefault(x => x.Value == customsWeightUnit).Key ?? string.Empty;
		}

		public static decimal ConvertWeightToCustomsWeight(decimal weight, string weightUnit, string procedureCode = "")
		{
			try
			{
				return IsCustomsWeightUnit(weightUnit, procedureCode) ? weight : Weight.ConvertSafe(weight, weightUnit, Weight.Kilograms);
			}
			catch (OverflowException)
			{
				return -1;
			}
		}

		public static decimal ConvertCustomsWeightToCW1Weight(decimal customsWeight, string customsWeightUnit, string procedureCode = "")
		{
			try
			{
				return CargoWiseUnitToCustomsUnitWeightDict.ContainsValue(customsWeightUnit) ? customsWeight : Weight.ConvertSafe(customsWeight, customsWeightUnit, CustomsWeightUnitList.Codes.Kilograms);
			}
			catch (OverflowException)
			{
				return -1;
			}
		}

		public static decimal ConvertVolumeToCustomsVolume(decimal volume, string volumeUnit, string procedureCode = "")
		{
			try
			{
				return IsCustomsVolumeUnit(volumeUnit) ? volume : Volume.ConvertSafe(volume, volumeUnit, Volume.CubicMetres);
			}
			catch (OverflowException)
			{
				return -1;
			}
		}

		public static decimal ConvertCustomsVolumeToCW1Volume(decimal customsVolume, string customsVolumeUnit, string procedureCode = "")
		{
			try
			{
				return CargoWiseUnitToCustomsUnitVolumeDict.ContainsValue(customsVolumeUnit) ? customsVolume : Volume.ConvertSafe(customsVolume, customsVolumeUnit, CustomsVolumeUnitList.Codes.CubicMeters);
			}
			catch (OverflowException)
			{
				return -1;
			}
		}

		readonly static Dictionary<string, string> CargoWiseUnitToCustomsUnitWeightDict = new Dictionary<string, string>
		{
			{ Core.Constants.Weight.Kilograms, CustomsWeightUnitList.Codes.Kilograms },
			{ Core.Constants.Weight.Tonnes, CustomsWeightUnitList.Codes.Tonnes },
			{ Core.Constants.Weight.Grams, CustomsWeightUnitList.Codes.Gram },
			{ Core.Constants.Weight.Pounds, CustomsWeightUnitList.Codes.Pound }
		};

		readonly static Dictionary<string, string> CargoWiseUnitToCustomsUnitVolumeDict = new Dictionary<string, string>
		{
			{ Core.Constants.Volume.CubicMetres, CustomsVolumeUnitList.Codes.CubicMeters },
			{ VolumeList.Codes.BoardFoot, CustomsVolumeUnitList.Codes.BoardFeet },
			{ Core.Constants.Volume.CubicFeet, CustomsVolumeUnitList.Codes.CubicFeet }
		};

		static int GetScale(string procedureCode)
		{
			return procedureCode switch
			{
				JPProcedureCodeList.Codes.HDF01 => 1,
				JPProcedureCodeList.Codes.HCH01 => 1,
				JPProcedureCodeList.Codes.NVC01 => 3,
				JPProcedureCodeList.Codes.VAN => 3,
				JPProcedureCodeList.Codes.VAE => 3,
				_ => -1,
			};
		}
	}
}
