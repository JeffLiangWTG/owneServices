using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public partial class DutyModeList
	{
		#region LevyType2DutyModes

		static readonly ImmutableDictionary<string, string[]> levyType2DutyModesDictionary = ImmutableDictionary.CreateRange(new Dictionary<string, string[]>
		{
			{ LevyTypeList.Codes._101, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._118, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._119, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._201, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._299, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._301, new [] { Codes._1, Codes._2, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._307, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._399, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._401, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._402, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._403, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._405, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._406, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._407, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._408, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._409, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._412, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._413, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._417, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._418, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._419, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._420, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._421, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._422, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._423, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._426, new [] { Codes._1, Codes._2, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._499, new [] { Codes._1, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._501, new [] { Codes._3, Codes._4 } },
			{ LevyTypeList.Codes._502, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._503, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._506, new [] { Codes._1, Codes._2, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._510, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._601, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._602, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._603, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._605, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._606, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._608, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._609, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._611, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._698, new [] { Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._789, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._799, new [] { Codes._1, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._801, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._802, new [] { Codes._1, Codes._3, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._803, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._811, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._818, new [] { Codes._1, Codes._5, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._819, new [] { Codes._1, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._888, new [] { Codes._4 } },
			{ LevyTypeList.Codes._898, new [] { Codes._1, Codes._2, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._899, new [] { Codes._1, Codes._2, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._927, new [] { Codes._1, Codes._2, Codes._3, Codes._4, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._997, new [] { Codes._1, Codes._3, Codes._4, Codes._5, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._998, new [] { Codes._1, Codes._4, Codes._5, Codes._6, Codes._7 } },
			{ LevyTypeList.Codes._999, new [] { Codes._1, Codes._2, Codes._3, Codes._4, Codes._6, Codes._7 } }
		});

		#endregion

		public static CodeDescriptionPairList GetSupportedDutyModesByLevyType(BusinessObjectFactory factory, ZString levyType, bool isImport = false)
		{
			CodeDescriptionPairList result;
			var fullList = factory.GetCachedValue<DutyModeList>();
			var supportedDutyModes = levyType2DutyModesDictionary.ContainsKey(levyType) ? levyType2DutyModesDictionary[levyType] : System.Array.Empty<string>();
			if (supportedDutyModes.Any())
			{
				if (isImport)
				{
					supportedDutyModes = supportedDutyModes.Except(new[] { Codes._9 }).ToArray();
				}
				result = fullList.FilterListByCodes(supportedDutyModes);
			}
			else
			{
				if (isImport)
				{
					result = new UntranslatableCodeDescriptionPairList((NoResString)"DutyModeList is untranslatable");
					foreach (var code in fullList.GetAllCodes())
					{
						if (code != Codes._9)
						{
							result.AddPair(code, fullList.GetDescriptionFromCode(code));
						}
					}
				}
				else
				{
					result = fullList;
				}
			}

			return result;
		}

		public static decimal GetDutyFraction(string dutyMode)
		{
			decimal result;

			switch (dutyMode)
			{
				case Codes._3:
				case Codes._4:
					result = 0m;
					break;
				case Codes._2:
				case Codes._8:
					result = 0.5m;
					break;
				default:
					result = 1m;
					break;
			}

			return result;
		}

		public static decimal GetExciseFraction(string dutyMode)
		{
			return dutyMode == Codes._8 || dutyMode == Codes._9 ? 0m : 1m;
		}

		public static decimal GetVATFraction(string dutyMode)
		{
			decimal result;

			switch (dutyMode)
			{
				case Codes._3:
				case Codes._9:
					result = 0m;
					break;
				case Codes._2:
				case Codes._8:
					result = 0.5m;
					break;
				default:
					result = 1m;
					break;
			}

			return result;
		}

		public static decimal GetDutyFractionForVAT(string dutyMode)
		{
			decimal result;

			switch (dutyMode)
			{
				case Codes._2:
					result = 0.5m;
					break;
				case Codes._4:
					result = 0m;
					break;
				case Codes._8:
					result = 1.5m;
					break;
				default:
					result = 1m;
					break;
			}

			return result;
		}
	}
}
