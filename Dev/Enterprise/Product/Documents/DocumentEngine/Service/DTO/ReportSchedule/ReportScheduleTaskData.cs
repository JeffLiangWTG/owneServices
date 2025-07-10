using System;

namespace Enterprise.DocumentEngine
{
	public class ReportScheduleTaskData
	{
		public DateTime? NextRunTimeLocal { get; set; }
		public Guid Branch { get; set; }
		public bool IsActive { get; set; }
		public bool IsPrivate { get; set; }
		public string ScheduleDescription { get; set; }
		public DateTime? DateScheduleFirstRun { get; set; }
		public int ScheduleActualRunCount { get; set; }
		public Guid UserFk { get; set; }
		public ScheduleRecurrenceData Recurrence { get; set; }
		public Guid? Identifier { get; set; }
	}
}
