using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BatchProcessor
{
	public class Schedule
	{
		public void Setup(string scheduleRegistry)
		{
			SetupSchedule(scheduleRegistry);
			SetNextScheduledTime();
		}

		public bool IsScheduledTime
		{
			get
			{
				return fWeekSchedule != null && fWeekSchedule.Count > 0 &&
						Env.Time.CurrentLocalDateTime > fNextScheduledTime;
			}
		}

		#region IsTimeToCheckSheduledActionsResult

		public bool IsTimeToCheckSheduledActionsResult
		{
			get
			{
				return fWeekSchedule != null && fWeekSchedule.Count > 0 &&
					Env.Time.CurrentLocalDateTime > fNextScheduledIntervalEndTime;
			}
		}

		#endregion

		protected Hashtable fWeekSchedule;
		protected DateTime fNextScheduledTime = Env.Time.CurrentLocalDateTime;
		protected DateTime fNextScheduledIntervalStartTime = Env.Time.CurrentLocalDateTime;
		protected DateTime fNextScheduledIntervalEndTime = Env.Time.CurrentLocalDateTime;

		#region Schedule Setup

		protected const char DaySeparator = ',';
		protected const char TimeMark = '*';
		protected const int MaxDayScheduleLength = 48;

		protected void SetupSchedule(string scheduleRegistry)
		{
			try
			{
				string[] scheduledDays = scheduleRegistry.Split(DaySeparator);
				fWeekSchedule = TableOfDays(scheduledDays);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				fWeekSchedule = null;
				throw new OdysseyException(exception.Message, exception);
			}
		}

		protected Hashtable TableOfDays(string[] scheduledDays)
		{
			Hashtable result = new Hashtable();

			for (int i = 0; i < scheduledDays.Length && i < 7; i++)
			{
				string oneDaySchedule = FormattedDaySchedule(scheduledDays[i]);
				ArrayList intervals = ListOfIntervals(oneDaySchedule);
				if (intervals.Count > 0)
				{
					result.Add((DayOfWeek)i, intervals);
				}
			}

			return result;
		}

		protected ArrayList ListOfTimes(string oneDaySchedule)
		{
			ArrayList result = new ArrayList();

			int timePos = oneDaySchedule.IndexOf(TimeMark);
			while (timePos >= 0)
			{
				int hour = timePos / 2;
				int minute = (timePos % 2) * 30;
				result.Add(new HourMinute(hour, minute));

				timePos = oneDaySchedule.IndexOf(TimeMark, timePos + 1);
			}

			result.Sort(new HourMinuteComparer());
			return result;
		}

		protected ArrayList ListOfIntervals(string oneDaySchedule)
		{
			ArrayList result = new ArrayList();

			int timePos = oneDaySchedule.IndexOf(TimeMark);
			while (timePos >= 0)
			{
				int hour = timePos / 2;
				int minute = (timePos % 2) * 30;
				HourMinute intervalStart = new HourMinute(hour, minute);

				HourMinute intervalEnd = new HourMinute(hour, minute);
				intervalEnd.AddMinutes(30);

				char[] chars = oneDaySchedule.ToCharArray();
				while ((timePos + 1) < chars.Length && chars[timePos + 1] == TimeMark)
				{
					intervalEnd.AddMinutes(30);
					timePos++;
				}

				result.Add(new TimeInterval(intervalStart, intervalEnd));

				if ((timePos + 1) < oneDaySchedule.Length)
				{
					timePos = oneDaySchedule.IndexOf(TimeMark, timePos + 1);
				}
				else
				{
					timePos = -1;
				}
			}

			result.Sort(new TimeIntervalComparer());
			return result;
		}

		protected string FormattedDaySchedule(string daySchedule)
		{
			if (daySchedule.Length > MaxDayScheduleLength)
			{
				return daySchedule.Substring(0, MaxDayScheduleLength);
			}
			else
			{
				return daySchedule;
			}
		}

		#endregion

		#region Set Next Scheduled Time

		public void SetNextScheduledTime()
		{
			TimeInterval interval;
			if (NextTimeInTheSameDay(out interval))
			{
				fNextScheduledIntervalStartTime = interval.Start.AddTimePart(fNextScheduledTime);
				fNextScheduledIntervalEndTime = interval.End.AddTimePart(fNextScheduledTime);
				fNextScheduledTime = fNextScheduledIntervalStartTime;
				return;
			}
			else
			{
				for (int i = 1; i <= 7; i++)
				{
					DateTime tempNextTime = fNextScheduledTime.AddDays(i).Date;
					if (FirstTimeInADay(tempNextTime, out interval))
					{
						fNextScheduledIntervalStartTime = interval.Start.AddTimePart(tempNextTime);
						fNextScheduledIntervalEndTime = interval.End.AddTimePart(tempNextTime);
						fNextScheduledTime = fNextScheduledIntervalStartTime;
						return;
					}
				}
			}
		}

		protected bool NextTimeInTheSameDay(out TimeInterval anInterval)
		{
			DayOfWeek day = fNextScheduledIntervalEndTime.DayOfWeek;
			anInterval = null;

			if (fWeekSchedule.Contains(day))
			{
				ArrayList intervals = (ArrayList)fWeekSchedule[day];
				foreach (TimeInterval interval in intervals)
				{
					if (interval.End.AddTimePart(fNextScheduledIntervalStartTime.Date) > fNextScheduledIntervalEndTime)
					{
						anInterval = interval;
						return true;
					}
				}
			}

			return false;
		}

		protected bool FirstTimeInADay(DateTime aDateTime, out TimeInterval anInterval)
		{
			anInterval = null;
			DayOfWeek day = aDateTime.DayOfWeek;
			if (fWeekSchedule.Contains(day))
			{
				ArrayList intervals = (ArrayList)fWeekSchedule[day];
				anInterval = (TimeInterval)intervals[0];
				return true;
			}
			return false;
		}

		#endregion

		#region Randomise Next Scheduled Time

		public void RandomiseNextScheduledTime()
		{
			RandomiseNextScheduledTime(0);
		}

		public void RandomiseNextScheduledTime(int reserveMinutes)
		{
			Random randomObj = new Random();
			int randomizeMax = (fNextScheduledIntervalEndTime.Hour * 60 + fNextScheduledIntervalEndTime.Minute
				- fNextScheduledIntervalStartTime.Hour * 60 - fNextScheduledIntervalStartTime.Minute);
			if (reserveMinutes < randomizeMax)
			{
				randomizeMax -= reserveMinutes;
			}

			fNextScheduledTime = fNextScheduledIntervalStartTime.AddMinutes(randomObj.Next(randomizeMax));
		}

		#endregion

		#region HourMinute Class

		protected class HourMinute
		{
			public HourMinute(int hour, int minute)
			{
				if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
				{
					throw new OdysseyException("Invalid time " + hour.ToString("00") + ":" + minute.ToString("00"));
				}
				this.Hour = hour;
				this.Minute = minute;
			}

			public bool IsAfter(TimeSpan aTime)
			{
				return (this.Hour > aTime.Hours) ||
					((this.Hour == aTime.Hours) && (this.Minute > aTime.Minutes));
			}

			public bool IsBefore(TimeSpan aTime)
			{
				return (this.Hour < aTime.Hours) ||
					((this.Hour == aTime.Hours) && (this.Minute < aTime.Minutes));
			}

			/// <summary>
			/// Adds its time part to a date only DateTime
			/// </summary>
			public DateTime AddTimePart(DateTime dateOnly)
			{
				DateTime result = dateOnly.Date;
				result = result.AddHours(this.Hour);
				result = result.AddMinutes(this.Minute);

				return result;
			}

			public void AddMinutes(int minutes)
			{
				Hour += (Minute + minutes) / 60;
				Minute = (Minute + minutes) % 60;
			}

			public int Hour;
			public int Minute;
		}

		protected class HourMinuteComparer : IComparer<HourMinute>, IComparer
		{
			int IComparer.Compare(object x, object y)
			{
				return Compare((HourMinute)x, (HourMinute)y);
			}

			public int Compare(HourMinute x, HourMinute y)
			{
				int xTotalMinutes = (x.Minute + (x.Hour * 60));
				int yTotalMinutes = (y.Minute + (y.Hour * 60));

				return (xTotalMinutes - yTotalMinutes);
			}
		}

		#endregion

		#region Time Interval

		protected class TimeInterval
		{
			public TimeInterval(HourMinute start, HourMinute end)
			{
				this.Start = start;
				this.End = end;
			}

			public HourMinute Start;
			public HourMinute End;

			public TimeSpan TimeStart
			{
				get { return new TimeSpan(Start.Hour, Start.Minute, 0); }
			}

			public TimeSpan TimeEnd
			{
				get { return new TimeSpan(End.Hour, End.Minute, 0); }
			}

			public int DurationMinutes
			{
				get { return End.Hour * 60 + End.Minute - Start.Hour * 60 - Start.Minute; }
			}
		}

		protected class TimeIntervalComparer : IComparer
		{
			int IComparer.Compare(object x, object y)
			{
				HourMinute xStart = ((TimeInterval)x).Start;
				HourMinute yStart = ((TimeInterval)y).Start;
				int xTotalMinutes = xStart.Minute + (xStart.Hour * 60);
				int yTotalMinutes = yStart.Minute + (yStart.Hour * 60);

				return (xTotalMinutes - yTotalMinutes);
			}
		}

		#endregion

	}
}
