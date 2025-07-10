using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class CapacityCalculationWorkingDaysTest : BMSTestCaseWithFactory
	{
		[TestDate(2013, 7, 12)]
		public void TestCapacityCalculationUsesBMSLeave()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BRO", "Brogrammer");

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, staff.PK);

			var buffer = BMSTestHelper.CreateBuffer(system);

			Factory.Save();

			AssertEquals(48.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).FullCapacity);

			staff.Holidays.RemoveAndDeleteAll();
			var holidayStart = ZDateTime.Today;
			var holidayEnd = ZDateTime.Today.AddDays(1);
			var nonFactoredHoliday = BMSTestHelper.CreateStaffHoliday(Factory, holidayStart, holidayEnd, staff.PK);
			staff.Holidays.Add(nonFactoredHoliday);
			Factory.Save();

			AssertEquals(44.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).FullCapacity);

			staff.Holidays.RemoveAndDeleteAll();
			var bmsLeaveWithNoAvailability = BMSTestHelper.CreateStaffHoliday(Factory, holidayStart, holidayEnd, staff.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0);
			staff.Holidays.Add(bmsLeaveWithNoAvailability);
			Factory.Save();

			AssertEquals(44.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).FullCapacity);

			staff.Holidays.RemoveAndDeleteAll();
			var bmsLeaveWithHalfAvailability = BMSTestHelper.CreateStaffHoliday(Factory, holidayStart, holidayEnd, staff.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 50);
			staff.Holidays.Add(bmsLeaveWithHalfAvailability);
			Factory.Save();

			AssertEquals(46.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).FullCapacity);
		}

		[TestDate(2020, 04, 27, 0, 0, 0)]
		public void TestCapacityCalculationOvernight_In30MinuteSteps()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 8 * 60, loadLimitPercent: 100);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frogrammer");
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			Factory.Save();

			const int halfHourChunksPerDay = 24 * 2;

			//reduce unit test time
			var tomorrow = new DateTime(2020, 04, 28, 0, 0, 0).AddHours(-22);
			var today = new DateTime(2020, 04, 27, 0, 0, 0).AddHours(-10);
			TestDateAttribute.Date = new DateTime(2020, 04, 27, 0, 0, 0).AddHours(-2);

			var workingHours = new List<char>(halfHourChunksPerDay);
			workingHours.AddRange("****************".ToCharArray());
			workingHours.AddRange("                                ".ToCharArray());
			AssertEquals(48, workingHours.Count);

			while (TestDateAttribute.Date < tomorrow)
			{
				for (int ix = 0; ix < 8 * 2; ix++)
				{
					AssertEquals($"Index {ix} is an asterisk", '*', workingHours[ix]);
				}

				var minutesSinceMidnight = (float)TestDateAttribute.Date.Subtract(today).TotalMinutes;
				var hoursSinceMidnight = minutesSinceMidnight / 60.0f;
				var hoursToConsider = GetHoursToConsider(hoursSinceMidnight);

				for (var countHalfHourIncrements = 0;
					countHalfHourIncrements < halfHourChunksPerDay;
					countHalfHourIncrements++)
				{
					var currentWorkingHours = string.Join(string.Empty, workingHours);
					WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staff.PK, currentWorkingHours.TrimEnd());
					Factory.Save();

					var expectedCapacity = GetExpectedCapacity(hoursToConsider, currentWorkingHours);
					var totalCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).FullCapacity;

					AssertEquals("Full capacity matches expectation", expectedCapacity, (float)totalCapacity);

					workingHours.Insert(0, workingHours.Last());
					workingHours.RemoveAt(workingHours.Count - 1);

					//delete worktime history records to reduce unit test time
					WorkingDaysTestHelper.DeleteAllStaffWorkTimes(Factory, staff.PK);
				}

				TestDateAttribute.AddMinutes(30);
			}
		}

		static float GetExpectedCapacity(string hoursToConsider, string currentWorkingHours)
		{
			const int halfHourChunksPerDay = 24 * 2;

			var expectedCapacity = 0.0f;
			for (int ix = 0; ix < halfHourChunksPerDay; ix++)
			{
				var resourceTime = currentWorkingHours[ix];
				var toConsider = hoursToConsider[ix];
				if (toConsider == '*' && resourceTime == '*')
				{
					expectedCapacity += 0.5f;
				}
			}

			return expectedCapacity;
		}

		static string GetHoursToConsider(float hoursSinceMidnight)
		{
			const int endWorkDayHour = 17;
			const int startWorkDayHour = 9;
			const int halfHourChunksPerDay = 24 * 2;

			var hoursToConsider = string.Empty;
			if (hoursSinceMidnight > endWorkDayHour)
			{
				var afterHours = WorkingDaysTestHelper.GetWorkingHoursString(hoursSinceMidnight, 24).Substring(endWorkDayHour * 2);
				var beforeHours = WorkingDaysTestHelper.GetWorkingHoursString(0, endWorkDayHour);

				hoursToConsider = beforeHours + afterHours;
			}
			else if (hoursSinceMidnight > startWorkDayHour)
			{
				hoursToConsider = new string('*', halfHourChunksPerDay);
			}
			else if (hoursSinceMidnight <= startWorkDayHour)
			{
				hoursToConsider = WorkingDaysTestHelper.GetWorkingHoursString(hoursSinceMidnight, endWorkDayHour);
				hoursToConsider = hoursToConsider + new string(' ', halfHourChunksPerDay - hoursToConsider.Length);
			}
			AssertEquals(halfHourChunksPerDay, hoursToConsider.Length);

			return hoursToConsider;
		}

		[TestDate(2020, 04, 27, 0, 0, 0)]
		public void TestCapacityCalculationOvernight_SpecificCases()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 8 * 60, loadLimitPercent: 100);
			var staffA = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRA", "Frogrammer");
			Factory.Save();
			var staffB = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRB", "Frogrammer");
			Factory.Save();
			var staffC = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRC", "Frogrammer");
			Factory.Save();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staffA.PK, WorkingDaysTestHelper.GetWorkingHoursString(09.0f, 17.0f));
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staffB.PK, WorkingDaysTestHelper.GetWorkingHoursString(11.0f, 19.0f));
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staffC.PK, WorkingDaysTestHelper.GetWorkingHoursString(15.0f, 23.0f));

			Factory.Save();

			var today = new DateTime(2020, 04, 27, 0, 0, 0).AddHours(-10);
			TestDateAttribute.Date = today.AddHours(18).AddMinutes(6);
			CombineAssertions(() =>
			{
				AssertEquals(8m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffA, buffer).FullCapacity);
				AssertEquals(6.9m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffB, buffer).FullCapacity);
				AssertEquals(6.9m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffC, buffer).FullCapacity);
			});

			today = new DateTime(2020, 04, 27, 0, 0, 0).AddHours(-10);
			TestDateAttribute.Date = today.AddHours(18).AddMinutes(19);
			CombineAssertions(() =>
			{
				AssertEquals(8m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffA, buffer).FullCapacity);
				AssertEquals(6.68m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffB, buffer).FullCapacity);
				AssertEquals(6.68m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffC, buffer).FullCapacity);
			});

			today = new DateTime(2020, 04, 27, 0, 0, 0).AddHours(-10);
			TestDateAttribute.Date = today.AddHours(19).AddMinutes(4);
			CombineAssertions(() =>
			{
				AssertEquals(8m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffA, buffer).FullCapacity);
				AssertEquals(6m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffB, buffer).FullCapacity);
				AssertEquals(5.93m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffC, buffer).FullCapacity);
			});
		}

		[TestDate(2020, 05, 12, 0, 0, 0)]
		public void TestCapacityCalculationOvernight_Starting4pm()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 8 * 60, loadLimitPercent: 100);
			var staffA = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRA", "Frogrammer");
			Factory.Save();
			var staffB = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRB", "Frogrammer");
			Factory.Save();
			var staffC = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRC", "Frogrammer");
			Factory.Save();

			var fourPMtoMidnight = WorkingDaysTestHelper.GetWorkingHoursString(16, 24);
			AssertEquals(48, fourPMtoMidnight.Length);

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Monday, fourPMtoMidnight);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Tuesday, fourPMtoMidnight);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Wednesday, fourPMtoMidnight);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Thursday, fourPMtoMidnight);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Friday, fourPMtoMidnight);

			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staffA.PK, WorkingDaysTestHelper.GetWorkingHoursString(09.0f, 17.0f));
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staffB.PK, WorkingDaysTestHelper.GetWorkingHoursString(11.0f, 19.0f));
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staffC.PK, WorkingDaysTestHelper.GetWorkingHoursString(15.0f, 23.0f));

			Factory.Save();

			var today = new DateTime(2020, 04, 27, 0, 0, 0).AddHours(-10);
			TestDateAttribute.Date = today.AddHours(16);

			CombineAssertions(() =>
			{
				AssertEquals(1m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffA, buffer).FullCapacity);
				AssertEquals(3m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffB, buffer).FullCapacity);
				AssertEquals(7m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffC, buffer).FullCapacity);
			});

			TestDateAttribute.Date = today.AddHours(16).AddMinutes(1);
			CombineAssertions(() =>
			{
				AssertEquals(8m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffA, buffer).FullCapacity);
				AssertEquals(8m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffB, buffer).FullCapacity);
				AssertEquals(8m, CapacityCalculator.GetUtilisedCapacityBreakdown(staffC, buffer).FullCapacity);
			});
		}

		[TestDate(2020, 05, 29, 0, 0, 0)]
		public void TestCapacityCalculation_ForSeconds()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 8 * 60, loadLimitPercent: 100);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frogrammer");
			Factory.Save();

			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, staff.PK, WorkingDaysTestHelper.GetWorkingHoursString(11.0f, 19.0f));
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			Factory.Save();

			var today = new DateTime(2020, 05, 29, 0, 0, 0).AddHours(-10);

			var timeIs6AM = today.AddHours(6);
			var timeIsAlmost9AM = today.AddHours(8).AddMinutes(59);
			var timeIs11AM = today.AddHours(11);
			var timeIsAlmost5PM = today.AddHours(16).AddMinutes(59);
			var timeIs9PM = today.AddHours(21);
			var secondsToTest = new int[] { 0, 1, 15, 30, 45, 59 };

			foreach (var numberOfSeconds in secondsToTest)
			{
				CombineAssertions($"Seconds are at {numberOfSeconds}", () =>
				{
					AssertCapacityForDateTime("Time is 6am", buffer, staff, timeIs6AM.AddSeconds(numberOfSeconds), 6m);
					AssertCapacityForDateTime("Time is almost 9am", buffer, staff, timeIsAlmost9AM.AddSeconds(numberOfSeconds), 6m);
					AssertCapacityForDateTime("Time at 11am", buffer, staff, timeIs11AM.AddSeconds(numberOfSeconds), 8m);
					AssertCapacityForDateTime("Time almost 5pm", buffer, staff, timeIsAlmost5PM.AddSeconds(numberOfSeconds), 8m);
					AssertCapacityForDateTime("Time is 9pm", buffer, staff, timeIs9PM.AddSeconds(numberOfSeconds), 8m);
				});
			}
		}

		protected static void AssertCapacityForDateTime(string message, BMComponent buffer, GlbStaff staff, DateTime date, decimal capacity)
		{
			TestDateAttribute.Date = date;
			AssertEquals(message, capacity, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).FullCapacity);
		}

		protected BMSystem system;

		protected override void SetUp()
		{
			base.SetUp();
			system = BMSTestHelper.CreateSystem(Factory, "DUM");
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
