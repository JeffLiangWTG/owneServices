using System;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.ProductRegistration.Common
{
	public class BillingTimeZoneInfo
	{
		public BillingTimeZoneInfo(DateTime utcTime)
		{
			currentBillingTimeZoneUtcOffset = UtcOffsetIfBillingTimeZoneNotFound;
			nextBillingTimeZoneUtcOffset = UtcOffsetIfBillingTimeZoneNotFound;
			nextUtcOffsetEffectiveTimeUtc = DateTime.UtcNow;

			var billingTimeZone = GetTimeZone();
			if (billingTimeZone != null)
			{
				var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, billingTimeZone);

				var year = localTime.Year;
				var rule = GetAdjustmentRuleForYear(billingTimeZone, year);
				var ruleNextYear = GetAdjustmentRuleForYear(billingTimeZone, year + 1);

				if (rule != null && ruleNextYear != null)
				{
					DateTime? nextOffsetEffectiveLocalTime;
					if (localTime.IsDaylightSavingTime())
					{
						nextOffsetEffectiveLocalTime = GetTransitionDateForYear(rule.DaylightTransitionEnd, year);
						if (nextOffsetEffectiveLocalTime < localTime)
						{
							nextOffsetEffectiveLocalTime = GetTransitionDateForYear(ruleNextYear.DaylightTransitionEnd, year + 1);
						}
					}
					else
					{
						nextOffsetEffectiveLocalTime = GetTransitionDateForYear(rule.DaylightTransitionStart, year);
						if (nextOffsetEffectiveLocalTime < localTime)
						{
							nextOffsetEffectiveLocalTime = GetTransitionDateForYear(ruleNextYear.DaylightTransitionStart, year + 1);
						}
					}

					if (nextOffsetEffectiveLocalTime != null)
					{
						currentBillingTimeZoneUtcOffset = billingTimeZone.GetUtcOffset(localTime).TotalHours;
						nextBillingTimeZoneUtcOffset = billingTimeZone.GetUtcOffset(nextOffsetEffectiveLocalTime.Value.AddDays(1)).TotalHours;  //Use offset on the next day to avoid ambiguous local time on the transition day
						nextUtcOffsetEffectiveTimeUtc = nextOffsetEffectiveLocalTime.Value.AddHours(-currentBillingTimeZoneUtcOffset);
					}
				}
			}
		}

		readonly double currentBillingTimeZoneUtcOffset;
		readonly double nextBillingTimeZoneUtcOffset;
		readonly DateTime nextUtcOffsetEffectiveTimeUtc;

		public double CurrentBillingTimeZoneUtcOffset { get { return currentBillingTimeZoneUtcOffset; } }
		public double NextBillingTimeZoneUtcOffset { get { return nextBillingTimeZoneUtcOffset; } }
		public DateTime NextUtcOffsetEffectiveTimeUtc { get { return nextUtcOffsetEffectiveTimeUtc; } }

		static TimeZoneInfo GetTimeZone()
		{
			TimeZoneInfo result = null;
			try
			{
				result = TimeZoneInfo.FindSystemTimeZoneById(BillingTimeZoneId);
			}
			catch (TimeZoneNotFoundException) { }
			catch (InvalidTimeZoneException) { }
			return result;
		}

		static TimeZoneInfo.AdjustmentRule GetAdjustmentRuleForYear(TimeZoneInfo timeZoneInfo, int year)
		{
			Argument.NotNull(timeZoneInfo, nameof(timeZoneInfo));

			var adjustments = timeZoneInfo.GetAdjustmentRules();
			return adjustments.Where(x => x.DateStart.Year <= year && x.DateEnd.Year >= year).FirstOrDefault();
		}

		static DateTime? GetTransitionDateForYear(TimeZoneInfo.TransitionTime transitionTime, int year)
		{
			if (transitionTime.Month >= 1 && year >= 1 && year <= 9999)
			{
				if (transitionTime.IsFixedDateRule)
				{
					return GetFixedDateRuleDate(transitionTime, year);
				}
				else
				{
					return GetFloatingDateRuleDate(transitionTime, year);
				}
			}
			else
			{
				return null;
			}
		}

		static DateTime GetFixedDateRuleDate(TimeZoneInfo.TransitionTime transitionTime, int year)
		{
			return new DateTime(year,
							   transitionTime.Month,
							   transitionTime.Day,
							   transitionTime.TimeOfDay.Hour,
							   transitionTime.TimeOfDay.Minute,
							   transitionTime.TimeOfDay.Second,
							   DateTimeKind.Unspecified);
		}

		static DateTime GetFloatingDateRuleDate(TimeZoneInfo.TransitionTime transitionTime, int year)
		{
			var localCalendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;

			int startOfWeek = transitionTime.Week * 7 - 6;

			int firstWeedayOfMonth = (int)localCalendar.GetDayOfWeek(new DateTime(year, transitionTime.Month, 1));

			int transitionDay;
			int changeDayOfWeek = (int)transitionTime.DayOfWeek;
			if (firstWeedayOfMonth <= changeDayOfWeek)
			{
				transitionDay = startOfWeek + (changeDayOfWeek - firstWeedayOfMonth);
			}
			else
			{
				transitionDay = startOfWeek + (7 - firstWeedayOfMonth + changeDayOfWeek);
			}

			if (transitionDay > localCalendar.GetDaysInMonth(year, transitionTime.Month))
			{
				transitionDay -= 7;
			}

			return new DateTime(year,
					   transitionTime.Month,
					   transitionDay,
					   transitionTime.TimeOfDay.Hour,
					   transitionTime.TimeOfDay.Minute,
					   transitionTime.TimeOfDay.Second,
					   DateTimeKind.Unspecified);
		}

		const string BillingTimeZoneId = "AUS Eastern Standard Time";
		const double UtcOffsetIfBillingTimeZoneNotFound = 10;
	}
}