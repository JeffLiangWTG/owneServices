using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.HR.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.HR.PayrollMetrics.Testing
{
	class PayrollMetricsWorkingStaffTest : TestCaseWithFactory
	{
		public void TestConvertHoursToDays()
		{
			var staff38 = CreateStaff("AAA", "1001");
			var workStaff38 = new PayrollMetricsWorkingStaff(staff38);
			WorkingStaffTest.SetWtg38HourWeekEightThirtyStart(staff38.WorkTimes);
			var mon = new DateTime(2017, 10, 2);
			var tue = new DateTime(2017, 10, 3);
			var leaveMon2Hours = CreatePartDayLeave(mon, mon.AddHours(8.5), mon.AddHours(2), 2);
			var leaveMonAvo = CreatePartDayLeave(mon, mon.AddHours(13), mon.AddHours(16.5), 3.5m);
			var leaveMon = CreateWholeDaysLeave(mon, mon, 1, 7m);
			var leaveTue2Hours = CreatePartDayLeave(tue, tue.AddHours(8.5), tue.AddHours(2), 2);
			var leaveTueAvo = CreatePartDayLeave(tue, tue.AddHours(13.5), tue.AddHours(17.5), 4m);
			var leaveTue = CreateWholeDaysLeave(tue, tue, 1, 7m);
			var leaveMonToFri = CreateWholeDaysLeave(mon, mon, 5, 38);
			AssertEquals(Utilities.Round(2m / 7m, 2), workStaff38.ConvertHoursToDays(leaveMon2Hours));
			AssertEquals(0.5m, workStaff38.ConvertHoursToDays(leaveMonAvo));
			AssertEquals(1m, workStaff38.ConvertHoursToDays(leaveMon));
			AssertEquals(0.25m, workStaff38.ConvertHoursToDays(leaveTue2Hours));
			AssertEquals(0.5m, workStaff38.ConvertHoursToDays(leaveTueAvo));
			AssertEquals(1m, workStaff38.ConvertHoursToDays(leaveTue));
			AssertEquals(5m, workStaff38.ConvertHoursToDays(leaveMonToFri));
		}

		EDIGlbStaff CreateStaff(string code, string payrollId)
		{
			return CreateStaff(Factory, code, payrollId);
		}

		public static EDIGlbStaff CreateStaff(BusinessObjectFactory factory, string code, string payrollId)
		{
			return WorkingStaffTest.CreateStaff(factory, code, Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.PID, payrollId);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static Leave CreatePartDayLeave(DateTime modifiedTime, DateTime from, DateTime to, decimal hoursOrWeeks, bool approvedElseCancelled = true, string type = "Annual")
		{
			return CreateLeave(modifiedTime, true, from, to, hoursOrWeeks, approvedElseCancelled, type);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static Leave CreateWholeDaysLeave(DateTime modifiedTime, DateTime fromDay, int days, decimal hoursOrWeeks, bool approvedElseCancelled = true, string type = "Annual")
		{
			return CreateLeave(modifiedTime, false, fromDay, fromDay.AddDays(days - 1), hoursOrWeeks, approvedElseCancelled, type);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static Leave CreateLeave(DateTime modifiedTime, bool isPartDayLeave, DateTime from, DateTime to, decimal hoursOrWeeks, bool approvedElseCancelled = true, string type = "Annual")
		{
			return new Leave()
			{ DateFrom = from, DateTo = to, StatusModifiedTime = modifiedTime, LeaveStatus = approvedElseCancelled ? "Approved" : "Cancelled", LeavePayElement = type, HoursOrWeeks = hoursOrWeeks, IsPartDayLeave = isPartDayLeave };
		}
	}
}
