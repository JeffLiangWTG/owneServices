using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class CapacityCalculatorTest : BMSTestCaseWithFactory
	{
		#region Full Capacity

		[TestDate(2019, 1, 1)]
		public void TestFullCapacity_ForInactiveStaff()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var context = WorkingTimeContext.Create(config.Buffer);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			resource2.GS_IsActive = false;

			Factory.Save();

			var capacity1 = CapacityCalculator.GetFullCapacity_ForTest(resource1, config.Buffer, context);
			var capacity2 = CapacityCalculator.GetFullCapacity_ForTest(resource2, config.Buffer, context);

			AssertEquals(48m, capacity1.FullCapacity);
			AssertEquals(0m, capacity2.FullCapacity);
		}

		[TestDate(2013, 11, 1, 9, 0, 0)]
		public void TestFullCapacity_ForCCR()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var ccrResource = Factory.NewWithValidTestData<GlbStaff>();
			ccrResource.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			ccrResource.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var nonCcrResource = Factory.NewWithValidTestData<GlbStaff>();
			nonCcrResource.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			nonCcrResource.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			buffer.FC_NonCCRTemporaryOverloadLimitMultiplier = 2.5;
			var constraint = CreateConstraint(buffer, offsetMinutes: 64 * 60);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(ccrResource, nonCcrResource);

			ccrResource.DesignateAsCCR(buffer);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CCR resource capacity", 64m, CapacityCalculator.GetUtilisedCapacityBreakdown(ccrResource, buffer).AvailableCapacity);
				AssertEquals("Non-CCR resource capacity (for non-CCR work)", 48m, CapacityCalculator.GetUtilisedCapacityBreakdown(nonCcrResource, buffer).AvailableCapacity);
				AssertEquals("Non-CCR resource capacity (for CCR work)", 120m, CapacityCalculator.GetUtilisedCapacityBreakdown(nonCcrResource, buffer).AvailableCapacityForWorkInvolvingCCR);
			});
		}

		[TestDate(2013, 11, 1, 9, 0, 0)]
		public void TestFullCapacity_ForCCR_ButNotInConstrainedReleaseGroup()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var ccrResource = Factory.NewWithValidTestData<GlbStaff>();
			ccrResource.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			ccrResource.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var nonCcrResource = Factory.NewWithValidTestData<GlbStaff>();
			nonCcrResource.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			nonCcrResource.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var constraint = CreateConstraint(buffer, offsetMinutes: 64 * 60);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			ccrResource.DesignateAsCCR(buffer);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CCR resource capacity (not in constrained mode release group)", 48m, CapacityCalculator.GetUtilisedCapacityBreakdown(ccrResource, buffer).AvailableCapacity);
				AssertEquals("Non-CCR resource capacity (not in constrained mode release group)", 48m, CapacityCalculator.GetUtilisedCapacityBreakdown(nonCcrResource, buffer).AvailableCapacity);
			});
		}

		public void TestCapacity_MultipleResources()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			resource1.Capabilities.Add(capability);
			resource2.Capabilities.Add(capability);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			BMSTestHelper.CreateWorkflowAndTask(Factory, "comp", buffer, staffCode: resource1.GS_Code, lowEstMinutes: 120, capability: capability);
			BMSTestHelper.CreateWorkflowAndTask(Factory, "comp", buffer, staffCode: resource2.GS_Code, lowEstMinutes: 240, capability: capability);

			Factory.Save();

			var capacityForResource1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, buffer);
			var capacityForResource2 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, buffer);

			var capacityForBothResources = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2 }, buffer);

			CombineAssertions("Calculating capacity for both resources should be the same as calculating individually", () =>
			{
				AssertEquals(capacityForResource1.AvailableCapacity, capacityForBothResources[resource1].AvailableCapacity);
				AssertEquals(capacityForResource1.FullCapacity, capacityForBothResources[resource1].FullCapacity);
				AssertEquals(capacityForResource1.UtilisedCapacity, capacityForBothResources[resource1].UtilisedCapacity);
				AssertEquals(capacityForResource2.AvailableCapacity, capacityForBothResources[resource2].AvailableCapacity);
				AssertEquals(capacityForResource2.FullCapacity, capacityForBothResources[resource2].FullCapacity);
				AssertEquals(capacityForResource2.UtilisedCapacity, capacityForBothResources[resource2].UtilisedCapacity);
			});
		}

		[TestDate(2013, 11, 1, 9, 0, 0)]
		public void TestFullCapacity_ForCCR_ButNoReleaseGroupsInConstrainedMode()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var ccrResource = Factory.NewWithValidTestData<GlbStaff>();
			ccrResource.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			ccrResource.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var nonCcrResource = Factory.NewWithValidTestData<GlbStaff>();
			nonCcrResource.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			nonCcrResource.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var constraint = CreateConstraint(buffer, offsetMinutes: 64 * 60);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: null);
			group.Staff.AddRange(ccrResource, nonCcrResource);

			ccrResource.DesignateAsCCR(buffer);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CCR resource capacity (release group not in constrained mode)", 48m, CapacityCalculator.GetUtilisedCapacityBreakdown(ccrResource, buffer).AvailableCapacity);
				AssertEquals("Non-CCR resource capacity (release group not in constrained mode)", 48m, CapacityCalculator.GetUtilisedCapacityBreakdown(nonCcrResource, buffer).AvailableCapacity);
			});
		}

		[TestDate(2013, 11, 1)]
		public void TestZoneMultipliers_ShouldApplyByDefault()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var zoneMultiplier = Factory.New<BMZoneCapacityMultiplier>();
			zoneMultiplier.BZC_FC_Component = buffer.PK;
			zoneMultiplier.BZC_Zone0Multiplier = 3;
			zoneMultiplier.BZC_Zone1Multiplier = 2;
			zoneMultiplier.BZC_Zone2Multiplier = 1;
			zoneMultiplier.BZC_Zone3Multiplier = 1;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-100);
			var task = CreateTask(workflow, staff.GS_Code, 60);

			Factory.Save();

			var fullCapacity = CapacityCalculator.GetFullCapacity_ForTest(staff, buffer);
			var expectedCapacity = fullCapacity - 4.5m;

			AssertEquals(expectedCapacity, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).AvailableCapacity);
		}

		public void TestCapacity_StaffCanHaveTwoLetterCodes()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TZ";
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var zoneMultiplier = Factory.New<BMZoneCapacityMultiplier>();
			zoneMultiplier.BZC_FC_Component = buffer.PK;
			zoneMultiplier.BZC_Zone0Multiplier = 3;
			zoneMultiplier.BZC_Zone1Multiplier = 2;
			zoneMultiplier.BZC_Zone2Multiplier = 1;
			zoneMultiplier.BZC_Zone3Multiplier = 1;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-100);
			var task = CreateTask(workflow, staff.GS_Code, 60);

			Factory.Save();

			var fullCapacity = CapacityCalculator.GetFullCapacity_ForTest(staff, buffer);
			var expectedCapacity = fullCapacity - 4.5m;

			AssertEquals(expectedCapacity, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).AvailableCapacity);
		}

		public void TestGetFullCapacityHours_ForNullStaff()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			AssertEquals(0m, CapacityCalculator.GetFullCapacity_ForTest(null, buffer));
		}

		[TestDate(2013, 4, 16)]
		public void TestGetFullCapacity()
		{
			UpdateDepartmentWeekDaysTo9To5ExceptMonday9To4(GlbDepartment.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			UpdateStaffWeekDaysTo9To5ExceptMonday9To4(staff.PK);
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_BufferTimespanInMinutes = 60;

			Factory.Save();

			AssertEquals(0.5m, CapacityCalculator.GetFullCapacity_ForTest(staff, buffer));
		}

		[TestDate(2015, 1, 1)]
		public void TestGetFullCapacity_WhenOverlappingStaffLeaveRecordsExist_ShouldNotGoNegative()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = CreateStaffInCurrentBranchDept("JRP", "Jamesey");
			Factory.Save();

			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Monday, "");
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Tuesday, "                        **********");
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Wednesday, "             ********** ************");
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Thursday, "             ********** **********");
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Friday, "             ********** *******");
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Saturday, "");
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Sunday, "");

			AssertEquals(42m, CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer));

			var staffHoliday1 = BMSTestHelper.CreateStaffHoliday(Factory, new ZDateTime(2015, 1, 8), new ZDateTime(2015, 1, 28), resource.PK, leaveDaysTaken: 20, availabilityFactor: 0);
			var staffHoliday2 = BMSTestHelper.CreateStaffHoliday(Factory, new ZDateTime(2015, 1, 7), new ZDateTime(2015, 1, 27), resource.PK, leaveDaysTaken: 20, availabilityFactor: 0);
			Factory.Save();

			AssertEquals("Should be some capacity in the days before holidays kick in. Overlapping holiday shouldn't suck all this up.", 10m, CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer));
		}

		#region Capacity for Staff in Different Branches

		[TestDate(2020, 2, 19)]
		public void TestResourceBranchWorkHoursShouldOverrideBufferAgingBranchWorkHours_WhenCalculatingCapacity()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_BranchName = "Los Angeles Opera";
			branch1.GB_RL_NKHomePort = "AUSYD";

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_BranchName = "Metropolitan Opera";
			branch2.GB_RL_NKHomePort = "AUSYD";
			var branch2Holiday = branch2.GlbHolidays.AddNew();
			branch2Holiday.GH_HolidayName = "La traviata";
			branch2Holiday.GH_Date = ZDateTime.Today; // branch2 does not work today

			TestDateAttribute.Date = new DateTime(2020, 1, 1);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_GB_HomeBranch = branch1.PK;
			resource1.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			resource1.GS_Code = "PLC";
			resource1.GS_FullName = "Placido Domingo";

			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_GB_HomeBranch = branch2.PK;
			resource2.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			resource2.GS_Code = "LUC";
			resource2.GS_FullName = "Luciano Pavarotti";

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource1.PK);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource1.PK, DayOfWeek.Thursday, ""); // resource1 does not work tomorrow

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource2.PK);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, resource2.PK, DayOfWeek.Thursday, ""); // resource2 does not work tomorrow

			TestDateAttribute.Date = new DateTime(2020, 2, 19);

			AssertEquals("Precondition", DayOfWeek.Wednesday, ZDateTime.Today.DayOfWeek);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.BufferTimespan = new ZDateTime(2020, 1, 2, 0, 0, 0);
			AssertEquals("Precondition: 24 hour buffer", 24d, config.Buffer.BufferTimeSpanHours);
			config.Buffer.FC_BufferLoadLimitPercent = 100;
			config.Buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			config.Buffer.FC_GB_AgingBranch = branch2.PK;

			Factory.Save();

			AssertEquals("Precondition", 11, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			const decimal resource1ExpectedWorkHoursFrom11To5 = 6m;
			AssertEquals("Resource1's capacity should not be zero as their branch allows to work today regardless the buffer's aging branch which does not", resource1ExpectedWorkHoursFrom11To5, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).FullCapacity);
			AssertEquals("Resource2's capacity should be zero as their branch (which is the same as the buffer's aging brach) has a holiday today and the resource does not work within the buffer windows", 0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).FullCapacity);
		}

		[TestDate(2020, 2, 17, 9, 0, 0)]
		public void TestFullCapacityShouldConsiderStaffWorkTimeInRelationToStaffBranchTimeZone()
		{
			var zonesConfig = new TimeZonesTestConfig(Factory);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZone0.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZoneMinus3.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZonePlus3.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.BufferTimespan = new ZDateTime(2020, 1, 1, 4, 0, 0);
			AssertEquals("Precondition: 4 hour buffer", 4d, config.Buffer.BufferTimeSpanHours);
			config.Buffer.FC_BufferLoadLimitPercent = 100;
			config.Buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			config.Buffer.FC_GB_AgingBranch = zonesConfig.BranchTimeZone0.PK; // buffer asssigned to UTC+0 

			Factory.Save();

			void AddHours(TimeSpan interval)
			{
				var hours = Convert.ToInt32(interval.TotalHours);
				TestDateAttribute.AddHours(hours);
				config.Buffer.AgingBranch.HomePort.LocalDateTime = config.Buffer.AgingBranch.HomePort.LocalDateTime.AddHours(hours);
			}

			decimal GetFullCapacity(GlbStaff staff) => CapacityCalculator.GetUtilisedCapacityBreakdown(staff, config.Buffer, useCache: false).FullCapacity;

			AssertEquals("Precondition", DayOfWeek.Monday, ZDateTime.Today.DayOfWeek);

			AssertEquals("Precondition", 9, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 4 work hours (9:00-13:00)", 4m, GetFullCapacity(zonesConfig.ResourceTimeZone0));
			AssertEquals("UTC-3 resource's capacity: just 1 hour (9:00-10:00) of the buffer window (6:00-10:00) should fall into the staff work hours (9:00-17:00) due to -3 time shift", 1m, GetFullCapacity(zonesConfig.ResourceTimeZoneMinus3));
			AssertEquals("UTC+3 resource's capacity: all 4 hours (12:00-16:00) of the buffer window should fall into the work hours (9:00-17:00) even with the +3 time shift", 4m, GetFullCapacity(zonesConfig.ResourceTimeZonePlus3));

			AddHours(TimeSpan.FromHours(3));
			AssertEquals("Precondition", 12, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 4 work hours (12:00-16:00)", 4m, GetFullCapacity(zonesConfig.ResourceTimeZone0));
			AssertEquals("UTC-3 resource's capacity: all 4 hours (9:00-13:00) of the buffer window should fall into the work hours (9:00-17:00) even with the -3 time shift", 4m, GetFullCapacity(zonesConfig.ResourceTimeZoneMinus3));
			AssertEquals("UTC+3 resource's capacity: just 2 hours (15:00-17:00) of the buffer window (15:00-19:00) should fall into the staff work hours (9:00-17:00) due to +3 time shift", 2m, GetFullCapacity(zonesConfig.ResourceTimeZonePlus3));

			AddHours(TimeSpan.FromHours(3));
			AssertEquals("Precondition", 15, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 2 work hours today (15:00-17:00) and 2 work hours tomorrow (9:00-11:00) from the buffer window (from 15:00 today to 11:00 tomorrow)", 4m, GetFullCapacity(zonesConfig.ResourceTimeZone0));
			AssertEquals("UTC-3 resource's capacity: 5 work hours (12:00-17:00) from the buffer window (from 12:00 today to 8:00 tomorrow) capped at the buffer size of 4 hours", 4m, GetFullCapacity(zonesConfig.ResourceTimeZoneMinus3));
			AssertEquals("UTC+3 resource's capacity: 5 work hours (9:00-14:00 tomorrow) from the buffer window (from 18:00 today to 14:00 tomorrow) capped at the buffer size of 4 hours", 4m, GetFullCapacity(zonesConfig.ResourceTimeZonePlus3));

			AddHours(TimeSpan.FromHours(1));
			AssertEquals("Precondition", 16, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 1 work hour today (16:00-17:00) and 3 work hours tomorrow (9:00-12:00) from the buffer window (from 16:00 today to 12:00 tomorrow)", 4m, GetFullCapacity(zonesConfig.ResourceTimeZone0));
			AssertEquals("UTC-3 resource's capacity: 4 work hours (13:00-17:00) of the buffer window (from 13:00 today to 9:00 tomorrow)", 4m, GetFullCapacity(zonesConfig.ResourceTimeZoneMinus3));
			AssertEquals("UTC+3 resource's capacity: 6 work hours (9:00-15:00 tomorrow) from the buffer window (from 19:00 today to 15:00 tomorrow) capped at the buffer size of 4 hours", 4m, GetFullCapacity(zonesConfig.ResourceTimeZonePlus3));
		}

		[TestDate(2020, 2, 17, 9, 0, 0)]
		public void TestFullCapacityShouldConsiderStaffHolidaysInRelationToStaffBranchTimeZone()
		{
			var zonesConfig = new TimeZonesTestConfig(Factory);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZone0.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZoneMinus3.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZonePlus3.PK);

			AssertEquals("Precondition", DayOfWeek.Monday, ZDateTime.Today.DayOfWeek);

			var resourceTimeZone0Holiday = zonesConfig.ResourceTimeZone0.Holidays.AddNew();
			resourceTimeZone0Holiday.GA_StartTime = new ZDateTime(2020, 2, 17, 9, 0, 0); // 9 to 10 am local time
			resourceTimeZone0Holiday.GA_EndTime = new ZDateTime(2020, 2, 17, 10, 0, 0);
			resourceTimeZone0Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			var resourceTimeZoneMinus3Holiday = zonesConfig.ResourceTimeZoneMinus3.Holidays.AddNew();
			resourceTimeZoneMinus3Holiday.GA_StartTime = new ZDateTime(2020, 2, 17, 9, 0, 0); // 9 to 11 am local time
			resourceTimeZoneMinus3Holiday.GA_EndTime = new ZDateTime(2020, 2, 17, 11, 0, 0);
			resourceTimeZoneMinus3Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			var resourceTimeZonePlus3Holiday = zonesConfig.ResourceTimeZonePlus3.Holidays.AddNew();
			resourceTimeZonePlus3Holiday.GA_StartTime = new ZDateTime(2020, 2, 17, 15, 0, 0); // 3 to 5 pm local time
			resourceTimeZonePlus3Holiday.GA_EndTime = new ZDateTime(2020, 2, 17, 17, 0, 0);
			resourceTimeZonePlus3Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.BufferTimespan = new ZDateTime(2020, 1, 1, 4, 0, 0);
			AssertEquals("Precondition: 4 hour buffer", 4d, config.Buffer.BufferTimeSpanHours);
			config.Buffer.FC_BufferLoadLimitPercent = 100;
			config.Buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			config.Buffer.FC_GB_AgingBranch = zonesConfig.BranchTimeZone0.PK; // buffer asssigned to UTC+0 

			Factory.Save();

			AssertEquals("Precondition", 9, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 3 work hours (10:00-13:00) of the buffer window (9:00-13:00) because of the staff leave (9:00-10:00)", 3m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZone0, config.Buffer).FullCapacity);
			AssertEquals("UTC-3 resource's capacity: zero hours of the buffer window (6:00-10:00) because of the staff leave (9:00-11:00)", 0m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZoneMinus3, config.Buffer).FullCapacity);
			AssertEquals("UTC+3 resource's capacity: 3 work hours (12:00-15:00) of the buffer window (12:00-16:00) because of the staff leave (15:00-17:00)", 3m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZonePlus3, config.Buffer).FullCapacity);
		}

		[TestDate(2020, 2, 17, 15, 0, 0)]
		public void TestFullCapacityShouldConsiderBranchHolidaysInRelationToStaffBranchTimeZone()
		{
			var zonesConfig = new TimeZonesTestConfig(Factory);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZone0.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZoneMinus3.PK);
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZonePlus3.PK);

			AssertEquals("Precondition", DayOfWeek.Monday, ZDateTime.Today.DayOfWeek);

			var branchTimeZoneMinus3Holiday = zonesConfig.BranchTimeZoneMinus3.GlbHolidays.AddNew();
			branchTimeZoneMinus3Holiday.GH_HolidayName = "Some Argentinian National Day";
			branchTimeZoneMinus3Holiday.GH_Date = ZDateTime.Today.AddDays(1); // imagine Buenos Aires does not work tomorrow

			var branchTimeZonePlus3Holiday = zonesConfig.BranchTimeZonePlus3.GlbHolidays.AddNew();
			branchTimeZonePlus3Holiday.GH_HolidayName = "Some Russian National Day";
			branchTimeZonePlus3Holiday.GH_Date = ZDateTime.Today.AddDays(1); // imagine Moscow does not work tomorrow

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.BufferTimespan = new ZDateTime(2020, 1, 1, 6, 0, 0);
			AssertEquals("Precondition: 6 hour buffer", 6d, config.Buffer.BufferTimeSpanHours);
			config.Buffer.FC_BufferLoadLimitPercent = 100;
			config.Buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			config.Buffer.FC_GB_AgingBranch = zonesConfig.BranchTimeZone0.PK; // buffer asssigned to UTC+0 

			Factory.Save();

			AssertEquals("Precondition", 15, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 2 work hours today (15:00-17:00) and 4 work hours tomorrow (9:00-13:00) from the buffer window (from 15:00 today to 13:00 tomorrow)", 6m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZone0, config.Buffer).FullCapacity);
			AssertEquals("UTC-3 resource's capacity: just 5 work hours today (12:00-17:00) from the buffer window (from 12:00 today to 10:00 tomorrow) because branch does not work tomorrow", 5m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZoneMinus3, config.Buffer).FullCapacity);
			AssertEquals("UTC+3 resource's capacity: zero work hours from the buffer window (from 18:00 today to 16:00 tomorrow) because the branch does not work tomorrow", 0m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZonePlus3, config.Buffer).FullCapacity);
		}

		[TestDate(2020, 2, 17, 9, 0, 0)]
		public void TestFullCapacityShouldConsiderStaffWorkHoursInRelationToStaffBranchTimeZone()
		{
			var zonesConfig = new TimeZonesTestConfig(Factory);

			AssertEquals("Precondition", DayOfWeek.Monday, ZDateTime.Today.DayOfWeek);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, zonesConfig.ResourceTimeZone0.PK);
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZoneMinus3.PK, WorkingDaysTestHelper.GetWorkingHoursString(7, 17));
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZonePlus3.PK, WorkingDaysTestHelper.GetWorkingHoursString(9, 15));

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.BufferTimespan = new ZDateTime(2020, 1, 1, 4, 0, 0);
			AssertEquals("Precondition: 4 hour buffer", 4d, config.Buffer.BufferTimeSpanHours);
			config.Buffer.FC_BufferLoadLimitPercent = 100;
			config.Buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			config.Buffer.FC_GB_AgingBranch = zonesConfig.BranchTimeZone0.PK; // buffer asssigned to UTC+0 

			Factory.Save();

			AssertEquals("Precondition", 9, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 4 work hours (9:00-13:00)", 4m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZone0, config.Buffer).FullCapacity);
			AssertEquals("UTC-3 resource's capacity: 3 work hours (7:00-10:00) of the buffer window (6:00-10:00) should fall into the staff work hours (7:00-17:00)", 3m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZoneMinus3, config.Buffer).FullCapacity);
			AssertEquals("UTC+3 resource's capacity: 3 work hours (12:00-15:00) of the buffer window (12:00-16:00) should fall into the staff work hours (9:00-15:00)", 3m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZonePlus3, config.Buffer).FullCapacity);
		}

		[TestDate(2020, 2, 17, 9, 0, 0)]
		public void TestFullCapacityShouldConsiderDepartmentWorkHoursInRelationToStaffBranchTimeZone_WhenStaffWorkHoursAreNotSet()
		{
			var zonesConfig = new TimeZonesTestConfig(Factory);

			var deptTimeZone0 = Factory.NewWithValidTestData<GlbDepartment>();
			deptTimeZone0.GE_Code = "LON";
			zonesConfig.ResourceTimeZone0.GS_GE_HomeDepartment = deptTimeZone0.PK;

			var deptTimeZoneMinus3 = Factory.NewWithValidTestData<GlbDepartment>();
			deptTimeZoneMinus3.GE_Code = "BUE";
			zonesConfig.ResourceTimeZoneMinus3.GS_GE_HomeDepartment = deptTimeZoneMinus3.PK;

			var deptTimeZonePlus3 = Factory.NewWithValidTestData<GlbDepartment>();
			deptTimeZonePlus3.GE_Code = "MOW";
			zonesConfig.ResourceTimeZonePlus3.GS_GE_HomeDepartment = deptTimeZonePlus3.PK;

			AssertEquals("Precondition", DayOfWeek.Monday, ZDateTime.Today.DayOfWeek);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, deptTimeZone0.PK);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, deptTimeZoneMinus3.PK);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, deptTimeZoneMinus3.PK, DayOfWeek.Monday, WorkingDaysTestHelper.GetWorkingHoursString(7, 17));

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, deptTimeZonePlus3.PK);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, deptTimeZonePlus3.PK, DayOfWeek.Monday, WorkingDaysTestHelper.GetWorkingHoursString(9, 15));

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.BufferTimespan = new ZDateTime(2020, 1, 1, 4, 0, 0);
			AssertEquals("Precondition: 4 hour buffer", 4d, config.Buffer.BufferTimeSpanHours);
			config.Buffer.FC_BufferLoadLimitPercent = 100;
			config.Buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			config.Buffer.FC_GB_AgingBranch = zonesConfig.BranchTimeZone0.PK; // buffer asssigned to UTC+0 

			Factory.Save();

			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZone0.PK, string.Empty);
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZoneMinus3.PK, string.Empty);
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZonePlus3.PK, string.Empty);

			AssertEquals("Precondition", 9, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 4 work hours (9:00-13:00)", 4m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZone0, config.Buffer).FullCapacity);
			AssertEquals("UTC-3 resource's capacity: 3 work hours (7:00-10:00) of the buffer window (6:00-10:00) should fall into the department work hours (7:00-17:00)", 3m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZoneMinus3, config.Buffer).FullCapacity);
			AssertEquals("UTC+3 resource's capacity: 3 work hours (12:00-15:00) of the buffer window (12:00-16:00) should fall into the department work hours (9:00-15:00)", 3m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZonePlus3, config.Buffer).FullCapacity);
		}

		[TestDate(2019, 12, 1)]
		public void TestFullCapacityShouldConsiderWeekdaysAndWeekendsInRelationToStaffBranchTimeZone_WhenNeitherStaffNorDepartmentWorkHoursAreSet()
		{
			var zonesConfig = new TimeZonesTestConfig(Factory);

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Monday, string.Empty);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Tuesday, string.Empty);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Wednesday, string.Empty);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Thursday, string.Empty);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Friday, string.Empty);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Saturday, string.Empty);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Sunday, string.Empty);

			TestDateAttribute.Date = new DateTime(2020, 2, 17);

			AssertEquals("Precondition", DayOfWeek.Monday, ZDateTime.Today.DayOfWeek);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.BufferTimespan = new ZDateTime(2020, 1, 2, 0, 0, 0);
			AssertEquals("Precondition: 24 hour buffer", 24d, config.Buffer.BufferTimeSpanHours);
			config.Buffer.FC_BufferLoadLimitPercent = 100;
			config.Buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			config.Buffer.FC_GB_AgingBranch = zonesConfig.BranchTimeZone0.PK; // buffer asssigned to UTC+0 

			Factory.Save();

			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZone0.PK, string.Empty);
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZoneMinus3.PK, string.Empty);
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, zonesConfig.ResourceTimeZonePlus3.PK, string.Empty);

			AssertEquals("Precondition", 0, config.Buffer.AgingBranch.HomePort.LocationDateTime.Hour);
			AssertEquals("UTC+0 resource's capacity: 24 work hours (entire day)", 24m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZone0, config.Buffer).FullCapacity);
			AssertEquals("UTC-3 resource's capacity: just 21 work hours (0:00-21:00) of the buffer window (from 21:00 yesterday to 21:00 today) because yesterday was a weekend", 21m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZoneMinus3, config.Buffer).FullCapacity);
			AssertEquals("UTC+3 resource's capacity: all 24 work hours of the buffer window (from 3:00 today to 3:00 tomorrow)", 24m, CapacityCalculator.GetUtilisedCapacityBreakdown(zonesConfig.ResourceTimeZonePlus3, config.Buffer).FullCapacity);
		}

		class TimeZonesTestConfig
		{
			public TimeZonesTestConfig(BusinessObjectFactory factory)
			{
				BranchTimeZone0 = factory.NewWithValidTestData<GlbBranch>();
				BranchTimeZone0.GB_BranchName = "Europe/London";
				BranchTimeZone0.GB_RL_NKHomePort = "GBLON";

				BranchTimeZoneMinus3 = factory.NewWithValidTestData<GlbBranch>();
				BranchTimeZoneMinus3.GB_BranchName = "America/Argentina/Buenos_Aires";
				BranchTimeZoneMinus3.GB_RL_NKHomePort = "ARBUE";

				BranchTimeZonePlus3 = factory.NewWithValidTestData<GlbBranch>();
				BranchTimeZonePlus3.GB_BranchName = "Europe/Moscow";
				BranchTimeZonePlus3.GB_RL_NKHomePort = "RUMOW";

				ResourceTimeZone0 = factory.NewWithValidTestData<GlbStaff>();
				ResourceTimeZone0.GS_GB_HomeBranch = BranchTimeZone0.PK;
				ResourceTimeZone0.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
				ResourceTimeZone0.GS_Code = "WLS";
				ResourceTimeZone0.GS_FullName = "William Shakespeare";

				ResourceTimeZoneMinus3 = factory.NewWithValidTestData<GlbStaff>();
				ResourceTimeZoneMinus3.GS_GB_HomeBranch = BranchTimeZoneMinus3.PK;
				ResourceTimeZoneMinus3.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
				ResourceTimeZoneMinus3.GS_Code = "JLB";
				ResourceTimeZoneMinus3.GS_FullName = "Jorge Luis Borges";

				ResourceTimeZonePlus3 = factory.NewWithValidTestData<GlbStaff>();
				ResourceTimeZonePlus3.GS_GB_HomeBranch = BranchTimeZonePlus3.PK;
				ResourceTimeZonePlus3.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
				ResourceTimeZonePlus3.GS_Code = "FDD";
				ResourceTimeZonePlus3.GS_FullName = "Fedor Dostoevsky";
			}

			public GlbBranch BranchTimeZone0;
			public GlbBranch BranchTimeZoneMinus3;
			public GlbBranch BranchTimeZonePlus3;

			public GlbStaff ResourceTimeZone0;
			public GlbStaff ResourceTimeZoneMinus3;
			public GlbStaff ResourceTimeZonePlus3;
		}

		#endregion

		#endregion

		#region Utilised Capacity

		public void TestGetUtilisedCapacity()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, timespanMinutes: 100);
			var bucket = CreateBucket(system);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = bucket.PK;

			var task1_1 = CreateTask(workflow1, staff1.GS_Code, 120);
			var task1_2 = CreateTask(workflow1, staff2.GS_Code, 60);

			var task2_1 = CreateTask(workflow2, staff1.GS_Code, 240);
			var task2_2 = CreateTask(workflow2, staff2.GS_Code, 180);

			var task3_1 = CreateTask(workflow3, staff1.GS_Code, 1000);

			Factory.Save();

			AssertEquals(540 / 60.0m, CapacityCalculator.GetUtilisedCapacity_ForTest(staff1, buffer));
			AssertEquals(360 / 60.0m, CapacityCalculator.GetUtilisedCapacity_ForTest(staff2, buffer));

			AssertEquals(1500 / 60.0m, CapacityCalculator.GetUtilisedCapacity_ForTest(staff1, bucket));
			AssertEquals(0.0m, CapacityCalculator.GetUtilisedCapacity_ForTest(staff2, bucket));
		}

		[TestDate(2013, 12, 10)]
		public void TestGetUtilisedCapacity_ShouldUseZoneMultipliers()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			CreateZoneMultiplier(buffer, zone0Multiplier: 3, zone3Multiplier: 1);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-20);
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			AssertEquals(1.5m * 3m, CapacityCalculator.GetUtilisedCapacity_ForTest(resource, buffer));

			workflow.FH_ReleaseDateTime = ZDateTime.Now;
			Factory.Save();

			AssertEquals(1.5m, CapacityCalculator.GetUtilisedCapacity_ForTest(resource, buffer));
		}

		[TestDate(2013, 12, 10)]
		public void TestGetUtilisedCapacity_ShouldUseZoneMultipliersAllZones()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			CreateZoneMultiplier(buffer, zone0Multiplier: 20, zone1Multiplier: 10, zone2Multiplier: 5, zone3Multiplier: 2);

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-20);
			var task1 = CreateTask(workflow1, resource.GS_Code, 60);

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-14);
			var task2 = CreateTask(workflow2, resource.GS_Code, 60);

			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow3.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-10);
			var task3 = CreateTask(workflow3, resource.GS_Code, 60);

			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow4.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-1);
			var task4 = CreateTask(workflow4, resource.GS_Code, 60);

			Factory.Save();

			var estimates = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer);

			AssertEquals("workflow1 is in zone 0", 30m, estimates.GetZoneReservedCapacity(0));
			AssertEquals("workflow1 is in zone 0", 1.5m, estimates.GetZoneAllocatedCapacity(0));
			AssertEquals("workflow2 is in zone 1", 15m, estimates.GetZoneReservedCapacity(1));
			AssertEquals("workflow2 is in zone 1", 1.5m, estimates.GetZoneAllocatedCapacity(1));
			AssertEquals("workflow3 is in zone 2", 7.5m, estimates.GetZoneReservedCapacity(2));
			AssertEquals("workflow3 is in zone 2", 1.5m, estimates.GetZoneAllocatedCapacity(2));
			AssertEquals("workflow4 is in zone 3", 3m, estimates.GetZoneReservedCapacity(3));
			AssertEquals("workflow4 is in zone 3", 1.5m, estimates.GetZoneAllocatedCapacity(3));
			AssertEquals("Total Capacity Consumed", 55.5m, estimates.UtilisedCapacity);
		}

		[TestDate(2013, 12, 13)]
		public void TestZoneCapacity_ReservationMatchesZone_AbnormalStaffHours_DayScale()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			UpdateStaffWeekDaysTo9To12(resource.PK);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, timespanMinutes: 60 * 8 * 3); // 3 working days?
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3).AddHours(1);
			var task1 = CreateTask(workflow1, resource.GS_Code, 40);

			Factory.Save();

			var estimates = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer);

			AssertEquals(1, workflow1.BufferZone);
			AssertReservedCapacityBreakdown(estimates, zone3Hours: 0m, zone2Hours: 0m, zone1Hours: 1m, zone0Hours: 0m);
		}

		[TestUtcOffset(10, 0, 0)]
		[TestDate(2013, 12, 13, 17, 0, 0)]
		public void TestZoneCapacity_ReservationMatchesZone_AbnormalStaffHours_HourScale()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			UpdateStaffWeekDaysTo9To12(resource.PK);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, timespanMinutes: 60 * 3); // 3 hours
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-3).AddMinutes(1).ToUniversalBranchTime(buffer.AgingBranch);
			var task1 = CreateTask(workflow1, resource.GS_Code, 40);

			Factory.Save();

			var estimates = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer);

			AssertEquals(1, workflow1.BufferZone);
			AssertReservedCapacityBreakdown(estimates, zone3Hours: 0m, zone2Hours: 0m, zone1Hours: 1m, zone0Hours: 0m);
		}

		[TestDate(2013, 12, 10)]
		public void TestGetUtilisedCapacity_ShouldUseZoneMultipliersAllZone0()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			CreateZoneMultiplier(buffer, zone0Multiplier: 2.7m);

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-20);
			var task1 = CreateTask(workflow1, resource.GS_Code, 60);

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-20);
			var task2 = CreateTask(workflow2, resource.GS_Code, 120);

			var workflow3 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow3.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-20);
			var task3 = CreateTask(workflow3, resource.GS_Code, 45);

			var workflow4 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow4.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-20);
			var task4 = CreateTask(workflow4, resource.GS_Code, 180);

			Factory.Save();

			var estimates = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, buffer);

			AssertEquals("all workflows are in zone 0", 27.34m, estimates.GetZoneReservedCapacity(0));
			AssertEquals("all workflows are in zone 0", 10.13m, estimates.GetZoneAllocatedCapacity(0));
			AssertEquals(0m, estimates.GetZoneReservedCapacity(1));
			AssertEquals(0m, estimates.GetZoneAllocatedCapacity(1));
			AssertEquals(0m, estimates.GetZoneReservedCapacity(2));
			AssertEquals(0m, estimates.GetZoneAllocatedCapacity(2));
			AssertEquals(0m, estimates.GetZoneReservedCapacity(3));
			AssertEquals(0m, estimates.GetZoneAllocatedCapacity(3));

			AssertEquals("Total Capacity Consumed", 27.34m, estimates.UtilisedCapacity);
		}

		[GuiTest, TestDate(2017, 3, 1)]
		public void TestGetUtilisedCapacityBreakdown_WhenQualityIterationWasCreatedInZone1_AndParentWorkflowWasDeferredAndIsNowBackInZone3_ZoneMultiplicationShouldWorkProperly()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "COD", "CBC");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBC", "ORG");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			BMSTestHelper.CreateZoneMultiplier(config.Buffer, zone3Multiplier: 1, zone2Multiplier: 1, zone1Multiplier: 2, zone0Multiplier: 3);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Celebrity Solstice", config.Buffer, ZDateTime.UtcNow.AddDays(-13));
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 40, taskType: "COD");
			var task2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 40, taskType: "CBC");

			Factory.Save();

			AssertEquals(1, workflow.BufferZone);

			var capacityForResource1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer);
			var capacityForResource2 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer);

			AssertReservedCapacityBreakdown("Initial reserved capacity breakdown", capacityForResource1, zone3Hours: 0, zone2Hours: 0, zone1Hours: 2, zone0Hours: 0);
			AssertReservedCapacityBreakdown("Initial reserved capacity breakdown", capacityForResource2, zone3Hours: 0, zone2Hours: 0, zone1Hours: 2, zone0Hours: 0);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var qualityIterationWorkflow = CreateQualityIteration(task1, task2);

			AssertEquals(1, qualityIterationWorkflow.BufferZone);
			AssertEquals("This defect occurs when the parent workflow has no open tasks, but the QI workflow does have open tasks", 0, workflow.Tasks.Count(t => t.IsOpen));
			AssertEquals(workflow.FH_ReleaseDateTime, qualityIterationWorkflow.FH_ReleaseDateTime);

			Factory.Save();

			capacityForResource1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer);
			capacityForResource2 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer);

			AssertReservedCapacityBreakdown("Capacity usage should be the same as the parent workflow's tasks are closed, and the QI has replaced it with tasks of the same estimated size", capacityForResource1, zone3Hours: 0, zone2Hours: 0, zone1Hours: 2, zone0Hours: 0);
			AssertReservedCapacityBreakdown("Capacity usage should be the same as the parent workflow's tasks are closed, and the QI has replaced it with tasks of the same estimated size", capacityForResource2, zone3Hours: 0, zone2Hours: 0, zone1Hours: 2, zone0Hours: 0);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			Factory.Save();

			AssertEquals("Quality iteration should still be in zone 1 since it had its Last Transfer Date set to the same value as its parent workflow at the time", 1, qualityIterationWorkflow.BufferZone);

			capacityForResource1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer);
			capacityForResource2 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer);

			AssertReservedCapacityBreakdown("Even though the parent workflow Last Transfer Time would place it in zone 3, it's actually not in the buffer so should be ignored", capacityForResource1, zone3Hours: 0, zone2Hours: 0, zone1Hours: 2, zone0Hours: 0);
			AssertReservedCapacityBreakdown("Even though the parent workflow Last Transfer Time would place it in zone 3, it's actually not in the buffer so should be ignored", capacityForResource2, zone3Hours: 0, zone2Hours: 0, zone1Hours: 2, zone0Hours: 0);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			AssertEquals("Re-releasing the parent workflow should move it to zone 3", 3, workflow.BufferZone);
			AssertEquals("Re-releasing the parent workflow should cause the QI workflow to also be in zone 3", 3, qualityIterationWorkflow.BufferZone);

			capacityForResource1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer);
			capacityForResource2 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer);

			AssertReservedCapacityBreakdown("Capacity usage should now be all within zone 3", capacityForResource1, zone3Hours: 1, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
			AssertReservedCapacityBreakdown("Capacity usage should now be all within zone 3", capacityForResource2, zone3Hours: 1, zone2Hours: 0, zone1Hours: 0, zone0Hours: 0);
		}

		#endregion

		#region Resource Membership

		[TestDate(2013, 7, 15)]
		public void TestGetCapacity_ShouldUseBufferResourceMembership()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var buffer = BMSTestHelper.CreateBuffer(system);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var bufferMembership = (BMComponentResourceLink)staff.ComponentMembership.AddNew();
			bufferMembership.FD_FC_Component = buffer.PK;
			bufferMembership.FD_CapacityLimitPercent = 100;

			Factory.Save();

			var capacity = CapacityCalculator.GetFullCapacity_ForTest(staff, buffer);
			AssertEquals("Capacity based on 100% buffer membership", 48m, capacity);

			bufferMembership.FD_CapacityLimitPercent = 50;

			capacity = CapacityCalculator.GetFullCapacity_ForTest(staff, buffer);
			AssertEquals("Capacity based on 50% buffer membership", 24m, capacity);

			bufferMembership.FD_CapacityLimitPercent = 0;

			capacity = CapacityCalculator.GetFullCapacity_ForTest(staff, buffer);
			AssertEquals("Capacity based on 0% buffer membership", 0m, capacity);
		}

		[TestDate(2013, 7, 15)]
		public void TestGetAvailableBufferHours_ShouldUseBufferResourceMembership()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer1 = CreateBuffer(system, "buffer1", loadLimitPercent: 50);
			var buffer2 = CreateBuffer(system, "buffer2", loadLimitPercent: 60);

			var staff1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var staff2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer1.PK;
			var task1_1 = CreateTask(workflow1, staff1.GS_Code, 60 * 4);
			var task1_2 = CreateTask(workflow1, staff2.GS_Code, 60 * 2);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer2.PK;
			var task2_1 = CreateTask(workflow2, staff1.GS_Code, 60 * 2);
			var task2_2 = CreateTask(workflow2, staff2.GS_Code, 60 * 4);

			var buffer1Staff1Membership = (BMComponentResourceLink)staff1.ComponentMembership.AddNew();
			buffer1Staff1Membership.FD_FC_Component = buffer1.PK;
			buffer1Staff1Membership.FD_CapacityLimitPercent = 80;

			var buffer2Staff1Membership = (BMComponentResourceLink)staff1.ComponentMembership.AddNew();
			buffer2Staff1Membership.FD_FC_Component = buffer2.PK;
			buffer2Staff1Membership.FD_CapacityLimitPercent = 60;

			var buffer1Staff2Membership = (BMComponentResourceLink)staff2.ComponentMembership.AddNew();
			buffer1Staff2Membership.FD_FC_Component = buffer1.PK;
			buffer1Staff2Membership.FD_CapacityLimitPercent = 50;

			Factory.Save();

			CombineAssertions("Available capacity by resource", () =>
			{
				AssertEquals("staff1 capacity in buffer1", 32.4m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff1, buffer1).AvailableCapacity);
				AssertEquals("staff2 capacity in buffer1", 21.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff2, buffer1).AvailableCapacity);
				AssertEquals("staff1 capacity in buffer2", 31.56m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff1, buffer2).AvailableCapacity);
				AssertEquals("staff2 capacity in buffer2", 51.6m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff2, buffer2).AvailableCapacity);
			});
		}

		#endregion

		#region Capability Tasks

		[TestDate(2014, 12, 21)]
		public void TestGetAvailableCapacity_ShouldIncludeTasksAssignedToCapabilityNominatedResourcePosesses()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("BEN", "Benedict Bumcerbatch");
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			resource.Capabilities.Add(capability);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer, ZDateTime.UtcNow.AddDays(-100));
			var task = CreateTask(workflow, string.Empty, 60, estVariationFactor: 1, capability: capability);

			Factory.Save();

			var fullCapacity = CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer);
			var availableCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer);

			AssertNotEquals("There should be a reduction on capacity by the capability task", fullCapacity, availableCapacity.AvailableCapacity);
			AssertEquals("The reduction in capacity should be the full task standard estimate since no other resources have this capability", fullCapacity - task.StandardEstimateHours, availableCapacity.AvailableCapacity);
		}

		public void TestUtilisedCapacity_WhenOneResourceHavingCapabilityIsInactive_ShouldNotSplitWithThatResource()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var capability = BMSTestHelper.CreateCapability(Factory, "THR", "Threesources");

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", capability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource. Ah ah ahhh!", capability);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Frack", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, capability: capability, lowEstMinutes: 60, estVariationFactor: 1);

			Factory.Save();

			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer), zone3Hours: 0.33m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer), zone3Hours: 0.33m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource3, config.Buffer), zone3Hours: 0.33m);

			resource3.GS_IsActive = false;
			Factory.Save();

			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer), zone3Hours: 0.5m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer), zone3Hours: 0.5m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource3, config.Buffer), zone3Hours: 0);
		}

		public void TestUtilisedCapacity_WhenGroupCapabilityAndResourceIsNotInReleaseGroup_ShouldNotSplitWithResource()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var capability = BMSTestHelper.CreateCapability(Factory, "CPY", "C");
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", capability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource", capability);

			releaseGroup.Staff.AddRange(resource1, resource2, resource3);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Pizza", config.Buffer, releaseGroupPK: releaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, capability: capability, lowEstMinutes: 60, estVariationFactor: 1);

			Factory.Save();

			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer), zone3Hours: 0.33m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer), zone3Hours: 0.33m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource3, config.Buffer), zone3Hours: 0.33m);

			releaseGroup.Staff.Remove(resource3);
			Factory.Save();

			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer), zone3Hours: 0.5m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer), zone3Hours: 0.5m);
			AssertReservedCapacityBreakdown(CapacityCalculator.GetUtilisedCapacityBreakdown(resource3, config.Buffer), zone3Hours: 0);
		}

		public void TestUtilisedCapacity_WhenDifferentCapabilitiesForMultipleResources_ShouldConsiderWorkflowReleaseGroupWhenGroupCapability()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var globalCapability = BMSTestHelper.CreateCapability(Factory, "GLB", "C");
			globalCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", groupCapability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", groupCapability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource", globalCapability);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FOR", "Four resource", globalCapability);

			releaseGroup.Staff.AddRange(resource1, resource3);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Pizza", config.Buffer, releaseGroupPK: releaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Hamburger", config.Buffer);
			var task2 = BMSTestHelper.CreateTask(workflow2, capability: globalCapability, lowEstMinutes: 60, estVariationFactor: 1);

			Factory.Save();

			var staffCapacities = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2, resource3, resource4 }, config.Buffer);

			AssertReservedCapacityBreakdown(staffCapacities[resource1], zone3Hours: 1);
			AssertReservedCapacityBreakdown(staffCapacities[resource2], zone3Hours: 0);
			AssertReservedCapacityBreakdown(staffCapacities[resource3], zone3Hours: 0.5m);
			AssertReservedCapacityBreakdown(staffCapacities[resource4], zone3Hours: 0.5m);
		}

		public void TestUtilisedCapacity_WhenDifferentCapabilitiesForMultipleResources_ShouldConsiderTaskReleaseGroupWhenGroupCapability()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var otherReleaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", groupCapability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", groupCapability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource", groupCapability);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FOR", "Four resource", groupCapability);

			releaseGroup.Staff.AddRange(resource1, resource2);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Lazanha", config.Buffer, releaseGroupPK: otherReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: releaseGroup.PK);

			Factory.Save();

			var staffCapacities = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2, resource3, resource4 }, config.Buffer);

			AssertReservedCapacityBreakdown(staffCapacities[resource1], zone3Hours: 0.5m);
			AssertReservedCapacityBreakdown(staffCapacities[resource2], zone3Hours: 0.5m);
			AssertReservedCapacityBreakdown(staffCapacities[resource3], zone3Hours: 0);
			AssertReservedCapacityBreakdown(staffCapacities[resource4], zone3Hours: 0);

			releaseGroup.Staff.AddRange(resource3, resource4);

			Factory.Save();

			staffCapacities = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2, resource3, resource4 }, config.Buffer);

			AssertReservedCapacityBreakdown(staffCapacities[resource1], zone3Hours: 0.25m);
			AssertReservedCapacityBreakdown(staffCapacities[resource2], zone3Hours: 0.25m);
			AssertReservedCapacityBreakdown(staffCapacities[resource3], zone3Hours: 0.25m);
			AssertReservedCapacityBreakdown(staffCapacities[resource4], zone3Hours: 0.25m);
		}

		public void TestUtilisedCapacity_ShouldConsumeCapacity_WhenWorkflowAndTaskDoesntHaveReleaseGroup_ForGroupCapability()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", groupCapability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", groupCapability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource", groupCapability);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FOR", "Four resource", groupCapability);

			releaseGroup.Staff.AddRange(resource1, resource2);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Lazanha", config.Buffer, releaseGroupPK: null);
			var task1 = BMSTestHelper.CreateTask(workflow, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: null);
			var task2 = BMSTestHelper.CreateTask(workflow, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: null);
			var task3 = BMSTestHelper.CreateTask(workflow, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: releaseGroup.PK);
			var task4 = BMSTestHelper.CreateTask(workflow, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: releaseGroup.PK);

			Factory.Save();

			var staffCapacities = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2, resource3, resource4 }, config.Buffer);

			AssertEquals(4m, staffCapacities.Sum(s => s.Value.GetZoneReservedCapacity(3)));

			AssertReservedCapacityBreakdown(staffCapacities[resource1], zone3Hours: 1.5m); //(task1,2,3,4 = 4h / 4 = 1 + task1,2 = 2h / 4 = 0.5) = 1.5h
			AssertReservedCapacityBreakdown(staffCapacities[resource2], zone3Hours: 1.5m); //(task1,2,3,4 = 4h / 4 = 1 + task1,2 = 2h / 4 = 0.5) = 1.5h
			AssertReservedCapacityBreakdown(staffCapacities[resource3], zone3Hours: 0.5m); //task1,2 = 2h / 4 = 0.5 
			AssertReservedCapacityBreakdown(staffCapacities[resource4], zone3Hours: 0.5m); //task1,2 = 2h / 4 = 0.5 
		}

		public void TestUtilisedCapacity_WhenStaffIsNotInGroup_DoesNotIncludeTasksFromGroup()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, timespanMinutes: 3600, loadLimitPercent: 20);

			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "One resource", groupCapability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Two resource", groupCapability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "THR", "Three resource", groupCapability);
			var resource4 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FOR", "Four resource", groupCapability);

			releaseGroup1.Staff.AddRange(resource1, resource2);
			releaseGroup2.Staff.AddRange(resource3, resource4);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Pizza", buffer, releaseGroupPK: releaseGroup1.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: releaseGroup2.PK);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Hamburger", buffer, releaseGroupPK: releaseGroup2.PK);
			var task2 = BMSTestHelper.CreateTask(workflow2, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: releaseGroup1.PK);
			var task3 = BMSTestHelper.CreateTask(workflow2, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1, taskGroupPK: releaseGroup2.PK);
			var task4 = BMSTestHelper.CreateTask(workflow2, capability: groupCapability, lowEstMinutes: 60, estVariationFactor: 1);

			Factory.Save();

			var staffCapacities = CapacityCalculator.GetUtilisedCapacityBreakdown(new[] { resource1, resource2, resource3, resource4 }, buffer);

			AssertReservedCapacityBreakdown(staffCapacities[resource1], zone3Hours: 0.5m); //Only Task2 / 2 resources = 30minutes or 0.5 hours
			AssertReservedCapacityBreakdown(staffCapacities[resource2], zone3Hours: 0.5m); //Only Task2 / 2 resources = 30minutes or 0.5 hours
			AssertReservedCapacityBreakdown(staffCapacities[resource3], zone3Hours: 1.5m); //Task 1,3,4 / 2 resources = 90minutes or 1.5 hours
			AssertReservedCapacityBreakdown(staffCapacities[resource4], zone3Hours: 1.5m); //Task 1,3,4 / 2 resources = 90minutes or 1.5 hours
		}

		#endregion

		#region Available Capacity

		[TestDate(2014, 12, 21)]
		public void TestGetAvailableCapacity_OutOfHours_ShouldConsiderCapabilityTaskZoneMultipliers()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource = CreateStaffInCurrentBranchDept("BEN", "Benedict Bumcerbatch");
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			resource.Capabilities.Add(capability);

			AssertEquals(false, resource.IsWorkingToday);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var multiplier = config.Buffer.ZoneCapacityMultipliers.AddNew();
			multiplier.BZC_Zone0Multiplier = 10;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer, ZDateTime.UtcNow.AddDays(-100));
			var task = CreateTask(workflow, string.Empty, 60, estVariationFactor: 1, capability: capability);

			Factory.Save();

			const decimal expectedFullCapacity = 48m;
			const decimal expectedUtilisedCapacity = 10m;
			const decimal expectedAvailableCapacity = expectedFullCapacity - expectedUtilisedCapacity;

			AssertEquals(expectedFullCapacity, CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer));
			AssertEquals("1 hour in the buffer, multiplied by 10x for zone 0", expectedUtilisedCapacity, CapacityCalculator.GetUtilisedCapacity_ForTest(resource, config.Buffer));
			AssertEquals("1 hour in the buffer, multiplied by 10x for zone 0", expectedAvailableCapacity, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			AssertEquals(expectedAvailableCapacity, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
		}

		[TestDate(2014, 12, 21)]
		public void TestGetAvailableCapacity_OutOfHours_ForTeamReservation_ShouldConsiderCapabilityTaskZoneMultipliers()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Benedict Bumcerbatch");
			var resource2 = CreateStaffInCurrentBranchDept("JWT", "Wohn Jatson");
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.ResourcesWithCapability.AddRange(resource1, resource2);

			AssertEquals(false, resource1.IsWorkingToday);
			AssertEquals(false, resource2.IsWorkingToday);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var multiplier = config.Buffer.ZoneCapacityMultipliers.AddNew();
			multiplier.BZC_Zone0Multiplier = 10;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer, ZDateTime.UtcNow.AddDays(-100));
			var task = CreateTask(workflow, string.Empty, 60, estVariationFactor: 1, capability: capability);

			Factory.Save();

			const decimal expectedFullCapacity = 48m;
			const decimal expectedUtilisedCapacity = 5m; // 30 mins in the buffer, multiplied by 10x for zone 0
			const decimal expectedAvailableCapacity = expectedFullCapacity - expectedUtilisedCapacity;

			var resources = capability.ResourcesWithCapability.Cast<GlbStaff>().ToArray();

			foreach (var resource in resources)
			{
				AssertEquals(expectedFullCapacity, CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer));
				AssertEquals(expectedUtilisedCapacity, CapacityCalculator.GetUtilisedCapacity_ForTest(resource, config.Buffer));
				AssertEquals(expectedAvailableCapacity, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			}
		}

		[TestDate(2014, 12, 21)]
		public void TestGetAvailableCapacity_OutOfHours_WhenTasksExistInReleaseGate_ShouldConsiderCapabilityTaskZoneMultipliers()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Benedict Bumcerbatch");
			var resource2 = CreateStaffInCurrentBranchDept("JWT", "Wohn Jatson");
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.ResourcesWithCapability.AddRange(resource1, resource2);

			AssertEquals(false, resource1.IsWorkingToday);
			AssertEquals(false, resource2.IsWorkingToday);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var multiplier = config.Buffer.ZoneCapacityMultipliers.AddNew();
			multiplier.BZC_Zone0Multiplier = 10;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer, ZDateTime.UtcNow.AddDays(-100));
			var task1 = CreateTask(workflow1, string.Empty, 60, estVariationFactor: 1, capability: capability);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task2_1 = CreateTask(workflow2, resource1.GS_Code, 60, estVariationFactor: 1, capability: capability);
			var task2_2 = CreateTask(workflow2, resource2.GS_Code, 60, estVariationFactor: 1, capability: capability);

			Factory.Save();

			const decimal expectedFullCapacity = 48m;
			const decimal expectedUtilisedCapacity = 5m; // 30 mins in the buffer, multiplied by 10x for zone 0
			const decimal expectedAvailableCapacity = expectedFullCapacity - expectedUtilisedCapacity;

			var resources = capability.ResourcesWithCapability.Cast<GlbStaff>().ToArray();

			foreach (var resource in resources)
			{
				AssertEquals(expectedFullCapacity, CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer));
				AssertEquals(expectedUtilisedCapacity, CapacityCalculator.GetUtilisedCapacity_ForTest(resource, config.Buffer));
				AssertEquals(expectedAvailableCapacity, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			}
		}

		[TestDate(2013, 7, 12)]
		public void TestCapacity_ShouldNotCountTasksWithCapabilityInFull()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff.Capabilities.Add(capability);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			var fullCapacity = CapacityCalculator.GetFullCapacity_ForTest(staff, buffer);
			var expectedCapacity = fullCapacity - 1.0m * 1.5m;
			var actualCapacity = CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer).AvailableCapacity;

			AssertEquals("Should reduce STD estimate by registry reduction percentage when calculating capacity. Full capacity = " + fullCapacity, expectedCapacity, actualCapacity);
		}

		[TestDate(2013, 7, 12)]
		public void TestCapacity_ShouldNotIgnoreCapabilityReductionsInDifferentReleaseGroups()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var staff1InReleaseGroup = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			var staff2InReleaseGroup = CreateStaffInCurrentBranchDept("BLA", "Bladonildo", capability);
			var staffNotInReleaseGroup = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins", capability);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff1InReleaseGroup.Groups.Add(group);
			staff2InReleaseGroup.Groups.Add(group);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = CreateTask(workflow, ZString.Empty, 60, capability: capability);

			Factory.Save();

			var fullCapacityStaff1InReleaseGroup = CapacityCalculator.GetFullCapacity_ForTest(staff1InReleaseGroup, buffer);
			var fullCapacityStaff2InReleaseGroup = CapacityCalculator.GetFullCapacity_ForTest(staff2InReleaseGroup, buffer);
			var expectedCapacityForStaffsInReleaseGroup = fullCapacityStaff1InReleaseGroup - 0.5m * task.RelevantEstimateHours;
			var fullCapacityStaffsNotInReleaseGroup = CapacityCalculator.GetFullCapacity_ForTest(staffNotInReleaseGroup, buffer);
			var actualCapacityStaff1InReleaseGroup = CapacityCalculator.GetUtilisedCapacityBreakdown(staff1InReleaseGroup, buffer).AvailableCapacity;
			var actualCapacityStaff2InReleaseGroup = CapacityCalculator.GetUtilisedCapacityBreakdown(staff2InReleaseGroup, buffer).AvailableCapacity;
			var actualCapacityStaffNotInReleaseGroup = CapacityCalculator.GetUtilisedCapacityBreakdown(staffNotInReleaseGroup, buffer).AvailableCapacity;

			AssertEquals("Should reduce STD estimate by registry reduction percentage of staffs how bellow to the release when calculating capacity. Full capacity = " + fullCapacityStaff1InReleaseGroup, expectedCapacityForStaffsInReleaseGroup, actualCapacityStaff1InReleaseGroup);
			AssertEquals("Should reduce STD estimate by registry reduction percentage of staffs how bellow to the release when calculating capacity. Full capacity = " + fullCapacityStaff2InReleaseGroup, expectedCapacityForStaffsInReleaseGroup, actualCapacityStaff2InReleaseGroup);
			AssertEquals("Staff not in Release Group should have full capacity since he doesn't bellow to the release group and the capability is GroupScope.", fullCapacityStaffsNotInReleaseGroup, actualCapacityStaffNotInReleaseGroup);
		}

		[TestDate(2013, 7, 12)]
		public void TestCapacity_ShouldIgnoreCapabilityReductionsForOtherCapabilities()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			var staff1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);
			var staff2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins", capability1, capability2);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(staff1, staff2);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = CreateTask(workflow, ZString.Empty, 60, capability: capability1);
			var task2 = CreateTask(workflow, ZString.Empty, 60, capability: capability2);

			Factory.Save();

			var fullCapacityStaff1 = CapacityCalculator.GetFullCapacity_ForTest(staff1, buffer);
			var fullCapacityStaff2 = CapacityCalculator.GetFullCapacity_ForTest(staff2, buffer);

			var expectedCapacityStaff1 = fullCapacityStaff1 - 0.5m * task1.RelevantEstimateHours;
			var expectedCapacityStaff2 = fullCapacityStaff2 - ((0.5m * task1.RelevantEstimateHours) + (1.0m * task2.RelevantEstimateHours));

			var actualCapacityStaff1 = CapacityCalculator.GetUtilisedCapacityBreakdown(staff1, buffer).AvailableCapacity;
			var actualCapacityStaff2 = CapacityCalculator.GetUtilisedCapacityBreakdown(staff2, buffer).AvailableCapacity;

			AssertEquals("Staff1 capacity should include first task only in capacity reduction since resource has just one capability. Full capacity = " + fullCapacityStaff1, expectedCapacityStaff1, actualCapacityStaff1);
			AssertEquals("Staff2 capacity should include both tasks in capacity reduction since resource has both capabilities. Full capacity = " + fullCapacityStaff2, expectedCapacityStaff2, actualCapacityStaff2);
		}

		[TestDate(2013, 4, 10)]
		public void TestCapacityShouldTakeHolidaysIntoAccount()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			UpdateStaffWeekDaysTo9To5ExceptMonday9To4(staff.PK);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			buffer.FC_BufferLoadLimitPercent = 100;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = CreateTask(workflow, staff.GS_Code, 120);

			Factory.Save();

			AssertEquals(91m, CapacityCalculator.GetUtilisedCapacityBreakdown(staff, buffer, useCache: false).AvailableCapacity);

			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(6), staff.PK, workHolidayType: "OVS"); // 5 day holiday, but only 3 days are weekdays
			Factory.Save();

			// Loading in a new factory since WorkingDays is cached in the factory, so new holiday is not taken into account yet.
			var newFactory = new BusinessObjectFactory();
			var loadedStaff = newFactory.Load<GlbStaff>(staff.PK);
			var loadedBuffer = newFactory.Load<BMComponent>(buffer.PK);

			AssertEquals(68m, CapacityCalculator.GetUtilisedCapacityBreakdown(loadedStaff, loadedBuffer).AvailableCapacity);
		}

		#endregion

		#region Performance

		[TestDate(2015, 11, 14)]
		public void TestCapacityCalculator_ShouldUseTableValuedParametersInResourcesWithTasksInReleaseGateQuery()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Bring me the blue pages", config.Bucket);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				ReleaseGateKeeperTest.RunReleaseGate(config.System);
				var resourcesWithTasksInReleaseGateSql = TestConnection.ExecutedCommandsAndQueryPlans.Single(t => t.Item1.Contains("DECLARE @Result TABLE")).Item1;
				AssertEquals(true, resourcesWithTasksInReleaseGateSql.Contains("(SELECT Value FROM @ComponentPKs)"));
				AssertEquals(true, resourcesWithTasksInReleaseGateSql.Contains("Params"));
			}
		}

		[TestDate(2015, 11, 14)]
		public void TestCapacityCalculator_GlbGroupLink_DbHits()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var resources = new List<GlbStaff>();

			for (int i = 0; i < 5; i++)
			{
				var group = Factory.NewWithValidTestData<GlbGroup>();

				var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				resource.Capabilities.Add(groupCapability);
				resources.Add(resource);
				group.Staff.Add(resource);

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, i.ToString(), config.Buffer, releaseGroupPK: group.PK);

				BMSTestHelper.CreateTask(workflow, staffCode: ZString.Empty, 60, capability: groupCapability);
			}

			var expectedHits = new Dictionary<string, int>
			{
				{ GlbGroupLinkSchema.Constants.TableName, 1 },
				{ BMZoneCapacityMultiplierSchema.Constants.TableName, 1 },
			};

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var buffer = newFactory.Load<BMComponent>(config.Buffer.PK);

			CapacityCalculator.GetUtilisedCapacityBreakdown(resources, buffer);

			AssertDbHits(expectedHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		[TestDate(2015, 11, 14)]
		public void TestCapacityCalculatorQueryPlan_WhenFilteringByStaff_ShouldNotHaveConversionWarning()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var groupCapability = BMSTestHelper.CreateCapability(Factory, "GRP", "C");
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var resources = new List<GlbStaff>();

			for (int i = 0; i < 5; i++)
			{
				var group = Factory.NewWithValidTestData<GlbGroup>();

				var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				resource.Capabilities.Add(groupCapability);
				resources.Add(resource);
				group.Staff.Add(resource);

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, i.ToString(), config.Buffer, releaseGroupPK: group.PK);

				BMSTestHelper.CreateTask(workflow, staffCode: ZString.Empty, 60, capability: groupCapability);
			}

			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				CapacityCalculator.GetUtilisedCapacityBreakdown(resources, config.Buffer);

				var queryAndPlan = TestConnection.ExecutedCommandsAndQueryPlans.Single(x => x.Item1.Contains("@StaffPKs"));

				foreach (var plan in queryAndPlan.Item2)
				{
					AssertNotContains("If the plan contains this warning then it means we're specifying the wrong type for our @StaffPKs parameter, which will affect performance.",
						"G5_GS_Resource]=CONVERT_IMPLICIT(uniqueidentifier", plan);
				}
			}
		}

		#endregion

		#region Cache

		[TestDate(2015, 11, 14)]
		public void TestShouldNotUseStaleCacheData()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Bring me the blue pages", config.Bucket);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60);

			Factory.Save();

			// Force calculation and caching of capacity.
			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			AssertEquals("workflow1 is now released, so cached capacity is updated", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Don't touch the red pages", config.Buffer);
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 60);

			Factory.Save();

			AssertEquals("Cached capacity isn't stale yet, so new workflow in the buffer doesn't affect it", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(75);
			AssertEquals("Cached capacity still isn't stale yet, so re-use it (use the default stale interval: 15m * 5)", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AssertEquals("Cached capacity is stale now, so ditch it", 36.38m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
		}

		[TestDate(2015, 11, 14)]
		public void TestShouldNotUseStaleCacheData_WhenReleaseGateRunnerServiceTaskIntervalLongerThanDefaultCacheStaleTime()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.CreateServiceTask(Factory, ReleaseGateRunnerServiceTask.Code, ReleaseGateRunnerServiceTask.Description, taskPeriodCount: 3, taskPeriod: 'H'); // Run every 3 hours

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Bring me the blue pages", config.Bucket);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60);

			Factory.Save();

			// Force calculation and caching of capacity.
			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			AssertEquals("workflow1 is now released, so cached capacity is updated", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Don't touch the red pages", config.Buffer);
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 60);

			Factory.Save();

			AssertEquals("Cached capacity isn't stale yet, so new workflow in the buffer doesn't affect it", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			// Interval now defaults to fixed 15 minutes, it is no longer supported to query the service task schedule period.
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(75);
			AssertEquals("Cached capacity still isn't stale yet, so re-use it (just reached default interval of 15m * factor)", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AssertEquals("Cached capacity is stale now, so ditch it", 36.38m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
		}

		[TestDate(2015, 11, 14)]
		public void TestShouldNotUseStaleCacheData_WhenThereAreMultipleServiceTasks_ShouldUseDurationOfLastActiveServiceTaskToRun()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.CreateServiceTask(Factory, ReleaseGateRunnerServiceTask.Code, ReleaseGateRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', active: false, nextRunTime: ZDateTime.UtcNow.AddMinutes(5)); // Run every 1 hour, never because it's inactive
			BMSTestHelper.CreateServiceTask(Factory, ReleaseGateRunnerServiceTask.Code, ReleaseGateRunnerServiceTask.Description, taskPeriodCount: 2, taskPeriod: 'H', active: true, nextRunTime: ZDateTime.UtcNow.AddDays(-1)); // Run every 2 hours, yesterday
			BMSTestHelper.CreateServiceTask(Factory, ReleaseGateRunnerServiceTask.Code, ReleaseGateRunnerServiceTask.Description, taskPeriodCount: 3, taskPeriod: 'H', active: true, nextRunTime: ZDateTime.UtcNow.AddMinutes(10)); // Run every 3 hours, in 10 mins time

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Bring me the blue pages", config.Bucket);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60);

			Factory.Save();

			// Force calculation and caching of capacity.
			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			AssertEquals("workflow1 is now released, so cached capacity is updated", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Don't touch the red pages", config.Buffer);
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 60);

			Factory.Save();

			AssertEquals("Cached capacity isn't stale yet, so new workflow in the buffer doesn't affect it", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			// Interval now defaults to fixed 15 minutes, it is no longer supported to query the service task schedule period.
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(75);
			AssertEquals("Cached capacity still isn't stale yet, so re-use it (just reached default interval 15m * factor)", 37.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AssertEquals("Cached capacity is stale now, so ditch it", 36.38m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
		}

		#endregion

		#region Capacity Ceiling

		[TestUtcOffset(11, 0, 0)] // AEDT time zone
		[TestDate(2018, 10, 19, 9, 0, 0)] // Friday
		public void TestFullCapacity_ShouldBeCappedAtBufferSize()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			branch.GB_RL_NKHomePort = "AUSYD";

			WorkingDaysTestHelper.SetDefaultDepartmentWorkTimeWeek(Factory, department.PK);

			config.Buffer.FC_GB_AgingBranch = branch.PK;
			config.Buffer.FC_GE_AgingDepartment = department.PK;
			config.Buffer.FC_BufferTimespanInMinutes = 8 * 60 * 3; // Three 8-hour days

			var resource1 = BMSTestHelper.CreateStaff(Factory, "RC1", "Robert One", branch, department);
			var resource2 = BMSTestHelper.CreateStaff(Factory, "RC2", "Robert Two", branch, department);

			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource1.PK); // Rob1 works weekdays
			WorkingDaysTestHelper.UpdateStaffWeekDaysTo9To5(Factory, resource2.PK); // Rob2 works weekdays

			UpdateStaffWeekendsTo9To5(resource2.PK); // Rob2 also works weekends, bless him.

			Factory.Save();

			AssertFullCapacity(resource1, config.Buffer, 12m);
			AssertFullCapacity("Even though Rob2 works the entire buffer time window period, including non-work hours in this window, he shouldn't get dumped on with work to fill the entire time window all at once.", resource2, config.Buffer, 12m);
		}

		#endregion

		#region Turned off in registry

		public void TestGetComponentContents_WhenDisableCapacityCalculationsEnabled_ShouldReportError()
		{
			AssertEquals(false, BMSRegistry.Instance.DisableCapacityCalculations.Value);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			Factory.Save();

			CapacityCalculator.GetComponentContents_ForTest(config.Buffer, new[] { staff });
			AssertEquals("No error should be reported because capacity calculation is enabled.", string.Empty, ErrorReporter.LastMessageReported);

			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CapacityCalculator.GetComponentContents_ForTest(config.Buffer, new[] { staff });

			AssertEquals("Capacity was calculated even though the DisableCapacityCalculations registry item was enabled.", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		#endregion

		#region Implementation

		void UpdateStaffWeekendsTo9To5(ZGuid staffPK)
		{
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Saturday, NineToFive);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Sunday, NineToFive);
		}

		void UpdateStaffWeekDaysTo9To12(ZGuid staffPK)
		{
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Monday, NineToTwelve);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Tuesday, NineToTwelve);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Wednesday, NineToTwelve);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Thursday, NineToTwelve);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Friday, NineToTwelve);
		}

		void UpdateDepartmentWeekDaysTo9To5ExceptMonday9To4(ZGuid departmentPK)
		{
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Monday, NineToFour);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Tuesday, NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Wednesday, NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Thursday, NineToFive);
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, departmentPK, DayOfWeek.Friday, NineToFive);
		}

		void UpdateStaffWeekDaysTo9To5ExceptMonday9To4(ZGuid staffPK)
		{
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Monday, NineToFour);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Tuesday, NineToFive);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Wednesday, NineToFive);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Thursday, NineToFive);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staffPK, DayOfWeek.Friday, NineToFive);
		}

		static string NineToFive
		{
			get { return WorkingDaysTestHelper.NineToFive; }
		}

		static string NineToFour
		{
			get { return NineToFive.Substring(0, NineToFive.Length - 2); }
		}

		static string NineToTwelve
		{
			get { return NineToFive.Substring(0, NineToFive.Length - 10); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		#endregion
	}

	#region Non-transactioned Test

	class CapacityCalculatorNonTransactionedTest : NonTransactionedTestCase
	{
		[SnailTest]
		[RequiresLargeLogFile]
		[TestDate(2015, 7, 14)]
		public void TestGetComponentContents_QueryPlanShouldNotIncludeBadThings_SelectFromWorkflowDurations()
		{
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			const int numResources = 500;
			const int numCapabilities = 50;
			const int numJobs = 100;
			const int numWorkflowsPerJob = 5;
			const int numUnrelatedShapes = 1000;
			SchematicTestConfig config;
			GlbStaff[] resources;

			SetupGetComponentContentsTestData(numResources, numCapabilities, numJobs, numWorkflowsPerJob, numUnrelatedShapes, out config, out resources);

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var results = CapacityCalculator.GetComponentContents_ForTest(config.Buffer, resources);
				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("CAPACITY CALCULATION")); // Get the first instance because we execute the statement in batches of resources.
				var planalyzer = new QueryPlanalyzer(queryPlans.Item2.Last()); // Get the plan for the last statement in the batch because that's the chunky one.

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("TableScans", new[]
					{
						"#WorkFlowDurations",
					}, planalyzer.TableScans.Select(x => x.TableName));

					AssertNotNull(planalyzer.TableScans.Single(x => x.TableName == "#WorkFlowDurations"));
					AssertEquals(2, planalyzer.IndexScans.Count(x => x.TableName == BMComponentSchema.Constants.TableName));

					QueryPlanalyzer.AssertNoRIDLookups(planalyzer);

					// Not asserting index scans/seeks because they are too reliant on SQL statistics and data combinations.
				});
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestGetComponentContents_ShouldLogProvider_IfUsingOldQuery()
		{
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var log = GetLogForGetComponentContentsTest();
			AssertNotContains("Using Capacity Query...", log);
			AssertContains("Using Old Capacity Query...", log);
			AssertContains("Finished loading workflows", log);
		}

		[TestDate(2015, 7, 14)]
		public void TestGetComponentContents_ShouldLogProvider_IfUsingSimpleQuery()
		{
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();

			var log = GetLogForGetComponentContentsTest();
			AssertContains("Using Capacity Query...", log);
			AssertNotContains("Using Old Capacity Query...", log);
			AssertContains("Finished loading workflows", log);
		}

		string GetLogForGetComponentContentsTest()
		{
			const int numResources = 50;
			const int numCapabilities = 5;
			const int numJobs = 10;
			const int numWorkflowsPerJob = 5;
			const int numUnrelatedShapes = 100;

			SetupGetComponentContentsTestData(numResources, numCapabilities, numJobs, numWorkflowsPerJob, numUnrelatedShapes, out var config, out var resources);

			var logger = new BufferManagementLogger();
			CapacityCalculator.GetComponentContents_ForTest(config.Buffer, resources, logger);
			return logger.ToString();
		}

		void SetupGetComponentContentsTestData(int numResources, int numCapabilities, int numJobs, int numWorkflowsPerJob, int numUnrelatedShapes, out SchematicTestConfig config, out GlbStaff[] resources)
		{
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			resources = Enumerable.Range(0, numResources).Select(i => BMSTestHelper.CreateStaffInCurrentBranchDept(Factory)).ToArray();
			var capabilities = Enumerable.Range(0, numCapabilities).Select(i => BMSTestHelper.CreateCapability(Factory)).ToArray();

			const int numBatches = 10;

			for (var batchNumber = 0; batchNumber < numBatches; batchNumber++)
			{
				var resourceBatch = resources.Skip(batchNumber * (numResources / numBatches)).Take(numResources / numBatches);
				var capabilityBatch = capabilities.Skip(batchNumber * (numCapabilities / numBatches)).Take(numResources / numBatches);

				resourceBatch.ForEach(r => r.Capabilities.AddRange(capabilityBatch));
			}

			Factory.Save();

			var shapesInsertHelper = new BatchInsertHelper();
			var schedulesInsertHelper = new BatchInsertHelper();
			var attachmentsInsertHelper = new BatchInsertHelper();
			var workflowInsertHelper = new BatchInsertHelper();
			var processHeaderLinkInsertHelper = new BatchInsertHelper();
			var taskInsertHelper = new BatchInsertHelper();

			var estimatedDuration = new ZInt(60).GetDateTimeFromMinutes().ToDateTime();

			for (var shapeIndex = 0; shapeIndex < numUnrelatedShapes; shapeIndex++)
			{
				var diagramPK = Guid.NewGuid();
				var shape1PK = Guid.NewGuid();
				var shape2PK = Guid.NewGuid();

				shapesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeInsertSql(diagramPK, "Diagram " + shapeIndex, approvedBy: "DEA", shapeType: "DIA", relatedEntityTableCode: ""));
				schedulesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeScheduleInsertSql(diagramPK));

				shapesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeInsertSql(shape1PK, "Shape1 " + shapeIndex, approvedBy: "DEA", shapeType: "SHP", relatedEntityTableCode: "", parentShape: diagramPK, rootShape: diagramPK));
				schedulesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeScheduleInsertSql(shape1PK));

				shapesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeInsertSql(shape2PK, "Shape2 " + shapeIndex, approvedBy: "DEA", shapeType: "SHP", relatedEntityTableCode: "", parentShape: diagramPK, rootShape: diagramPK));
				schedulesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeScheduleInsertSql(shape2PK));

				attachmentsInsertHelper.AddInsertStatement(BMDbTestHelper.GetAttachmentInsertSql(Guid.NewGuid(), shape1PK, shape2PK, diagramPK, "DEP", approvedBy: "DEA"));
			}

			for (var jobIndex = 0; jobIndex < numJobs; jobIndex++)
			{
				var jobHeaderPK = Guid.NewGuid();
				var parentJobPK = Guid.NewGuid();
				var workflowPKs = Enumerable.Range(0, numWorkflowsPerJob).Select(i => Guid.NewGuid()).ToArray();

				workflowInsertHelper.AddInsertStatement(BMDbTestHelper.GetProcessHeaderInsertSql(jobHeaderPK, null, parentJobPK));

				if (jobIndex % 3 == 0)
				{
					var diagramPK = Guid.NewGuid();

					shapesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeInsertSql(diagramPK, "Diagram for job " + jobIndex, approvedBy: "DEA", shapeType: "DIA", relatedEntityPK: jobHeaderPK));
					schedulesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeScheduleInsertSql(diagramPK, scheduledStartTime: ZDateTime.UtcNow.AddDays(-15).ToDateTime(), scheduledFinishTime: ZDateTime.UtcNow.AddDays(-10).ToDateTime(), durationMinutes: 2400, isBuffered: true));

					var bufferShapePK = Guid.NewGuid();

					shapesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeInsertSql(bufferShapePK, "Project buffer for job " + jobIndex, approvedBy: "DEA", shapeType: "BUF", parentShape: diagramPK, rootShape: diagramPK));
					schedulesInsertHelper.AddInsertStatement(BMDbTestHelper.GetShapeScheduleInsertSql(bufferShapePK, scheduledStartTime: ZDateTime.UtcNow.AddDays(-10).ToDateTime(), scheduledFinishTime: ZDateTime.UtcNow.AddDays(-5).ToDateTime(), durationMinutes: 2400, isBuffered: true));
					attachmentsInsertHelper.AddInsertStatement(BMDbTestHelper.GetAttachmentInsertSql(Guid.NewGuid(), diagramPK, bufferShapePK, diagramPK, "BUF", approvedBy: "DEA"));
				}

				for (var workflowIndex = 0; workflowIndex < numWorkflowsPerJob; workflowIndex++)
				{
					var workflowPK = workflowPKs[workflowIndex];
					var componentPK = workflowIndex % 2 == 0 ? config.Bucket.PK : config.Buffer.PK;

					workflowInsertHelper.AddInsertStatement(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK, jobHeaderPK, parentJobPK, templatePK: null, completionStatement: "Workflow " + workflowIndex, componentPK: componentPK.ToGuid()));

					foreach (var resource in resources)
					{
						taskInsertHelper.AddInsertStatement(BMDbTestHelper.GetProcessTasksInsertSql(Guid.NewGuid(), workflowPK, parentJobPK, "WKI", "ASN", assignedStaff: resource.GS_Code, estimatedDuration: estimatedDuration));
					}

					foreach (var capability in capabilities)
					{
						taskInsertHelper.AddInsertStatement(BMDbTestHelper.GetProcessTasksInsertSql(Guid.NewGuid(), workflowPK, parentJobPK, "WKI", "ASN", requiredCapability: capability.PK.ToGuid(), estimatedDuration: estimatedDuration));
					}

					if (workflowIndex < numWorkflowsPerJob / 2)
					{
						if (workflowIndex > 0)
						{
							// Make some workflows inherit buffer penetration from their parents.
							processHeaderLinkInsertHelper.AddInsertStatement(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflowPK, workflowPKs[workflowIndex - 1], ProcessHeaderLinkTypeList.Codes.ParentChild, syncBufferPenetration: true));
						}
					}
					else
					{
						// Add some dependencies to complicate matters
						processHeaderLinkInsertHelper.AddInsertStatement(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflowPK, workflowPKs[workflowIndex - 1], ProcessHeaderLinkTypeList.Codes.Dependency));
					}
				}
			}

			shapesInsertHelper.ExecuteAll(TestConnection);
			schedulesInsertHelper.ExecuteAll(TestConnection);
			attachmentsInsertHelper.ExecuteAll(TestConnection);
			workflowInsertHelper.ExecuteAll(TestConnection);
			processHeaderLinkInsertHelper.ExecuteAll(TestConnection);
			taskInsertHelper.ExecuteAll(TestConnection);
		}
	}

	#endregion
}
