using System;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class PeriodCalculator
	{
		public PeriodCalculator(string periodFormat, DateTime startDate, string periodEndWeekDay)
		{
			this.PeriodFormat = periodFormat;
			this.StartDate = startDate;
			this.PeriodEndWeekDay = periodEndWeekDay;
		}

		public PeriodCalculator(string periodFormat, DateTime startDate, string periodEndWeekDay, int year)
		{
			this.PeriodFormat = periodFormat;
			this.StartDate = startDate;
			this.PeriodEndWeekDay = periodEndWeekDay;
			this.year = year;
		}
		int year;

		public int PeriodsPerYear
		{
			get
			{
				switch (PeriodFormat)
				{
					case "445": return 12;
					case "4WK": return 13;
					case "1WK": return 52;
					case "CAL":	return 12;
					default: return -1;
				}
			}
		}

		public DateTime FirstPeriodEndDate()
		{
			if (PeriodFormat == "CAL")
			{
				return StartDate.AddDays(DateTime.DaysInMonth(StartDate.Year, StartDate.Month)).AddSeconds(-1);
			}
			else
			{
				int firstDayOfFiscalYearDayOfWeek = (int)StartDate.DayOfWeek;
				int diff = PeriodEndWeekDayNo - firstDayOfFiscalYearDayOfWeek;
				if (diff < 0)
				{
					diff += 7;
				}

				DateTime result;
				if (diff >= WeekBreak)
				{
					result = StartDate.AddDays(diff);
					if ((PeriodFormat == "445") || (PeriodFormat == "4WK"))
					{
						result = result.AddDays(21);
					}
				}
				else
				{
					result = StartDate.AddDays(diff + FirstPeriodLength);
				}

				return result.AddDays(1).AddSeconds(-1);
			}
		}

		public int GetPeriodFromDate(DateTime periodDate)
		{
			DateTime endOfAPeriod = FirstPeriodEndDate();
			if (periodDate <= endOfAPeriod)
			{
				return Year * 100 + 1;
			}
			else
			{
				for (int period = 2; period <= PeriodsPerYear; period++)
				{
					DateTime end = GetEndDayOfPeriod(Year * 100 + period);
					if (periodDate <= end)
					{
						return Year * 100 + period;
					}
				}
				return -1;
			}
		}

		public DateTime GetStartDayOfPeriod(int period)
		{
			int year = period / 100;
			int periodNumber = period - year * 100;
			if (periodNumber == 1)
			{
				return StartDate;
			}
			else
			{
				switch (PeriodFormat)
				{
					case "445":
						DateTime result = FirstPeriodEndDate().AddSeconds(1);
						for (int i = 2; i < periodNumber; i++)
						{
							int daysIncrement = ((i % 3) == 0) ? 7 * 5 : 7 * 4;
							result = result.AddDays(daysIncrement);
						}
						return result;

					case "CAL":
						return StartDate.AddMonths(periodNumber - 1);

					case "1WK":
					case "4WK":
						return FirstPeriodEndDate().AddSeconds(1).AddDays(FirstPeriodLength * (periodNumber - 2));

					default:
						return new DateTime(1, 1, 1);
				}
			}
		}

		public DateTime GetEndDayOfPeriod(int period)
		{
			int year = period / 100;
			int periodNumber = period - year * 100;
			if (periodNumber == 1)
			{
				return FirstPeriodEndDate();
			}
			else if (periodNumber == PeriodsPerYear)
			{
				return EndDate;
			}
			else
			{
				switch (PeriodFormat)
				{
					case "445":
						DateTime result = FirstPeriodEndDate();
						for (int i = 2; i <= periodNumber; i++)
						{
							int daysIncrement = ((i % 3) == 0) ? 7 * 5 : 7 * 4;
							result = result.AddDays(daysIncrement);
						}
						return result;

					case "CAL":
						return StartDate.AddMonths(periodNumber).AddSeconds(-1);

					case "1WK":
					case "4WK":
						return FirstPeriodEndDate().AddDays(FirstPeriodLength * (periodNumber - 1));

					default:
						return new DateTime(1, 1, 1);
				}
			}
		}

		public int GetNextPeriod(int period, bool isExtending = false)
		{
			int year = period / 100;
			int currentPeriod = period - year * 100;
			return !isExtending && currentPeriod + 1 > PeriodsPerYear ? (year + 1) * 100 + 1 : period + 1;
		}

		public int GetPreviousPeriod(int period)
		{
			int year = period / 100;
			int currentPeriod = period - year * 100;
			if (currentPeriod - 1 == 0)
			{
				return (year - 1) * 100 + PeriodsPerYear;
			}
			else
			{
				return (year * 100) + (currentPeriod - 1);
			}
		}

		public ZDateTime GetEndDayOfPeriodForExtend(int period, ZDateTime startDate)
		{
			var year = period / 100;
			var periodNumber = period - year * 100;
			switch (PeriodFormat)
			{
				case "445":
					var result = startDate;
					var daysIncrement = ((periodNumber % 3) == 0) ? 7 * 5 : 7 * 4;
					result = result.AddDays(daysIncrement).AddSeconds(-1);
					return result;

				case "CAL":
					return startDate.Day == 1 ? startDate.AddMonths(1).AddSeconds(-1) : new ZDateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddSeconds(-1);

				case "1WK":
				case "4WK":
					return startDate.AddDays(FirstPeriodLength).AddSeconds(-1);

				default:
					return new DateTime(1, 1, 1);
			}
		}

		#region Implementation

		const int WeekBreak = 3;
		protected string PeriodFormat;
		protected DateTime StartDate;
		protected string PeriodEndWeekDay;
		protected int PeriodEndWeekDayNo
		{
			get { return GetDayNo(PeriodEndWeekDay); }
		}

		protected DateTime EndDate
		{
			get { return StartDate.AddYears(1).AddSeconds(-1); }
		}

		protected int Year
		{
			get
			{
				if (year == 0)
				{
					year = EndDate.Year;
				}
				return year;
			}
		}

		protected int GetDayNo(string dayString)
		{
			switch (dayString)
			{
				case "SUN": return 0;
				case "MON": return 1;
				case "TUE": return 2;
				case "WED": return 3;
				case "THU": return 4;
				case "FRI": return 5;
				case "SAT": return 6;
				default: return -1;
			}
		}

		protected int FirstPeriodLength
		{
			get
			{
				switch (PeriodFormat)
				{
					case "445":
					case "4WK":
						return 4 * 7;

					case "1WK":
						return 7;

					default:
						return -1;
				}
			}
		}

		#endregion
	}
}
