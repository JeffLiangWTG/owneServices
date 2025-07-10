using CargoWise.PAVE.Common.Interfaces;
using CargoWise.PAVE.Common.TestUtils;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestDate(2014, 7, 15, 10, 0, 0)]
	class BufferPenetrationCalculatorTest : BMSTestCaseWithFactory
	{
		public void TestCalculatePenetration_WhenCircularAncestryExists_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "The earth spins on its axis", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "One man struggles", config.Buffer);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "While another relaxes", config.Buffer);

			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2);
			BMSTestHelper.CreateTask(workflow3);

			BMSTestHelper.CreateParentChildLink(workflow3, workflow2, syncBufferPenetration: true);
			BMSTestHelper.CreateParentChildLink(workflow2, workflow1, syncBufferPenetration: true);
			BMSTestHelper.CreateParentChildLink(workflow1, workflow3, syncBufferPenetration: true);

			Factory.Save();

			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow1, context, Factory).Penetration);
			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow2, context, Factory).Penetration);
			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow3, context, Factory).Penetration);
		}

		public void TestCalculatePenetration_ForPCH()
		{
			TestDateAttribute.Date = new ZDateTime(1999, 12, 1, 0, 0, 0).ToUniversalBranchTime().ToDateTime();
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			TestDateAttribute.Date = new ZDateTime(2000, 1, 1, 0, 0, 0).ToUniversalBranchTime().ToDateTime();
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>();

			var parentWorkflow = CreateWorkflow(jobHeader, "parentWorkflow");
			parentWorkflow.FH_FC_CurrentComponent = config.Buffer.PK;
			parentWorkflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-3);
			var parentTask = CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertEquals(0.25m, BufferPenetrationCalculator.CalculatePenetrationPercentage(parentWorkflow, context, Factory).Penetration);

			var childWorkflow = CreateWorkflow(jobHeader, "childWorkflow");
			childWorkflow.FH_FC_CurrentComponent = config.Buffer.PK;
			childWorkflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-1);
			var childTask = CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			var link = childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			Factory.Save();

			AssertEquals("GIVEN childWorkflow is-child-of parentWorkflow", true, childWorkflow.IsChildOf(parentWorkflow));
			AssertEquals("GIVEN UseParentBufferPenetration=false", false, ((IChildBufferedItem)childWorkflow).UseParentBufferPenetration);
			AssertEquals(0.0833333333333333333333333333m, BufferPenetrationCalculator.CalculatePenetrationPercentage(childWorkflow, context, Factory).Penetration);

			link.FP_SynchroniseBufferPenetration = true;

			Factory.Save();

			AssertEquals("WHEN Setting UseParentBufferPenetration = true", true, ((IChildBufferedItem)childWorkflow).UseParentBufferPenetration);
			AssertEquals(0.265625m, BufferPenetrationCalculator.CalculatePenetrationPercentage(parentWorkflow, context, Factory).Penetration);
			AssertEquals(0.265625m, BufferPenetrationCalculator.CalculatePenetrationPercentage(childWorkflow, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_ChildWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(description: "jobHeader");
			var parentWorkflow = CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = CreateWorkflow(jobHeader, "childWorkflow");

			var parentTask = CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 60, description: "parentTask");
			var childTask = CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60, description: "childTask");

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			parentWorkflow.FH_FC_CurrentComponent = config.Buffer.PK;
			parentWorkflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-6); // includes 2 weekends

			childWorkflow.FH_FC_CurrentComponent = config.Buffer.PK;
			childWorkflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			AssertEquals(1m, BufferPenetrationCalculator.CalculatePenetrationPercentage(parentWorkflow, context, Factory).Penetration);
			AssertEquals(0.25m, BufferPenetrationCalculator.CalculatePenetrationPercentage(childWorkflow, context, Factory).Penetration);

			AssertReservedCapacityBreakdown("When we aren't synchronising buffer penetration, parent and child workflows reserve capacity in different zones",
				CapacityCalculator.GetUtilisedCapacityBreakdown(GlbStaff.CurrentUser, config.Buffer, useCache: false), zone3Hours: 1.5m, zone1Hours: 1.5m);

			childWorkflow.SynchroniseBufferPenetration = true;
			Factory.Save();

			AssertEquals(1m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(parentWorkflow, context, Factory).Penetration);
			AssertEquals(1m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(childWorkflow, context, Factory).Penetration);

			AssertReservedCapacityBreakdown("Now that we're synchronising buffer penetration between parent and child, capacity algorithm should produce the same result as buffer penetration",
				CapacityCalculator.GetUtilisedCapacityBreakdown(GlbStaff.CurrentUser, config.Buffer, useCache: false), zone1Hours: 3m);

			parentWorkflow.FH_FC_CurrentComponent = config.Bucket.PK;
			AssertEquals("WHEN moving parentWorkflow to bucket THEN parentWorkflow.penetration should be 0", 0.0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(parentWorkflow, context, Factory).Penetration);
			AssertEquals("THEN childWorkflow SHOULD not be parent penetration", 0.25m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(childWorkflow, context, Factory).Penetration);

			parentWorkflow.FH_FC_CurrentComponent = config.Buffer.PK;
			parentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			childTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals("WHEN closing all tasks, THEN the penetration = 0", 0.0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(parentWorkflow, context, Factory).Penetration);
			AssertEquals("WHEN closing all tasks, THEN the penetration = 0", 0.0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(childWorkflow, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_WhenAllItemsAreClosed()
		{
			var buffer = new DummyBuffer { SizeInMinutes = 10 };
			var item1 = new DummyBufferedItem(buffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-10) };
			var item2 = new DummyBufferedItem(buffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-10) };

			buffer.RelatedBufferedItems = new[] { item1, item2 };

			AssertEquals(1.0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(buffer, context, Factory).Penetration);

			item1.WorkStatus = WorkStatus.Cancelled;
			AssertEquals(1.0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(buffer, context, Factory).Penetration);

			item2.WorkStatus = WorkStatus.Complete;
			AssertEquals(0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(buffer, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_OfProjectBuffer_NoRelatedItems()
		{
			var projectBuffer = new DummyBuffer { Type = BufferType.Project, SizeInMinutes = 10 };
			projectBuffer.RelatedBufferedItems = System.Array.Empty<IBufferedItem>();

			AssertEquals(decimal.Zero, BufferPenetrationCalculator.CalculatePenetrationPercentage(projectBuffer, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_OfProjectBuffer()
		{
			var projectBuffer = new DummyBuffer { Type = BufferType.Project, SizeInMinutes = 10 };
			var item1 = new DummyBufferedItem(projectBuffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-10) };
			var item2 = new DummyBufferedItem(projectBuffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-5) };
			var item3 = new DummyBufferedItem(projectBuffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-2) };
			var item4 = new DummyBufferedItem(projectBuffer) { StartableTime = ZDateTime.UtcNow.ToDateTime() };

			projectBuffer.RelatedBufferedItems = new[] { item2, item3, item4 };

			AssertEquals(1.0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item1, context, Factory).Penetration);
			AssertEquals(0.5m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item2, context, Factory).Penetration);
			AssertEquals(0.2m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item3, context, Factory).Penetration);
			AssertEquals(0.0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item4, context, Factory).Penetration);

			AssertEquals("Should use the worst case of all related items (item 1 is not considered since it isn't related to the project buffer)", 0.5m, BufferPenetrationCalculator.CalculatePenetrationPercentage(projectBuffer, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_ForItemWithNoRelatedBuffers()
		{
			var item = new DummyBufferedItem();
			AssertEquals(decimal.Zero, BufferPenetrationCalculator.CalculatePenetrationPercentage(item, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage()
		{
			var item = new DummyBufferedItem(buffer_12min);

			item.StartableTime = ZDateTime.UtcNow.ToDateTime();
			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item, context, Factory).Penetration);

			for (int i = 1; i <= buffer_12min.SizeInMinutes; i++)
			{
				item.StartableTime = item.StartableTime.AddMinutes(-1);
				AssertEquals(decimal.Round((1 / 12m) * i, 2), decimal.Round(BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(item, context, Factory).Penetration, 2));
			}
		}

		public void TestCalculatePenetrationPercentage_WhenBufferWasDeleted_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>();
			var parentWorkflow = CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = CreateWorkflow(jobHeader, "childWorkflow");

			var parentTask = CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			var childTask = CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			parentWorkflow.FH_FC_CurrentComponent = config.Buffer.PK;
			parentWorkflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-6);

			childWorkflow.FH_FC_CurrentComponent = config.Buffer.PK;
			childWorkflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			AssertEquals(1m, BufferPenetrationCalculator.CalculatePenetrationPercentage(parentWorkflow, context, Factory).Penetration);

			config.Buffer.Delete();

			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(parentWorkflow, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_MultipleBuffers_ShouldUseWorstCase()
		{
			var item = new DummyBufferedItem(buffer_12min, buffer_6min);

			item.StartableTime = ZDateTime.UtcNow.ToDateTime();
			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item, context, Factory).Penetration);

			for (int i = 1; i <= buffer_12min.SizeInMinutes; i++)
			{
				item.StartableTime = item.StartableTime.AddMinutes(-1);
				AssertEquals(decimal.Round((1 / 12m) * i * 2, 2), decimal.Round(BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(item, context, Factory).Penetration, 2));
			}
		}

		public void TestCalculatePenetrationPercentage_PartiallyCompleted_OnTrackForDelivery()
		{
			var item = new DummyBufferedItem(buffer_12min)
			{
				StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-1),
				PlannedDurationInMinutes = 10,
				RemainingEstimateInMinutes = 9,
			};

			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_PartiallyCompleted_FallingBehind()
		{
			var item = new DummyBufferedItem(buffer_12min)
			{
				StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-7),
				PlannedDurationInMinutes = 10,
				RemainingEstimateInMinutes = 9,
			};

			AssertEquals(0.5m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_PartiallyCompleted_AheadOfSchedule()
		{
			var item = new DummyBufferedItem(buffer_12min)
			{
				StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-6),
				PlannedDurationInMinutes = 10,
				RemainingEstimateInMinutes = 1,
			};

			AssertEquals(0.0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item, context, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_MoreWorkThanOriginallyThought()
		{
			var item = new DummyBufferedItem(buffer_12min)
			{
				StartableTime = ZDateTime.UtcNow.ToDateTime(),
				PlannedDurationInMinutes = 10,
				RemainingEstimateInMinutes = 13,
			};

			AssertEquals(0.25m, BufferPenetrationCalculator.CalculatePenetrationPercentage(item, context, Factory).Penetration);
		}

		#region New Release Gate

		public void TestCalculatePenetrationPercentage_EffectiveBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "WTB";
			branch.GB_RL_NKHomePort = "AUSYD"; // GMT +10

			TestDateAttribute.Date = new ZDateTime(2013, 12, 15, 23, 0, 0).ToUniversalBranchTime().ToDateTime();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(description: "jobHeader");
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-3);

			Factory.Save();
			AssertEquals("Precondition: Extra hour", 25.0, (new ZDateTime(2013, 12, 14) - new ZDateTime(2013, 12, 12, 23, 0, 0)).TotalHours);
			AssertEquals(0.2604166666666666666666666667m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow, workflow.WorkingTimeContext, Factory).Penetration);

			jobHeader.FH_GB_Branch = branch.PK;
			AssertEquals("Precondition", branch.PK, workflow.FH_GB_EffectiveBranch);

			Factory.Save();
			AssertEquals("Precondition: No extra hour", 24.0, (new ZDateTime(2013, 12, 14) - new ZDateTime(2013, 12, 13, 0, 0, 0)).TotalHours);
			AssertEquals(0.25m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow, workflow.WorkingTimeContext, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_EffectiveDepartment()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "DP1";

			TestDateAttribute.Date = new ZDateTime(1999, 12, 1, 0, 0, 0).ToUniversalBranchTime().ToDateTime();
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, department.PK);
			TestDateAttribute.Date = new ZDateTime(2000, 1, 1, 0, 0, 0).ToUniversalBranchTime().ToDateTime();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(description: "jobHeader");
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddDays(-3);

			Factory.Save();
			AssertEquals("Precondition: Penetration amount in hours", 72.0, (ZDateTime.Now - workflow.FH_ReleaseDateTime).TotalHours);
			AssertEquals(0.75m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow, workflow.WorkingTimeContext, Factory).Penetration);

			jobHeader.FH_GE_Department = department.PK;
			AssertEquals("Precondition", department.PK, workflow.FH_GE_EffectiveDepartment);

			Factory.Save();
			AssertEquals("Precondition: Penetration amount in hours", 24.0, (ZDateTime.Now - workflow.FH_ReleaseDateTime).TotalDays * 8);
			AssertEquals(0.25m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow, workflow.WorkingTimeContext, Factory).Penetration);
		}

		public void TestCalculatePenetrationPercentage_WithBufferTimespan_12min()
		{
			AssertEquals(0.25m, CalculatePenetrationPercentage_WithBufferTimespanHelper(12, setBufferTimespanOnJobHeader: false));
		}

		public void TestCalculatePenetrationPercentage_WithBufferTimespan_6min()
		{
			AssertEquals(0.5m, CalculatePenetrationPercentage_WithBufferTimespanHelper(6, setBufferTimespanOnJobHeader: false));
		}

		public void TestCalculatePenetrationPercentage_WithEffectiveBufferTimespan_12min()
		{
			AssertEquals(0.25m, CalculatePenetrationPercentage_WithBufferTimespanHelper(12, setBufferTimespanOnJobHeader: true));
		}

		public void TestCalculatePenetrationPercentage_WithEffectiveBufferTimespan_6min()
		{
			AssertEquals(0.5m, CalculatePenetrationPercentage_WithBufferTimespanHelper(6, setBufferTimespanOnJobHeader: true));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		decimal CalculatePenetrationPercentage_WithBufferTimespanHelper(int timespanInMinutes, bool setBufferTimespanOnJobHeader)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(description: "jobHeader");
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20, description: "task");

			var timespan = Factory.New<BMBufferTimespan>();
			timespan.BMT_Name = "Test Span";
			timespan.BMT_BufferTimespanInMinutes = timespanInMinutes;

			(setBufferTimespanOnJobHeader ? jobHeader : workflow).FH_BMT_BufferTimespan = timespan.PK;
			AssertEquals("Precondition: Effective Buffer Duration", timespanInMinutes, workflow.EffectiveBufferDurationMinutes);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.ToDateTime();
			workflow.FH_RemainingMinutesToComplete = 30;
			workflow.FH_PlannedDurationInMinutes = 27;

			AssertEquals("Precondition: Penetration amount in minutes", 3, workflow.FH_RemainingMinutesToComplete - workflow.FH_PlannedDurationInMinutes);
			return BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow, context, Factory).Penetration;
		}

		public void TestCalculatePenetration_WhenUsingBufferTimespan_ShouldNotAffectCurrentStatus()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(description: "jobHeader");
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20, description: "task");

			AssertEquals("Precondition", "buffer - Zone 3", workflow.CurrentStatus);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20, description: "task2");

			var timespan = Factory.NewWithValidTestData<BMBufferTimespan>();
			timespan.BMT_BufferTimespanInMinutes = config.Buffer.FC_BufferTimespanInMinutes;
			workflow2.FH_BMT_BufferTimespan = timespan.PK;

			AssertEquals("Current Status should be the same with or without Buffer Timespan set", "buffer - Zone 3", workflow2.CurrentStatus);
		}

		#endregion

		WorkingTimeContext context;
		IBuffer buffer_12min;
		IBuffer buffer_6min;

		protected override void SetUp()
		{
			base.SetUp();

			context = WorkingTimeContext.Create(Factory);
			buffer_12min = new DummyBuffer { SizeInMinutes = 12 };
			buffer_6min = new DummyBuffer { SizeInMinutes = 6 };
		}
	}
}
