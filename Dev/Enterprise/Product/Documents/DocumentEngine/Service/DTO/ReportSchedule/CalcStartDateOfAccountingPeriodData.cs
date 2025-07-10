using System;

namespace Enterprise.DocumentEngine
{
	public class CalcStartDateOfAccountingPeriodData
	{
		public int DayOfAccountingPeriod { get; set; }

		public bool IsCalculatedByDay { get; set; }

		public DateTime StartDateLocal { get; set; }
	}
}
