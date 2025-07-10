using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	public class ScheduleRecurrenceData
	{
		public DateTime? StartDateLocal { get; set; }
		public string RecurrenceType { get; set; }
		public int TaskPeriodCount { get; set; }
		public bool IsWeekDayOnly { get; set; }
		public double RecurringStartTimeZone { get; set; }
		public DateTime? RecurringStartTimeLocal { get; set; }
		public IReadOnlyCollection<bool> WeekDayList { get; set; }
		public bool IsMonthlyDay { get; set; }
		public int DayOfMonth { get; set; }
		public bool IsMonthlyLastDay { get; set; }
		public byte WeekDayNumber { get; set; }
		public byte WeekDayOccurrenceNumber { get; set; }
		public bool IsAccountingDay { get; set; }
		public int DayOfAccountingPeriod { get; set; }
		public bool IsAccountingLastDay { get; set; }
		public bool IsYearlyDay { get; set; }
		public byte EveryMonthNumber { get; set; }
		public byte MonthNumber { get; set; }
		public int EndAfterCount { get; set; }
		public DateTime? EndDateLocal { get; set; }
	}
}
