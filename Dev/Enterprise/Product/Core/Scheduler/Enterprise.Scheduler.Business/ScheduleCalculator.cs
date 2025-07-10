using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Scheduler.Business
{
	public class ScheduleCalculator
	{
		public ScheduleCalculator()
		{
		}

		public ScheduleCalculator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public ScheduleCalculator(GlbCompany scheduledTaskCompany)
		{
			this.scheduledTaskCompany = scheduledTaskCompany;
		}

		public ScheduleCalculator(BusinessObjectFactory factory, GlbCompany scheduledTaskCompany)
		{
			this.factory = factory;
			this.scheduledTaskCompany = scheduledTaskCompany;
		}

		public ZDateTime GetCorrectedDate(int year, int month, int day)
		{
			if (year == 1 && month == 1 && day < 2)
			{
				var firstDay = new ZDateTime(ZDateTime.Today.Year, 1, 1);
				return firstDay < ZDateTime.Today ? new ZDateTime(ZDateTime.Today.Year + 1, 1, 1) : firstDay;
			}

			day = day <= 0 ? 1 : day;
			day = day > DateTime.DaysInMonth(year, month) ? DateTime.DaysInMonth(year, month) : day;

			return new ZDateTime(year, month, day);
		}

		public DayOfWeek ConvertToDayOfWeek(int day)
		{
			if (day < 1 || day > 7)
			{
				throw new ArgumentOutOfRangeException(nameof(day), "Day cannot be less than one or more than seven.");
			}
			return (DayOfWeek)(day - 1);
		}

		public ZDateTime CalculateDayOfWeek(ZDateTime baseDateTime, int weeksToAdd, int day)
		{
			DayOfWeek dayOfWeek = ConvertToDayOfWeek(day);
			ZDateTime result = baseDateTime;
			int daysToAdd = (dayOfWeek > baseDateTime.DayOfWeek) ? 1 : -1;
			while (result.DayOfWeek != dayOfWeek)
			{
				result = result.AddDays(daysToAdd);
			}
			result = result.AddDays(7 * weeksToAdd);
			return result;
		}

		public ZDateTime CalculateDayOfMonth(ZDateTime baseDateTime, int monthsToAdd, int day)
		{
			ZDateTime result = baseDateTime.AddMonths(monthsToAdd);
			int actualDay = (day == LastDay) ? DateTime.DaysInMonth(result.Year, result.Month) : day;
			return GetCorrectedDate(result.Year, result.Month, actualDay);
		}

		public ZDateTime CalculateDayOfAccountingPeriod(ZDateTime baseDateTime, int periodsToAdd, int day)
		{
			ZDateTime result;
			AccountingPeriodCalculator calculator = new AccountingPeriodCalculator(factory ?? new BusinessObjectFactory());
			int period = CalculateAccountingPeriod(baseDateTime, periodsToAdd);

			if (period != 0)
			{
				ZDateTime firstDay = calculator.GetFirstDayForPeriod(period);
				if (day == 1)
				{
					result = firstDay;
				}
				else
				{
					ZDateTime lastDay = calculator.GetLastDayForPeriod(period);
					if (day == LastDay)
					{
						result = lastDay;
					}
					else
					{
						result = firstDay.AddDays(day - 1);
						if (result > lastDay)
						{
							result = lastDay;
						}
					}
				}
			}
			else
			{
				result = baseDateTime;
			}

			return result;
		}

		public ZDateTime CalculateDayOfYear(ZDateTime baseDateTime, int yearsToAdd, int day)
		{
			ZDateTime result = new ZDateTime(Math.Min(9998, Math.Max(2, baseDateTime.Year + yearsToAdd)), 1, 1);
			if (day == LastDay)
			{
				result = new ZDateTime(result.Year, 12, 31);
			}
			else
			{
				int daysInYear = DateTime.IsLeapYear(result.Year) ? 366 : 365;
				int daysToAdd = (day > daysInYear) ? daysInYear : day;
				result = result.AddDays(daysToAdd - 1);
			}
			return result;
		}

		public int CalculateAccountingPeriod(ZDateTime baseDateTime, int periodsToAdd)
		{
			return CalculateAccountingPeriod(AccPeriodCalculator.GetPeriodFromDate(baseDateTime), periodsToAdd);
		}

		public int CalculateAccountingPeriod(int period, int periodsToAdd)
		{
			int result = period;

			if (result != 0)
			{
				if (periodsToAdd < 0)
				{
					ZInt tempResult;
					while ((periodsToAdd < 0) && !(tempResult = AccPeriodCalculator.GetPreviousPeriod(result)).IsEmpty)
					{
						result = tempResult;
						periodsToAdd++;
					}
				}
				else if (periodsToAdd > 0)
				{
					ZInt tempResult;
					while ((periodsToAdd > 0) && !(tempResult = AccPeriodCalculator.GetNextPeriod(result)).IsEmpty)
					{
						result = tempResult;
						periodsToAdd--;
					}
				}
			}

			return result;
		}

		public AccountingPeriodCalculator AccPeriodCalculator
		{
			get
			{
				if (accPeriodCalculator == null)
				{
					if (factory == null)
					{
						if (scheduledTaskCompany == null)
						{
							accPeriodCalculator = new AccountingPeriodCalculator(new BusinessObjectFactory());
						}
						else
						{
							accPeriodCalculator = new AccountingPeriodCalculator(new BusinessObjectFactory(), scheduledTaskCompany);
#if DEBUG
							AccountingPeriodCalculatorWasCreatedWithScheduledTaskCompany_ForTest = true;
#endif
						}
					}
					else
					{
						if (scheduledTaskCompany == null)
						{
							accPeriodCalculator = new AccountingPeriodCalculator(factory);
						}
						else
						{
							accPeriodCalculator = new AccountingPeriodCalculator(factory, scheduledTaskCompany);
#if DEBUG
							AccountingPeriodCalculatorWasCreatedWithScheduledTaskCompany_ForTest = true;
#endif
						}
					}
				}
				return accPeriodCalculator;
			}
		}

#if DEBUG
		public bool AccountingPeriodCalculatorWasCreatedWithScheduledTaskCompany_ForTest;
#endif

		const int LastDay = 0;
		AccountingPeriodCalculator accPeriodCalculator;
		readonly BusinessObjectFactory factory;

#if DEBUG
		public
#endif
 GlbCompany scheduledTaskCompany;
	}
}
