using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class DstRuleParser
	{
		public DstRuleParser(
			int currentYear, string startOrEndRule, string weekdayOrMonthday, string transitionTimeBaseCode,
			DateTime dstDate, int weekOfMonth, string weekdayCode, string monthCode)
		{
			transitionType = (startOrEndRule == TimeZoneConstants.DstTransitionTypeStart) ? DstTransitionType.Start : DstTransitionType.End;
			timeBase = GetTimeBaseFromCode(transitionTimeBaseCode);

			transitionDateTime = GetInitialTransitionDateTime(
				currentYear, weekdayOrMonthday, dstDate, weekOfMonth, weekdayCode, monthCode);
		}

		/// <summary>
		/// Convert transition time to local
		/// </summary>
		public DateTime GetTransitionDateTimeInCurrentLocalTime(decimal utcOffsetStandard, decimal utcOffsetDst)
		{
			TimeSpan timeSpanDelta = TimeSpan.Zero;

			if (transitionType == DstTransitionType.End && timeBase == DstTransitionTimeBase.Standard)
			{
				decimal standardToDstOffset = utcOffsetDst - utcOffsetStandard;
				timeSpanDelta = new TimeSpan(0, 0, (int)(standardToDstOffset * 3600m));
			}
			else if (transitionType == DstTransitionType.End && timeBase == DstTransitionTimeBase.Utc)
			{
				timeSpanDelta = new TimeSpan(0, 0, (int)(utcOffsetDst * 3600m));
			}
			else if (transitionType == DstTransitionType.Start && timeBase == DstTransitionTimeBase.Utc)
			{
				timeSpanDelta = new TimeSpan(0, 0, (int)(utcOffsetStandard * 3600m));
			}

			DateTime result = transitionDateTime.AddTicks(timeSpanDelta.Ticks);
			return result;
		}

		/// <summary>
		/// Convert transition time to UTC
		/// </summary>
		public DateTime GetTransitionDateTimeInUtc(TimeSpan utcOffsetStandard, TimeSpan utcOffsetDst)
		{
			DateTime result = transitionDateTime;

			if (timeBase != DstTransitionTimeBase.Utc)
			{
				if (timeBase == DstTransitionTimeBase.Local && transitionType == DstTransitionType.End)
				{
					result = result.Subtract(utcOffsetDst);
				}
				else
				{
					result = result.Subtract(utcOffsetStandard);
				}
			}

			return result;
		}

		public DateTime TransitionDateTime
		{
			get { return transitionDateTime; }
		}

		readonly DateTime transitionDateTime;

		public DstTransitionType TransitionType
		{
			get { return transitionType; }
		}

		readonly DstTransitionType transitionType;

		readonly DstTransitionTimeBase timeBase;

		protected DateTime GetInitialTransitionDateTime(
			int currentYear, string weekdayOrMonthday, DateTime dstDate,
			int weekOfMonth, string weekdayCode, string monthCode)
		{
			int transitionYear = currentYear;
			int transitionMonth = -1;
			int transitionDay = -1;
			int transitionHour = dstDate.Hour;
			int transitionMinute = dstDate.Minute;
			int transitionSecond = dstDate.Second;

			if (weekdayOrMonthday == TimeZoneConstants.DstRuleDayOfMonth)
			{
				transitionMonth = dstDate.Month;
				transitionDay = dstDate.Day;
			}
			else
			{
				transitionMonth = TimeZoneConstants.GetMonthAsInt(monthCode);
				transitionDay = GetDstTransitionDayFromNthWeekOfMonth(transitionYear, transitionMonth, weekOfMonth, weekdayCode);
			}

			return new DateTime(currentYear, transitionMonth, transitionDay, transitionHour, transitionMinute, transitionSecond);
		}

		DstTransitionTimeBase GetTimeBaseFromCode(string transitionTimeBaseCode)
		{
			DstTransitionTimeBase result = DstTransitionTimeBase.Local;

			switch (transitionTimeBaseCode)
			{
				case TimeZoneConstants.DstTimeBaseStandard:
					result = DstTransitionTimeBase.Standard;
					break;

				case TimeZoneConstants.DstTimeBaseLocal:
					result = DstTransitionTimeBase.Local;
					break;

				case TimeZoneConstants.DstTimeBaseUtc:
					result = DstTransitionTimeBase.Utc;
					break;
			}

			return result;
		}

		#region Get Date For Weekday based Daylight Savings

		protected DayOfWeek GetDayOfWeekFromCode(string weekdayCode)
		{
			switch (weekdayCode)
			{
				case "SUN":
					return DayOfWeek.Sunday;
				case "MON":
					return DayOfWeek.Monday;
				case "TUE":
					return DayOfWeek.Tuesday;
				case "WED":
					return DayOfWeek.Wednesday;
				case "THU":
					return DayOfWeek.Thursday;
				case "FRI":
					return DayOfWeek.Friday;
				case "SAT":
					return DayOfWeek.Saturday;
				default:
					throw new DstRuleParsingException("Invalid week-day code [" + weekdayCode + "]");
			}
		}

		/// <summary>
		/// Converts a "nth weekday of month" into a month-day for the input year and transition month.
		/// </summary>
		/// <param name="currentYear">Year of the DST rule</param>
		/// <param name="transitionMonth">Month of DST transition</param>
		/// <param name="weekOfMonth">The week of the month: 1st, 2nd, 3rd, 4th or Last</param>
		/// <param name="weekdayCode">Should be in the form "SUN", "MON"</param>
		/// <returns>The day of the month</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message")]
		int GetDstTransitionDayFromNthWeekOfMonth(int currentYear, int transitionMonth, int weekOfMonth, string weekdayCode)
		{
			int result = -1;

			if (transitionMonth >= 1 && transitionMonth <= 12 && weekOfMonth >= 1 && weekOfMonth <= 5)
			{
				DayOfWeek weekday = GetDayOfWeekFromCode(weekdayCode);
				DateTime auxDate = new DateTime(currentYear, transitionMonth, 1);

				while (auxDate.DayOfWeek != weekday && auxDate.Day < 7)
				{
					auxDate = auxDate.AddDays(1);
				}

				if (auxDate.DayOfWeek == weekday)
				{
					auxDate = auxDate.AddDays((weekOfMonth - 1) * 7);

					if (transitionMonth != auxDate.Month && weekOfMonth == 5)
					{
						auxDate = auxDate.AddDays(-7);
					}

					result = auxDate.Day;
				}
			}

			if (result == -1)
			{
				string baseMessage =
					"Unable to determine day of the month with the provided arguments: "
					+ "Year = " + currentYear.ToString()
					+ ", Month = " + transitionMonth.ToString()
					+ ", Week = " + weekOfMonth.ToString()
					+ ", Weekday = [" + weekdayCode + "]";
				throw new DstRuleParsingException(baseMessage);
			}

			return result;
		}

		#endregion

	}
}
