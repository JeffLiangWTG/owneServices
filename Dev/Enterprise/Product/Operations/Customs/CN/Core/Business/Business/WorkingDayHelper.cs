using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public static class WorkingDayHelper
	{
		public static ZDateTime GetNearestWorkingdayAfter(ZDateTime date)
		{
			var result = ZDateTime.Empty;

			if (date.IsValid)
			{
				result = date.AddDays(1);

				while (!IsWorkingDay(result))
				{
					result = result.AddDays(1);
				}
			}

			return result;
		}

		static bool IsWorkingDay(ZDateTime date)
		{
			return !Holidays.Any(x => x.Date == date.Date)
				&& ((date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday) || WorkingDays.Contains(date.Date));
		}

		static readonly ImmutableArray<ZDateTime> Holidays = ImmutableArray.Create(
			new ZDateTime(2020, 1, 1),
			new ZDateTime(2020, 1, 24),
			new ZDateTime(2020, 1, 25),
			new ZDateTime(2020, 1, 26),
			new ZDateTime(2020, 1, 27),
			new ZDateTime(2020, 1, 28),
			new ZDateTime(2020, 1, 29),
			new ZDateTime(2020, 1, 30),
			new ZDateTime(2020, 1, 31),
			new ZDateTime(2020, 2, 1),
			new ZDateTime(2020, 2, 2),
			new ZDateTime(2020, 4, 4),
			new ZDateTime(2020, 4, 5),
			new ZDateTime(2020, 4, 6),
			new ZDateTime(2020, 5, 1),
			new ZDateTime(2020, 5, 2),
			new ZDateTime(2020, 5, 3),
			new ZDateTime(2020, 5, 4),
			new ZDateTime(2020, 5, 5),
			new ZDateTime(2020, 6, 25),
			new ZDateTime(2020, 6, 26),
			new ZDateTime(2020, 6, 27),
			new ZDateTime(2020, 10, 1),
			new ZDateTime(2020, 10, 2),
			new ZDateTime(2020, 10, 3),
			new ZDateTime(2020, 10, 4),
			new ZDateTime(2020, 10, 5),
			new ZDateTime(2020, 10, 6),
			new ZDateTime(2020, 10, 7),
			new ZDateTime(2020, 10, 8),
			new ZDateTime(2021, 1, 1)
		);

		static readonly ImmutableArray<ZDateTime> WorkingDays = ImmutableArray.Create(
			new ZDateTime(2020, 1, 19),
			new ZDateTime(2020, 4, 26),
			new ZDateTime(2020, 5, 9),
			new ZDateTime(2020, 6, 28),
			new ZDateTime(2020, 9, 27),
			new ZDateTime(2020, 10, 10)
		);
	}
}
