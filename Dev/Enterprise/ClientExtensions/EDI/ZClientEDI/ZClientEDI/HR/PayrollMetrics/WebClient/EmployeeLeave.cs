using System;
using System.Collections.Generic;

namespace Enterprise.Client.EDI.HR.PayrollMetrics
{
	public class EmployeeLeave
	{
		public string PayrollName;
		public string EmployeeNumber;
		public string FirstName;
		public string LastName;
		public IList<Leave> LeaveRequests;
	}

	public class Leave
	{
		public string LeavePayElement;
		public string LeaveIndicator;
		public DateTime DateFrom;
		public DateTime DateTo;
		public bool IsPartDayLeave;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public decimal HoursOrWeeks;
		public string Comment;
		public string LeaveStatus;
		public DateTime StatusModifiedTime;
	}
}
