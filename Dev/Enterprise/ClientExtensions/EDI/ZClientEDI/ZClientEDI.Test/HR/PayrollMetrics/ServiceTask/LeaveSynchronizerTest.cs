using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.HR.Test;
using Enterprise.Client.EDI.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.HR.PayrollMetrics.Testing
{
	class LeaveSynchronizerTest : TestCaseWithFactory
	{
		[TestDate(2018, 5, 25)]
		public void TestProcess()
		{
			// Monday
			var period1Day = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 2), 1);
			// Tuesday
			var periodHalfDay = LeavePeriod.CreatePartDay(new DateTime(2017, 10, 3, 9, 0, 0), 4);
			// Thu/Fri
			var period2Days = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 5), 2);
			// Mon..Wed
			var period3Days = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 9), 3);
			// Mon..Fri
			var period5Days = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 16), 5);
			var staffA = CreateStaff("AAA", "1001");
			var staffB = CreateStaff("BBB", "1002");
			var dbLeaveA1 = AddStaffHoliday(staffA, period1Day);
			var dbLeaveB1 = AddStaffHoliday(staffB, period3Days);
			Factory.Save();
			var modified = new DateTime(2017, 10, 1, 9, 0, 0);
			var empA = CreateEmployee("1001");
			var empB = CreateEmployee("1002");
			var leaveA1Cancelled = AddLeave(empA, modified, period1Day, false);
			var leaveA2 = AddLeave(empA, modified, periodHalfDay);
			var leaveA2Cancelled = AddLeave(empA, modified.AddHours(1), periodHalfDay, false);
			var leaveA2Again = AddLeave(empA, modified.AddHours(2), periodHalfDay);
			var leaveA3 = AddLeave(empA, modified.AddHours(3), period3Days);
			var leaveB1Duplicate = AddLeave(empB, modified.AddHours(4), period3Days);
			var leaveB2 = AddLeave(empB, modified.AddHours(5), period5Days);
			var leaveB3Cancelled = AddLeave(empB, modified, period1Day, false);
			var sync = new LeaveSynchronizer(Factory);
			sync.Process(new EmployeeLeave[] { empA, empB });
			var factory2 = new BusinessObjectFactory();
			var holidaysA = factory2.Load<GlbStaff>(staffA.PK).Holidays;
			var holidaysB = factory2.Load<GlbStaff>(staffB.PK).Holidays;
			AssertEquals(2, holidaysA.Count);
			AssertEquals(2, holidaysB.Count);
			AssertHoliday("A1", holidaysA[1], periodHalfDay);
			AssertHoliday("A2", holidaysA[0], period3Days);
			AssertHoliday("B1", holidaysB[1], period3Days);
			AssertHoliday("B2", holidaysB[0], period5Days);
			AssertEquals("PM autosync 2018-05-25 00:00:00Z - do not alter", holidaysA[0].GA_LeaveComment);
		}

		public void TestProcess_NumberNotFound_NameMatches()
		{
			var period1Day = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 2), 1);
			var staffA = CreateStaff("AAA", "");
			Factory.Save();
			var modified = new DateTime(2017, 10, 1, 9, 0, 0);
			var empA = CreateEmployee("1001", " AAAFirst ", " AAALast ");
			var leaveA1 = AddLeave(empA, modified, period1Day);
			var sync = new LeaveSynchronizer(Factory);
			sync.Process(new EmployeeLeave[] { empA });
			var factory2 = new BusinessObjectFactory();
			var holidaysA = factory2.Load<GlbStaff>(staffA.PK).Holidays;
			AssertEquals(1, holidaysA.Count);
			AssertHoliday("A1", holidaysA[0], period1Day);
		}

		public void TestProcess_EmployeeNotFound()
		{
			var period1Day = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 2), 1);
			var modified = new DateTime(2017, 10, 1, 9, 0, 0);
			var empA = CreateEmployee("1001", "AAAFirst", "AAALast");
			var leaveA1 = AddLeave(empA, modified, period1Day);
			var logger = new TestServiceLogger();
			var sync = new LeaveSynchronizer(Factory, logger);
			sync.Process(new EmployeeLeave[] { empA });
			AssertEquals(1, logger.Count);
			AssertEquals("Warning|Employee not found #1001 AAAFirst AAALast", logger[0]);
		}

		public void TestFind()
		{
			var modified = new DateTime(2017, 10, 1, 9, 0, 0);
			var staffA = CreateStaff("AAA", "1001");
			var staffB = CreateStaff("BBB", "1002");
			// Monday
			var period1Day = LeavePeriod.CreatePartDay(new DateTime(2017, 10, 2, 9, 0, 0), 8);
			var period1DayOffset = LeavePeriod.CreatePartDay(new DateTime(2017, 10, 2, 8, 0, 0), 8);
			// Tuesday
			var periodHalfDay1 = LeavePeriod.CreatePartDay(new DateTime(2017, 10, 3, 9, 0, 0), 4);
			var periodHalfDay2 = LeavePeriod.CreatePartDay(new DateTime(2017, 10, 3, 13, 0, 0), 4);
			var periodHalfDay1Offset = LeavePeriod.CreatePartDay(new DateTime(2017, 10, 3, 8, 0, 0), 4);
			// Mon..Fri
			var period5Days = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 16), 5);
			// overlapping leave types
			var holiday1 = AddStaffHoliday(staffA, period1Day, "ANN");
			var holiday2 = AddStaffHoliday(staffA, period1Day, "SIC");
			// 2 x half day
			var holiday3 = AddStaffHoliday(staffA, periodHalfDay1, "ANN");
			var holiday4 = AddStaffHoliday(staffA, periodHalfDay2, "SIC");
			var holidayB1 = AddStaffHoliday(staffB, periodHalfDay1, "ANN");
			var holidayB2 = AddStaffHoliday(staffB, periodHalfDay2, "ANN");
			var holidayB3 = AddStaffHoliday(staffB, period5Days, "SIC");
			Factory.Save();
			AssertFind("overlap picks correct type ANN", staffA, holiday1, period1Day, "ANN");
			AssertFind("overlap picks correct type SIC", staffA, holiday2, period1Day, "SIC");
			AssertFind("offset overlap ANN", staffA, holiday1, period1DayOffset, "ANN");
			AssertFind("offset overlap SIC", staffA, holiday2, period1DayOffset, "SIC");
			AssertFind("half day ANN", staffA, holiday3, periodHalfDay1, "ANN");
			AssertFind("half day SIC", staffA, holiday4, periodHalfDay2, "SIC");
			AssertFind("half day offset ANN", staffA, holiday3, periodHalfDay1Offset, "ANN");
			AssertFind("B morning exact match", staffB, holidayB1, periodHalfDay1, "ANN");
			AssertFind("B match start time if no match leave", staffB, holidayB1, periodHalfDay1, "SIC");
			AssertFind("B avo exact match", staffB, holidayB2, periodHalfDay2, "ANN");
			AssertFind("B avo match start if no match leave", staffB, holidayB2, periodHalfDay2, "SIC");
			AssertFind("wrong staff so no match", staffA, null, period5Days, "SIC");
			AssertFind("exact period5Days match", staffB, holidayB3, period5Days, "SIC");
			AssertFind("period5Days match except type", staffB, holidayB3, period5Days, "ANN");
		}

		public void Test38HourWeek()
		{
			var staffA = CreateStaff("AAA", "1001");
			SetWtg38HourWeek(staffA.WorkTimes);
			Factory.Save();
			var modified = new DateTime(2017, 10, 1, 9, 0, 0);
			// Monday
			var period1Day = new LeavePeriod(new DateTime(2017, 10, 2, 8, 30, 0), new DateTime(2017, 10, 2, 16, 30, 0), 8);
			// Fri..Mon
			var period2Days = new LeavePeriod(new DateTime(2017, 10, 6, 8, 30, 0), new DateTime(2017, 10, 9, 16, 30, 0), 16);
			// Week 2, Mon..Wed
			var period3Days = new LeavePeriod(new DateTime(2017, 10, 9, 8, 30, 0), new DateTime(2017, 10, 11, 17, 30, 0), 24);
			var empA = CreateEmployee("1001");
			AddLeave(empA, modified, period1Day);
			AddLeave(empA, modified, period2Days);
			AddLeave(empA, modified, period3Days);
			var sync = new LeaveSynchronizer(Factory);
			sync.Process(new EmployeeLeave[] { empA });
			var factory2 = new BusinessObjectFactory();
			var holidaysA = factory2.Load<GlbStaff>(staffA.PK).Holidays;
			AssertEquals(3, holidaysA.Count);
			AssertHoliday("A2", holidaysA[2], period1Day);
			AssertHoliday("A1", holidaysA[1], period2Days);
			AssertHoliday("A2", holidaysA[0], period3Days);
		}

		public void TestProcess_LeaveTimeBeforeWorkTime()
		{
			var staff11to7 = CreateStaff("AAA", "1001");
			var work = staff11to7.WorkTimes;
			var d1 = "                      *******  *******";
			var d2 = "                      *******  *********";
			var d3 = "                      *******  *********";
			var d4 = "                      *******  *********";
			var d5 = "                      *******  *******";
			var d6 = "";
			var d7 = "";
			work.MondayWorkingHours = d1;
			work.TuesdayWorkingHours = d2;
			work.WednesdayWorkingHours = d3;
			work.ThursdayWorkingHours = d4;
			work.FridayWorkingHours = d5;
			work.SaturdayWorkingHours = d6;
			work.SundayWorkingHours = d7;
			Factory.Save();
			var modified = new DateTime(2018, 1, 2, 9, 0, 0);
			var start = new DateTime(2018, 2, 2, 9, 0, 0);
			var periodHalfDay1 = new LeavePeriod(start, start.AddHours(3.5), 3.5m, true);
			var empA = CreateEmployee("1001");
			AddLeave(empA, modified, periodHalfDay1);
			var sync = new LeaveSynchronizer(Factory);
			sync.Process(new EmployeeLeave[] { empA });
			var factory2 = new BusinessObjectFactory();
			var holidays = factory2.Load<GlbStaff>(staff11to7.PK).Holidays;
			AssertEquals(1, holidays.Count);
			var holiday = holidays[0];
			AssertEquals(new ZDateTime(2018, 2, 2, 11, 0, 0), holiday.GA_StartTime);
			AssertEquals(holiday.GA_StartTime.AddHours(3.5), holiday.GA_EndTime);
			AssertEquals(0.5m, holiday.GA_DaysLeaveTaken);
		}

		public void TestProcessShouldReportErrorOnException()
		{
			var period1Day = LeavePeriod.CreateWholeDays9to5(new DateTime(2017, 10, 2), 1);
			// Tuesday
			var periodHalfDay = LeavePeriod.CreatePartDay(new DateTime(2017, 10, 3, 9, 0, 0), 4);
			// Mon..Fri
			var staffA = CreateStaff("AAA", "1001");
			AddStaffHoliday(staffA, period1Day);
			Factory.Save();
			var modified = new DateTime(2017, 10, 1, 9, 0, 0);
			var empA = CreateEmployee("1001");
			AddLeave(empA, modified, period1Day, false);
			AddLeave(empA, modified, periodHalfDay);
			AddLeave(empA, modified.AddHours(1), periodHalfDay, false);
			ErrorReporter.Clear();
			var sync = new LeaveSynchronizer(Factory, new ServiceLoggerForExceptionTest());
			sync.Process(new[] { empA });
			AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unhandled Exception in LeaveSynchronizer", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2018, 5, 25)]
		public void TestLoadStaff_DuplicateNumbers()
		{
			//staff A and B share the same pid, we shall pick the active staff.
			var staffA = CreateStaff("AAA", "1001");
			staffA.GS_LastActivityDate = new ZDateTime(2000, 1, 1);
			var staffB = CreateStaff("BBB", "1001");
			staffB.GS_LastActivityDate = new ZDateTime(2018, 1, 1);

			var staffC = CreateStaff("CCC", "1002");

			//staff D has two PID, we shall pick the latest one.
			var staffD = CreateStaff("DDD", "1003");
			var cert = staffD.Certificates.AddNew();
			cert.XZ_Type = "PID";
			cert.XZ_RN_NKCountryOfIssuance = "AU";
			cert.XZ_RefNumber = "1002";
			cert.XZ_SystemCreateTimeUtc = new ZDateTime(2000, 1, 1);

			Factory.Save();
			var empB = CreateEmployee("1001");
			var empC = CreateEmployee("1002");
			var empD = CreateEmployee("1003");

			var sync = new LeaveSynchronizer(Factory);
			var staffs = (Dictionary<EmployeeLeave, PayrollMetricsWorkingStaff>)CallMethod(sync, "LoadStaff", new List<EmployeeLeave>() { empB, empC, empD });
			AssertEquals(3, staffs.Count);
			AssertEquals(staffs[empB].Staff.PK, staffB.PK);
			AssertEquals(staffs[empC].Staff.PK, staffC.PK);
			AssertEquals(staffs[empD].Staff.PK, staffD.PK);
		}

		[TestDate(2018, 5, 25)]
		public void TestLoadStaff_DuplicateStaffNames()
		{
			//staff A and B share the same full names, we shall pick the active staff.
			var staffA = CreateStaff("AAA", "1008");
			staffA.GS_FullName = "jiM Green";
			staffA.GS_LastActivityDate = new ZDateTime(2000, 1, 1);
			var staffB = CreateStaff("BBB", "1009");
			staffB.GS_FullName = "Jim GrEEn";
			staffB.GS_LastActivityDate = new ZDateTime(2018, 1, 1);

			Factory.Save();
			var empB = CreateEmployee("1001", "JiM", "GREEN");

			var sync = new LeaveSynchronizer(Factory);
			var staffs = (Dictionary<EmployeeLeave, PayrollMetricsWorkingStaff>)CallMethod(sync, "LoadStaff", new List<EmployeeLeave>() { empB });
			AssertEquals(1, staffs.Count);
			AssertEquals(staffs[empB].Staff.PK, staffB.PK);
		}

		static object CallMethod(object obj, string methodName, params object[] args)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			MethodInfo mi;
			var type = obj.GetType();

			while ((mi = type.GetMethod(methodName, bindingFlags)) == null && (type = type.BaseType) != null)
			{
			}

			return mi.Invoke(obj, args);
		}

		void AssertFind(string msg, GlbStaff staff, GlbStaffHoliday expected, LeavePeriod period, string type = "ANN")
		{
			var emp = CreateEmployee("ANY");
			var leave = AddLeave(emp, new DateTime(2017, 10, 1), period, true);
			var actual = LeaveSynchronizer.Find(Factory, staff, leave, type);
			if (expected != null)
			{
				AssertEquals(msg, expected.PK, actual.PK);
			}
			else
			{
				AssertNull(msg, actual);
			}
		}

		void AssertHoliday(string msg, GlbStaffHoliday holiday, LeavePeriod period, string type = "ANN")
		{
			CombineAssertions(() =>
			{
				AssertEquals(msg + " start", period.DateFrom, holiday.GA_StartTime.ToDateTime());
				AssertEquals(msg + " end", period.DateTo, holiday.GA_EndTime.ToDateTime());
				AssertEquals(msg + " days", period.Days, (decimal)holiday.GA_DaysLeaveTaken);
				AssertEquals(msg + " type", type, (string)holiday.GA_WorkHolidayType);
			});
		}

		class LeavePeriod
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			public LeavePeriod(DateTime from, DateTime to, decimal hours, bool isPartDay = false)
			{
				DateFrom = from;
				DateTo = to;
				Hours = hours;
				IsPartDay = isPartDay;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			public static LeavePeriod CreatePartDay(DateTime from, int hours)
			{
				return new LeavePeriod(from, from.AddHours(hours), hours, true);
			}

			public static LeavePeriod CreateWholeDays9to5(DateTime from, int days)
			{
				from = from.Date;
				return new LeavePeriod(from.AddHours(9), from.AddDays(days - 1).AddHours(17), days * 8);
			}

			public readonly DateTime DateFrom;
			public readonly DateTime DateTo;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			public readonly decimal Hours;
			public readonly bool IsPartDay;
			public decimal Days => Hours / 8m;
		}

		GlbStaffHoliday AddStaffHoliday(GlbStaff staff, LeavePeriod period, string type = "ANN")
		{
			return AddStaffHoliday(staff, period.DateFrom, period.DateTo, period.Days, type);
		}

		GlbStaffHoliday AddStaffHoliday(GlbStaff staff, DateTime startTime, DateTime endTime, decimal days, string type = "ANN")
		{
			var holiday = staff.Holidays.AddNew();
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_DaysLeaveTaken = days;
			holiday.GA_StartTime = startTime;
			holiday.GA_EndTime = endTime;
			holiday.GA_WorkHolidayType = type;
			return holiday;
		}

		EmployeeLeave CreateEmployee(string payrollId, string firstName = null, string lastName = null)
		{
			return new EmployeeLeave()
			{ FirstName = firstName ?? payrollId, LastName = lastName ?? payrollId, EmployeeNumber = payrollId, LeaveRequests = new List<Leave>() };
		}

		Leave AddLeave(EmployeeLeave employee, DateTime modifiedTime, LeavePeriod period, bool approvedElseCancelled = true, string type = "Annual")
		{
			var from = period.DateFrom;
			var to = period.DateTo;
			if (!period.IsPartDay)
			{
				from = from.Date;
				to = to.Date;
			}

			return AddLeave(employee, modifiedTime, from, to, period.Hours, approvedElseCancelled, type);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		Leave AddLeave(EmployeeLeave employee, DateTime modifiedTime, DateTime from, DateTime to, decimal hoursOrWeeks, bool approvedElseCancelled = true, string type = "Annual")
		{
			var list = (List<Leave>)(employee.LeaveRequests);
			var leave = new Leave()
			{ DateFrom = from, DateTo = to, StatusModifiedTime = modifiedTime, LeaveStatus = approvedElseCancelled ? "Approved" : "Cancelled", LeavePayElement = type, HoursOrWeeks = hoursOrWeeks, IsPartDayLeave = hoursOrWeeks < 8 && type != "Long Service Leave" };
			list.Add(leave);
			return leave;
		}

		GlbStaff CreateStaff(string code, string payrollId)
		{
			return PayrollMetricsWorkingStaffTest.CreateStaff(Factory, code, payrollId);
		}

		void SetWtg38HourWeek(GlbWorkTimeCollection work)
		{
			WorkingStaffTest.SetWtg38HourWeekEightThirtyStart(work);
		}
	}
}
