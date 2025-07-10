using CargoWise.Types;

namespace Enterprise.Client.EDI
{
	public static class DateRange
	{
		public static bool HasOverlap(ZDateTime start1, ZDateTime end1, ZDateTime start2, ZDateTime end2)
		{
			return (start2.IsEmpty || end1.IsEmpty || start2 <= end1)
				&& (start1.IsEmpty || end2.IsEmpty || start1 <= end2);
		}

		public static bool HasOverlap(ZDate start1, ZDate end1, ZDate start2, ZDate end2)
		{
			return (start2.IsEmpty || end1.IsEmpty || start2 <= end1)
				&& (start1.IsEmpty || end2.IsEmpty || start1 <= end2);
		}

		/// <summary>
		/// One range starts a day after the other range ends.
		/// </summary>
		public static bool IsConsecutive(ZDate start1, ZDate end1, ZDate start2, ZDate end2)
		{
			return (!end1.IsEmpty && !start2.IsEmpty && end1.AddDays(1) == start2)
				|| (!end2.IsEmpty && !start1.IsEmpty && end2.AddDays(1) == start1);
		}

		public static ZDateTime Min(ZDateTime a, ZDateTime b)
		{
			return b.IsEmpty || a < b ? a : b;
		}

		public static ZDateTime Max(ZDateTime a, ZDateTime b)
		{
			return b.IsEmpty || a > b ? a : b;
		}

		public static ZDate Min(ZDate a, ZDate b)
		{
			return b.IsEmpty || a < b ? a : b;
		}

		public static ZDate Max(ZDate a, ZDate b)
		{
			return b.IsEmpty || a > b ? a : b;
		}
	}
}

