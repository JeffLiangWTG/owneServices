using System;
using CargoWise.Application;
using CargoWise.CalendarArithmetic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZDateEditCoreTest : TransactionedTestCase
	{
		const string AllDay = "************************************************";
		const string AllDayWithBreaks = "*****    ***********        ***********    *****";
		const string MidnightToNoon = "************************                        ";
		const string NoonToMidnight = "                        ************************";
		const string NineToFive = "                  ****************              ";
		const string NineThirtyToFourThirty = "                   **************               ";
		const string NineToFiveWithBreak = "                  ******  ********              ";
		const string NoWorkingHours = "                                                ";

		public void SetupWorkTime(BusinessObjectFactory factory, BusinessObject staffOrDepartment,
			string monday = NoWorkingHours,
			string tuesday = NoWorkingHours,
			string wednesday = NoWorkingHours,
			string thursday = NoWorkingHours,
			string friday = NoWorkingHours,
			string saturday = NoWorkingHours,
			string sunday = NoWorkingHours
			)
		{
			var workTime = staffOrDepartment["WorkTimes"];
			workTime.GetType().GetProperty("MondayWorkingHours").SetValue(workTime, (ZString)monday);
			workTime.GetType().GetProperty("TuesdayWorkingHours").SetValue(workTime, (ZString)tuesday);
			workTime.GetType().GetProperty("WednesdayWorkingHours").SetValue(workTime, (ZString)wednesday);
			workTime.GetType().GetProperty("ThursdayWorkingHours").SetValue(workTime, (ZString)thursday);
			workTime.GetType().GetProperty("FridayWorkingHours").SetValue(workTime, (ZString)friday);
			workTime.GetType().GetProperty("SaturdayWorkingHours").SetValue(workTime, (ZString)saturday);
			workTime.GetType().GetProperty("SundayWorkingHours").SetValue(workTime, (ZString)sunday);

			factory.Save();
		}

		public void SetupStaffHoliday(BusinessObjectFactory factory, BusinessObject glbStaff, DateTimeRange leaveTime,
			string approvalStatus = StaffHolidayApprovalCodes.Approved,
			string leaveRecordType = StaffHolidayRecordTypeCodes.Leave
			)
		{
			var holidays = (BusinessObjectCollection)glbStaff["Holidays"];
			var holiday = holidays.AddNew();

			holiday[GlbStaffHolidaySchema.Constants.GA_StartTime] = leaveTime.Start;
			holiday[GlbStaffHolidaySchema.Constants.GA_EndTime] = leaveTime.End;
			holiday[GlbStaffHolidaySchema.Constants.GA_GS] = glbStaff.PK;
			holiday[GlbStaffHolidaySchema.Constants.GA_ApprovalStatus] = approvalStatus;
			holiday[GlbStaffHolidaySchema.Constants.GA_RecordType] = leaveRecordType;
			holiday[GlbStaffHolidaySchema.Constants.GA_AvailabilityPercentage] = new ZByte(0);
			holiday[GlbStaffHolidaySchema.Constants.GA_IsWorkingAway] = false;

			factory.Save();
		}

		public void SetupBranchHoliday(BusinessObjectFactory factory, BusinessObject glbBranch, DateTime holidayDate, string holidayName)
		{
			var holidays = (IBusinessObjectCollection)glbBranch["GlbHolidays"];
			var holiday = holidays.AddNew();

			holiday[GlbHolidaySchema.Constants.GH_HolidayName] = holidayName;
			holiday[GlbHolidaySchema.Constants.GH_Date] = holidayDate;

			factory.Save();
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_WhenWorking()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should return today's working times", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_InFuture()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());

			SetupWorkTime(factory, staff, monday: AllDay, tuesday: MidnightToNoon);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should return tomorrow's working times", () =>
				{
					AssertConvertDate_AU("S+1", new DateTime(2019, 08, 20, 00, 00, 0));
					AssertConvertDate_AU("E+1", new DateTime(2019, 08, 20, 12, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_InPast()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());

			SetupWorkTime(factory, staff, saturday: AllDay, sunday: AllDayWithBreaks, monday: AllDay);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should return yesterday's working times", () =>
				{
					AssertConvertDate_AU("S-1", new DateTime(2019, 08, 18, 00, 00, 0));
					AssertConvertDate_AU("E-1", new DateTime(2019, 08, 19, 00, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_IgnoresDepartmentHours()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineToFive);

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.GS_GE_HomeDepartment] = department.PK;

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return staff's working times, ignoring the department's working times", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 20, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_AfterWorkHours()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty, tuesday: NineToFiveWithBreak);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should return today's working times", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_SkipsNonWorkingDays()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty, wednesday: NineToFiveWithBreak, saturday: AllDayWithBreaks);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should skip non-working days", () =>
				{
					AssertConvertDate_AU("S+1", new DateTime(2019, 08, 21, 09, 00, 0));
					AssertConvertDate_AU("E+1", new DateTime(2019, 08, 21, 17, 00, 0));
					AssertConvertDate_AU("S-1", new DateTime(2019, 08, 17, 00, 00, 0));
					AssertConvertDate_AU("E-1", new DateTime(2019, 08, 18, 00, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_WithNoHoursAtAll()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty);

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.Constants.GS_GE_HomeDepartment] = department.PK;

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return default department start and end work times of today", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_WithNoHoursToday()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty);

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.Constants.GS_GE_HomeDepartment] = department.PK;

			SetupWorkTime(factory, staff, tuesday: NineToFiveWithBreak);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return default department start and end work times of today", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_IgnoresCurrentDayLeave()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());

			var holiday = new DateTimeRange(new DateTime(2019, 08, 19), new DateTime(2019, 08, 20));

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty, tuesday: NineToFive);
			SetupStaffHoliday(factory, staff, holiday);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should ignore current day leave and return today's working hours", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_IgnoresFutureApprovedLeave()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty, tuesday: AllDay, wednesday: NineThirtyToFourThirty, thursday: MidnightToNoon, friday: NineToFive);

			var approvedHolidayLeave = new DateTimeRange(new DateTime(2019, 08, 20), new DateTime(2019, 08, 21));
			var approvedHolidayBMSLeave = new DateTimeRange(new DateTime(2019, 08, 21), new DateTime(2019, 08, 22));
			var requestedHolidayLeave = new DateTimeRange(new DateTime(2019, 08, 22), new DateTime(2019, 08, 23));

			SetupStaffHoliday(factory, staff, approvedHolidayLeave);
			SetupStaffHoliday(factory, staff, approvedHolidayBMSLeave, leaveRecordType: StaffHolidayRecordTypeCodes.BufferManagementLeave);
			SetupStaffHoliday(factory, staff, requestedHolidayLeave, approvalStatus: StaffHolidayApprovalCodes.Requested);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should ignore future staff leave and return regular working hours", () =>
				{
					AssertConvertDate_AU("S+1", new DateTime(2019, 08, 20, 00, 00, 0));
					AssertConvertDate_AU("E+1", new DateTime(2019, 08, 21, 00, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_IgnoresTodaysBranchHoliday()
		{
			var factory = new BusinessObjectFactory();
			var branch = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbBranch>());
			var branchHoliday = new DateTime(2019, 08, 19);

			SetupBranchHoliday(factory, branch, branchHoliday, "New Years in August");

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.Constants.GS_GB_HomeBranch] = branch.PK;

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty, tuesday: NoonToMidnight, wednesday: NineToFive);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should ignore today's branch holiday and return today's working times", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_SkipFutureBranchHolidays()
		{
			var factory = new BusinessObjectFactory();
			var branch = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbBranch>());
			var branchHoliday = new DateTime(2019, 08, 20);

			SetupBranchHoliday(factory, branch, branchHoliday, "Day Off");

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.Constants.GS_GB_HomeBranch] = branch.PK;

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty, tuesday: NoonToMidnight, wednesday: NineToFive);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				CombineAssertions("It should skip future branch holidays", () =>
				{
					AssertConvertDate_AU("S+1", new DateTime(2019, 08, 21, 09, 00, 0));
					AssertConvertDate_AU("E+1", new DateTime(2019, 08, 21, 17, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_WhenWorking()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return today's working times", () =>
				{
					AssertConvertDate_AU("DS", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("DE", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_InFuture()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: AllDay, tuesday: MidnightToNoon);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return tomorrow's working times", () =>
				{
					AssertConvertDate_AU("DS+1", new DateTime(2019, 08, 20, 00, 00, 0));
					AssertConvertDate_AU("DE+1", new DateTime(2019, 08, 20, 12, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_InPast()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, saturday: AllDay, sunday: AllDayWithBreaks, monday: AllDay);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return yesterday's working times", () =>
				{
					AssertConvertDate_AU("DS-1", new DateTime(2019, 08, 18, 00, 00, 0));
					AssertConvertDate_AU("DE-1", new DateTime(2019, 08, 19, 00, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_IgnoresStaffHours()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty);

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.GS_GE_HomeDepartment] = department.PK;

			SetupWorkTime(factory, staff, monday: NineToFive);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return department's working times, ignoring the staff's working times", () =>
				{
					AssertConvertDate_AU("DS", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("DE", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_IgnoresStaffHolidayLeave()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineToFive, tuesday: NineThirtyToFourThirty, wednesday: AllDay);

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.Constants.GS_GE_HomeDepartment] = department.PK;
			var approvedHolidayLeave = new DateTimeRange(new DateTime(2019, 08, 20), new DateTime(2019, 08, 21));

			SetupWorkTime(factory, staff, monday: NineThirtyToFourThirty, tuesday: AllDay, wednesday: NineThirtyToFourThirty);
			SetupStaffHoliday(factory, staff, approvedHolidayLeave);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should ignore staff holidays/leave and return regular department working times", () =>
				{
					AssertConvertDate_AU("DS+1", new DateTime(2019, 08, 20, 09, 30, 0));
					AssertConvertDate_AU("DE+1", new DateTime(2019, 08, 20, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 20, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_AfterWorkHours()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty, tuesday: NineToFiveWithBreak);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return today's working times", () =>
				{
					AssertConvertDate_AU("DS", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("DE", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_SkipsNonWorkingDays()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty, wednesday: NineToFiveWithBreak, saturday: AllDayWithBreaks);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should skip non-working days", () =>
				{
					AssertConvertDate_AU("DS+1", new DateTime(2019, 08, 21, 09, 00, 0));
					AssertConvertDate_AU("DE+1", new DateTime(2019, 08, 21, 17, 00, 0));
					AssertConvertDate_AU("DS-1", new DateTime(2019, 08, 17, 00, 00, 0));
					AssertConvertDate_AU("DE-1", new DateTime(2019, 08, 18, 00, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_WithNoHoursAtAll()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return default department hours, currently working all weekdays", () =>
				{
					AssertConvertDate_AU("DS", new DateTime(2019, 08, 19, 00, 00, 00));
					AssertConvertDate_AU("DE", new DateTime(2019, 08, 20, 00, 00, 00));
					AssertConvertDate_AU("DS-1", new DateTime(2019, 08, 16, 00, 00, 0));
					AssertConvertDate_AU("DE-1", new DateTime(2019, 08, 17, 00, 00, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 01, 05)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_WithNoHoursToday()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, tuesday: NineThirtyToFourThirty);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department.PK.ToGuid())))
			{
				CombineAssertions("It should return current time as fallback", () =>
				{
					AssertConvertDate_AU("S", new DateTime(2019, 08, 19, 12, 01, 05));
					AssertConvertDate_AU("E", new DateTime(2019, 08, 19, 12, 01, 05));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_IgnoresTodaysBranchHoliday()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty, tuesday: NineToFive);

			var branch = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbBranch>());
			var branchHoliday = new DateTime(2019, 08, 19);

			SetupBranchHoliday(factory, branch, branchHoliday, "Day Off");

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.Constants.GS_GB_HomeBranch] = branch.PK;

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid())))
			{
				CombineAssertions("It should ignore today's branch holiday and return today's working times", () =>
				{
					AssertConvertDate_AU("DS", new DateTime(2019, 08, 19, 09, 30, 0));
					AssertConvertDate_AU("DE", new DateTime(2019, 08, 19, 16, 30, 0));
				});
			}
		}

		[TestDate(2019, 08, 19, 12, 00, 0)]
		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_SkipsFutureBranchHolidays()
		{
			var factory = new BusinessObjectFactory();
			var department = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbDepartment>());

			SetupWorkTime(factory, department, monday: NineThirtyToFourThirty, tuesday: NoonToMidnight, wednesday: MidnightToNoon, thursday: NineToFive);

			var branch = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbBranch>());
			var branchHoliday1 = new DateTime(2019, 08, 20);
			var branchHoliday2 = new DateTime(2019, 08, 21);

			SetupBranchHoliday(factory, branch, branchHoliday1, "Day Off");
			SetupBranchHoliday(factory, branch, branchHoliday2, "Another Day Off");

			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff[GlbStaffSchema.Constants.GS_GB_HomeBranch] = branch.PK;

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid())))
			{
				CombineAssertions("It should skip future branch holidays", () =>
				{
					AssertConvertDate_AU("DS+1", new DateTime(2019, 08, 22, 09, 00, 0));
					AssertConvertDate_AU("DE+1", new DateTime(2019, 08, 22, 17, 00, 0));
				});
			}
		}

		public void TestGetDateTimeAsObject_RelativeDates_ToStaff_InvalidEntries()
		{
			var testControlDateEdit = (ZDateEdit)TestControl;
			testControlDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;

			AssertConvertDate_AU("EE", "EE");
			AssertConvertDate_AU("SS", "SS");
			AssertConvertDate_AU("E1", "E1");
			AssertConvertDate_AU("S1", "S1");

			AssertConvertDate_AU("E-", "E-");
			AssertConvertDate_AU("EE-1", "EE-1");
			AssertConvertDate_AU("E-E-1", "E-E-1");
			AssertConvertDate_AU("S-", "S-");
			AssertConvertDate_AU("SE-1", "SE-1");
			AssertConvertDate_AU("S-E-1", "S-E-1");

			AssertConvertDate_AU("+E", "+E");
			AssertConvertDate_AU("-E", "-E");
			AssertConvertDate_AU("+S", "+S");
			AssertConvertDate_AU("-S", "-S");
			AssertConvertDate_AU("E++", "E++");
			AssertConvertDate_AU("S++", "S++");

			AssertConvertDate_AU("ES", "ES");
			AssertConvertDate_AU("SE", "SE");
		}

		public void TestGetDateTimeAsObject_RelativeDates_ToDepartment_InvalidEntries()
		{
			var testControlDateEdit = (ZDateEdit)TestControl;
			testControlDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;

			AssertConvertDate_AU("DEDE", "DEDE");
			AssertConvertDate_AU("DSDS", "DSDS");
			AssertConvertDate_AU("DE1", "DE1");
			AssertConvertDate_AU("DS1", "DS1");

			AssertConvertDate_AU("DE-", "DE-");
			AssertConvertDate_AU("DEE-1", "DEE-1");
			AssertConvertDate_AU("DE-E-1", "DE-E-1");
			AssertConvertDate_AU("DS-", "DS-");
			AssertConvertDate_AU("DSE-1", "DSE-1");
			AssertConvertDate_AU("DS-E-1", "DS-E-1");

			AssertConvertDate_AU("+DE", "+DE");
			AssertConvertDate_AU("-DE", "-DE");
			AssertConvertDate_AU("+DS", "+DS");
			AssertConvertDate_AU("-DS", "-DS");
			AssertConvertDate_AU("DE++", "DE++");
			AssertConvertDate_AU("DS++", "DS++");

			AssertConvertDate_AU("DEDS", "DEDS");
			AssertConvertDate_AU("DSDE", "DSDE");
		}

		public void TestGetDateTimeAsObject_RelativeDates()
		{
			var testControlDateEdit = (ZDateEdit)TestControl;
			testControlDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;

			AssertConvertDate_AU("T", GetToday());
			AssertConvertDate_AU("Y", GetToday().AddDays(-1));
			AssertConvertDate_AU("T+1", GetToday().AddDays(1));
			AssertConvertDate_AU("T+7", GetToday().AddDays(7));
			AssertConvertDate_AU("T-1", GetToday().AddDays(-1));
			AssertConvertDate_AU("T-7", GetToday().AddDays(-7));

			AssertConvertDate_AU("T1", "T1");
			AssertConvertDate_AU("1T", "1T");
			AssertConvertDate_AU("T+", "T+");
			AssertConvertDate_AU("T-", "T-");
			AssertConvertDate_AU("T++", "T++");
			AssertConvertDate_AU("T*1", "T*1");
			AssertConvertDate_AU("Y+1", "Y+1");

			testControlDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;

			AssertConvertDate_AU("T", GetTodayWithoutTime());
			AssertConvertDate_AU("Y", GetTodayWithoutTime().AddDays(-1));
			AssertConvertDate_AU("T+1", GetTodayWithoutTime().AddDays(1));
			AssertConvertDate_AU("T+7", GetTodayWithoutTime().AddDays(7));
			AssertConvertDate_AU("T-1", GetTodayWithoutTime().AddDays(-1));
			AssertConvertDate_AU("T-7", GetTodayWithoutTime().AddDays(-7));
		}

		public void TestGetDateTimeAsObject_NoYearSpecified()
		{
			AssertConvertDate_AU("9/7", new DateTime(EnvProxy.Instance.Time.CurrentLocalDate.Year, 7, 9));
			AssertConvertDate_AU("0907", new DateTime(EnvProxy.Instance.Time.CurrentLocalDate.Year, 7, 9));
			AssertConvertDate_AU("3009", new DateTime(EnvProxy.Instance.Time.CurrentLocalDate.Year, 9, 30));

			AssertConvertDate_AU("3109", "3109");
			AssertConvertDate_AU("907", "907");
			AssertConvertDate_AU("97", "97");
		}

		public void TestGetDateTimeAsObject_AU()
		{
			AssertConvertDate_AU("9/7/02", TestDateTime1);
			AssertConvertDate_AU("9/07/02", TestDateTime1);
			AssertConvertDate_AU("09/7/02", TestDateTime1);
			AssertConvertDate_AU("09/07/02", TestDateTime1);
			AssertConvertDate_AU("090702", TestDateTime1);

			AssertConvertDate_AU("9/7/2", TestDateTime1);

			AssertConvertDate_AU("9/7/2002", TestDateTime1);
			AssertConvertDate_AU("9/07/2002", TestDateTime1);
			AssertConvertDate_AU("09/7/2002", TestDateTime1);
			AssertConvertDate_AU("09/07/2002", TestDateTime1);
			AssertConvertDate_AU("09072002", TestDateTime1);

			AssertConvertDate_AU("09Jul02", TestDateTime1);
			AssertConvertDate_AU("09-Jul-02", TestDateTime1);
			AssertConvertDate_AU("09Jul2002", TestDateTime1);
			AssertConvertDate_AU("09-Jul-2002", TestDateTime1);
			AssertConvertDate_AU("009-JUL-02", TestDateTime1);

			AssertConvertDate_AU("9Jul02", TestDateTime1);
			AssertConvertDate_AU("9Jul2", TestDateTime1);
			AssertConvertDate_AU("09Jul2", TestDateTime1);

			AssertConvertDate_AU("30/11/02", TestDateTime2);
			AssertConvertDate_AU("30/11/2002", TestDateTime2);
			AssertConvertDate_AU("301102", TestDateTime2);
			AssertConvertDate_AU("30112002", TestDateTime2);
			AssertConvertDate_AU("30-NOV-002", TestDateTime2);
			AssertConvertDate_AU("030-nov-002", TestDateTime2);

			AssertConvertDate_AU("30/11/2", TestDateTime2);

			AssertConvertDate_AU("301102 1330", TestDateTime3);
			AssertConvertDate_AU("301102:0130PM", TestDateTime3);
			AssertConvertDate_AU("301102T0130P", TestDateTime3);
			AssertConvertDate_AU("30Nov02 13:30", TestDateTime3);
			AssertConvertDate_AU("301102 13.30", TestDateTime3);
			AssertConvertDate_AU("30Nov2002 1:30PM", TestDateTime3);
			AssertConvertDate_AU("301102 1:30P", TestDateTime3);

			AssertConvertDate_AU("301102 9:30A", TestDateTime4);
			AssertConvertDate_AU("301102 9:30", TestDateTime4);
			AssertConvertDate_AU("301102 0930", TestDateTime4);
			AssertConvertDate_AU("301102 9:30 AM", TestDateTime4);

			AssertConvertDate_AU("301102 9:30 PM", TestDateTime5);
			AssertConvertDate_AU("30NOV02T9:30P", TestDateTime5);

			AssertConvertDate_AU("06-MAY-02 07:08 GMT+10:00", TestDateTime8);
			AssertConvertDate_AU("6-MAY-02 07:08 GMT+10:00", TestDateTime8);
			AssertConvertDate_AU("06-5-02 07:08 GMT+10:00", TestDateTime8);
			AssertConvertDate_AU("06-MAY-2 07:08 GMT+10:00", TestDateTime8);
			AssertConvertDate_AU("06-MAY-02 7:08 GMT+10:00", TestDateTime8);
			AssertConvertDate_AU("06-MAY-02 07:8 GMT+10:00", TestDateTime8);
			AssertConvertDate_AU("6-5-2 7:8 GMT+10:00", TestDateTime8);

			//BAD INPUTS
			AssertConvertDate_AU("30JUN!02", "30JUN!02");
			AssertConvertDate_AU("3112002", "3112002");
			AssertConvertDate_AU("31102", "31102");
			AssertConvertDate_AU("badinput", "badinput");
			AssertConvertDate_AU("", "");
			AssertConvertDate_AU("30022002", "30022002");
			AssertConvertDate_AU("31092002", "31092002");

			AssertConvertDate_AU("011OCT01", "011OCT01");
			AssertConvertDate_AU("01OCT001", "01OCT001");
			AssertConvertDate_AU("011OCT001", "011OCT001");

			// Pre Time Settings
			var control = TestControl as ZDateEdit;
			AssertNotNull("Control should not be null", control);
			AssertEquals("PreCondition: Control.DateTimeFormat", ZDateTimePickerFormat.Short, control.DateTimeFormat);

			AssertConvertDate_AU("0605", new DateTime(EnvProxy.Instance.Time.CurrentLocalDateTime.Year, 5, 6));

			// Time Only
			control.DateTimeFormat = ZDateTimePickerFormat.Time;

			AssertConvertDate_AU(" 16 : 45 ", TestDateTime6);
			AssertConvertDate_AU("1645 ", TestDateTime6);
			AssertConvertDate_AU(" 4 : 4 5 P M", TestDateTime6);
			AssertConvertDate_AU("445pM", TestDateTime6);
			AssertConvertDate_AU("04:45P", TestDateTime6);
			AssertConvertDate_AU("0445pm", TestDateTime6);
			AssertConvertDate_AU("04.45pm", TestDateTime6);

			AssertConvertDate_AU(" 6 : 0 5 ", TestDateTime7);
			AssertConvertDate_AU("0605", TestDateTime7);
			AssertConvertDate_AU(" 6 0 5 a", TestDateTime7);
			AssertConvertDate_AU(" 0 6 0 5 Am", TestDateTime7);
			AssertConvertDate_AU("6.05AM", TestDateTime7);

			// Bad Time Input
			AssertConvertDate_AU("2410", "2410");
			AssertConvertDate_AU("12-55AM", "12-55AM");
			AssertConvertDate_AU("1260", "1260");
		}

		public void TestGetDateTimeAsObject_US()
		{
			AssertConvertDate_US("7/9/02", TestDateTime1);
			AssertConvertDate_US("7/09/02", TestDateTime1);
			AssertConvertDate_US("07/9/02", TestDateTime1);
			AssertConvertDate_US("07/09/02", TestDateTime1);
			AssertConvertDate_US("070902", TestDateTime1);

			AssertConvertDate_US("7/9/2", TestDateTime1);

			AssertConvertDate_US("7/9/2002", TestDateTime1);
			AssertConvertDate_US("7/09/2002", TestDateTime1);
			AssertConvertDate_US("07/9/2002", TestDateTime1);
			AssertConvertDate_US("07/09/2002", TestDateTime1);
			AssertConvertDate_US("07092002", TestDateTime1);

			AssertConvertDate_US("Jul0902", TestDateTime1);
			AssertConvertDate_US("Jul-09-02", TestDateTime1);
			AssertConvertDate_US("Jul092002", TestDateTime1);
			AssertConvertDate_US("Jul-09-2002", TestDateTime1);

			AssertConvertDate_US("JUL-009-02", TestDateTime1);

			AssertConvertDate_US("11/30/02", TestDateTime2);
			AssertConvertDate_US("11/30/2002", TestDateTime2);
			AssertConvertDate_US("113002", TestDateTime2);
			AssertConvertDate_US("11302002", TestDateTime2);

			AssertConvertDate_US("11/30/2", TestDateTime2);

			AssertConvertDate_US("NOV-30-002", TestDateTime2);
			AssertConvertDate_US("nov-030-002", TestDateTime2);

			AssertConvertDate_US("113002T1330", TestDateTime3);
			AssertConvertDate_US("113002:0130PM", TestDateTime3);
			AssertConvertDate_US("113002-0130P", TestDateTime3);
			AssertConvertDate_US("113002.13:30", TestDateTime3);
			AssertConvertDate_US("113002/13.30", TestDateTime3);
			AssertConvertDate_US("113002+1:30PM", TestDateTime3);
			AssertConvertDate_US("113002 1:30P", TestDateTime3);

			AssertConvertDate_US("113002 9:30A", TestDateTime4);
			AssertConvertDate_US("113002 9:30", TestDateTime4);
			AssertConvertDate_US("113002 0930", TestDateTime4);
			AssertConvertDate_US("113002 9:30 AM", TestDateTime4);

			AssertConvertDate_US("MAY-06-02 07:08 GMT-05:00", TestDateTime8);
			AssertConvertDate_US("5-06-02 07:08 GMT-05:00", TestDateTime8);
			AssertConvertDate_US("MAY-6-02 07:08 GMT-05:00", TestDateTime8);
			AssertConvertDate_US("MAY-06-2 07:08 GMT-05:00", TestDateTime8);
			AssertConvertDate_US("MAY-06-02 7:08 GMT-05:00", TestDateTime8);
			AssertConvertDate_US("MAY-06-02 07:8 GMT-05:00", TestDateTime8);
			AssertConvertDate_US("5-6-2 7:8 GMT-05:00", TestDateTime8);

			//BAD INPUTS
			AssertConvertDate_US("1132002", "1132002");
			AssertConvertDate_US("11302", "11302");
			AssertConvertDate_US("badinput", "badinput");
			AssertConvertDate_US("", "");
			AssertConvertDate_US("02302002", "02302002");
			AssertConvertDate_US("09312002", "09312002");

			AssertConvertDate_US("OCT01101", "OCT01101");
			AssertConvertDate_US("OCT01001", "OCT01001");

			// Pre Time Settings
			var control = TestControl as ZDateEdit;
			AssertNotNull("Control should not be null", control);
			AssertEquals("PreCondition: Control.DateTimeFormat", ZDateTimePickerFormat.Short, control.DateTimeFormat);

			AssertConvertDate_US("0605", new DateTime(EnvProxy.Instance.Time.CurrentLocalDateTime.Year, 6, 5));

			// Time Only
			control.DateTimeFormat = ZDateTimePickerFormat.Time;

			AssertConvertDate_US(" 16 : 45 ", TestDateTime6);
			AssertConvertDate_US("1645 ", TestDateTime6);
			AssertConvertDate_US(" 4 : 4 5 P M", TestDateTime6);
			AssertConvertDate_US("445pM", TestDateTime6);
			AssertConvertDate_US("04:45P", TestDateTime6);
			AssertConvertDate_US("0445pm", TestDateTime6);
			AssertConvertDate_US("04.45pm", TestDateTime6);

			AssertConvertDate_US(" 6 : 0 5 ", TestDateTime7);
			AssertConvertDate_US("0605", TestDateTime7);
			AssertConvertDate_US(" 6 0 5 a", TestDateTime7);
			AssertConvertDate_US(" 0 6 0 5 Am", TestDateTime7);
			AssertConvertDate_US("6.05AM", TestDateTime7);

			// Bad Time Input
			AssertConvertDate_US("2410", "2410");
			AssertConvertDate_US("12-55AM", "12-55AM");
			AssertConvertDate_US("1260", "1260");
		}

		public void TestGetDateTimeAsObject_JP()
		{
			AssertConvertDate_JP("02/7/9", TestDateTime1);
			AssertConvertDate_JP("02/7/09", TestDateTime1);
			AssertConvertDate_JP("02/07/9", TestDateTime1);
			AssertConvertDate_JP("02/07/09", TestDateTime1);
			AssertConvertDate_JP("020709", TestDateTime1);

			AssertConvertDate_JP("2/7/9", TestDateTime1);

			AssertConvertDate_JP("2002/7/9", TestDateTime1);
			AssertConvertDate_JP("2002/7/09", TestDateTime1);
			AssertConvertDate_JP("2002/07/9", TestDateTime1);
			AssertConvertDate_JP("2002/07/09", TestDateTime1);
			AssertConvertDate_JP("20020709", TestDateTime1);

			AssertConvertDate_JP("02Jul09", TestDateTime1);
			AssertConvertDate_JP("02-Jul-09", TestDateTime1);
			AssertConvertDate_JP("2002Jul09", TestDateTime1);
			AssertConvertDate_JP("2002-Jul-09", TestDateTime1);

			AssertConvertDate_JP("02Jul9", TestDateTime1);
			AssertConvertDate_JP("2Jul9", TestDateTime1);
			AssertConvertDate_JP("2Jul09", TestDateTime1);

			AssertConvertDate_JP("02-JUL-009", TestDateTime1);

			AssertConvertDate_JP("02/11/30", TestDateTime2);
			AssertConvertDate_JP("2002/11/30", TestDateTime2);
			AssertConvertDate_JP("021130", TestDateTime2);
			AssertConvertDate_JP("20021130", TestDateTime2);

			AssertConvertDate_JP("2/11/30", TestDateTime2);

			AssertConvertDate_JP("002-NOV-30", TestDateTime2);
			AssertConvertDate_JP("002-nov-030", TestDateTime2);

			AssertConvertDate_JP("021130 1330", TestDateTime3);
			AssertConvertDate_JP("021130 0130PM", TestDateTime3);
			AssertConvertDate_JP("021130 0130P", TestDateTime3);
			AssertConvertDate_JP("021130 13:30", TestDateTime3);
			AssertConvertDate_JP("021130 13.30", TestDateTime3);
			AssertConvertDate_JP("021130 1:30PM", TestDateTime3);
			AssertConvertDate_JP("021130 1:30P", TestDateTime3);

			AssertConvertDate_JP("021130 9:30A", TestDateTime4);
			AssertConvertDate_JP("021130 9:30", TestDateTime4);
			AssertConvertDate_JP("021130 0930", TestDateTime4);
			AssertConvertDate_JP("021130 9:30 AM", TestDateTime4);

			var firstJuly02 = new DateTime(2002, 07, 1);
			AssertConvertDate_JP("2JUL1", firstJuly02);
			AssertConvertDate_JP("2!JUL1", firstJuly02);
			AssertConvertDate_JP("2JUL!1", firstJuly02);

			AssertConvertDate_JP("02-MAY-06 07:08 GMT+09:00", TestDateTime8);
			AssertConvertDate_JP("2-MAY-06 07:08 GMT+09:00", TestDateTime8);
			AssertConvertDate_JP("02-5-06 07:08 GMT+09:00", TestDateTime8);
			AssertConvertDate_JP("02-MAY-6 07:08 GMT+09:00", TestDateTime8);
			AssertConvertDate_JP("02-MAY-06 7:08 GMT+09:00", TestDateTime8);
			AssertConvertDate_JP("2-5-6 7:8 GMT+09:00", TestDateTime8);

			//BAD INPUTS
			AssertConvertDate_JP("2002113", "2002113");
			AssertConvertDate_JP("02113", "02113");
			AssertConvertDate_JP("badinput", "badinput");
			AssertConvertDate_JP("", "");
			AssertConvertDate_JP("20020230", "20020230");
			AssertConvertDate_JP("20020931", "20020931");

			AssertConvertDate_JP("01OCT011", "01OCT011");
			AssertConvertDate_JP("001OCT01", "001OCT01");
			AssertConvertDate_JP("001OCT011", "001OCT011");

			// Pre Time Settings
			var control = TestControl as ZDateEdit;
			AssertNotNull("Control should not be null", control);
			AssertEquals("PreCondition: Control.DateTimeFormat", ZDateTimePickerFormat.Short, control.DateTimeFormat);

			AssertConvertDate_JP("0605", "0605");

			// Time Only
			control.DateTimeFormat = ZDateTimePickerFormat.Time;

			AssertConvertDate_JP(" 16 : 45 ", TestDateTime6);
			AssertConvertDate_JP("1645 ", TestDateTime6);
			AssertConvertDate_JP(" 4 : 4 5 P M", TestDateTime6);
			AssertConvertDate_JP("445pM", TestDateTime6);
			AssertConvertDate_JP("04:45P", TestDateTime6);
			AssertConvertDate_JP("0445pm", TestDateTime6);
			AssertConvertDate_JP("04.45pm", TestDateTime6);

			AssertConvertDate_JP(" 6 : 0 5 ", TestDateTime7);
			AssertConvertDate_JP("0605", TestDateTime7);
			AssertConvertDate_JP(" 6 0 5 a", TestDateTime7);
			AssertConvertDate_JP(" 0 6 0 5 Am", TestDateTime7);
			AssertConvertDate_JP("6.05AM", TestDateTime7);

			// Bad Time Input
			AssertConvertDate_JP("2410", "2410");
			AssertConvertDate_JP("12-55AM", "12-55AM");
			AssertConvertDate_JP("1260", "1260");
		}

		public void TestConvertTextToDateTime()
		{
			AssertEquals("Empty String", DBNull.Value, TestCore.ConvertTextToDateTime(""));
			AssertEquals("Valid Date", new DateTime(2003, 12, 01, 3, 0, 0), TestCore.ConvertTextToDateTime("01122003 0300"));
			AssertEquals("Invalid Date", DateTime.MinValue, TestCore.ConvertTextToDateTime("TestJunkData"));
		}

		public void TestDateToString()
		{
			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("DateToString for DateTimeFormat.Long", "01-Jan-04 12:15", Core.DateToString(new ZDateTime(2004, 1, 1, 12, 15, 0), null));
			AssertEquals("DateToString for DateTimeFormat.Long", "01-Jan-04 12:15", Core.DateToString(new ZDateTime(2004, 1, 1, 12, 15, 59), null));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals("DateToString for DateTimeFormat.Short", "01-Jan-04", Core.DateToString(new ZDateTime(2004, 1, 1, 12, 15, 0), null));

			AssertEquals("DateToString for ZDateTime.Empty", ZDateTime.Empty.ToString(), Core.DateToString(ZDateTime.Empty, null));
			AssertEquals("DateToString for ZDateTime.Invalid", ZDateTime.Invalid.ToString(), Core.DateToString(ZDateTime.Invalid, null));
		}

		public void TestStringToDateWithMonthYearEntry()
		{
			AssertEquals("MC0104 should convert to 01 Jan 2004.", new ZDateTime(2004, 1, 1), Core.StringToDate("MC0104"));
			AssertEquals("MC104  should convert to 01 Jan 2004.", new ZDateTime(2004, 1, 1), Core.StringToDate("MC104"));
			AssertEquals("MC0404 should convert to 01 Apr 2004.", new ZDateTime(2004, 4, 1), Core.StringToDate("MC0404"));
			AssertEquals("MC404  should convert to 01 Apr 2004.", new ZDateTime(2004, 4, 1), Core.StringToDate("MC404"));
			AssertEquals("MC1204 should convert to 01 Dec 2004.", new ZDateTime(2004, 12, 1), Core.StringToDate("MC1204"));

			AssertEquals("ME0104 should convert to 31 Jan 2004.", new ZDateTime(2004, 1, 31, 23, 59, 59), Core.StringToDate("ME0104"));
			AssertEquals("ME104  should convert to 31 Jan 2004.", new ZDateTime(2004, 1, 31, 23, 59, 59), Core.StringToDate("ME104"));
			AssertEquals("ME0404 should convert to 30 Apr 2004.", new ZDateTime(2004, 4, 30, 23, 59, 59), Core.StringToDate("ME0404"));
			AssertEquals("ME404  should convert to 30 Apr 2004.", new ZDateTime(2004, 4, 30, 23, 59, 59), Core.StringToDate("ME404"));
			AssertEquals("ME1204 should convert to 31 Dec 2004.", new ZDateTime(2004, 12, 31, 23, 59, 59), Core.StringToDate("ME1204"));

			AssertEquals("MC1304 should convert to ZDateTime.Invalid.", ZDateTime.Invalid, Core.StringToDate("MC1304"));
			AssertEquals("ME1304 should convert to ZDateTime.Invalid.", ZDateTime.Invalid, Core.StringToDate("ME1304"));
		}

		public void TestStringToDate_OutOfSqlRangeValues()
		{
			var dateTooSmall = System.Data.SqlTypes.SqlDateTime.MinValue.Value.AddSeconds(-1);
			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals(ZDateTime.Invalid, Core.StringToDate(dateTooSmall.ToString("dd-MM-yyyy HH:mm")));
		}

		public void TestStringToDateWithCountrySpecificCulture()
		{
			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;

			var expectedValue = new ZDateTime(2011, 9, 22, 9, 0, 0);

			AssertEquals("22-SEP-11 should convert to 22 Sep 2011.", new ZDateTime(2011, 9, 22), Core.StringToDate("22-SEP-11"));
			AssertEquals("22-09-11 should convert to 22 Sep 2011.", new ZDateTime(2011, 9, 22), Core.StringToDate("22-09-11"));
			AssertEquals("22-SEP-11 09:00 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 09:00"));
			AssertEquals("22-SEP-11 0900 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 0900"));
			AssertEquals("22-SEP-11 09:00 GMT+02:00 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 09:00 GMT+02:00"));
			AssertEquals("22-SEP-11 0900 GMT+02:00 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 0900 GMT+02:00"));

			var factory = new BusinessObjectFactory();

			var japaneseCompany = factory.New<IGlbCompany>();
			japaneseCompany.SetCountry("JP");
			((BusinessObject)japaneseCompany)[GlbCompanySchema.GC_Code] = "XXX";

			var branch = factory.New<IGlbBranch>();
			branch.GB_GC = japaneseCompany.PK;
			((BusinessObject)branch)[GlbBranchSchema.GB_Code] = "JAP";
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("22-SEP-11 should convert to 22 Sep 2011.", new ZDateTime(2011, 9, 22), Core.StringToDate("22-SEP-11"));
				AssertEquals("22-09-11 should convert to 11 Sep 2022.", new ZDateTime(2022, 9, 11), Core.StringToDate("22-09-11"));
				AssertEquals("22-SEP-11 09:00 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 09:00"));
				AssertEquals("22-SEP-11 0900 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 0900"));
				AssertEquals("22-SEP-11 09:00 GMT+02:00 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 09:00 GMT+02:00"));
				AssertEquals("22-SEP-11 0900 GMT+02:00 should convert to 22 Sep 2011 09:00.", expectedValue, Core.StringToDate("22-SEP-11 0900 GMT+02:00"));
			}
		}

		public void TestStringToDateInAllLanguages()
		{
			foreach (var language in DataFile.GetAvailableLanguages())
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					using (var dateEdit = new ZDateEdit())
					{
						var core = new ZDateEditCoreForTesting(dateEdit);
						dateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
						var theDate = new ZDateTime(2012, 10, 11);
						AssertEquals("StringToDate for ShortDateString in " + language, theDate, core.StringToDate(theDate.ToShortDateString()));

						dateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
						theDate = new ZDateTime(2012, 10, 11, 4, 34, 0);
						AssertEquals("StringToDate for ShortDateString in " + language, theDate, core.StringToDate(theDate.ToLongTimeString()));

						dateEdit.DateTimeFormat = ZDateTimePickerFormat.LongIncludingSeconds;
						theDate = new ZDateTime(2012, 10, 11, 4, 34, 0);
						AssertEquals("StringToDate for ShortDateString in " + language, theDate, core.StringToDate(theDate.ToString(DateTimeFormatStrings.LongTimeIncludingSecondsFormat)));
					}
				}
			}
		}

		public void TestStringToDateWithTryParse()
		{
			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Custom;
			AssertEquals("01-JAN-04 should convert to 01 Jan 2004.", new ZDateTime(2004, 1, 1), Core.StringToDate("01-JAN-04"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals("01-JAN-04 should convert to 01 Jan 2004.", new ZDateTime(2004, 1, 1), Core.StringToDate("01-JAN-04"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("01-JAN-04 should convert to 01 Jan 2004.", new ZDateTime(2004, 1, 1), Core.StringToDate("01-JAN-04"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Time;
			AssertEquals("01-JAN-04 should convert to 01 Jan 2004.", new ZDateTime(2004, 1, 1), Core.StringToDate("01-JAN-04"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Custom;
			AssertEquals("01-JAN-04 13:30 should convert to 01 Jan 2004 13:30.", new ZDateTime(2004, 1, 1, 13, 30, 0), Core.StringToDate("01-JAN-04 13:30"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals("01-JAN-04 13:30 should convert to 01 Jan 2004 13:30.", new ZDateTime(2004, 1, 1, 13, 30, 0), Core.StringToDate("01-JAN-04 13:30"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("01-JAN-04 13:30 should convert to 01 Jan 2004 13:30.", new ZDateTime(2004, 1, 1, 13, 30, 0), Core.StringToDate("01-JAN-04 13:30"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Time;
			AssertEquals("01-JAN-04 13:30 should convert to 01 Jan 2004 13:30.", new ZDateTime(2004, 1, 1, 13, 30, 0), Core.StringToDate("01-JAN-04 13:30"));

			var today = ZDateTime.Now;
			var expected = new ZDateTime(today.Year, today.Month, today.Day, 13, 30, 0);

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Custom;
			AssertEquals("13:30 should convert to 13:30.", new ZDateTime(today.Year, today.Month, today.Day, 13, 30, 0), Core.StringToDate("13:30"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals("13:30 should convert to 13:30.", new ZDateTime(today.Year, today.Month, today.Day, 13, 30, 0), Core.StringToDate("13:30"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("13:30 should convert to 13:30.", new ZDateTime(today.Year, today.Month, today.Day, 13, 30, 0), Core.StringToDate("13:30"));

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Time;
			AssertEquals("13:30 should convert to 13:30.", new ZDateTime(today.Year, today.Month, today.Day, 13, 30, 0), Core.StringToDate("13:30"));
		}

		/// <summary>
		/// This test is here because CargoWise.Types does not reference Core and therefore cannot use TestDate attribute.
		/// </summary>
		[TestDate(2005, 8, 12, 13, 14, 15)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestTryParseExactWithDifferentLocalPcAndZDateTimeDates()
		{
			AssertEquals("[PRE-CONDITION] ZDateTime.Now with TestDate attribute", new ZDateTime(2005, 8, 12, 13, 14, 15), ZDateTime.Now);

			ZDateTime parsedDateTime;

			// ---------------------
			// Formats with no year 
			// ---------------------

			var success = ZDateTime.TryParseExact("18:35", out parsedDateTime, "HH:mm");
			AssertEquals(true, success);
			AssertEquals("Date not in the format - Date should be ZDateTime.Today", ZDateTime.Today, parsedDateTime.Date);
			AssertEquals("Date not in the format - Parsed DateTime", new ZDateTime(2005, 08, 12, 18, 35, 00), parsedDateTime);

			success = ZDateTime.TryParseExact("30", out parsedDateTime, "dd");
			AssertEquals(true, success);
			AssertEquals("Day as specified, Year as ZDateTime.Now", new ZDateTime(2005, 01, 30, 00, 00, 00), parsedDateTime);

			success = ZDateTime.TryParseExact("1007", out parsedDateTime, "ddMM");
			AssertEquals(true, success);
			AssertEquals("Month/Day as specified, Year as ZDateTime.Now", new ZDateTime(2005, 07, 10, 00, 00, 00), parsedDateTime);

			success = ZDateTime.TryParseExact("10", out parsedDateTime, "MM");
			AssertEquals(true, success);
			AssertEquals("Month as specified, Year as ZDateTime.Now", new ZDateTime(2005, 10, 01, 00, 00, 00), parsedDateTime);

			// -------------
			// Format = "d" 
			// -------------

			success = ZDateTime.TryParseExact("19/03/2001", out parsedDateTime, "d");
			AssertEquals(true, success);
			AssertEquals("Date should be as specified", new ZDateTime(2001, 03, 19, 00, 00, 00), parsedDateTime);

			// ------------------
			// Formats with year 
			// ------------------

			success = ZDateTime.TryParseExact("16/06/2004", out parsedDateTime, "dd/MM/yyyy");
			AssertEquals(true, success);
			AssertEquals("Date should be as specified", new ZDateTime(2004, 06, 16, 00, 00, 00), parsedDateTime);

			success = ZDateTime.TryParseExact("15/05/1999 19:44", out parsedDateTime, "dd/MM/yyyy HH:mm");
			AssertEquals(true, success);
			AssertEquals("Date/Time should be as specified", new ZDateTime(1999, 05, 15, 19, 44, 00), parsedDateTime);

			success = ZDateTime.TryParseExact("2010", out parsedDateTime, "yyyy");
			AssertEquals(true, success);
			AssertEquals("Year as specified", new ZDateTime(2010, 01, 01, 00, 00, 00), parsedDateTime);

			success = ZDateTime.TryParseExact("200307", out parsedDateTime, "yyyyMM");
			AssertEquals(true, success);
			AssertEquals("Year/Month as specified", new ZDateTime(2003, 07, 01, 00, 00, 00), parsedDateTime);

			success = ZDateTime.TryParseExact("200231", out parsedDateTime, "yyyydd");
			AssertEquals(true, success);
			AssertEquals("Year/Day as specified", new ZDateTime(2002, 01, 31, 00, 00, 00), parsedDateTime);
		}

		public void TestStringToDate_WhenHourOrMinuteIsSingleNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("6-MAY-02 7:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-02 7:8"));
				AssertEquals("6-MAY-02 7:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-02 7:08"));
				AssertEquals("6-MAY-02 07:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-02 07:8"));
				AssertEquals("6-MAY-02 07:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-02 07:08"));
				AssertEquals("06-MAY-02 7:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-02 7:8"));
				AssertEquals("06-MAY-02 7:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-02 7:08"));
				AssertEquals("06-MAY-02 07:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-02 07:8"));
				AssertEquals("06-MAY-02 07:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-02 07:08"));
				AssertEquals("6-MAY-2 7:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-2 7:8"));
				AssertEquals("6-MAY-2 7:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-2 7:08"));
				AssertEquals("6-MAY-2 07:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-2 07:8"));
				AssertEquals("6-MAY-2 07:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("6-MAY-2 07:08"));
				AssertEquals("06-MAY-2 7:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-2 7:8"));
				AssertEquals("06-MAY-2 7:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-2 7:08"));
				AssertEquals("06-MAY-2 07:8 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-2 07:8"));
				AssertEquals("06-MAY-2 07:08 should convert to 06-MAY-02 07:08.", TestDateTime8, Core.StringToDate("06-MAY-2 07:08"));
			});
		}

		public void TestStringToDate_WhenInputTextIsNotStandardizedMissingZero_BranchIsInJapan()
		{
			TestStringToDate_WhenInputTextIsNotStandardizedMissingZero();

			var factory = new BusinessObjectFactory();
			var japansesBranch = CreateCorrespondingBranch(factory, "JP", "JAP");
			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, japansesBranch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				TestStringToDate_WhenInputTextIsNotStandardizedMissingZero();
			}
		}

		IGlbBranch CreateCorrespondingBranch(BusinessObjectFactory factory, string country, string code)
		{
			var company = factory.New<IGlbCompany>();
			company.SetCountry(country);
			((BusinessObject)company)[GlbCompanySchema.GC_Code] = "XXX";
			var branch = factory.New<IGlbBranch>();
			branch.GB_GC = company.PK;
			((BusinessObject)branch)[GlbBranchSchema.GB_Code] = code;
			factory.Save();
			return branch;
		}

		void TestStringToDate_WhenInputTextIsNotStandardizedMissingZero()
		{
			DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			var expectedValue = new ZDateTime(2001, 9, 2, 9, 1, 0);
			AssertEquals("2-SEP-01 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2-SEP-01 09:01"));
			AssertEquals("02-SEP-1 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("02-SEP-1 09:01"));
			AssertEquals("02-SEP-01 9:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("02-SEP-01 9:01"));
			AssertEquals("02-SEP-01 09:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("02-SEP-01 09:1"));
			AssertEquals("2-SEP-1 9:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2-SEP-1 9:1"));
			AssertEquals("02-SEP-01 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("02-SEP-01 09:01"));
		}

		public void TestStringToDate_WhenInputTextIsNotStandardizedMissingZero_InMultilingualScenarios()
		{
			var expectedValue = new ZDateTime(2001, 9, 2, 9, 1, 0);

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Hungarian))
			{
				DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
				AssertEquals("1. SEP 02 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("1. SEP 02 09:01"));
				AssertEquals("01. SEP 2 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01. SEP 2 09:01"));
				AssertEquals("01. SEP 02 9:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01. SEP 02 9:01"));
				AssertEquals("01. SEP 02 09:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01. SEP 02 09:1"));
				AssertEquals("1. SEP 2 9:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("1. SEP 2 9:1"));
				AssertEquals("01. SEP 02 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01. SEP 02 09:01"));
			}
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
				AssertEquals("2001-9-2 9:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 9:01"));
				AssertEquals("2001-9-2 09:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 09:1"));
				AssertEquals("2001-9-2 9:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 9:1"));
				AssertEquals("2001-9-2 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 09:01"));
			}
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			{
				DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
				AssertEquals("2001-9-2 9:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 9:01"));
				AssertEquals("2001-9-2 09:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 09:1"));
				AssertEquals("2001-9-2 9:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 9:1"));
				AssertEquals("2001-9-2 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 09:01"));
			}
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseTraditional))
			{
				DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
				AssertEquals("2001-9-2 9:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 9:01"));
				AssertEquals("2001-9-2 09:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 09:1"));
				AssertEquals("2001-9-2 9:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 9:1"));
				AssertEquals("2001-9-2 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("2001-9-2 09:01"));
			}
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Korean))
			{
				DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
				AssertEquals("1-SEP-02 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("1-SEP-02 09:01"));
				AssertEquals("01-SEP-2 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-2 09:01"));
				AssertEquals("01-SEP-02 9:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-02 9:01"));
				AssertEquals("01-SEP-02 09:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-02 09:1"));
				AssertEquals("1-SEP-2 9:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("1-SEP-2 9:1"));
				AssertEquals("01-SEP-02 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-02 09:01"));
			}
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Lithuanian))
			{
				DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
				AssertEquals("1-SEP-02 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("1-SEP-02 09:01"));
				AssertEquals("01-SEP-2 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-2 09:01"));
				AssertEquals("01-SEP-02 9:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-02 9:01"));
				AssertEquals("01-SEP-02 09:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-02 09:1"));
				AssertEquals("1-SEP-2 9:1 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("1-SEP-2 9:1"));
				AssertEquals("01-SEP-02 09:01 should convert to 22 Sep 2001 09:01.", expectedValue, Core.StringToDate("01-SEP-02 09:01"));
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			DateEdit = new ZDateEdit();
			Core = new ZDateEditCoreForTesting(DateEdit);
			TestControl = GetNewTestControl();
			TestCore = new ZDateEditCore(TestControl);
			TestDateTime1 = new DateTime(2002, 07, 09, 0, 0, 0, 0);
			TestDateTime2 = new DateTime(2002, 11, 30, 0, 0, 0, 0);
			TestDateTime3 = new DateTime(2002, 11, 30, 13, 30, 0, 0);
			TestDateTime4 = new DateTime(2002, 11, 30, 9, 30, 0, 0);
			TestDateTime5 = new DateTime(2002, 11, 30, 21, 30, 0, 0);
			var today = EnvProxy.Instance.Time.CurrentLocalDateTime;
			TestDateTime6 = new DateTime(today.Year, today.Month, today.Day, 16, 45, 0);
			TestDateTime7 = new DateTime(today.Year, today.Month, today.Day, 6, 5, 0);
			TestDateTime8 = new DateTime(2002, 5, 6, 7, 8, 0, 0);
		}

		protected override void TearDown()
		{
			base.TearDown();

			((IDisposable)TestControl).Dispose();
			DateEdit.Dispose();
		}

		ZDateEditCoreForTesting Core;
		ZDateEdit DateEdit;

		ZDateEditCore TestCore;
		IDateInputControl TestControl;
		DateTime TestDateTime1;
		DateTime TestDateTime2;
		DateTime TestDateTime3;
		DateTime TestDateTime4;
		DateTime TestDateTime5;
		DateTime TestDateTime6;
		DateTime TestDateTime7;
		DateTime TestDateTime8;

		static IDateInputControl GetNewTestControl()
		{
			return new ZDateEdit();
		}

		void AssertConvertDate_AU(string text, object expectedResult)
		{
			AssertEquals("DateTime Conversion for " + text, expectedResult, TestCore.GetDateTimeAsObject(text, CountryDateTimeFormat.Other));
		}

		void AssertConvertDate_US(string text, object expectedResult)
		{
			AssertEquals("DateTime Conversion for " + text, expectedResult, TestCore.GetDateTimeAsObject(text, CountryDateTimeFormat.US));
		}

		void AssertConvertDate_JP(string text, object expectedResult)
		{
			AssertEquals("DateTime Conversion for " + text, expectedResult, TestCore.GetDateTimeAsObject(text, CountryDateTimeFormat.Japan));
		}

		static DateTime GetToday()
		{
			var currentTime = EnvProxy.Instance.Time.CurrentLocalDateTime;
			if (currentTime.Second == 59)
			{
				System.Threading.Thread.Sleep(1000);
				currentTime = EnvProxy.Instance.Time.CurrentLocalDateTime;
			}

			return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
		}

		DateTime GetTodayWithoutTime()
		{
			var today = GetToday();
			return new DateTime(today.Year, today.Month, today.Day);
		}

		#endregion
	}
}
