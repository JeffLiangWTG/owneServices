using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public static class WorkingDaysHelper
	{
		public static ZDateTime GetNearestWorkingdayBefore(ZDateTime dateStart)
		{
			if (dateStart.IsValid)
			{
				var result = dateStart;

				while (!IsWorkingDay(result))
				{
					result = result.AddDays(-1);
				}

				return result;
			}

			return ZDateTime.Empty;
		}

		static bool IsWorkingDay(ZDateTime dateToCheck)
		{
			return !Holidays.Any(x => x.Date == dateToCheck.Date)
				   && dateToCheck.DayOfWeek != DayOfWeek.Saturday
				   && dateToCheck.DayOfWeek != DayOfWeek.Sunday;
		}

		static readonly ImmutableArray<ZDateTime> Holidays = ImmutableArray.Create(

			//2021
			new ZDateTime(2021, 1, 1),
			new ZDateTime(2021, 2, 15),
			new ZDateTime(2021, 2, 16),
			new ZDateTime(2021, 4, 2),
			new ZDateTime(2021, 4, 21),
			new ZDateTime(2021, 5, 1),
			new ZDateTime(2021, 6, 3),
			new ZDateTime(2021, 9, 7),
			new ZDateTime(2021, 10, 12),
			new ZDateTime(2021, 11, 2),
			new ZDateTime(2021, 11, 15),
			new ZDateTime(2021, 12, 25),

			//2022
			new ZDateTime(2022, 1, 1),
			new ZDateTime(2022, 2, 28),
			new ZDateTime(2022, 3, 1),
			new ZDateTime(2022, 4, 15),
			new ZDateTime(2022, 4, 21),
			new ZDateTime(2022, 5, 1),
			new ZDateTime(2022, 6, 16),
			new ZDateTime(2022, 7, 9),
			new ZDateTime(2022, 10, 12),
			new ZDateTime(2022, 11, 2),
			new ZDateTime(2022, 11, 15),
			new ZDateTime(2022, 12, 25),

			//2023
			new ZDateTime(2023, 1, 1),
			new ZDateTime(2023, 2, 20),
			new ZDateTime(2023, 2, 21),
			new ZDateTime(2023, 4, 7),
			new ZDateTime(2023, 4, 21),
			new ZDateTime(2023, 5, 1),
			new ZDateTime(2023, 6, 8),
			new ZDateTime(2023, 9, 7),
			new ZDateTime(2023, 10, 12),
			new ZDateTime(2023, 11, 2),
			new ZDateTime(2023, 11, 15),
			new ZDateTime(2023, 12, 25)
		);
	}
}
