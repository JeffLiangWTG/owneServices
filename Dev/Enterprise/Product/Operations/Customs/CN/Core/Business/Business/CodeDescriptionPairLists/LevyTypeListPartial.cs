using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public partial class LevyTypeList
	{
		#region Immutable

		static readonly ImmutableDictionary<string, string[]> procedure2LevyTypesForImport = ImmutableDictionary.CreateRange(new Dictionary<string, string[]>
		{
			{ CNRefCusProcedure.Codes._0110, new [] { Codes._101, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._405, Codes._406, Codes._408, Codes._409, Codes._412, Codes._413, Codes._418, Codes._423, Codes._426, Codes._605, Codes._606, Codes._608, Codes._609, Codes._698, Codes._789, Codes._799, Codes._888, Codes._898, Codes._927, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0130, new [] { Codes._101, Codes._301, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0139, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0200, new [] { Codes._101, Codes._299 } },
			{ CNRefCusProcedure.Codes._0214, new [] { Codes._502, Codes._999 } },
			{ CNRefCusProcedure.Codes._0245, new [] { Codes._101, Codes._301, Codes._307, Codes._898, Codes._927, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0314, new [] { Codes._502, Codes._999 } },
			{ CNRefCusProcedure.Codes._0320, new [] { Codes._501 } },
			{ CNRefCusProcedure.Codes._0345, new [] { Codes._101, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._426, Codes._501, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._789, Codes._799, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0400, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0420, new [] { Codes._101, Codes._501, Codes._999 } },
			{ CNRefCusProcedure.Codes._0444, new [] { Codes._101, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._501, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0445, new [] { Codes._101, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._501, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0446, new [] { Codes._101, Codes._301, Codes._307, Codes._403, Codes._406, Codes._412, Codes._426, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._789, Codes._799, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0513, new [] { Codes._502, Codes._999 } },
			{ CNRefCusProcedure.Codes._0544, new [] { Codes._101, Codes._301, Codes._307, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0545, new [] { Codes._101, Codes._301, Codes._307, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0615, new [] { Codes._503, Codes._999 } },
			{ CNRefCusProcedure.Codes._0642, new [] { Codes._101, Codes._301, Codes._307, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._0644, new [] { Codes._101, Codes._301, Codes._307, Codes._423, Codes._898, Codes._927, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0715, new [] { Codes._503, Codes._999 } },
			{ CNRefCusProcedure.Codes._0744, new [] { Codes._101, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._426, Codes._501, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._789, Codes._799, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0815, new [] { Codes._502, Codes._503, Codes._101, Codes._999 } },
			{ CNRefCusProcedure.Codes._0844, new [] { Codes._101, Codes._301, Codes._307, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0845, new [] { Codes._101, Codes._301, Codes._307, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1039, new [] { Codes._101, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1139, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._1210, new [] { Codes._101, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1215, new [] { Codes._503 } },
			{ CNRefCusProcedure.Codes._1239, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._1300, new [] { Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._501, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._789, Codes._799, Codes._888, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1371, new [] { Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._501, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1427, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._1500, new [] { Codes._299, Codes._426, Codes._605, Codes._606, Codes._608, Codes._789, Codes._799 } },
			{ CNRefCusProcedure.Codes._1523, new [] { Codes._426, Codes._605, Codes._606, Codes._608, Codes._789, Codes._799, Codes._999 } },
			{ CNRefCusProcedure.Codes._1616, new [] { Codes._101, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._1741, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._1831, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._2025, new [] { Codes._405, Codes._423, Codes._426, Codes._601, Codes._602, Codes._606, Codes._608, Codes._999 } },
			{ CNRefCusProcedure.Codes._2210, new [] { Codes._101, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._2225, new [] { Codes._405, Codes._423, Codes._426, Codes._603, Codes._606, Codes._608, Codes._999 } },
			{ CNRefCusProcedure.Codes._2439, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._2600, new [] { Codes._299, Codes._426, Codes._605, Codes._606, Codes._608 } },
			{ CNRefCusProcedure.Codes._2700, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._2939, new [] { Codes._101, Codes._999 } },
			{ CNRefCusProcedure.Codes._3010, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3039, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3100, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3339, new [] { Codes._101, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._413, Codes._418, Codes._426, Codes._606, Codes._608, Codes._609, Codes._789, Codes._799, Codes._801, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._3410, new [] { Codes._101, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._3511, new [] { Codes._201 } },
			{ CNRefCusProcedure.Codes._3611, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3612, new [] { Codes._101, Codes._301, Codes._401, Codes._413, Codes._609, Codes._698, Codes._801, Codes._802, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._3910, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3939, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._4019, new [] { Codes._301, Codes._506, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._4039, new [] { Codes._101, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._4200, new [] { Codes._101, Codes._299, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._4239, new [] { Codes._101, Codes._299, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._4539, new [] { Codes._101, Codes._201, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._501, Codes._502, Codes._503, Codes._506, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._4561, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._5010, new [] { Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._5014, new [] { Codes._502, Codes._999 } },
			{ CNRefCusProcedure.Codes._5015, new [] { Codes._503, Codes._999 } },
			{ CNRefCusProcedure.Codes._5200, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._5335, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._5361, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._9600, new [] { Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._9610, new [] { Codes._101, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._9639, new [] { Codes._101, Codes._999 } },
			{ CNRefCusProcedure.Codes._9700, new [] { Codes._101, Codes._201, Codes._299, Codes._301, Codes._307, Codes._399, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._501, Codes._506, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._801, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._9739, new [] { Codes._101, Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._606, Codes._608, Codes._609, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._9800, new [] { Codes._299, Codes._301, Codes._307, Codes._401, Codes._403, Codes._406, Codes._412, Codes._418, Codes._426, Codes._501, Codes._601, Codes._602, Codes._603, Codes._606, Codes._608, Codes._609, Codes._789, Codes._799, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._9839, new [] { Codes._101, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._9900, new [] { Codes._101, Codes._299, Codes._401, Codes._417, Codes._601, Codes._602, Codes._603, Codes._698, Codes._898, Codes._999 } }
		});

		static readonly ImmutableDictionary<string, string[]> procedure2LevyTypesForExport = ImmutableDictionary.CreateRange(new Dictionary<string, string[]>
		{
			{ CNRefCusProcedure.Codes._0110, new [] { Codes._101, Codes._299, Codes._301, Codes._399, Codes._413, Codes._601, Codes._602, Codes._603, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0130, new [] { Codes._101, Codes._301, Codes._399, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0139, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0200, new [] { Codes._101, Codes._299 } },
			{ CNRefCusProcedure.Codes._0214, new [] { Codes._502 } },
			{ CNRefCusProcedure.Codes._0243, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0245, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0265, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._0314, new [] { Codes._502, Codes._999 } },
			{ CNRefCusProcedure.Codes._0345, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0400, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0444, new [] { Codes._101, Codes._301, Codes._601, Codes._602, Codes._603, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0445, new [] { Codes._101, Codes._301, Codes._601, Codes._602, Codes._603, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0446, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0466, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._0513, new [] { Codes._101, Codes._502, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0544, new [] { Codes._101, Codes._301, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0545, new [] { Codes._101, Codes._301, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._0615, new [] { Codes._503 } },
			{ CNRefCusProcedure.Codes._0642, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0644, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0664, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._0715, new [] { Codes._503 } },
			{ CNRefCusProcedure.Codes._0744, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0815, new [] { Codes._502, Codes._503, Codes._101, Codes._999 } },
			{ CNRefCusProcedure.Codes._0844, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0845, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._0864, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._0865, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._1039, new [] { Codes._101, Codes._299, Codes._301, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1210, new [] { Codes._101, Codes._299, Codes._301, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1215, new [] { Codes._503 } },
			{ CNRefCusProcedure.Codes._1239, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._1300, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._1371, new [] { Codes._299, Codes._301, Codes._601, Codes._602, Codes._603, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._1427, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._1500, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._1523, new [] { Codes._101, Codes._999 } },
			{ CNRefCusProcedure.Codes._2210, new [] { Codes._101, Codes._299, Codes._301, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._2600, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._2700, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._3010, new [] { Codes._101, Codes._299, Codes._301, Codes._399, Codes._601, Codes._602, Codes._603, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._3039, new [] { Codes._101, Codes._299, Codes._301, Codes._399, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._3422, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3511, new [] { Codes._201 } },
			{ CNRefCusProcedure.Codes._3611, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3910, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._3939, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._4019, new [] { Codes._101, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._4039, new [] { Codes._101, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._4561, new [] { Codes._299 } },
			{ CNRefCusProcedure.Codes._5010, new [] { Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._5014, new [] { Codes._502, Codes._999 } },
			{ CNRefCusProcedure.Codes._5015, new [] { Codes._503, Codes._999 } },
			{ CNRefCusProcedure.Codes._5200, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._5335, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._5361, new [] { Codes._101, Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._9600, new [] { Codes._299, Codes._999 } },
			{ CNRefCusProcedure.Codes._9610, new [] { Codes._101, Codes._299, Codes._301, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._9700, new [] { Codes._101, Codes._301, Codes._399, Codes._502, Codes._503, Codes._601, Codes._602, Codes._603, Codes._898, Codes._999 } },
			{ CNRefCusProcedure.Codes._9739, new [] { Codes._101, Codes._299, Codes._301, Codes._399, Codes._898, Codes._998, Codes._999 } },
			{ CNRefCusProcedure.Codes._9900, new [] { Codes._101, Codes._299, Codes._502, Codes._503, Codes._999 } }
		});

		internal static readonly ImmutableArray<string> procedureAllowEmptyLevyTypesForImport = ImmutableArray.Create(
			CNRefCusProcedure.Codes._0245, CNRefCusProcedure.Codes._0255, CNRefCusProcedure.Codes._0258,
			CNRefCusProcedure.Codes._0300, CNRefCusProcedure.Codes._0345, CNRefCusProcedure.Codes._0446,
			CNRefCusProcedure.Codes._0456, CNRefCusProcedure.Codes._0642, CNRefCusProcedure.Codes._0644,
			CNRefCusProcedure.Codes._0654, CNRefCusProcedure.Codes._0657, CNRefCusProcedure.Codes._0700,
			CNRefCusProcedure.Codes._0744, CNRefCusProcedure.Codes._0844, CNRefCusProcedure.Codes._0845,
			CNRefCusProcedure.Codes._1233, CNRefCusProcedure.Codes._1234, CNRefCusProcedure.Codes._1741,
			CNRefCusProcedure.Codes._1831, CNRefCusProcedure.Codes._3611, CNRefCusProcedure.Codes._3910,
			CNRefCusProcedure.Codes._4400, CNRefCusProcedure.Codes._4600, CNRefCusProcedure.Codes._0200,
			CNRefCusProcedure.Codes._0400, CNRefCusProcedure.Codes._0500, CNRefCusProcedure.Codes._1200,
			CNRefCusProcedure.Codes._4500, CNRefCusProcedure.Codes._5000, CNRefCusProcedure.Codes._5033,
			CNRefCusProcedure.Codes._5034, CNRefCusProcedure.Codes._5100, CNRefCusProcedure.Codes._5200,
			CNRefCusProcedure.Codes._5300, CNRefCusProcedure.Codes._6033, CNRefCusProcedure.Codes._9600,
			CNRefCusProcedure.Codes._3939
		);

		internal static readonly ImmutableArray<string> procedureAllowEmptyLevyTypesForExport = ImmutableArray.Create(
			CNRefCusProcedure.Codes._0245, CNRefCusProcedure.Codes._0255, CNRefCusProcedure.Codes._0258,
			CNRefCusProcedure.Codes._0300, CNRefCusProcedure.Codes._0345, CNRefCusProcedure.Codes._0446,
			CNRefCusProcedure.Codes._0456, CNRefCusProcedure.Codes._0642, CNRefCusProcedure.Codes._0644,
			CNRefCusProcedure.Codes._0654, CNRefCusProcedure.Codes._0657, CNRefCusProcedure.Codes._0700,
			CNRefCusProcedure.Codes._0744, CNRefCusProcedure.Codes._0844, CNRefCusProcedure.Codes._0845,
			CNRefCusProcedure.Codes._1233, CNRefCusProcedure.Codes._1234, CNRefCusProcedure.Codes._1741,
			CNRefCusProcedure.Codes._1831, CNRefCusProcedure.Codes._3611, CNRefCusProcedure.Codes._3910,
			CNRefCusProcedure.Codes._4400, CNRefCusProcedure.Codes._4600, CNRefCusProcedure.Codes._0200,
			CNRefCusProcedure.Codes._0400, CNRefCusProcedure.Codes._0500, CNRefCusProcedure.Codes._1200,
			CNRefCusProcedure.Codes._4500, CNRefCusProcedure.Codes._5000, CNRefCusProcedure.Codes._5033,
			CNRefCusProcedure.Codes._5034, CNRefCusProcedure.Codes._5100, CNRefCusProcedure.Codes._5200,
			CNRefCusProcedure.Codes._5300, CNRefCusProcedure.Codes._6033, CNRefCusProcedure.Codes._9600,
			CNRefCusProcedure.Codes._0243, CNRefCusProcedure.Codes._3939
		);

		static readonly ImmutableArray<string> LevyTypesRequiresRelatedEntryNumbe = ImmutableArray.Create(
			CNRefCusProcedure.Codes._0255, CNRefCusProcedure.Codes._0258, CNRefCusProcedure.Codes._0265, CNRefCusProcedure.Codes._0456, CNRefCusProcedure.Codes._0466,
			CNRefCusProcedure.Codes._0500, CNRefCusProcedure.Codes._0654, CNRefCusProcedure.Codes._0657, CNRefCusProcedure.Codes._0664, CNRefCusProcedure.Codes._1500,
			CNRefCusProcedure.Codes._1523, CNRefCusProcedure.Codes._4400, CNRefCusProcedure.Codes._4561, CNRefCusProcedure.Codes._4600, CNRefCusProcedure.Codes._5361
		);

		static readonly ImmutableArray<string> LevyTypesRequiresRelatedEntryNumberForImport = ImmutableArray.Create(
			CNRefCusProcedure.Codes._1427, CNRefCusProcedure.Codes._4500
		);

		static readonly ImmutableArray<string> LevyTypesRequiresRelatedEntryNumberForExport = ImmutableArray.Create(
			CNRefCusProcedure.Codes._0300, CNRefCusProcedure.Codes._0700, CNRefCusProcedure.Codes._1300, CNRefCusProcedure.Codes._2600, CNRefCusProcedure.Codes._3100
		);

		static readonly ImmutableArray<string> levyTypesForExport = ImmutableArray.Create(
			Codes._101, Codes._118, Codes._119, Codes._201, Codes._299,
			Codes._301, Codes._399, Codes._402, Codes._407, Codes._413,
			Codes._502, Codes._503, Codes._510, Codes._601, Codes._602,
			Codes._603, Codes._704, Codes._705, Codes._707, Codes._710,
			Codes._711, Codes._898, Codes._899, Codes._997, Codes._998,
			Codes._999);

		static readonly ImmutableArray<string> levyTypesRequireManualNo = ImmutableArray.Create(
			Codes._201, Codes._401, Codes._406, Codes._413, Codes._417,
			Codes._420, Codes._421, Codes._422, Codes._423, Codes._499,
			Codes._501, Codes._502, Codes._503, Codes._506, Codes._605,
			Codes._606, Codes._608, Codes._609, Codes._611, Codes._789,
			Codes._799, Codes._801, Codes._802, Codes._999);

		static readonly ImmutableDictionary<string, string[]> supportedManualType = ImmutableDictionary.CreateRange(new Dictionary<string, string[]>
		{
			{ Codes._201, new [] { ManualType_Z } },
			{ Codes._401, new [] { ManualType_Z } },
			{ Codes._406, new [] { ManualType_Z } },
			{ Codes._413, new [] { ManualType_Z } },
			{ Codes._417, new [] { ManualType_Z } },
			{ Codes._420, new [] { ManualType_Z } },
			{ Codes._421, new [] { ManualType_Z } },
			{ Codes._422, new [] { ManualType_Z } },
			{ Codes._499, new [] { ManualType_Z } },
			{ Codes._501, new [] { ManualType_D } },
			{ Codes._506, new [] { ManualType_Z } },
			{ Codes._605, new [] { ManualType_Z } },
			{ Codes._606, new [] { ManualType_Z } },
			{ Codes._608, new [] { ManualType_Z } },
			{ Codes._609, new [] { ManualType_Z } },
			{ Codes._611, new [] { ManualType_Z } },
			{ Codes._789, new [] { ManualType_Z } },
			{ Codes._799, new [] { ManualType_Z } },
			{ Codes._801, new [] { ManualType_Z } },
			{ Codes._802, new [] { ManualType_Z } },
			{ Codes._999, new [] { ManualType_Z } }
		});

		const string ManualType_Z = "Z";
		const string ManualType_D = "D";

		#endregion

		public static bool RequiresRelatedEntryNumber(string style, bool isEntering)
		{
			return
				!isEntering && LevyTypesRequiresRelatedEntryNumbe.Contains(style)
				|| isEntering && LevyTypesRequiresRelatedEntryNumberForImport.Contains(style)
				|| LevyTypesRequiresRelatedEntryNumberForExport.Contains(style);
		}

		public static bool GetProcedureAllowEmptyLevyTypes(ZString procedure, ZBool isImport)
		{
			return isImport ? procedureAllowEmptyLevyTypesForImport.Contains(procedure) : procedureAllowEmptyLevyTypesForExport.Contains(procedure);
		}

		public static bool RequiresManualNo(ZString levyType)
		{
			return levyTypesRequireManualNo.Contains(levyType);
		}

		public static string[] GetSupportedManualTypesByLevyType(ZString levyType)
		{
			return supportedManualType.ContainsKey(levyType) ? supportedManualType[levyType] : System.Array.Empty<string>();
		}

		public static CodeDescriptionPairList GetSupportedLevyTypeList(BusinessObjectFactory factory, bool isImport)
		{
			return factory.GetCachedValue($"CN_LevyTypeList{isImport}", () =>
			{
				CodeDescriptionPairList result;
				var fullList = new LevyTypeList();
				if (isImport)
				{
					fullList.RemoveCode(Codes._510);
					result = fullList;
				}
				else
				{
					result = fullList.FilterListByCodes(levyTypesForExport.OfType<string>());
				}
				return result;
			});
		}

		public static string[] GetSupportedLevyTypesByProcedureCode(BusinessObjectFactory factory, ZString procedureCode, bool isImport)
		{
			var dictionary = isImport ? procedure2LevyTypesForImport : procedure2LevyTypesForExport;
			return dictionary.ContainsKey(procedureCode) ? dictionary[procedureCode] : System.Array.Empty<string>();
		}

		public static string[] GetProcedureCodeOnlyAllowEmptyLevyType(ZBool isImport)
		{
			return isImport ? procedureAllowEmptyLevyTypesForImport.Where(code => !procedure2LevyTypesForImport.ContainsKey(code)).ToArray()
								: procedureAllowEmptyLevyTypesForExport.Where(code => !procedure2LevyTypesForExport.ContainsKey(code)).ToArray();
		}
	}
}
