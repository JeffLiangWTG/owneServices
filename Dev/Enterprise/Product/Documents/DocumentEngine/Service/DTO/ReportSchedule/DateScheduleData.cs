using System;

namespace Enterprise.DocumentEngine
{
	public class DateScheduleData
	{
		public string RecurrenceType { get; set; }
		public string PeriodScope { get; set; }
		public short DayNameAsDayNumber { get; set; }
		public short DayNumber { get; set; }
		public int PeriodCount { get; set; }
		public bool IsLastDay { get; set; }
		public short Hour { get; set; }
		public short MinuteOfHour { get; set; }
		public DateTime? StorageValue { get; set; }
		public DateTime? ScheduleDate { get; set; }
		public string CalculatedResult { get; set; }
	}
}
