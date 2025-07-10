using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class ReleaseGateKeeperTest : BMSTestCaseWithFactory
	{
		#region Constrained Mode

		[TestDate(2015, 4, 5)]
		class ConstrainedModeTestCase : BMSTestCaseWithFactory
		{
			public void TestCCRWorkflowInConstrainedModeReleaseGroup()
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: constrainedModeReleaseGroup.PK);

				var task1 = BMSTestHelper.CreateTask(workflow, ccr.GS_Code, 60, description: "task1");
				var task2 = BMSTestHelper.CreateTask(workflow, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task2");
				var task3 = BMSTestHelper.CreateTask(workflow, nonCCRInNonConstrainedReleaseGroup.GS_Code, 60, description: "task3");

				Factory.Save();

				task1.P9_TaskID = "task1";
				task2.P9_TaskID = "task2";
				task3.P9_TaskID = "task3";

				var expectedLog = string.Format(
@"Released [workflow] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has {0} hours of available capacity (for work involving a CCR) at 1st place in the queue for this resource.
		Tasks assigned: task2 (1.5 hours)
	Jean-Luc Picard: required 1.5 hours, currently has {1} hours of available capacity (for work involving a CCR) at 1st place in the queue for this resource.
		Tasks assigned: task3 (1.5 hours)
	Kathryn Janeway: required 1.5 hours, currently has {2} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task1 (1.5 hours)
",
 GetFullCapacityHoursFormatted(0.5m * 2m), // Non-CCR resource in Constrained Mode group
 GetFullCapacityHoursFormatted(0.5m * 2m), // Non-CCR in non-Constrained Mode group
 GetFullCapacityHoursFormatted(2m / 3m) // CCR
 );

				RunReleaseGateAndAssertLoggedCapacity(expectedLog, @"
Non-CCR in Constrained Mode group should use the Buffer Load Limit AND Non-CCR Overload Multiplier as the basis for Full Capacity
CCR should use the Constraint offset as the basis for Full Capacity
Non-CCR in non-Constrained Mode group should use the Buffer Load Limit as the basis for Full Capacity
");
			}

			public void TestNonCCRWorkflowInConstrainedModeReleaseGroup()
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: constrainedModeReleaseGroup.PK);

				var task1 = BMSTestHelper.CreateTask(workflow, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task1");
				var task2 = BMSTestHelper.CreateTask(workflow, nonCCRInNonConstrainedReleaseGroup.GS_Code, 60, description: "task2");

				Factory.Save();

				task1.P9_TaskID = "task1";
				task2.P9_TaskID = "task2";

				var expectedLog = string.Format(
@"Released [workflow] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has {0} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task1 (1.5 hours)
	Jean-Luc Picard: required 1.5 hours, currently has {1} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task2 (1.5 hours)
",
 GetFullCapacityHoursFormatted(0.5m), // Non-CCR resource in Constrained Mode group
 GetFullCapacityHoursFormatted(0.5m) // Non-CCR in non-Constrained Mode group
 );

				RunReleaseGateAndAssertLoggedCapacity(expectedLog, @"
Non-CCR in Constrained Mode group for non-CCR workflow should use the Buffer Load Limit only as the basis for Full Capacity
Non-CCR in non-Constrained Mode group should use the Buffer Load Limit as the basis for Full Capacity
");
			}

			public void TestCCRWorkflow_InSystemWithNoReleaseGroups()
			{
				constrainedModeReleaseGroup.Delete();
				nonConstrainedModeReleaseGroup.Delete();
				config.System.ReleaseGroups.DeleteAll();
				config.ComponentReleaseGroupLink.Delete();

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

				var task1 = BMSTestHelper.CreateTask(workflow, ccr.GS_Code, 60, description: "task1");
				var task2 = BMSTestHelper.CreateTask(workflow, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task2");
				var task3 = BMSTestHelper.CreateTask(workflow, nonCCRInNonConstrainedReleaseGroup.GS_Code, 60, description: "task3");

				Factory.Save();

				task1.P9_TaskID = "task1";
				task2.P9_TaskID = "task2";
				task3.P9_TaskID = "task3";

				var expectedLog = string.Format(
@"Released [workflow] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has {0} hours of available capacity (for work involving a CCR) at 1st place in the queue for this resource.
		Tasks assigned: task2 (1.5 hours)
	Jean-Luc Picard: required 1.5 hours, currently has {1} hours of available capacity (for work involving a CCR) at 1st place in the queue for this resource.
		Tasks assigned: task3 (1.5 hours)
	Kathryn Janeway: required 1.5 hours, currently has {2} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task1 (1.5 hours)
",
 GetFullCapacityHoursFormatted(0.5m * 2m), // Non-CCR resource in Constrained Mode group
 GetFullCapacityHoursFormatted(0.5m * 2m), // Non-CCR in non-Constrained Mode group
 GetFullCapacityHoursFormatted(2m / 3m) // CCR
 );

				RunReleaseGateAndAssertLoggedCapacity(expectedLog, @"
Non-CCR in Constrained Mode group should use the Buffer Load Limit AND Non-CCR Overload Multiplier as the basis for Full Capacity
CCR should use the Constraint offset as the basis for Full Capacity
Non-CCR in non-Constrained Mode group should use the Buffer Load Limit as the basis for Full Capacity
");
			}

			public void TestNonCCRWorkflow_InSystemWithNoReleaseGroups()
			{
				constrainedModeReleaseGroup.Delete();
				nonConstrainedModeReleaseGroup.Delete();
				config.System.ReleaseGroups.DeleteAll();
				config.ComponentReleaseGroupLink.Delete();

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

				var task1 = BMSTestHelper.CreateTask(workflow, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task1");
				var task2 = BMSTestHelper.CreateTask(workflow, nonCCRInNonConstrainedReleaseGroup.GS_Code, 60, description: "task2");

				Factory.Save();

				task1.P9_TaskID = "task1";
				task2.P9_TaskID = "task2";

				var expectedLog = string.Format(
@"Released [workflow] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has {0} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task1 (1.5 hours)
	Jean-Luc Picard: required 1.5 hours, currently has {1} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task2 (1.5 hours)
",
 GetFullCapacityHoursFormatted(0.5m), // Non-CCR resource in Constrained Mode group
 GetFullCapacityHoursFormatted(0.5m) // Non-CCR in non-Constrained Mode group
 );

				RunReleaseGateAndAssertLoggedCapacity(expectedLog, @"
Non-CCR in Constrained Mode group for non-CCR workflow should use the Buffer Load Limit only as the basis for Full Capacity
Non-CCR in non-Constrained Mode group should use the Buffer Load Limit as the basis for Full Capacity
");
			}

			public void TestCCRWorkflowInNonConstrainedModeReleaseGroup()
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: nonConstrainedModeReleaseGroup.PK);

				var task1 = BMSTestHelper.CreateTask(workflow, ccr.GS_Code, 60, description: "task1");
				var task2 = BMSTestHelper.CreateTask(workflow, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task2");
				var task3 = BMSTestHelper.CreateTask(workflow, nonCCRInNonConstrainedReleaseGroup.GS_Code, 60, description: "task3");

				Factory.Save();

				task1.P9_TaskID = "task1";
				task2.P9_TaskID = "task2";
				task3.P9_TaskID = "task3";

				var expectedLog = string.Format(
@"Released [workflow] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has {0} hours of available capacity (for work involving a CCR) at 1st place in the queue for this resource.
		Tasks assigned: task2 (1.5 hours)
	Jean-Luc Picard: required 1.5 hours, currently has {1} hours of available capacity (for work involving a CCR) at 1st place in the queue for this resource.
		Tasks assigned: task3 (1.5 hours)
	Kathryn Janeway: required 1.5 hours, currently has {2} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task1 (1.5 hours)
",
 GetFullCapacityHoursFormatted(0.5m * 2m), // Non-CCR resource in Constrained Mode group
 GetFullCapacityHoursFormatted(0.5m * 2m), // Non-CCR in non-Constrained Mode group
 GetFullCapacityHoursFormatted(2m / 3m) // CCR
 );

				RunReleaseGateAndAssertLoggedCapacity(expectedLog, @"
Non-CCR for workflow in non-Constrained Mode group should use just the Buffer Load Limit as the basis for Full Capacity
CCR should use the Constraint offset as the basis for Full Capacity
Non-CCR in non-Constrained Mode group should use the Buffer Load Limit as the basis for Full Capacity
");
			}

			public void TestNonCCRWorkflowInNonConstrainedModeReleaseGroup()
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: nonConstrainedModeReleaseGroup.PK);

				var task1 = BMSTestHelper.CreateTask(workflow, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task1");
				var task2 = BMSTestHelper.CreateTask(workflow, nonCCRInNonConstrainedReleaseGroup.GS_Code, 60, description: "task2");

				Factory.Save();

				task1.P9_TaskID = "task1";
				task2.P9_TaskID = "task2";

				var expectedLog = string.Format(
@"Released [workflow] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has {0} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task1 (1.5 hours)
	Jean-Luc Picard: required 1.5 hours, currently has {1} hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task2 (1.5 hours)
",
 GetFullCapacityHoursFormatted(0.5m), // Non-CCR resource in Constrained Mode group
 GetFullCapacityHoursFormatted(0.5m) // Non-CCR in non-Constrained Mode group
 );

				RunReleaseGateAndAssertLoggedCapacity(expectedLog, @"
Non-CCR in Constrained Mode group for non-CCR workflow should use the Buffer Load Limit only as the basis for Full Capacity
Non-CCR in non-Constrained Mode group should use the Buffer Load Limit as the basis for Full Capacity
");
			}

			public void TestReleaseCCRWorkflowBehindNonCCRWorkflow_ShouldUseExtendedCapacityForWorkInvolvingCCR_AndMaintainBothCapacityFigures()
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				((OrgHeader)jobHeader.Parent).OH_Code = "MAIFRISTORG";

				var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: constrainedModeReleaseGroup.PK);
				var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: constrainedModeReleaseGroup.PK);
				var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: constrainedModeReleaseGroup.PK);
				var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: constrainedModeReleaseGroup.PK);
				var alreadyReleasedWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "alreadyReleasedWorkflow", releaseGroupPK: constrainedModeReleaseGroup.PK, currentComponent: config.Buffer);

				workflow1.FH_VoteUpDownAmount = 30;
				workflow2.FH_VoteUpDownAmount = 40;
				workflow3.FH_VoteUpDownAmount = 20;
				workflow4.FH_VoteUpDownAmount = 10;

				var task1_1 = BMSTestHelper.CreateTask(workflow1, ccr.GS_Code, 60, description: "task1_1");
				var task1_2 = BMSTestHelper.CreateTask(workflow1, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task1_2");
				var task2 = BMSTestHelper.CreateTask(workflow2, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task2");
				var task3 = BMSTestHelper.CreateTask(workflow3, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task3");
				var task4_1 = BMSTestHelper.CreateTask(workflow4, ccr.GS_Code, 60, description: "task4_1");
				var task4_2 = BMSTestHelper.CreateTask(workflow4, nonCCRInConstrainedReleaseGroup.GS_Code, 60, description: "task4_2");
				var alreadyReleasedTask = BMSTestHelper.CreateTask(alreadyReleasedWorkflow, nonCCRInConstrainedReleaseGroup.GS_Code, (int)(GetFullCapacityHours(0.5m) * 60), estVariationFactor: 1, description: "task3");

				Factory.Save();

				task1_1.P9_TaskID = "task1_1";
				task1_2.P9_TaskID = "task1_2";
				task2.P9_TaskID = "task2";
				task3.P9_TaskID = "task3";
				task4_1.P9_TaskID = "task4_1";
				task4_2.P9_TaskID = "task4_2";

				Factory.Save();

				alreadyReleasedTask.P9_TaskID = "ReleasedTask";

				RunReleaseGate(config.System);

				var newFactory = Factory.CreateNewFactory();
				var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
				var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
				var loadedWorkflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);
				var loadedWorkflow4 = newFactory.Load<ProcessHeader>(workflow4.PK);

				AssertSamePK("Should have released CCR workflow as there was enough free capacity for CCR work", config.Buffer, loadedWorkflow1.CurrentComponent);
				AssertSamePK("Should not have released non-CCR workflow as there was insufficient capacity for non-CCR work", config.Bucket, loadedWorkflow2.CurrentComponent);
				AssertSamePK("Should not have released non-CCR workflow as there was insufficient capacity for non-CCR work", config.Bucket, loadedWorkflow3.CurrentComponent);
				AssertSamePK("Should have released CCR workflow as there was enough free capacity for CCR work", config.Buffer, loadedWorkflow4.CurrentComponent);

				var link1 = newFactory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, ccr.GS_Code));
				var link2 = newFactory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, nonCCRInConstrainedReleaseGroup.GS_Code));

				CombineAssertions("CapacityReservationDetails", () =>
				{
					AssertMultilineASCIIEquals("CCR", @"Release Gate Run at 05-Apr-15 00:00 UTC
RELEASED, MAIFRISTORG - workflow1, Available Capacity: 64 hours, Reservation: 1.5 hours, Remaining Capacity: 62.5 hours, Other resources considered: Benjamin Sisko.
RELEASED, MAIFRISTORG - workflow4, Available Capacity: 62.5 hours, Reservation: 1.5 hours, Remaining Capacity: 61 hours, Other resources considered: Benjamin Sisko.", link1.CapacityReservationDetails);

					AssertMultilineASCIIEquals("Non-CCR", @"Release Gate Run at 05-Apr-15 00:00 UTC
BLOCKED, MAIFRISTORG - workflow2, Available Capacity: 0 hours, Reservation: 1.5 hours, Remaining Capacity: -1.5 hours
RELEASED, MAIFRISTORG - workflow1, Available Capacity: 46.5 hours, Reservation: 1.5 hours, Remaining Capacity: 45 hours (for work involving a CCR), Other resources considered: Kathryn Janeway.
BLOCKED, MAIFRISTORG - workflow3, Available Capacity: -3 hours, Reservation: 1.5 hours, Remaining Capacity: -4.5 hours
RELEASED, MAIFRISTORG - workflow4, Available Capacity: 43.5 hours, Reservation: 1.5 hours, Remaining Capacity: 42 hours (for work involving a CCR), Other resources considered: Kathryn Janeway.", link2.CapacityReservationDetails);
				});

				CombineAssertions("Successful Release Notes", () =>
				{
					AssertMultilineASCIIEquals("workflow1",
@"Released [workflow1] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has 46.5 hours of available capacity (for work involving a CCR) at 2nd place in the queue for this resource.
		Tasks assigned: task1_2 (1.5 hours)
	Kathryn Janeway: required 1.5 hours, currently has 64 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: task1_1 (1.5 hours)", loadedWorkflow1.GetSuccessfulReleaseNotes());

					AssertMultilineASCIIEquals("workflow4",
@"Released [workflow4] into [buffer] at 05-Apr-15 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Benjamin Sisko: required 1.5 hours, currently has 43.5 hours of available capacity (for work involving a CCR) at 4th place in the queue for this resource.
		Tasks assigned: task4_2 (1.5 hours)
	Kathryn Janeway: required 1.5 hours, currently has 62.5 hours of available capacity at 2nd place in the queue for this resource.
		Tasks assigned: task4_1 (1.5 hours)", loadedWorkflow4.GetSuccessfulReleaseNotes());
				});
			}

			#region Implementation

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			static string GetFullCapacityHoursFormatted(decimal loadMultiplier)
			{
				return GetFullCapacityHours(loadMultiplier).FormatWithNoMoreThanTwoDecimalPlaces();
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			static decimal GetFullCapacityHours(decimal loadMultiplier)
			{
				return Utilities.Round(BaseResourceAvailableHours * loadMultiplier, CapacityCalculator.DecimalPlaces);
			}

			void RunReleaseGateAndAssertLoggedCapacity(string expectedSuccessfulReleaseLog, string message)
			{
				Factory.Save();

				RunReleaseGate(config.System);

				var newFactory = Factory.CreateNewFactory();
				var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

				AssertSamePK(config.Buffer, loadedWorkflow.CurrentComponent);

				AssertMultilineASCIIEquals(message, expectedSuccessfulReleaseLog, loadedWorkflow.GetSuccessfulReleaseNotes());
			}

			protected override void SetUp()
			{
				base.SetUp();

				WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
				BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);

				constrainedModeReleaseGroup = config.ReleaseGroup;
				nonConstrainedModeReleaseGroup = Factory.NewWithValidTestData<GlbGroup>();

				ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JWY", "Kathryn Janeway");
				nonCCRInConstrainedReleaseGroup = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SKO", "Benjamin Sisko");
				nonCCRInNonConstrainedReleaseGroup = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PIC", "Jean-Luc Picard");

				ccr.DesignateAsCCR(config.Buffer);

				constrainedModeReleaseGroup.Staff.AddRange(ccr, nonCCRInConstrainedReleaseGroup);
				nonConstrainedModeReleaseGroup.Staff.Add(nonCCRInNonConstrainedReleaseGroup);

				BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

				Factory.Save();
			}

			ConstrainedSchematicTestConfig config;
			GlbGroup constrainedModeReleaseGroup, nonConstrainedModeReleaseGroup;
			GlbStaff ccr, nonCCRInConstrainedReleaseGroup, nonCCRInNonConstrainedReleaseGroup;

			ProcessHeader workflow;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			const decimal BaseResourceAvailableHours = 96m;

			#endregion
		}

		#endregion

		#region Capability Tasks

		protected string QueryLogString() => UsingSimpleQuery
			? @"Using Capacity Query...
Staff Capabilities query executed in 00:00:00
Task Estimate query executed in 00:00:00
Finished loading workflows in 00:00:00"
			: @"Using Old Capacity Query...
Finished loading workflows in 00:00:00";

		[TestDate(2016, 9, 11)]
		public void TestRelease_WithCapabilityTask_OutOfOfficeHours_ShouldStillRelease()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = BMSTestHelper.CreateCapability(Factory, "NAR", "Narp");
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DE", "Courageous First Responder");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "67.7");
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, capability: capability);

			var tag = TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(Factory));

			BMSTestHelper.CreateRuleRunnerTagLink(tag, workflow);

			Factory.Save();

			AssertEquals(false, resource.IsWorkingToday);

			var logger = new BufferManagementLogger();
			RunReleaseGate(config.System, logger, failureLogService: ReleaseLogFailureServiceForTest);

			AssertSamePK(config.Bucket, workflow.CurrentComponent);
			workflow.Reload();

			CombineAssertions("Should release CCPM item now ready for release", () =>
			{
				AssertSamePK(config.Buffer, workflow.CurrentComponent);
				AssertEquals(string.Empty, workflow.GetReleaseFailureReasonForBuffer(config.Buffer));
				AssertMultilineASCIIEquals("", $@"WTGDEV - ORG.buffer: Determining workflows eligible for Release Gate.
WTGDEV - ORG.buffer: Assembling reservation tracker.
WTGDEV - ORG.buffer: Sorting workflows into release sequence.
WTGDEV - ORG.buffer: Starting Release Gate run with 1 workflows to process.
WTGDEV - ORG.buffer: Workflow 67.7 for job XVBQP68SIYXQ has been released.", logger.ToString());
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestRelease_WithCapabilityTask_AndOneResourceWithCapabilityIsInactive_ShouldStillRelease()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = BMSTestHelper.CreateCapability(Factory, "THN", "Thing");
			var inactiveResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NOP", "Nope", capability);
			inactiveResource.GS_IsActive = false;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Really important thing");
			var task = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 10, capability: capability);

			Factory.Save();

			var logger = new BufferManagementLogger();
			RunReleaseGate(config.System, logger);

			workflow.Reload();

			CombineAssertions("Should not release workflow because there are no active resources with the capability", () =>
			{
				AssertSamePK(config.Bucket, workflow.CurrentComponent);

				AssertMultilineASCIIEquals("", $@"WTGDEV - ORG.buffer: Determining workflows eligible for Release Gate.
WTGDEV - ORG.buffer: Assembling reservation tracker.
WTGDEV - ORG.buffer: Sorting workflows into release sequence.
WTGDEV - ORG.buffer: Starting Release Gate run with 1 workflows to process.
", logger.ToString());
			});

			var activeResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "YEP", "Yep", capability);
			Factory.Save();

			logger = new BufferManagementLogger();
			RunReleaseGate(config.System, logger);

			workflow.Reload();

			CombineAssertions("Should release workflow now that there is an active resource with the capability", () =>
			{
				AssertSamePK(config.Buffer, workflow.CurrentComponent);

				AssertMultilineASCIIEquals("", $@"WTGDEV - ORG.buffer: Determining workflows eligible for Release Gate.
WTGDEV - ORG.buffer: Calculating Capacity for 1 staff.
{QueryLogString()}
WTGDEV - ORG.buffer: Assembling reservation tracker.
WTGDEV - ORG.buffer: Sorting workflows into release sequence.
WTGDEV - ORG.buffer: Starting Release Gate run with 1 workflows to process.
WTGDEV - ORG.buffer: Workflow Really important thing for job XVBQP68SIYXQ has been released.
WTGDEV - ORG.buffer: Persisting updated capacity after Release Gate run.
", logger.ToString());
			});
		}

		#endregion

		#region Logging

		[TestDate(2014, 11, 27)]
		public void TestRelease_ShouldLogCapacityDetailsOfResourcesAssignedToTasks()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee");
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_Description = "Coding";
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "SME";
			capability2.G4_Description = "Smelling the Roses";

			resource1.Capabilities.Add(capability1);
			resource1.Capabilities.Add(capability2);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_Name = "Dis Buffer";
			config.Buffer.FC_DisplaySequence = 1;

			var buffer2 = CreateBuffer(config.System, "Dat Buffer");
			var link = LinkComponents(config.Buffer, buffer2, sequence: 2);
			link.FL_TransferRulesEnabled = false;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);

			workflow1.FH_VoteUpDownAmount = 20;
			workflow2.FH_VoteUpDownAmount = 10;

			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 120);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 120);

			var task2_1 = CreateTask(workflow2, resource1.GS_Code, 30);
			var task2_2 = CreateTask(workflow2, resource1.GS_Code, 30);
			var task2_3 = CreateTask(workflow2, resource2.GS_Code, 60);
			var task2_4 = CreateTask(workflow2, resource2.GS_Code, 60);
			var task2_5 = CreateTask(workflow2, string.Empty, 120, estVariationFactor: 3, capability: capability1);
			var task2_6 = CreateTask(workflow2, string.Empty, 60, estVariationFactor: 1, capability: capability2);

			Factory.Save();

			task1_1.P9_TaskID = "T0010001";
			task1_2.P9_TaskID = "T0010002";
			task2_1.P9_TaskID = "T0020001";
			task2_2.P9_TaskID = "T0020002";
			task2_3.P9_TaskID = "T0020003";
			task2_4.P9_TaskID = "T0020004";
			task2_5.P9_TaskID = "T0020005";
			task2_6.P9_TaskID = "T0020006";

			Factory.Save();

			RunReleaseGate(config.System);

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);

			AssertSamePK(config.Buffer, loadedWorkflow2.CurrentComponent);
			var noteText = loadedWorkflow2.GetSuccessfulReleaseNotes();

			AssertMultilineASCIIEquals("",
@"Released [workflow2] into [Dis Buffer] at 27-Nov-14 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Frodo Baggins: required 6.5 hours, currently has 45 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T0020001 (0.75 hours), T0020002 (0.75 hours)
		Tasks requiring capability [Coding]: T0020005 (4 hours)
		Tasks requiring capability [Smelling the Roses]: T0020006 (1 hours)
	Samwise Gamgee: required 3 hours, currently has 45 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T0020003 (1.5 hours), T0020004 (1.5 hours)
", noteText);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			link.FL_TransferRulesEnabled = true;
			link.Factory.Save();

			RunReleaseGate(config.System);

			newFactory = Factory.CreateNewFactory();
			loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);

			AssertSamePK(buffer2, loadedWorkflow2.CurrentComponent);
			noteText = loadedWorkflow2.GetSuccessfulReleaseNotes();

			AssertMultilineASCIIEquals("",
@"Released [workflow2] into [Dis Buffer] at 27-Nov-14 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Frodo Baggins: required 6.5 hours, currently has 45 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T0020001 (0.75 hours), T0020002 (0.75 hours)
		Tasks requiring capability [Coding]: T0020005 (4 hours)
		Tasks requiring capability [Smelling the Roses]: T0020006 (1 hours)
	Samwise Gamgee: required 3 hours, currently has 45 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T0020003 (1.5 hours), T0020004 (1.5 hours)

Released [workflow2] into [Dat Buffer] at 28-Nov-14 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Frodo Baggins: required 6.5 hours, currently has 45 hours of available capacity at 2nd place in the queue for this resource.
		Tasks assigned: T0020001 (0.75 hours), T0020002 (0.75 hours)
		Tasks requiring capability [Coding]: T0020005 (4 hours)
		Tasks requiring capability [Smelling the Roses]: T0020006 (1 hours)
	Samwise Gamgee: required 3 hours, currently has 45 hours of available capacity at 2nd place in the queue for this resource.
		Tasks assigned: T0020003 (1.5 hours), T0020004 (1.5 hours)
", noteText);
		}

		[TestDate(2015, 1, 2)]
		public void TestRelease_ShouldLogCapacityDetailsOfResourcesAssignedToTasks_ForApprovedCCPMItems()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_Name = "Dis Buffer";

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			var task = CreateTask(workflow, resource.GS_Code, 120);

			BMSTestHelper.CreateRuleRunnerTagLink(TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(Factory)), jobHeader);

			Factory.Save();

			task.P9_TaskID = "T0010001";
			Factory.Save();

			RunReleaseGate(config.System);

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			AssertSamePK(config.Buffer, loadedWorkflow.CurrentComponent);
			var noteText = loadedWorkflow.GetSuccessfulReleaseNotes();

			AssertMultilineASCIIEquals("",
@"Released [workflow] into [Dis Buffer] at 02-Jan-15 00:00 UTC. It was considered releasable based on its approved CCPM schedule. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 3 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T0010001 (3 hours)
", noteText);
		}

		[TestDate(2015, 1, 2)]
		public void TestRelease_ShouldLogCapacityDetailsOfResourcesAssignedToTasks_InOrderOfAvailableCapacityDescending()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Fracking";
			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability);
			var resource2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins", capability);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_Name = "Dis Buffer";

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			var task1 = CreateTask(workflow1, string.Empty, 120, capability: capability);
			var task2 = CreateTask(workflow2, resource1.GS_Code, 120);

			BMSTestHelper.CreateRuleRunnerTagLink(TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(Factory)), jobHeader);

			Factory.Save();

			task1.P9_TaskID = "T0010001";
			Factory.Save();

			RunReleaseGate(config.System);

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow1.PK);

			AssertSamePK(config.Buffer, loadedWorkflow.CurrentComponent);
			var noteText = loadedWorkflow.GetSuccessfulReleaseNotes();

			AssertMultilineASCIIEquals("",
@"Released [workflow1] into [Dis Buffer] at 02-Jan-15 00:00 UTC. It was considered releasable based on its approved CCPM schedule. Details of relevant resource capacity are listed below.
	Bilbo Baggins: required 1.5 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Fracking]: T0010001 (1.5 hours)
	Frodo Baggins: required 1.5 hours, currently has 45 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Fracking]: T0010001 (1.5 hours)
", noteText);
		}

		[TestDate(2014, 11, 27)]
		public void TestRelease_ShouldLogCapacityDetailsOfResourcesAssignedToTasks_ForWorkflowsWithCapabilityTasksOnly()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee");
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_Description = "Coding";
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "SME";
			capability2.G4_Description = "Smelling the Roses";

			resource1.Capabilities.Add(capability1);
			resource1.Capabilities.Add(capability2);
			resource2.Capabilities.Add(capability2);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_Name = "Dis Buffer";
			config.Buffer.FC_DisplaySequence = 2;

			var buffer2 = CreateBuffer(config.System, "Dat Buffer");
			LinkComponents(config.Buffer, buffer2, sequence: 1);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket);

			var task1_1 = CreateTask(workflow1, string.Empty, 120, estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, string.Empty, 120, estVariationFactor: 1);

			var task2_1 = CreateTask(workflow2, string.Empty, 30, estVariationFactor: 1, capability: capability1);
			var task2_2 = CreateTask(workflow2, string.Empty, 30, estVariationFactor: 1, capability: capability2);

			Factory.Save();

			task1_1.P9_TaskID = "T0010001";
			task1_2.P9_TaskID = "T0010002";
			task2_1.P9_TaskID = "T0020001";
			task2_2.P9_TaskID = "T0020002";

			Factory.Save();

			RunReleaseGate(config.System);

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);

			AssertSamePK(config.Buffer, loadedWorkflow2.CurrentComponent);
			var noteText = loadedWorkflow2.GetSuccessfulReleaseNotes();

			AssertMultilineASCIIEquals("",
@"Released [workflow2] into [Dis Buffer] at 27-Nov-14 00:00 UTC. It was considered releasable based on the capacity of the resources listed below.
	Frodo Baggins: required 0.75 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Coding]: T0020001 (0.5 hours)
		Tasks requiring capability [Smelling the Roses]: T0020002 (0.25 hours)
	Samwise Gamgee: required 0.25 hours, currently has 48 hours of available capacity at 1st place in the queue for this resource.
		Tasks requiring capability [Smelling the Roses]: T0020002 (0.25 hours)
", noteText);
		}

		[TestDate(2014, 11, 27)]
		public void TestRelease_ShouldLogCapacityDetailsOfResourcesAssignedToTasks_WhenDisabledInRegistry()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_Name = "Dis Buffer";

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);

			var task1 = CreateTask(workflow, resource.GS_Code, 30);
			var task2 = CreateTask(workflow, resource.GS_Code, 30);

			Factory.Save();

			RunReleaseGate(config.System);

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow.PK);

			AssertSamePK(config.Buffer, loadedWorkflow2.CurrentComponent);
			var noteText = loadedWorkflow2.GetSuccessfulReleaseNotes();

			AssertEquals(string.Empty, noteText);
		}

		[TestDate(2014, 8, 13)]
		public void TestProcess_WhenExistingItemsInBuffer_ShouldLogAppropriatePlaceInQueue()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var workflow1 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow1, resource.GS_Code, 6000);
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;

			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task2 = CreateTask(workflow2, resource.GS_Code, 60);
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow3", config.Bucket);
			var task3 = CreateTask(workflow3, resource.GS_Code, 60);
			workflow3.FH_VoteUpDownAmount = 100;

			var workflow4 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow4", config.Bucket);
			var task4 = CreateTask(workflow4, resource.GS_Code, 60);
			workflow4.FH_FC_CurrentComponent = config.Bucket.PK;
			workflow4.FH_VoteUpDownAmount = -100;

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);

			workflow3.Reload();
			workflow4.Reload();

			AssertEquals(config.Bucket.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals(config.Bucket.PK, workflow4.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals("Should not consider work already in the buffer when considering a workflow's place in the queue",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 1.5 hours, currently has -103.5 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001002 (1.5 hours)".StripTaskIds(), workflow3.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());

			AssertMultilineASCIIEquals("Should not consider work already in the buffer when considering a workflow's place in the queue",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 1.5 hours, currently has -105 hours of available capacity at 2nd place in the queue for this resource.
		Tasks assigned: T00001003 (1.5 hours)".StripTaskIds(), workflow4.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		public void TestCanDeleteStmNoteWithFactoryForLogging()
		{
			var factory = ReleaseGateKeeper.GetFactoryForSavingLogs();

			var jobHeader = CreateJobHeader<OrgHeader>();
			var stmNote = factory.NewWithValidTestData<StmNote>();

			stmNote.ST_ParentID = jobHeader.PK;
			stmNote.ST_Table = ProcessHeaderSchema.Constants.TableName;

			factory.Save();

			Assert("StmNote should be in database", stmNote.IsInDatabase);

			stmNote.Delete();

			Assert("StmNote should be deleted", stmNote.IsDeleted);
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		#endregion

		#region Allowed BusinessObject Types

		[ExpectNoExceptions]
		[TestDate(2015, 2, 26)]
		[TestUtcOffset(11, 1, 0)]
		public void TestProcess_ShouldNotReportErrorsWhenAllowedTablesLoaded()
		{
			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.GB_RL_NKHomePort = "AUSYD";

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = BMSTestHelper.CreateCapability(Factory, "GRY", "Gryffindor");
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "HAR", "I'm Harry Potter", capability);
			resource.GS_GB_HomeBranch = branch.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: group.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: group.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 10, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 10);

			Factory.Save();

			RunReleaseGate(config.System);
		}

		[ExpectNoExceptions]
		[TestDate(2015, 2, 26)]
		[TestUtcOffset(11, 1, 0)]
		public void TestProcess_ShouldNotReportErrorsWhenAllowedTablesLoaded_NotPlanningManagement()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.GB_RL_NKHomePort = "AUSYD";

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = BMSTestHelper.CreateCapability(Factory, "GRY", "Gryffindor");
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "HAR", "I'm Harry Potter", capability);
			resource.GS_GB_HomeBranch = branch.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: group.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: group.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 10, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 10);

			var diagram = jobHeader.GetDefaultDiagram();

			Factory.Save();

			RunReleaseGate(config.System);
		}

		#endregion

		#region Performance

		public void TestApprovedCCPMReleaseCacheQuery_ShouldUseParameterisedQuery()
		{
			var tagDef = TagProvider.GetCCPMReleaseTagGroup(Factory);
			var rtrTag = TagProvider.GetCCPMReadyToReleaseTag(tagDef);
			var rblTag = TagProvider.GetCCPMReleaseBlockedTag(tagDef);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var tagLink = BMSTestHelper.CreateRuleRunnerTagLink(rtrTag, workflow);
			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				var cache = new ApprovedCcpmReleaseCache(workflow.Factory, new[] { workflow.PK });

				Assert("Our cache should be properly populated with a parameterisedly-pulled PK, and yet...", cache.IsCCPMReadyToRelease(workflow.PK));

				Assert("We should have executed a query, instead... we didn't!", !TestConnection.ExecutedCommands.IsNullOrEmpty());

				AssertCollectionContains("We should have executed a query with this specific bit of text in it and instead we didn't!", TestConnection.ExecutedCommands, sB => sB.Contains("SELECT FH_PK FROM dbo.ProcessHeader WHERE ("));

				var command = TestConnection.ExecutedCommands.Single(c => c.Contains("SELECT FH_PK FROM dbo.ProcessHeader WHERE ("));

				AssertContains("The query should be parameterised, and yet...", "@TagMag", command);
				AssertContains("The query should be parameterised, and yet...", "@CWO", command);
			}
		}

		public void TestProcess_ShouldNotCreateTerribleProcessHeaderLinkQueries()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ", shouldUseExistingSystem: true);
			BMSTestHelper.CreateWorkflows(config.Bucket, 20, 1);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				RunReleaseGate(config.System);

				var processHeaderLinkCommands = TestConnection.ExecutedCommands.Where(x => x.Contains(nameof(ProcessHeaderLink))).ToArray();
				var badCommands = processHeaderLinkCommands.Where(x => x.Contains(" or FP_FH_HeaderTo", StringComparison.OrdinalIgnoreCase) || x.Contains(" or FP_FH_HeaderFrom", StringComparison.OrdinalIgnoreCase));
				AssertContainsExactElementsInAnyOrder("Should not use queries with billions and billions of ORs", Enumerable.Empty<string>(), badCommands);
			}
		}

		[TestDate(2024, 02, 07)]
		public void TestProcessDbHits()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var bucket = CreateBucket(system, "bucket");
			var buffer = CreateBuffer(system, "buffer", timespanMinutes: 600000, loadLimitPercent: 100);
			var link = LinkComponents(bucket, buffer, sequence: 0, isReleaseGate: true);

			var staffs = new List<GlbStaff>();
			var workflows = new List<ProcessHeader>();
			var staffCodeCalculator = new StaffCodeCalculator(Factory);

			for (int i = 1; i <= 10; i++)
			{
				var staffCode = staffCodeCalculator.GetNextAvailableUniqueCode(i.ToString());
				var staff = CreateStaffInCurrentBranchDept(staffCode, "Staff" + i);
				staffs.Add(staff);

				var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);

				var workflowOnBuffer = BMSTestHelper.CreateWorkflow(jobHeader, "on buffer", buffer);
				var workflowOnBucket = BMSTestHelper.CreateWorkflow(jobHeader, "on bucket", bucket);

				CreateTask(workflowOnBuffer, staff.GS_Code, 60, estVariationFactor: 1);

				CreateTask(workflowOnBucket, staff.GS_Code, 60, estVariationFactor: 1);
				workflows.Add(workflowOnBucket);
			}

			Factory.Save();

			var hits = new Dictionary<string, int>
			{
				{ StmModuleFilterSchema.Constants.TableName, 2 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ TagMagnitudeSchema.Constants.TableName, 2 },
				{ TagDefinitionSchema.Constants.TableName, 2 },
				{ BMZoneCapacityMultiplierSchema.Constants.TableName, 1 },
				{ BMComponentSchema.Constants.TableName, 2 },
				{ BMComponentLinkSchema.Constants.TableName, 2 },
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 4 },
				{ ViewProcessTaskSchema.Constants.TableName, 1 },
				{ GlbHolidaySchema.Constants.TableName, 1 },
				{ ViewProcessHeaderSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 4 },
				{ GlbStaffHolidaySchema.Constants.TableName, 1 },
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 1 },
				{ GlbWorkTimeSchema.Constants.TableName, 2 },
				{ BMComponentResourceLinkSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories(hits, ignoreUnspecified: true, thresholdForUnspecified: 1))
			{
				RunReleaseGate(system);
			}
		}

		#endregion

		#region Cache

		[TestDate(2024, 02, 08, 1, 2, 3)]
		public void TestProcess_ShouldOnlyCacheCapacityForStaffOnBoards()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			system.FS_Name = "Test";
			var bucket = CreateBucket(system, "bucket");
			var buffer = CreateBuffer(system, "buffer", timespanMinutes: 600000, loadLimitPercent: 100);
			var link = LinkComponents(bucket, buffer, sequence: 0, isReleaseGate: true);
			var board1 = BMSTestHelper.CreateBoard(system, "Board1");
			var section1 = BMSTestHelper.CreateBoardSection(buffer, board1);
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section1);
			var board2 = BMSTestHelper.CreateBoard(system, "Board2");
			var section2 = BMSTestHelper.CreateBoardSection(buffer, board2);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section2);

			var staffs = new List<GlbStaff>();
			var workflows = new List<ProcessHeader>();
			var staffCodeCalculator = new StaffCodeCalculator(Factory);

			for (int i = 1; i <= 6; i++)
			{
				var staffCode = staffCodeCalculator.GetNextAvailableUniqueCode(i.ToString());
				var staff = CreateStaffInCurrentBranchDept(staffCode, "Staff" + i);
				staffs.Add(staff);

				var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);

				var workflowOnBuffer = BMSTestHelper.CreateWorkflow(jobHeader, "on buffer", buffer);
				var workflowOnBucket = BMSTestHelper.CreateWorkflow(jobHeader, "on bucket", bucket);

				CreateTask(workflowOnBuffer, staff.GS_Code, 60, estVariationFactor: 1);

				CreateTask(workflowOnBucket, staff.GS_Code, 60, estVariationFactor: 1);
				workflows.Add(workflowOnBucket);
			}

			var staff1 = staffs[0];
			channel1.MSC_ParentID = staff1.PK;
			var staff2 = staffs[3];
			channel2.MSC_ParentID = staff2.PK;

			Factory.Save();

			var releaseGateTestCoordinator = new ReleaseGateTestCoordinator()
			{
				SaveCapacityCacheForAllStaff = false
			};

			using (TestConnection.TrackExecutedCommands())
			{
				RunReleaseGate(system, coordinator: releaseGateTestCoordinator);

				var deleteCapacities = TestConnection.ExecutedCommands.FirstOrDefault(s => s.Contains(
$@"DELETE FROM dbo.BMCapacityCache WHERE BMC_FC_Component = @bufferPK
Params
@bufferPK: '{buffer.PK}'
"));
				AssertNotNull(deleteCapacities);

				var insertsInBMCapacityCacheSQL = TestConnection.ExecutedCommands.Where(x => x.Contains("INSERT INTO dbo.BMCapacityCache")).ToArray();
				AssertEquals(2, insertsInBMCapacityCacheSQL.Length);

				var staff1Inserts = insertsInBMCapacityCacheSQL.Count(s => s.Contains(staff1.GS_Code));
				AssertEquals(1, staff1Inserts);

				var staff2Inserts = insertsInBMCapacityCacheSQL.Count(s => s.Contains(staff2.GS_Code));
				AssertEquals(1, staff2Inserts);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestProcess_WhenCacheRegistryItemOff_ShouldNotCauseCapacityToBeCached()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "INFJ", config.Bucket);
			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 6000); // Won't get released so we keep caching capacity for this resource

			workflow1.FH_VoteUpDownAmount = 10; // Force workflow to be considered for release first.

			Factory.Save();

			AssertEquals("Pre-condition: initial capacity", 42.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			RunReleaseGate(config.System);
			workflow1.Reload();

			AssertEquals("Should cache resource capacity", 1, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM BMCapacityCache"));
			AssertEquals("Updated capacity", 40.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals("Capacity is cached, so doesn't update immediately", 40.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			// Reset task so we can close it later
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			BufferCapacityCacheTest.PurgeCachedCapacity();
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			RunReleaseGate(config.System);

			AssertEquals("Updated capacity", 40.5m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals("Capacity is NOT cached, so updates immediately", 42.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
		}

		[TestDate(2015, 7, 14)]
		public void TestProcess_StaffCodeWithCommasShouldNotBreakTheCache()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, ",,,", "COMMACOMMACOMMA");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Bucket);

			BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			AssertEquals("Pre-condition: initial resource capacity", 42.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			RunReleaseGate(config.System);
			workflow.Reload();

			var newFactory = Factory.CreateNewFactory();
			var cachedCapacity = BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource.GS_Code, newFactory);

			var capacityFromAPI = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer);

			AssertNotNull("Capacity should be present in the cache", cachedCapacity);

			AssertEquals("The release gate should update the cached capacity after a run so we have updated figures between runs.", 40.5m, cachedCapacity.AvailableCapacity);
			AssertEquals("API calculation should be the same as the cache.", 40.5m, capacityFromAPI.AvailableCapacity);
		}

		[TestDate(2015, 7, 14)]
		public void TestProcess_StaffCodeWithJustOneCommaShouldNotBreakTheCache()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "A,9", "ayCOMMAnine");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Bucket);

			BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			AssertEquals("Pre-condition: initial resource capacity", 42.0m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);

			RunReleaseGate(config.System);
			workflow.Reload();

			var newFactory = Factory.CreateNewFactory();
			var cachedCapacity = BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource.GS_Code, newFactory);
			var capacityFromAPI = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer);

			AssertNotNull("Capacity should be present in the cache", cachedCapacity);

			AssertEquals("The release gate should update the cached capacity after a run so we have updated figures between runs.", 40.5m, cachedCapacity.AvailableCapacity);
			AssertEquals("API calculation should be the same as the cache.", 40.5m, capacityFromAPI.AvailableCapacity);
		}

		#endregion

		#region Component Transfer Event

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestProcess_ShouldLogXFREventOnWorkflow()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow");
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertEquals(config.Bucket, workflow.CurrentComponent);

			RunReleaseGate(config.System);
			workflow.Reload();

			AssertEquals(config.Buffer, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ReleaseGate, config.Bucket.PK, config.Buffer.PK, config.ComponentLink.PK);

			AssertEquals("Moved from component [bucket] to [buffer]. It was released by the Release Gate along component link [Buffer Entry].", log.DisplayEventReference);
			AssertEquals($"|FRM={config.Bucket.PK}|LNK={config.ComponentLink.PK}|MOD=REL|STS=OPN|TO={config.Buffer.PK}", log.SL_Reference);
		}

		#endregion

		#region Release

		[TestDate(2019, 1, 1)]
		public void TestRelease_WithTaskAssignedToInactiveStaff_ShouldNotRelease()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var inactiveResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NOP", "Nope");
			inactiveResource.GS_IsActive = false;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Really important thing");
			var task = BMSTestHelper.CreateTask(workflow, inactiveResource.GS_Code, lowEstMinutes: 10);

			Factory.Save();

			var logger = new BufferManagementLogger();
			RunReleaseGate(config.System, logger);

			workflow.Reload();

			CombineAssertions("Should not release workflow because there is a task assigned to an inactive resource", () =>
			{
				AssertSamePK(config.Bucket, workflow.CurrentComponent);

				AssertMultilineASCIIEquals("", $@"WTGDEV - ORG.buffer: Determining workflows eligible for Release Gate.
WTGDEV - ORG.buffer: Assembling reservation tracker.
WTGDEV - ORG.buffer: Sorting workflows into release sequence.
WTGDEV - ORG.buffer: Starting Release Gate run with 1 workflows to process.", logger.ToString());
			});

			var activeResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "YEP", "Yep");
			task.P9_GS_NKAssignedStaffMember = activeResource.GS_Code;

			Factory.Save();

			logger = new BufferManagementLogger();
			RunReleaseGate(config.System, logger);

			workflow.Reload();

			CombineAssertions("Should release workflow because the task is assigned to an active resource who has capacity", () =>
			{
				AssertSamePK(config.Buffer, workflow.CurrentComponent);

				AssertMultilineASCIIEquals("", $@"WTGDEV - ORG.buffer: Determining workflows eligible for Release Gate.
WTGDEV - ORG.buffer: Calculating Capacity for 1 staff.
{QueryLogString()}
WTGDEV - ORG.buffer: Assembling reservation tracker.
WTGDEV - ORG.buffer: Sorting workflows into release sequence.
WTGDEV - ORG.buffer: Starting Release Gate run with 1 workflows to process.
WTGDEV - ORG.buffer: Workflow Really important thing for job XVBQP68SIYXQ has been released.
WTGDEV - ORG.buffer: Persisting updated capacity after Release Gate run.
", logger.ToString());
			});
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_NoCapabilityTasks_OneStaff_ShouldBeReleased()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen");
			Assert(staff.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);

			var workflowAlreadyReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = 43 * 60;
			BMSTestHelper.CreateTask(workflowAlreadyReleased, staff.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should be released", config.Buffer, workflowToBeReleased.CurrentComponent);

				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code));

				AssertMultilineASCIIEquals("Workflow should be released as all tasks are assigned to a single resource with capacity.",
@"Release Gate Run at 07-Nov-16 00:00 UTC
RELEASED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 10 hours, Remaining Capacity: -5 hours", link.CapacityReservationDetails);
			});

			AssertEquals(string.Empty, workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer));
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_WithCapabilityTasks_OneStaff_ShouldBeReleased()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			Assert(staff.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 80);

			var taskCapability = BMSTestHelper.CreateTask(workflowToBeReleased, string.Empty, 80, capability: capability);

			var workflowAlreadyReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = 39 * 60;
			BMSTestHelper.CreateTask(workflowAlreadyReleased, staff.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should be released", config.Buffer, workflowToBeReleased.CurrentComponent);

				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code));

				AssertMultilineASCIIEquals("Should be released since the capacity consumed by the directly assigned tasks still leaves some free capacity when we come to assess the requirements for the capability task.",
@"Release Gate Run at 07-Nov-16 00:00 UTC
RELEASED, MAISEKNDORG - Heads will roll, Available Capacity: 9 hours, Reservation: 10 hours, Remaining Capacity: -1 hours", link.CapacityReservationDetails);
			});

			AssertEquals(ZString.Empty, workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer));
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_WithCapabilityTasks_TwoStaff_ShouldBeReleased()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Benny Buffalo Burgendorf", capability);
			Assert(staff1.IsWorking(ZDate.Today));
			Assert(staff2.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased1 = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);

			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);

			var workflowAlreadyReleased1 = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer); // AAA available capacity 5 hours
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree_task1 = (int)((48 - 9m) * 60);
			BMSTestHelper.CreateTask(workflowAlreadyReleased1, staff1.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree_task1, estVariationFactor: 1);

			var workflowAlreadyReleased2 = BMSTestHelper.CreateWorkflow(jobHeader, "Tails will rock", config.Buffer); // BBB available capacity 4.5 hours
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree_task2 = (int)((48 - 8.5m) * 60);
			BMSTestHelper.CreateTask(workflowAlreadyReleased2, staff2.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree_task2, estVariationFactor: 1);

			var taskCapability1 = BMSTestHelper.CreateTask(workflowToBeReleased1, string.Empty, 120, capability: capability);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased1.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow 1 should be released to AAA", config.Buffer, workflowToBeReleased1.CurrentComponent);
				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff1.GS_Code));
				AssertMultilineASCIIEquals("staff1 - Should be released to capability OOO",
@"Release Gate Run at 07-Nov-16 00:00 UTC
RELEASED, MAISEKNDORG - Heads will roll, Available Capacity: 9 hours, Reservation: 9.5 hours, Remaining Capacity: -0.5 hours, Other resources considered: Benny Buffalo Burgendorf.", link.CapacityReservationDetails);

				link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff2.GS_Code));
				AssertMultilineASCIIEquals("staff2 - Should be released to capability OOO",
@"Release Gate Run at 07-Nov-16 00:00 UTC
RELEASED, MAISEKNDORG - Heads will roll, Available Capacity: 8.5 hours, Reservation: 9.5 hours, Remaining Capacity: -1 hours, Other resources considered: Aaron Aardvark Aanensen.", link.CapacityReservationDetails);
			});

			AssertEquals(ZString.Empty, workflowToBeReleased1.GetReleaseFailureReasonForBuffer(config.Buffer));
			AssertEquals(ZString.Empty, taskCapability1.P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_WithCapabilityTasks_TwoStaff_WhenInsufficientCapacityForOneStaff_ShouldNotBeReleased()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Benny Buffalo Burgendorf", capability);
			Assert(staff1.IsWorking(ZDate.Today));
			Assert(staff2.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased1 = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);

			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);

			var workflowAlreadyReleased1 = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = (int)((48 - 9.03m) * 60);
			BMSTestHelper.CreateTask(workflowAlreadyReleased1, staff1.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			var workflowAlreadyReleased2 = BMSTestHelper.CreateWorkflow(jobHeader, "Tails will rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingNotEnoughFree = (int)((48 - 8.46m) * 60);
			BMSTestHelper.CreateTask(workflowAlreadyReleased2, staff2.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingNotEnoughFree, estVariationFactor: 1);

			// OBSERVE the extra estimated minute on taskCapability1. It should make a world of difference!
			var taskCapability1 = BMSTestHelper.CreateTask(workflowToBeReleased1, string.Empty, 121, capability: capability);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);

			workflowToBeReleased1.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow 1 should be blocked", config.Bucket, workflowToBeReleased1.CurrentComponent);
				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff1.GS_Code));
				AssertMultilineASCIIEquals("Staff1 - Should not be released",
@"Release Gate Run at 07-Nov-16 00:00 UTC
PASSED (but not released), MAISEKNDORG - Heads will roll, Available Capacity: 9.03 hours, Reservation: 9.51 hours, Remaining Capacity: -0.48 hours, Other resources considered: Benny Buffalo Burgendorf.", link.CapacityReservationDetails);

				link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff2.GS_Code));
				AssertMultilineASCIIEquals("Staff2 - Should not be released",
@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 8.47 hours, Reservation: 9.51 hours, Remaining Capacity: -1.04 hours, Other resources considered: Aaron Aardvark Aanensen.", link.CapacityReservationDetails);
			});

			AssertMultilineASCIIEquals("Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 9.51 hours, currently has 9.03 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (2 hours), T00001001 (2 hours), T00001002 (2 hours), T00001003 (2 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001010 (1.51 hours)
	Benny Buffalo Burgendorf: required 9.51 hours, currently has 8.47 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001004 (2 hours), T00001005 (2 hours), T00001006 (2 hours), T00001007 (2 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001010 (1.51 hours)".StripTaskIds(), workflowToBeReleased1.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_WithCapabilityTasks_TwoStaff_WhenInsufficientCapacityForBothStaff_ShouldBeBlockedAndLoggedAppropriately()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Benny Buffalo Burgendorf", capability);
			Assert(staff1.IsWorking(ZDate.Today));
			Assert(staff2.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased1 = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff1.GS_Code, 80);

			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);
			BMSTestHelper.CreateTask(workflowToBeReleased1, staff2.GS_Code, 80);

			var workflowAlreadyReleased1 = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = (int)((48m - 4.5m) * 60);
			BMSTestHelper.CreateTask(workflowAlreadyReleased1, staff1.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			var workflowAlreadyReleased2 = BMSTestHelper.CreateWorkflow(jobHeader, "Tails will rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingNotEnoughFree = (int)((48m - 4.37m) * 60);
			BMSTestHelper.CreateTask(workflowAlreadyReleased2, staff2.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingNotEnoughFree, estVariationFactor: 1);

			var taskCapability1 = BMSTestHelper.CreateTask(workflowToBeReleased1, string.Empty, 121, capability: capability);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased1.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should be blocked for AAA", config.Bucket, workflowToBeReleased1.CurrentComponent);
				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff1.GS_Code));
				AssertMultilineASCIIEquals("Should not be released",
@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 4.5 hours, Reservation: 9.51 hours, Remaining Capacity: -5.01 hours, Other resources considered: Benny Buffalo Burgendorf.", link.CapacityReservationDetails);

				link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff2.GS_Code));
				AssertMultilineASCIIEquals("Workflow should be blocked for BBB",
@"Release Gate Run at 07-Nov-16 00:00 UTC
BLOCKED, MAISEKNDORG - Heads will roll, Available Capacity: 4.38 hours, Reservation: 9.51 hours, Remaining Capacity: -5.13 hours, Other resources considered: Aaron Aardvark Aanensen.", link.CapacityReservationDetails);
			});

			AssertMultilineASCIIEquals("Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 9.51 hours, currently has 4.5 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (2 hours), T00001001 (2 hours), T00001002 (2 hours), T00001003 (2 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001010 (1.51 hours)
	Benny Buffalo Burgendorf: required 9.51 hours, currently has 4.38 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001004 (2 hours), T00001005 (2 hours), T00001006 (2 hours), T00001007 (2 hours)
		Tasks requiring capability [Orange Ocelot Ordering]: T00001010 (1.51 hours)".StripTaskIds(), workflowToBeReleased1.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2016, 11, 7)]
		public void TestReleaseWorkflowToBuffer_OneCapabilityTask_NoResourceTask_WhenSufficientCapacity_ShouldBeReleased()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			Assert(staff.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			var taskCapability = BMSTestHelper.CreateTask(workflowToBeReleased, string.Empty, 201, capability: capability);

			var workflowAlreadyReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Tails won't rock", config.Buffer);
			var taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree = 43 * 60;
			BMSTestHelper.CreateTask(workflowAlreadyReleased, staff.GS_Code, taskDurationToConsumeMostAvailableCapacity_LeavingSomeFree, estVariationFactor: 1);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should be blocked", config.Buffer, workflowToBeReleased.CurrentComponent);
				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code));
				AssertMultilineASCIIEquals("Should not be released",
@"Release Gate Run at 07-Nov-16 00:00 UTC
RELEASED, MAISEKNDORG - Heads will roll, Available Capacity: 5 hours, Reservation: 5.03 hours, Remaining Capacity: -0.03 hours", link.CapacityReservationDetails);
			});

			AssertEquals(ZString.Empty, workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer));
		}

		[TestDate(2017, 1, 31)]
		public void TestReleaseWorkflowToBuffer_TwoStaff_WhenInsufficientCapacityOnSecondStaff_SufficientCapacityOnFirstStaff_ShouldBeBlocked()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = BMSTestHelper.CreateCapability(Factory, "OOO", "Orange Ocelot Ordering", autoAssignTasks: true);
			var staffAAA = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen", capability);
			var staffBBB = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Benny Buffalo Burgendorf", capability);

			Assert(staffAAA.IsWorking(ZDate.Today));
			Assert(staffBBB.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAITRDORG";

			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Please release me let me go", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased, staffBBB.GS_Code, 30, description: "For I don't love you anymore");
			BMSTestHelper.CreateTask(workflowToBeReleased, staffAAA.GS_Code, 30, description: "Her lips are warm");
			BMSTestHelper.CreateTask(workflowToBeReleased, staffAAA.GS_Code, 30, description: "While yours are cold");

			var workflowInBufferAAA = BMSTestHelper.CreateWorkflow(jobHeader, "Not chewin' up capacitee for AAA", config.Buffer);
			BMSTestHelper.CreateTask(workflowInBufferAAA, staffAAA.GS_Code, 450);

			var workflowInBufferBBB = BMSTestHelper.CreateWorkflow(jobHeader, "Chewin' up capacitee for BBB", config.Buffer);
			BMSTestHelper.CreateTask(workflowInBufferBBB, staffBBB.GS_Code, 600);
			BMSTestHelper.CreateTask(workflowInBufferBBB, staffBBB.GS_Code, 600);
			BMSTestHelper.CreateTask(workflowInBufferBBB, staffBBB.GS_Code, 600);
			BMSTestHelper.CreateTask(workflowInBufferBBB, staffBBB.GS_Code, 600);
			BMSTestHelper.CreateTask(workflowInBufferBBB, staffBBB.GS_Code, 600);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			CombineAssertions(() =>
			{
				AssertSamePK("Workflow should be blocked", config.Bucket, workflowToBeReleased.CurrentComponent);

				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staffBBB.GS_Code));
				AssertMultilineASCIIEquals("Should not be released",
@"Release Gate Run at 31-Jan-17 00:00 UTC
BLOCKED, MAITRDORG - Please release me let me go, Available Capacity: -27 hours, Reservation: 0.75 hours, Remaining Capacity: -27.75 hours, Other resources considered: Aaron Aardvark Aanensen.", link.CapacityReservationDetails);
			});

			AssertMultilineASCIIEquals("Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Aaron Aardvark Aanensen: required 1.5 hours, currently has 36.75 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (0.75 hours), T00001002 (0.75 hours)
	Benny Buffalo Burgendorf: required 0.75 hours, currently has -27 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (0.75 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2017, 2, 23)]
		public void TestReleaseWorkflowToBuffer_TwoStaff_WhenInsufficientCapacityOnSecondStaff_FirstStaffIsOnLeave_ShouldNotBeReleased()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var staffAAA = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen");
			var staffBBB = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Benny Buffalo Burgendorf");

			BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today, ZDateTime.Today.AddDays(2), staffAAA.PK, availabilityFactor: 0);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAITRDORG";

			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Please release me let me go", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased, staffAAA.GS_Code, 30, description: "For I don't love you anymore", taskStatus: "CLS", sequence: 10);
			BMSTestHelper.CreateTask(workflowToBeReleased, staffAAA.GS_Code, 30, description: "Her lips are warm", taskStatus: "CLS", sequence: 20);
			BMSTestHelper.CreateTask(workflowToBeReleased, staffAAA.GS_Code, 30, description: "While yours are cold", sequence: 40);
			BMSTestHelper.CreateTask(workflowToBeReleased, staffBBB.GS_Code, 30, description: "Release me my darling", sequence: 30);
			BMSTestHelper.CreateTask(workflowToBeReleased, staffBBB.GS_Code, 30, description: "Let me go", sequence: 50);

			var workflowInBufferAAA = BMSTestHelper.CreateWorkflow(jobHeader, "Slightly diminished capacity for AAA", config.Buffer);
			BMSTestHelper.CreateTask(workflowInBufferAAA, staffAAA.GS_Code, 450);

			var workflowInBufferBBB = BMSTestHelper.CreateWorkflow(jobHeader, "Greatly diminished capacity for BBB", config.Buffer);
			BMSTestHelper.CreateTask(workflowInBufferBBB, staffBBB.GS_Code, 9001);

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();
			AssertEquals(string.Empty, workflowToBeReleased.GetSuccessfulReleaseNotes());

			CombineAssertions(() =>
			{
				var link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staffAAA.GS_Code));
				AssertMultilineASCIIEquals("Staff AAA - Should not be released",
@"Release Gate Run at 23-Feb-17 00:00 UTC
PASSED (but not released), MAITRDORG - Please release me let me go, Available Capacity: 36.75 hours, Reservation: 0.75 hours, Remaining Capacity: 36 hours, Other resources considered: Benny Buffalo Burgendorf.", link.CapacityReservationDetails);

				link = Factory.LoadTop1<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staffBBB.GS_Code));
				AssertMultilineASCIIEquals("Staff BBB - Should not be released",
@"Release Gate Run at 23-Feb-17 00:00 UTC
BLOCKED, MAITRDORG - Please release me let me go, Available Capacity: -177.03 hours, Reservation: 1.5 hours, Remaining Capacity: -178.53 hours, Other resources considered: Aaron Aardvark Aanensen.", link.CapacityReservationDetails);

				AssertSamePK("Workflow should be blocked, and yet!...", config.Bucket, workflowToBeReleased.CurrentComponent);

				AssertMultilineASCIIEquals("Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Benny Buffalo Burgendorf: required 1.5 hours, currently has -177.03 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001003 (0.75 hours), T00001004 (0.75 hours)
	Aaron Aardvark Aanensen: required 0.75 hours, currently has 36.75 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001002 (0.75 hours)".StripTaskIds(), workflowToBeReleased.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestReleaseBlockedWorkflow_ShouldNotUnBlock()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Release me", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Please");
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			RunReleaseGate(config.System);

			Factory.ReloadBusinessObjects(ProcessHeaderSchema.Instance, workflow2);

			AssertEquals(config.Buffer, workflow2.CurrentComponent);
			AssertEquals("Releasing a workflow shouldn't un-block it", WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			workflow2.FH_CompletionStatement = "Nope. Nope. Nope.";
			Factory.Save();

			AssertEquals("After saving, the workflow shouldn't change status", WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
		}

		[TestDate(2013, 8, 28)]
		public void TestReleaseToMultipleBuffers_ShouldMoveToOneWithLowestSequence()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 0);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1", sequence: 2);
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2", sequence: 1);

			LinkComponents(bucket, buffer1, sequence: 0);
			LinkComponents(bucket, buffer2, sequence: 0);

			buffer1.FC_BufferTimespanInMinutes = buffer2.FC_BufferTimespanInMinutes = 96 * 60;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket.PK;

			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code);

			Factory.Save();

			RunReleaseGate(system);

			workflow.Reload();
			AssertEquals(buffer2.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertEquals(buffer2.PK, workflow.FH_FC_CurrentComponent);
		}

		[TestDate(2013, 8, 28)]
		public void TestRelease_ShouldNotReleaseWorkflowWhenInDifferentComponent()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 0);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1", sequence: 1);
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2", sequence: 2);

			LinkComponents(bucket, buffer1, sequence: 0);

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket.PK;

			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code);

			Factory.Save();

			var coordinator = new ReleaseGateTestCoordinator();

			coordinator.IsWorkflowReleasableFunc = w =>
			{
				w.VFH_FC_CurrentComponent = buffer2.PK;

				return true;
			};

			RunReleaseGate(system, coordinator: coordinator);

			workflow.Reload();
			AssertEquals("Should not have been released", bucket.PK, workflow.FH_FC_CurrentComponent);

			var noteText = workflow.GetSuccessfulReleaseNotes();

			AssertEquals("Should not have logged anything", string.Empty, noteText);
		}

		#endregion

		#region Process

		[TestDate(2014, 3, 21)]
		public void TestProcess_ShouldPickupWorkflowsInReleaseGate()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			system.FS_Name = "WTGDEV";

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var buffer = CreateBuffer(system);
			LinkComponents(bucket1, bucket2);
			LinkComponents(bucket2, buffer);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			var task1 = CreateTask(workflow1, resource.GS_Code, 60);
			var task2 = CreateTask(workflow2, resource.GS_Code, 60);

			Factory.Save();

			var logger = new BufferManagementLogger();
			RunReleaseGate(system, logger);

			workflow1.Reload();
			workflow2.Reload();

			AssertEquals("workflow1 should not have moved", bucket1.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals("workflow2 should have been picked up within the release gate and released into the buffer", buffer.PK, workflow2.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals("", $@"WTGDEV.buffer: Determining workflows eligible for Release Gate.
WTGDEV.buffer: Calculating Capacity for 1 staff.
{QueryLogString()}
WTGDEV.buffer: Assembling reservation tracker.
WTGDEV.buffer: Sorting workflows into release sequence.
WTGDEV.buffer: Starting Release Gate run with 1 workflows to process.
WTGDEV.buffer: Workflow workflow2 for job MAIORGSYD has been released.
WTGDEV.buffer: Persisting updated capacity after Release Gate run.", logger.ToString());
		}

		[TestDate(2013, 10, 31, 14, 54, 0)]
		public void TestProcess_ShouldRecordReservationsAgainstResourceBufferPivot()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			system.FS_Name = "WTGDEV";

			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);

			LinkComponents(bucket, buffer);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIORG1";
			var workflow1 = ProcessJobHeader.GetForParent(job1, Factory).ProcessHeaders[0];
			workflow1.FH_VoteUpDownAmount = 40;
			workflow1.FH_CompletionStatement = "workflow1";
			CreateTask(workflow1, staff.GS_Code, 60);

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAIORG2";
			var workflow2 = ProcessJobHeader.GetForParent(job2, Factory).ProcessHeaders[0];
			workflow2.FH_VoteUpDownAmount = 30;
			workflow2.FH_CompletionStatement = "workflow2";
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			var job3 = Factory.NewWithValidTestData<OrgHeader>();
			job3.OH_Code = "MAIORG3";
			var workflow3 = ProcessJobHeader.GetForParent(job3, Factory).ProcessHeaders[0];
			workflow3.FH_VoteUpDownAmount = 20;
			workflow3.FH_CompletionStatement = "workflow3";
			CreateTask(workflow3, staff.GS_Code, 6000);

			var job4 = Factory.NewWithValidTestData<OrgHeader>();
			job4.OH_Code = "MAIORG4";
			var workflow4 = ProcessJobHeader.GetForParent(job4, Factory).ProcessHeaders[0];
			workflow4.FH_CompletionStatement = "workflow4";
			workflow4.FH_VoteUpDownAmount = 10;
			CreateTask(workflow4, staff.GS_Code, 60);

			Factory.Save();

			RunReleaseGate(system);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();

			AssertEquals("Should release workflow with small estimate", buffer, workflow1.CurrentComponent);
			AssertEquals("Should NOT release workflow with large estimate", bucket, workflow3.CurrentComponent);
			AssertEquals("Should NOT release workflow with small estimate since the larger one has reserved capacity", bucket, workflow4.CurrentComponent);

			var query = new ZDBOnlyQuery(typeof(BMComponentResourceLink));
			query.AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code);

			var link = Factory.LoadTop1<BMComponentResourceLink>(query);
			AssertNotNull("Should create a link record for the resource", link);
			AssertEquals("We don't want to limit resource capacity!", (byte)100, link.FD_CapacityLimitPercent);

			AssertMultilineASCIIEquals("CapacityReservationDetails",
@"Release Gate Run at 31-Oct-13 14:54 UTC
RELEASED, MAIORG1 - workflow1, Available Capacity: 48 hours, Reservation: 1.5 hours, Remaining Capacity: 46.5 hours
BLOCKED, MAIORG3 - workflow3, Available Capacity: 46.5 hours, Reservation: 150 hours, Remaining Capacity: -103.5 hours
BLOCKED, MAIORG4 - workflow4, Available Capacity: -103.5 hours, Reservation: 1.5 hours, Remaining Capacity: -105 hours
", link.CapacityReservationDetails);

			workflow1.FH_FC_CurrentComponent = workflow2.FH_FC_CurrentComponent = workflow3.FH_FC_CurrentComponent = workflow4.FH_FC_CurrentComponent = bucket.PK;

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 10, 31, 15, 54, 0);
			RunReleaseGate(system);

			link = new BusinessObjectFactory().LoadTop1<BMComponentResourceLink>(query);
			AssertMultilineASCIIEquals("CapacityReservationDetails - running the release gate again should overwrite previous log details",
@"Release Gate Run at 31-Oct-13 15:54 UTC
RELEASED, MAIORG1 - workflow1, Available Capacity: 48 hours, Reservation: 1.5 hours, Remaining Capacity: 46.5 hours
BLOCKED, MAIORG3 - workflow3, Available Capacity: 46.5 hours, Reservation: 150 hours, Remaining Capacity: -103.5 hours
BLOCKED, MAIORG4 - workflow4, Available Capacity: -103.5 hours, Reservation: 1.5 hours, Remaining Capacity: -105 hours
", link.CapacityReservationDetails);
		}

		[TestDate(2013, 11, 17, 9, 0, 0)]
		public void TestProcess_WhenAllowedByOneResourceButNotActuallyReleased_ShouldIndicateWhenResourceDidNotBlockRelease()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			system.FS_Name = "WTGDEV";

			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);

			LinkComponents(bucket, buffer);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff1.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
			staff1.GS_FullName = "Bendy";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff2.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
			staff2.GS_FullName = "Baps";

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIORG1";
			var workflow1 = ProcessJobHeader.GetForParent(job1, Factory).ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			CreateTask(workflow1, staff1.GS_Code, 60);
			CreateTask(workflow1, staff2.GS_Code, 60);

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAIORG2";
			var workflow2 = ProcessJobHeader.GetForParent(job2, Factory).ProcessHeaders[0];
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			CreateTask(workflow2, staff2.GS_Code, 6000);

			Factory.Save();

			RunReleaseGate(system);

			workflow1.Reload();

			AssertEquals("Should NOT release workflow with small estimate due to existing drain on staff2 capacity", bucket.PK, workflow1.FH_FC_CurrentComponent);

			var link_staff1 = Factory.LoadTop1<BMComponentResourceLink>(new ZDBOnlyQuery(typeof(BMComponentResourceLink)).AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, staff1.GS_Code));
			AssertNotNull("Should create a link record for the resource", link_staff1);
			AssertEquals("We don't want to limit resource capacity!", (byte)100, link_staff1.FD_CapacityLimitPercent);

			var link_staff2 = Factory.LoadTop1<BMComponentResourceLink>(new ZDBOnlyQuery(typeof(BMComponentResourceLink)).AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, staff2.GS_Code));
			AssertNotNull("Should create a link record for the resource", link_staff2);
			AssertEquals("We don't want to limit resource capacity!", (byte)100, link_staff2.FD_CapacityLimitPercent);

			CombineAssertions("Should log who blocks workflow from release", () =>
			{
				AssertMultilineASCIIEquals("Staff1 CapacityReservationDetails",
@"Release Gate Run at 17-Nov-13 09:00 UTC
PASSED (but not released), MAIORG1 - workflow1, Available Capacity: 48 hours, Reservation: 1.5 hours, Remaining Capacity: 46.5 hours, Other resources considered: Baps.
", link_staff1.CapacityReservationDetails);

				AssertMultilineASCIIEquals("Staff2 CapacityReservationDetails",
@"Release Gate Run at 17-Nov-13 09:00 UTC
BLOCKED, MAIORG1 - workflow1, Available Capacity: -102 hours, Reservation: 1.5 hours, Remaining Capacity: -103.5 hours, Other resources considered: Bendy.
", link_staff2.CapacityReservationDetails);
			});
		}

		public void TestProcessForWorkflowDeletedAfterItWasLoaded()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, shouldUseExistingSystem: true);

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			workflow.AddTag(config.RedTag);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);

			var elevenWorkflows = Enumerable.Range(0, 11).Select(_ =>
			{
				var otherWorkflow = jobHeader.ProcessHeaders.AddNew();
				otherWorkflow.FH_FC_CurrentComponent = config.Bucket.PK;
				otherWorkflow.AddTag(config.PlatinumTag);

				return otherWorkflow;
			}).ToArray();

			Factory.Save();

			var coordinator = new ReleaseGateTestCoordinator { IsWorkflowReleasableFunc = CanTransfer_ButAlsoDeleteWorkflow };

			bool CanTransfer_ButAlsoDeleteWorkflow(ViewProcessHeader view)
			{
				var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedWorkflow = newFactory1.Load<ProcessHeader>(workflow.PK);

				if (loadedWorkflow != null)
				{
					loadedWorkflow.Delete();
					newFactory1.Save();
				}

				return true;
			}

			AssertNoExceptionThrown(() => RunReleaseGate(config.System, coordinator: coordinator));

			var newFactory2 = new BusinessObjectFactory();
			AssertNull(newFactory2.Load<ProcessHeader>(workflow.PK));
		}

		public void TestProcess_WhenWorkflowHasNoTasks_ShouldNotRelease()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var buffer = config.Buffer;
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "I'm good good good and OH so smart!");

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer, shouldBeReleasedToBuffer: false, failureLogService: ReleaseLogFailureServiceForTest);

			AssertMultilineASCIIEquals("Workflow shouldn't be released to the buffer if it's empty because it won't appear on anyone's board and will likely go missing, but still contribute to metrics like Zone 0 Job Days.",
@"There are no assigned tasks in this workflow.", workflow.GetReleaseFailureReasonForBuffer(buffer));
		}

		public void TestProcess_WhenWorkflowHasUnAssignedTask_ShouldNotRelease()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var buffer = config.Buffer;
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "I'm good good good and OH so smart!");
			var task = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer, shouldBeReleasedToBuffer: false, failureLogService: ReleaseLogFailureServiceForTest);

			AssertMultilineASCIIEquals("Workflow shouldn't be released to the buffer if it only has non-assigned tasks because it won't appear on anyone's board and will likely go missing, but still contribute to metrics like Zone 0 Job Days.",
@"There are no assigned tasks in this workflow.", workflow.GetReleaseFailureReasonForBuffer(buffer));
		}

		[TestDate(2019, 1, 1)]
		public void TestProcess_WhenWorkflowHasCapabilityTaskWithNoWorkingResources_ShouldNotRelease_AndLogAppropriately()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var capability = BMSTestHelper.CreateCapability(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "I'm good good good and OH so smart!");
			var task = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 1, capability: capability);

			Factory.Save();

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer, shouldBeReleasedToBuffer: false, failureLogService: ReleaseLogFailureServiceForTest);

			AssertMultilineASCIIEquals("Workflow shouldn't be released to the buffer if it only has non-assigned tasks because it won't appear on anyone's board and will likely go missing, but still contribute to metrics like Zone 0 Job Days.",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Capability []: required 0.03 hours, but there are currently no working resources who possess this capability.
		Tasks requiring this capability: T00001000 (0.03 hours)".StripTaskIds(), workflow.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2013, 7, 15)]
		public void TestProcess_ShouldUseResourceMembership()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			system.FS_Name = "WTGDEV";

			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var link = LinkComponents(bucket, buffer);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			staff.GS_FullName = "Frodo Baggins";
			var bufferMembership = (BMComponentResourceLink)staff.ComponentMembership.AddNew();
			bufferMembership.FD_CapacityLimitPercent = 0;
			bufferMembership.FD_FC_Component = buffer.PK;

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_EstDuration = new ZInt(10).GetDateTimeFromMinutes();

			Factory.Save();

			RunReleaseGate(system, failureLogService: ReleaseLogFailureServiceForTest);
			AssertSamePK(bucket, workflow.CurrentComponent);

			AssertMultilineASCIIEquals("Release Gate log",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 0.25 hours, currently has 0 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (0.25 hours)".StripTaskIds(), workflow.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());

			bufferMembership.FD_CapacityLimitPercent = 50;
			Factory.Save();

			var logger = new BufferManagementLogger();
			RunReleaseGate(system, logger, failureLogService: ReleaseLogFailureServiceForTest);

			Factory.ReloadBusinessObjects(ProcessHeaderSchema.Instance, workflow);

			AssertSamePK(buffer, workflow.CurrentComponent);

			AssertMultilineASCIIEquals("Release Gate log",
	$@"WTGDEV.Buffer: Determining workflows eligible for Release Gate.
WTGDEV.Buffer: Calculating Capacity for 1 staff.
{QueryLogString()}
WTGDEV.Buffer: Assembling reservation tracker.
WTGDEV.Buffer: Sorting workflows into release sequence.
WTGDEV.Buffer: Starting Release Gate run with 2 workflows to process.
WTGDEV.Buffer: Workflow  for job MAIORGSYD has been released.
WTGDEV.Buffer: Persisting updated capacity after Release Gate run.", logger.ToString());
		}

		[SnailTest]
		[TestDate(2013, 7, 17, 9, 0, 0)]
		public void TestProcessRequestsInBatches()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_BufferTimespanInMinutes = 3600;
			config.Buffer.FC_BufferLoadLimitPercent = 60;

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflows = Enumerable.Range(1, 80).Select(i =>
			{
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_CompletionStatement = "Workflow " + i;
				workflow.FH_VoteUpDownAmount = (short)i;

				var task = job.WorkflowItems.AddNew();
				task.P9_FH_ProcessHeader = workflow.PK;
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task.P9_EstDuration = new ZInt(20).GetDateTimeFromMinutes();

				return workflow;
			}).ToArray();

			Factory.Save();

			var logger = new BufferManagementLogger();
			var expectedMessage = new StringBuilder($@"WTGDEV - ORG.buffer: Determining workflows eligible for Release Gate.
WTGDEV - ORG.buffer: Calculating Capacity for 1 staff.
{QueryLogString()}
WTGDEV - ORG.buffer: Assembling reservation tracker.
WTGDEV - ORG.buffer: Sorting workflows into release sequence.
WTGDEV - ORG.buffer: Starting Release Gate run with 81 workflows to process.

");

			const int expectedNumberOfWorkflowsReleased = 72;

			for (int i = workflows.Length - 1, count = 0; i > 0 && count < expectedNumberOfWorkflowsReleased; i--, count++)
			{
				var workflow = workflows[i];
				var message = string.Format("WTGDEV - ORG.buffer: Workflow {0} for job {1} has been released.",
					workflow.FH_CompletionStatement,
					job.OH_Code);

				expectedMessage.AppendLine(message);
			}

			expectedMessage.Append("WTGDEV - ORG.buffer: Persisting updated capacity after Release Gate run.");

			RunReleaseGate(config.System, logger);

			var loadedWorkflows = new BusinessObjectFactory().Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflows.Select(w => w.PK).ToArray())).OrderByDescending(w => int.Parse(w.FH_CompletionStatement.Split(' ')[1])).ToArray();
			CombineAssertions(() =>
			{
				foreach (var workflow in loadedWorkflows.Take(expectedNumberOfWorkflowsReleased))
				{
					AssertSamePK(string.Format("Workflow {0} should have been moved to the buffer", workflow.FH_CompletionStatement), config.Buffer, workflow.CurrentComponent);
				}
				foreach (var workflow in loadedWorkflows.Skip(expectedNumberOfWorkflowsReleased))
				{
					AssertSamePK(string.Format("Workflow {0} should still be in the bucket - no capacity", workflow.FH_CompletionStatement), config.Bucket, workflow.CurrentComponent);
				}
			});

			AssertMultilineASCIIEquals("", expectedMessage.ToString(), logger.ToString());
		}

		#endregion

		#region Run
		[TestDate(2013, 8, 30)]
		public void TestRunReleaseGate_ShouldLogReleaseRuleFailure()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system, "buffer1");

			LinkComponents(bucket, buffer);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, staff.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * (buffer.FC_BufferLoadLimitPercent / 100.0)));
			var task2 = CreateTask(workflow2, staff.GS_Code, 60);

			Factory.Save();

			RunReleaseGate(system, failureLogService: ReleaseLogFailureServiceForTest);

			workflow2.Reload();
			AssertEquals(bucket, workflow2.CurrentComponent);

			AssertMultilineASCIIEquals("",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	: required 1.5 hours, currently has -24 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());
		}

		[TestDate(2013, 8, 30)]
		public void TestRunReleaseGate_ShouldRemoveReleaseFailureReasonForSuccessfulRelease()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system, "buffer1");
			LinkComponents(bucket, buffer);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = CreateTask(workflow1, staff.GS_Code, (int)(buffer.FC_BufferTimespanInMinutes * (buffer.FC_BufferLoadLimitPercent / 100.0)));
			var task2 = CreateTask(workflow2, staff.GS_Code, 60);

			Factory.Save();

			RunReleaseGate(system, failureLogService: ReleaseLogFailureServiceForTest);

			workflow2.Reload();
			AssertEquals(bucket, workflow2.CurrentComponent);

			AssertMultilineASCIIEquals("",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 1.5 hours, currently has -24 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			RunReleaseGate(system, failureLogService: ReleaseLogFailureServiceForTest);
			workflow2.Reload();

			AssertEquals(string.Empty, workflow2.GetReleaseFailureReasonForBuffer(buffer));
		}

		[TestDate(2013, 7, 15)]
		public void TestRunReleaseGate_ShouldNotCreateLogsThatBreakExcel()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			LinkComponents(bucket, buffer);

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			for (int i = 0; i < 50; i++)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
				staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
				staff.GS_FullName = "Ant Man" + i;
				staff.Capabilities.Add(capability);
			}

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();

			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_EstDuration = new ZInt(1).GetDateTimeFromMinutes();

			Factory.Save();

			var logger = new BufferManagementLogger();
			RunReleaseGate(system, logger);

			Assert(string.Format("No logs should violate the Excel export length. Length: {0} Message: {1}", logger.ToString().Length, logger.ToString()), !(logger.ToString().Length > 32767));
		}

		#endregion

		#region Error Handling

		public void TestRunReleaseGate_WhenGetRequestsByWorkflowPKAndApplicationExceptionIsThrown_ShouldCatchLogAndReportError()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);
			var company = Factory.New<IGlbCompany>();

			((BusinessObject)company).FillWithValidTestData();
			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var bucket1 = CreateBucket(system, "bucket");
			var bucket2 = CreateBuffer(system, "buffer");
			var link = LinkComponents(bucket1, bucket2, sequence: 0, isReleaseGate: true);

			Factory.Save();

			var logger = new BufferManagementLogger();

			ErrorReporter.Clear();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new DummyClientHook()))
			{
				AssertNoExceptionThrown("Processing links with country-specific and a non-country-specific identifier " +
					"with a non-country-specific client override identifier. should not throw exception!", () => RunReleaseGate(system, logger));
			}

			var applicationException = ErrorReporter.LastExceptionReported.InnerException.InnerException;
			AssertType<ApplicationException>("Sorry, you cannot have a country-specific and a non-country-specific identifier with a non-country-specific client override identifier", applicationException);

			CombineAssertions("Should have log and inner Exception should have CountryCode and DummyClientHook infos", () =>
			{
				AssertContains("Should log the exception", @"buffer: Determining workflows eligible for Release Gate.
Error - Error on getting Workflows Exception has been thrown by the target of an invocation.", logger.ToString());
				AssertEquals("CountryCode Should be AU", applicationException.Data["CountryCode"], "AU");
				AssertStartsWith("TypePath should be the DummyClientHook",
					"Enterprise.BufferManagement.Business.Test.ReleaseGateKeeperTest+DummyClientHook,Enterprise.BufferManagement.Business.Test",
					applicationException.Data["TypePath"].ToString());
			});

			ErrorReporter.Clear();
		}

		class DummyClientHook : ClientHook
		{
			public override Clients Client => Clients.EDI;
			public override string ClientDisplayName => nameof(DummyClientHook);
			protected override NewClientModuleInfo[] NewClientModulesCore
			{
				get
				{
					var clientOverrideModuleIdentifier = new ClientOverrideModuleIdentifier(ModuleIDs.Customs.AU.AirCargoOutturnBills);
					var info = new ClientOverrideModuleInfo(clientOverrideModuleIdentifier, typeof(DummyClientHook), "AU");
					var newClientModuleInfo = new NewClientModuleInfo(info);
					return new[] { newClientModuleInfo };
				}
			}
		}

		public void TestRunReleaseGate_WhenGetRequestsByWorkflowPKThrowsSqlInfrastructureException_ShouldHandleErrorReport_WithoutCausingAdditionalErrorReportAboutBranchNotSet()
		{
			var buffer = BMSTestHelper.CreateBuffer(system);
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (Env.Instance.TemporaryServiceTaskContext(ReleaseGateRunnerServiceTask.Code, canRunInAnyBranch: true))
			using (DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider())
			using (Globals.SetIsUserInteractiveForTest(false)) // will make it attempt to send an email rather than just show a popup.
			{
				try
				{
					Globals.SetIsUnitTestingProductionFunctionality(true); // will make it attempt to send an email rather than just show a popup.

					var keeper = new ReleaseGatekeeper_ForBranchNotSetTest(buffer, new BufferManagementLogger());
					AssertNoExceptionThrown("The infrastructure error should be handled appropriately without causing the whole service task to crash.", () => keeper.Process(CancellationToken.None));
				}
				finally
				{
					Globals.SetIsUnitTestingProductionFunctionality(false);
				}
			}

			if (ErrorReporter.TotalErrorCount == 1)
			{
				AssertEquals("ReleaseGateKeeper.Process_GetRequestsByWorkflowPK", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear(); // if there's more than 1, or if it's not that key, then we'll want the details of what it is.
			}
		}

		class ReleaseGatekeeper_ForBranchNotSetTest : ReleaseGateKeeper
		{
			protected internal ReleaseGatekeeper_ForBranchNotSetTest(BMComponent buffer, ILogger logger)
				: base(buffer, logger, new ReleaseGateLoggerWithFailureServices(new ReleaseGateFailureLogService_ForTest()))
			{
			}

			protected override LinksProcessorWithDeactivation GetLinksProcessor(HashSet<ReleaseGateRequest> eligibleWorkflowsForReleaseGate, TransferRuleRunnerLogger logger, ReleaseGateTransferRuleRunnerDataAccessor dataAccessor)
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(18470, "Login failed for user 'ediprod_RestrictedReaderLogin'. Reason: The account is disabled.");
				throw new TargetInvocationException("Something went terribly wrong", sqlException);
			}
		}

		#endregion

		#region Release Sequences

		public void TestWorkflowsAreReleasedAccordingToNudge()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 0);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", sequence: 1);
			LinkComponents(bucket, buffer, sequence: 0);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();

			//                                                                                                                      Nudge       EffNudge     SeqNudge
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: releaseGroup1.PK, nudge: 997);  // 997          997         n/a
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: releaseGroup2.PK, nudge: 1001); // 1001         1001        1000 - (3 - 1) * 0.0001 = 999.9998
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: releaseGroup2.PK, nudge: 998);  // 999.9999     998         1000 - (2 - 1) * 0.0001 = 999.9999
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: releaseGroup2.PK, nudge: 999);  // 999          999         n/a
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5", releaseGroupPK: releaseGroup2.PK, nudge: 990);  // 1000         990         1000 - (1 - 1) * 0.0001 = 1000

			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			workflow3.FH_FC_CurrentComponent = bucket.PK;
			workflow4.FH_FC_CurrentComponent = bucket.PK;
			workflow5.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, lowEstMinutes: 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, resource.GS_Code, lowEstMinutes: 60);
			var task4 = BMSTestHelper.CreateTask(workflow4, resource.GS_Code, lowEstMinutes: 60);
			var task5 = BMSTestHelper.CreateTask(workflow5, resource.GS_Code, lowEstMinutes: 60);

			var sequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			sequence.BMR_GG_ReleaseGroup = releaseGroup2.PK;

			var sequenceItem1 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem1.BMI_BMR_Sequence = sequence.PK;
			sequenceItem1.BMI_FH_ProcessHeader = workflow2.PK;
			sequenceItem1.BMI_Position = 3;

			var sequenceItem2 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem2.BMI_BMR_Sequence = sequence.PK;
			sequenceItem2.BMI_FH_ProcessHeader = workflow3.PK;
			sequenceItem2.BMI_Position = 2;

			var sequenceItem3 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem3.BMI_BMR_Sequence = sequence.PK;
			sequenceItem3.BMI_FH_ProcessHeader = workflow5.PK;
			sequenceItem3.BMI_Position = 1;

			Factory.Save();

			RunReleaseGate(system);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();

			AssertEquals(buffer.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow4.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow5.FH_FC_CurrentComponent);

			var expectedList = new List<string> { "workflow2", "workflow5", "workflow3", "workflow4", "workflow1" };
			var actualList = (new List<ProcessHeader> { workflow1, workflow2, workflow3, workflow4, workflow5 })
				.OrderBy(w => w.FH_SystemLastEditTimeUtc).Select(w => w.Name).ToList();

			AssertEquals("Workflow last edit times should have been set according to overall nudge",
				string.Join(",", expectedList),
				string.Join(",", actualList));
		}

		public void TestChildrenInheritHighestParentNudge()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 0);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", sequence: 1);
			LinkComponents(bucket, buffer, sequence: 0);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			//                                                                                                                     Nudge        EffNudge     SeqNudge
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: releaseGroup.PK, nudge: 1001); // 1001         1001         1000 - (2 - 1) * 0.0001 = 999.9999
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: releaseGroup.PK, nudge: 999);  // 999.9998     999          1000 - (3 - 1) * 0.0001 = 999.9998
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: releaseGroup.PK, nudge: 950);  // 999.9999     950           n/a                                     overall Nudge inherited from highest parent SeqNudge (W1)
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: releaseGroup.PK, nudge: 970);  // 1000         970           1000 - (1 - 1) * 0.0001 = 1000          higher than all parents SeqNudges
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5", releaseGroupPK: releaseGroup.PK, nudge: 900);  // 1000         900           1000 - (10 - 1) * 0.0001 = 999.9991     overall nudge inherited from highest parent SeqNudge (W4)

			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			workflow3.FH_FC_CurrentComponent = bucket.PK;
			workflow4.FH_FC_CurrentComponent = bucket.PK;
			workflow5.FH_FC_CurrentComponent = bucket.PK;

			var link1 = BMSTestHelper.MakeChildOfAndGetLink(workflow3, workflow1);
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			var link2 = BMSTestHelper.MakeChildOfAndGetLink(workflow3, workflow2);
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			var link3 = BMSTestHelper.MakeChildOfAndGetLink(workflow4, workflow3);
			link3.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			var link4 = BMSTestHelper.MakeChildOfAndGetLink(workflow5, workflow4);
			link4.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, lowEstMinutes: 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, resource.GS_Code, lowEstMinutes: 60);
			var task4 = BMSTestHelper.CreateTask(workflow4, resource.GS_Code, lowEstMinutes: 60);
			var task5 = BMSTestHelper.CreateTask(workflow5, resource.GS_Code, lowEstMinutes: 60);

			var sequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			sequence.BMR_GG_ReleaseGroup = releaseGroup.PK;

			var sequenceItem1 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem1.BMI_BMR_Sequence = sequence.PK;
			sequenceItem1.BMI_FH_ProcessHeader = workflow1.PK;
			sequenceItem1.BMI_Position = 2;

			var sequenceItem2 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem2.BMI_BMR_Sequence = sequence.PK;
			sequenceItem2.BMI_FH_ProcessHeader = workflow2.PK;
			sequenceItem2.BMI_Position = 3;

			var sequenceItem3 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem3.BMI_BMR_Sequence = sequence.PK;
			sequenceItem3.BMI_FH_ProcessHeader = workflow4.PK;
			sequenceItem3.BMI_Position = 1;

			var sequenceItem4 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem4.BMI_BMR_Sequence = sequence.PK;
			sequenceItem4.BMI_FH_ProcessHeader = workflow5.PK;
			sequenceItem4.BMI_Position = 10;

			Factory.Save();

			RunReleaseGate(system);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();

			AssertEquals(buffer.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow4.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow5.FH_FC_CurrentComponent);

			var expectedListAlt1 = new List<string> { "workflow1", "workflow4", "workflow5", "workflow3", "workflow2" };
			var expectedListAlt2 = new List<string> { "workflow1", "workflow5", "workflow4", "workflow3", "workflow2" };
			var actualList = (new List<ProcessHeader> { workflow1, workflow2, workflow3, workflow4, workflow5 })
				.OrderBy(w => w.FH_SystemLastEditTimeUtc).Select(w => w.Name).ToList();

			AssertContains("Workflow last edit times should have been set according to overall nudge",
				string.Join(",", actualList),
				string.Join(",", expectedListAlt1) + " OR " + string.Join(",", expectedListAlt2));
		}

		public void TestWorkflowsAreReleasedAccordingToEffectiveNudge_WhenReleaseSequencesModuleIsDisabled()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 0);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", sequence: 1);
			LinkComponents(bucket, buffer, sequence: 0);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: releaseGroup1.PK, nudge: 3);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: releaseGroup2.PK, nudge: 2);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: releaseGroup2.PK, nudge: 1);

			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			workflow3.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, lowEstMinutes: 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, resource.GS_Code, lowEstMinutes: 60);

			var sequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			sequence.BMR_GG_ReleaseGroup = releaseGroup2.PK;

			var sequenceItem1 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem1.BMI_BMR_Sequence = sequence.PK;
			sequenceItem1.BMI_FH_ProcessHeader = workflow1.PK;
			sequenceItem1.BMI_Position = 3;

			var sequenceItem2 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem2.BMI_BMR_Sequence = sequence.PK;
			sequenceItem2.BMI_FH_ProcessHeader = workflow2.PK;
			sequenceItem2.BMI_Position = 2;

			var sequenceItem3 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem3.BMI_BMR_Sequence = sequence.PK;
			sequenceItem3.BMI_FH_ProcessHeader = workflow3.PK;
			sequenceItem3.BMI_Position = 1;

			Factory.Save();

			RunReleaseGate(system);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();

			AssertEquals(buffer.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow3.FH_FC_CurrentComponent);

			var expectedList = new List<string> { "workflow1", "workflow2", "workflow3" };
			var actualList = (new List<ProcessHeader> { workflow1, workflow2, workflow3 })
				.OrderBy(w => w.FH_SystemLastEditTimeUtc).Select(w => w.Name).ToList();

			AssertEquals("Workflow last edit times should have been set according to overall nudge",
				string.Join(",", expectedList),
				string.Join(",", actualList));
		}

		public void TestIndividualSequenceNudges()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bucket = BMSTestHelper.CreateBucket(system, "bucket", sequence: 0);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", sequence: 1);
			LinkComponents(bucket, buffer, sequence: 0);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			//                                                                                                                     Nudge         EffNudge     SeqNudge
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: releaseGroup.PK, nudge: 1000); // 3000          1000         3000 - (1 - 1) * 0.0001 = 3000
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseGroupPK: releaseGroup.PK, nudge: 1000); // 2999.9999     1000         3000 - (2 - 1) * 0.0001 = 2999.9999
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseGroupPK: releaseGroup.PK, nudge: 1000); // 4999.9998     1000         5000 - (3 - 1) * 0.0001 = 4999.9998
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4", releaseGroupPK: releaseGroup.PK, nudge: 1000); // 4999.9997     1000         5000 - (4 - 1) * 0.0001 = 4999.9997

			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			workflow3.FH_FC_CurrentComponent = bucket.PK;
			workflow4.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, lowEstMinutes: 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, resource.GS_Code, lowEstMinutes: 60);
			var task4 = BMSTestHelper.CreateTask(workflow4, resource.GS_Code, lowEstMinutes: 60);

			var sequence1 = Factory.NewWithValidTestData<BMReleaseSequence>();
			sequence1.BMR_GG_ReleaseGroup = releaseGroup.PK;
			sequence1.BMR_SequenceNudge = 3000;

			var sequenceItem1 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem1.BMI_BMR_Sequence = sequence1.PK;
			sequenceItem1.BMI_FH_ProcessHeader = workflow1.PK;
			sequenceItem1.BMI_Position = 1;

			var sequenceItem2 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem2.BMI_BMR_Sequence = sequence1.PK;
			sequenceItem2.BMI_FH_ProcessHeader = workflow2.PK;
			sequenceItem2.BMI_Position = 2;

			var sequence2 = Factory.NewWithValidTestData<BMReleaseSequence>();
			sequence2.BMR_GG_ReleaseGroup = releaseGroup.PK;
			sequence2.BMR_SequenceNudge = 5000;

			var sequenceItem3 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem3.BMI_BMR_Sequence = sequence2.PK;
			sequenceItem3.BMI_FH_ProcessHeader = workflow3.PK;
			sequenceItem3.BMI_Position = 3;

			var sequenceItem4 = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			sequenceItem4.BMI_BMR_Sequence = sequence2.PK;
			sequenceItem4.BMI_FH_ProcessHeader = workflow4.PK;
			sequenceItem4.BMI_Position = 4;

			Factory.Save();

			RunReleaseGate(system);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();

			AssertEquals(buffer.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals(buffer.PK, workflow4.FH_FC_CurrentComponent);

			var expectedList = new List<string> { "workflow3", "workflow4", "workflow1", "workflow2" };
			var actualList = (new List<ProcessHeader> { workflow1, workflow2, workflow3, workflow4 })
				.OrderBy(w => w.FH_SystemLastEditTimeUtc).Select(w => w.Name).ToList();

			AssertEquals("Workflow last edit times should have been set according to overall nudge",
				string.Join(",", expectedList),
				string.Join(",", actualList));
		}

		#endregion

		#region

		public void TestEnsureBuffersForRequestsAreTheSameSpecifiedBuffer()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");
			var bucket = BMSTestHelper.CreateBucket(system);
			var componentLink1 = BMSTestHelper.LinkComponents(bucket, buffer1);
			var componentLink2 = BMSTestHelper.LinkComponents(bucket, buffer2);

			workflow.MoveToComponent(bucket);

			Factory.Save();

			var keeper = new ReleaseGateKeeper(buffer1, null, null);
			var request1 = new ReleaseGateRequest(workflow, componentLink1);
			var request2 = new ReleaseGateRequest(workflow, componentLink2);

			keeper.EnsureBuffersForRequestsAreTheSameSpecifiedBuffer_Exposed(workflow.PK, new[] { request1, request2 });

			AssertEquals("EnsureBuffersForRequestsAreTheSameSpecifiedBuffer", ErrorReporter.LastKeyReported);
			AssertMultilineASCIIEquals($@"I believe, the buffer for requests are always one and the same buffer passed to the constructor, but this reported proves this assumption is wrong.
Buffer for ReleaseGateKeeper: Buffer 1, PK={buffer1.PK}
Workflow PK = {workflow.PK}
Requests for the workflow:
FromComponent PK = {bucket.PK}, Buffer PK (ComponentTo PK) = {buffer1.PK}, Buffer = Buffer 1, ComponentLink PK = {componentLink1.PK}
FromComponent PK = {bucket.PK}, Buffer PK (ComponentTo PK) = {buffer2.PK}, Buffer = Buffer 2, ComponentLink PK = {componentLink2.PK}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Setting Dedicated Buffer

		[TestDate(2024, 11, 21)]
		public void TestReleaseWorkflowToBuffer_ShouldSetDedicatedBufferToNull()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen");
			Assert(staff.IsWorking(ZDate.Today));

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			((OrgHeader)jobHeader.Parent).OH_Code = "MAISEKNDORG";
			var workflowToBeReleased = BMSTestHelper.CreateWorkflow(jobHeader, "Heads will roll", config.Bucket);
			BMSTestHelper.CreateTask(workflowToBeReleased, staff.GS_Code, 10);
			workflowToBeReleased.FH_FC_DedicatedBuffer = config.Buffer.PK;

			Factory.Save();

			RunReleaseGate(config.System, failureLogService: ReleaseLogFailureServiceForTest);
			workflowToBeReleased.Reload();

			AssertSamePK("Workflow should be released", config.Buffer, workflowToBeReleased.CurrentComponent);
			AssertEquals("Should set dedicated buffer to null after release", ZGuid.Empty, workflowToBeReleased.FH_FC_DedicatedBuffer);
		}

		#endregion

		#region Implementation

		public static void RunReleaseGate(
			BMSystem system,
			ILogger logger = null,
			ReleaseGateFailureLogService_ForTest failureLogService = null,
			ReleaseGateTestCoordinator coordinator = null)
		{
			using (DisableAsyncBehaviour())
			using (Env.Instance.TemporaryServiceTaskContext(ReleaseGateRunnerServiceTask.Code, canRunInAnyBranch: true))
			using (DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider())
			{
				var releaseGateLogger = new ReleaseGateLoggerWithFailureServices(failureLogService ?? new ReleaseGateFailureLogService_ForTest());
				var director = new ReleaseGateDirector_ForTest(system, logger ?? new BufferManagementLogger(), releaseGateLogger, coordinator ?? new ReleaseGateTestCoordinator());
				director.Process();
			}
		}

		protected BMSystem system;

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "DEHAM";
			branch.GB_RN_NKCountryCode = "DE";

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: branch.PK.ToGuid());

			Factory.Save();
			LocalSetUp();
		}

		protected abstract void LocalSetUp();

		protected virtual bool UsingSimpleQuery => false;

		#endregion
	}
}
