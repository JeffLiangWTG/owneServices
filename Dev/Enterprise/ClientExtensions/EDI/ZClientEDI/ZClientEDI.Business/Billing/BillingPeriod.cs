using System;

using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingPeriodConverter
	{
		public BillingPeriodConverter()
		{
			billingTimeZone = GetBillingTimeZone();
		}
		readonly TimeZoneInfo billingTimeZone;

		public static int ToInt(ZDateTime period) => period.Year * 100 + period.Month;

		public int UtcToPeriod(DateTime utc)
		{
			DateTime billingTime;
			if (utc < MonthSwitchingFromUtcToSydneyTime)
			{
				billingTime = utc;
			}
			else if (billingTimeZone != null)
			{
				billingTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), billingTimeZone);
			}
			else
			{
				billingTime = utc.AddHours(-UtcOffsetIfBillingTimeZoneNotFound);
			}

			return ToInt(billingTime);
		}

		public ZDateTime ConvertBillingTimeZoneTimeToUtc(ZDateTime localTime)
		{
			if (billingTimeZone != null)
			{
				return TimeZoneInfo.ConvertTimeToUtc(localTime.ToDateTime(), billingTimeZone);
			}
			else
			{
				return localTime.AddHours(UtcOffsetIfBillingTimeZoneNotFound);
			}
		}

		static TimeZoneInfo GetBillingTimeZone()
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(BillingTimeZoneId);
			}
			catch (TimeZoneNotFoundException) { }
			catch (InvalidTimeZoneException) { }

			return null;
		}

		public static DateTime MonthSwitchingFromUtcToSydneyTime
		{
			get { return new DateTime(2016, 2, 1, 0, 0, 0, DateTimeKind.Unspecified); }
		}

		const string BillingTimeZoneId = "AUS Eastern Standard Time";
		const double UtcOffsetIfBillingTimeZoneNotFound = -10;
	}

	public class BillingPeriod
	{
		public BillingPeriod(ZDateTime period)
		{
			var converter = new BillingPeriodConverter();

			this.PeriodStartDate = new ZDateTime(period.Year, period.Month, 1);

			ZDateTime periodStartTimeInclusive = DateTime.SpecifyKind(PeriodStartDate.ToDateTime(), DateTimeKind.Unspecified);
			var periodEndTimeExclusive = periodStartTimeInclusive.AddMonths(1);

			this.StartTimeUtc = periodStartTimeInclusive <= BillingPeriodConverter.MonthSwitchingFromUtcToSydneyTime
							? new ZDateTime(periodStartTimeInclusive, DateTimeKind.Utc)
							: converter.ConvertBillingTimeZoneTimeToUtc(periodStartTimeInclusive);

			this.EndTimeUtc = periodStartTimeInclusive < BillingPeriodConverter.MonthSwitchingFromUtcToSydneyTime
							? new ZDateTime(periodEndTimeExclusive, DateTimeKind.Utc)
							: converter.ConvertBillingTimeZoneTimeToUtc(periodEndTimeExclusive);

			this.Period = PeriodStartDate.Year * 100 + PeriodStartDate.Month;
		}

		public ZDateTime PeriodStartDate { get; }
		public ZDateTime StartTimeUtc { get; }
		public ZDateTime EndTimeUtc { get; }
		public ZInt Period { get; }
	}
}

