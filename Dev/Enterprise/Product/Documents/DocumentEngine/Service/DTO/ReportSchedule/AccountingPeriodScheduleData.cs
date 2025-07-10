
using System;

namespace Enterprise.DocumentEngine
{
	public class AccPeriodScheduleData
	{
		public string PeriodScope { get; set; }
		public int PeriodCount { get; set; }
		public DateTime? StorageValue { get; set; }
		public int? SchedulePeriod { get; set; }
		public string CalculatedResult { get; set; }
	}
}
