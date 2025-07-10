using System;
using System.Collections;
using System.Collections.Specialized;

namespace Enterprise.BatchProcessor
{
	sealed class ScheduleForTest : Schedule
	{
		public DateTime NextScheduledTime
		{
			get { return fNextScheduledTime; }
			set { fNextScheduledTime = value; }
		}

		public DateTime NextScheduledIntervalStartTime
		{
			get { return fNextScheduledIntervalStartTime; }
			set { fNextScheduledIntervalStartTime = value; }
		}

		public DateTime NextScheduledIntervalEndTime
		{
			get { return fNextScheduledIntervalEndTime; }
			set { fNextScheduledIntervalEndTime = value; }
		}

		public DateTime NextScheduledTimePlusPlus()
		{
			DateTime result = fNextScheduledTime;
			SetNextScheduledTime();
			return result;
		}

		public StringCollection ScheduleList
		{
			get
			{
				StringCollection result = new StringCollection();
				for (int i = 0; i < 7; i++)
				{
					DayOfWeek day = (DayOfWeek)i;
					if (fWeekSchedule.Contains(day))
					{
						ArrayList daySchedule = (ArrayList)fWeekSchedule[day];
						foreach (TimeInterval interval in daySchedule)
						{
							result.Add(day.ToString() + " - From " + interval.Start.Hour.ToString("00") + ":" + interval.Start.Minute.ToString("00") + " To " + interval.End.Hour.ToString("00") + ":" + interval.End.Minute.ToString("00"));
						}
					}
				}
				return result;
			}
		}
	}
}
