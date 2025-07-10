using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkingResourceCalculatorTest : BMSTestCaseWithFactory
	{
		[TestDate(2013, 12, 15, 23, 59, 0)]
		public void TestGetWorkingResourcesWithCapability_ShouldUseResourceLocalTime()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var homeBranch = Factory.NewWithValidTestData<GlbBranch>();
			homeBranch.GB_RL_NKHomePort = "AUSYD";

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_GB_HomeBranch = homeBranch.PK;
			resource.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			resource.Capabilities.Add(capability);

			Factory.Save();

			var resources = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability.PK, Factory, WorkingTimeContext.Create(Factory)).ToArray();
			AssertEquals(1, resources.Length);
			AssertEquals(resource, resources[0]);
		}

		[TestDate(2013, 12, 15, 23, 59, 0)]
		public void TestGetWorkingResourcesWithCapability_ShouldFilterOnCapabilityScope()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var system = CreateSystem("ORG");

			var staff1 = CreateStaffInCurrentBranchDept("DEA", "Big Dave");
			var staff2 = CreateStaffInCurrentBranchDept("DNK", "Dan the Man");
			var staff3 = CreateStaffInCurrentBranchDept("BEN", "Big Ben");
			var staff4 = CreateStaffInCurrentBranchDept("ALE", "Alex Awesome");
			var staff5 = CreateStaffInCurrentBranchDept("FRO", "Froyo");
			var staff6 = CreateStaffInCurrentBranchDept("BIL", "Biblios");
			var staff7 = CreateStaffInCurrentBranchDept("EEE", "Im falling eee");
			var staff8 = CreateStaffInCurrentBranchDept("TUR", "Turtle");

			var globalCapability = Factory.NewWithValidTestData<GlbCapability>();
			globalCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			staff3.Capabilities.Add(globalCapability);
			staff4.Capabilities.Add(globalCapability);

			var groupCapability = Factory.NewWithValidTestData<GlbCapability>();
			groupCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			staff7.Capabilities.Add(groupCapability);
			staff8.Capabilities.Add(groupCapability);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup1 = CreateReleaseGroup(system, group1);
			group1.Staff.AddRange(staff1, staff3);

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = CreateReleaseGroup(system, group2);
			group2.Staff.AddRange(staff5, staff7);

			Factory.Save();

			var resourcesWithGlobalCapability = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(globalCapability.PK, Factory, WorkingTimeContext.Create(Factory), group1.PK).ToList();
			var resourcesWithGroupCapability = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(groupCapability.PK, Factory, WorkingTimeContext.Create(Factory), group2.PK).ToList();

			AssertEquals(2, resourcesWithGlobalCapability.Count);
			AssertCollectionContains(staff3, resourcesWithGlobalCapability);
			AssertCollectionContains(staff4, resourcesWithGlobalCapability);

			AssertEquals(1, resourcesWithGroupCapability.Count);
			AssertCollectionContains(staff7, resourcesWithGroupCapability);
		}

		[TestDate(2015, 05, 20, 10, 45, 52)]
		public void TestGetWorkingResourcesWithCapability_CanGetLeaveOrWorkingDayCorrectly()
		{
			var orgTestDate = TestDateAttribute.Date;

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var system = CreateSystem("ORG");

			var staff1 = CreateStaffInCurrentBranchDept("BAT", "Batman");

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			staff1.Capabilities.Add(capability1);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup1 = CreateReleaseGroup(system, group1);
			group1.Staff.AddRange(staff1);

			Factory.Save();

			//Ensure the day is a working day before BMSLeave is set
			var resourcesWithCapability1 = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability1.PK, Factory, WorkingTimeContext.Create(Factory), group1.PK).ToList();
			AssertEquals(TestDateAttribute.Date.ToShortDateString() + " should be working before setting BMSLeave", 1, resourcesWithCapability1.Count);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			resourcesWithCapability1 = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability1.PK, Factory, WorkingTimeContext.Create(Factory), group1.PK).ToList();
			AssertEquals(TestDateAttribute.Date.ToShortDateString() + " should be working before setting BMSLeave", 1, resourcesWithCapability1.Count);

			//Now add BMSLeave
			TestDateAttribute.Date = orgTestDate;
			var staffHoliday = staff1.HolidaysIncBMSLeave.AddNew();
			staffHoliday.GA_StartTime = orgTestDate.Date;
			staffHoliday.GA_EndTime = orgTestDate.AddDays(1).Date;
			staffHoliday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			Factory.Save();

			resourcesWithCapability1 = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability1.PK, Factory, WorkingTimeContext.Create(Factory), group1.PK).ToList();
			AssertEquals(TestDateAttribute.Date.ToShortDateString() + " should be on leave after setting BMSLeave ", 0, resourcesWithCapability1.Count);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			resourcesWithCapability1 = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability1.PK, Factory, WorkingTimeContext.Create(Factory), group1.PK).ToList();
			AssertEquals(TestDateAttribute.Date.ToShortDateString() + " should be working after setting BMSLeave", 1, resourcesWithCapability1.Count);
		}

		[TestDate(2013, 12, 15, 23, 59, 0)] // Sunday
		public void TestGetWorkingResourcesWithCapability_WithNoLocalTime_ShouldUseValueFromWorkingTimeContext()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.Capabilities.Add(capability);
			resource.ResetBranchAndDepartment();

			AssertNull("Precondition - Resource should have no home branch", resource.HomeBranch);

			var branchUsedInEnvironmentContext = Factory.NewWithValidTestData<GlbBranch>();
			branchUsedInEnvironmentContext.GB_Code = "EVB";
			branchUsedInEnvironmentContext.GB_RL_NKHomePort = "USSFO"; // GMT -5 (Still Sunday)

			var branchUsedInWorkingTimeContext = Factory.NewWithValidTestData<GlbBranch>();
			branchUsedInWorkingTimeContext.GB_Code = "WTB";
			branchUsedInWorkingTimeContext.GB_RL_NKHomePort = "AUSYD"; // GMT +10 (Now Monday)

			var workingTimeContext = WorkingTimeContext.Create(new DummyBranchDepartmentProvider(branchUsedInWorkingTimeContext, (GlbDepartment)Env.CurrentDepartment), resource);

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), branchUsedInEnvironmentContext.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

				var resources = WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capability.PK, Factory, workingTimeContext).ToArray();
				AssertEquals("We should have one resource working, as calculator should use branch from WorkingTimeContext", 1, resources.Length);
				AssertEquals("The resource we created should be working", resource, resources[0]);
			}
		}

		#region Performance

		public void TestResourcesWithTasksInReleaseGateSql_ShouldNotCheckForNullReleaseGroupOnProcessHeader()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			//_ = config.Buffer; // otherwise it will be null
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				ReleaseGateKeeperTest.RunReleaseGate(config.System, new BufferManagementLogger());

				var staffCodeQuery = TestConnection.ExecutedCommands.Single(x => x.Contains("DECLARE @Result TABLE (GS_Code varchar(3) not null)"));
				AssertNotContains("We should not include the null check in the query, because this will actually slow it down massively. SAD!", "FH_GG_ReleaseGroup IS NOT NULL", staffCodeQuery);
			}
		}

		#endregion

	}
}
