using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	abstract class CapacityConstrainedResourceStatusServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<CapacityConstrainedResourceStatusServiceTask>
	{
		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, CapacityConstrainedResourceStatusServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, CapacityConstrainedResourceStatusServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", CapacityConstrainedResourceStatusServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", CapacityConstrainedResourceStatusServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capacity Constrained Resource Status.", log.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capacity Constrained Resource Status.", log.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShoulDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capacity Constrained Resource Status.", log.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capacity Constrained Resource Status.", log.ToString());
		}
		#endregion

		public void TestRun_ShouldNotHitTheDatabaseExcessively()
		{
			var bucket1 = BMSTestHelper.CreateBucket(system, "Plank!");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Shift and hold 1");
			var bucket3 = BMSTestHelper.CreateBucket(system, "Shift and hold 2");
			var bucket4 = BMSTestHelper.CreateBucket(system, "Shift and hold 3");

			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.CreateConstraint(buffer, offsetMinutes: 3840);

			BMSTestHelper.LinkComponents(bucket1, bucket2);
			BMSTestHelper.LinkComponents(bucket1, bucket3);
			BMSTestHelper.LinkComponents(bucket1, bucket4);

			BMSTestHelper.LinkComponents(bucket2, buffer);
			BMSTestHelper.LinkComponents(bucket3, buffer);
			BMSTestHelper.LinkComponents(bucket4, buffer);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			Factory.Save();

			var task = GetNewServiceTask();
			var logger = InitialiseTaskSchedule(task);

			using (Db.Connection.TrackExecutedCommands())
			{
				task.RunTask();

				var capacityCalculationSqlStatements = Db.Connection.ExecutedCommands.Where(s => s.Contains("CAPACITY CALCULATION")).ToArray();
				var formattedSqlStatements = string.Join(System.Environment.NewLine + new string('-', 100) + System.Environment.NewLine, capacityCalculationSqlStatements);
				var message = string.Format("Expected capacity calculation to be executed once only for all resources together, per feeding component, and yet... Those statements were:{0}{0}{1}", System.Environment.NewLine, formattedSqlStatements);

				var expectedHits = UsingSimpleQuery ? 6 : 3;
				AssertEquals(message, expectedHits, capacityCalculationSqlStatements.Length);

				AssertNotNull(capacityCalculationSqlStatements.SingleOrDefault(s => s.Contains($"@BMComponentPK: '{bucket2.PK}'")));
				AssertNotNull(capacityCalculationSqlStatements.SingleOrDefault(s => s.Contains($"@BMComponentPK: '{bucket3.PK}'")));
				AssertNotNull(capacityCalculationSqlStatements.SingleOrDefault(s => s.Contains($"@BMComponentPK: '{bucket4.PK}'")));
			}
		}

		public void TestRun_ShouldIgnoreBuffersWithNoConstraintSubComponent()
		{
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BIL", "Bilbo Baggins");

			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "me@you.com";
			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);
			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1");
			buffer1.FC_BufferTimeCapacityConstraintThresholdMultiple = 2;
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");

			var subConstraint = BMSTestHelper.CreateConstraint(buffer1);
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer2);

			BMSTestHelper.LinkComponents(bucket, buffer1);
			BMSTestHelper.LinkComponents(bucket, buffer2);

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			var workflow2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, buffer1.FC_BufferTimespanInMinutes * 2);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, buffer1.FC_BufferTimespanInMinutes * 2);

			Factory.Save();

			var task = GetNewServiceTask();
			var logger = InitialiseTaskSchedule(task);
			task.RunTask();

			var query = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, buffer1.PK);
			query.AddToFilter(new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, buffer2.PK), JoinCondition.Or);
			var resourceLinks = new BusinessObjectFactory().Load<BMComponentResourceLink>(query);
			AssertEquals("Should have created one link per resource for the component which has a sub-constraint", 2, resourceLinks.Length);
			AssertEquals(buffer1.PK, resourceLinks[0].FD_FC_Component);
			AssertEquals(buffer1.PK, resourceLinks[1].FD_FC_Component);

			AssertMultilineASCIIEquals("", @"Information|The following resources were detected as being capacity constrained for the Release Gate into buffer buffer1. The threshold to being a CCR in this buffer is 192 hours. Their total time in the Release Gate is listed next to their names below.

Bilbo Baggins: 288 hours
Frodo Baggins: 288 hours", logger.ToString());
		}

		[TestDate(2013, 10, 25, 9, 0, 0)]
		public void TestRun_ShouldOnlyCreateOneEmailPerBuffer()
		{
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BIL", "Bilbo Baggins");

			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "me@you.com";
			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);
			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1");
			buffer1.FC_BufferTimeCapacityConstraintThresholdMultiple = 2;
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");

			var subConstraint1 = BMSTestHelper.CreateConstraint(buffer1);
			var subConstraint2 = BMSTestHelper.CreateConstraint(buffer2);

			BMSTestHelper.LinkComponents(bucket, buffer1);
			BMSTestHelper.LinkComponents(bucket, buffer2);

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			var workflow2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, buffer1.FC_BufferTimespanInMinutes * 2);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, buffer1.FC_BufferTimespanInMinutes * 2);

			Factory.Save();

			var task = GetNewServiceTask();
			var logger = InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals("Should not have created any emails - resources are not persistently overloaded yet", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var now = ZDateTime.UtcNow;
			var query = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, buffer1.PK);
			var resourceLinks = new BusinessObjectFactory().Load<BMComponentResourceLink>(query);
			AssertEquals(2, resourceLinks.Length);

			AssertEquals(now, resourceLinks[0].FD_CapacityConstraintDetectedUtc);
			AssertEquals(now, resourceLinks[1].FD_CapacityConstraintDetectedUtc);
			AssertEquals("Should be detected as CCR candidate, but not marked as persistently overloaded yet", false, resourceLinks[0].FD_IsPersistentlyOverloaded);
			AssertEquals("Should be detected as CCR candidate, but not marked as persistently overloaded yet", false, resourceLinks[1].FD_IsPersistentlyOverloaded);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(buffer1.FC_BufferTimespanInMinutes);
			task.RunTask();

			resourceLinks = new BusinessObjectFactory().Load<BMComponentResourceLink>(query);
			AssertEquals(2, resourceLinks.Length);

			AssertEquals(now, resourceLinks[0].FD_CapacityConstraintDetectedUtc);
			AssertEquals(now, resourceLinks[1].FD_CapacityConstraintDetectedUtc);
			AssertEquals("Enough time has passed to be classified as persistently overloaded", true, resourceLinks[0].FD_IsPersistentlyOverloaded);
			AssertEquals("Enough time has passed to be classified as persistently overloaded", true, resourceLinks[1].FD_IsPersistentlyOverloaded);

			AssertEquals("Should have created one email per component", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			var email1 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Mail recipient", 1, email1.Recipients.Count);
			AssertEquals("me@you.com", email1.Recipients[0].Email);
			AssertEquals("Detected Capacity Constrained Resources for buffer1", email1.Subject);
			AssertMultilineASCIIEquals("Email body", @"The following resources have been detected as being persistently overloaded in buffer buffer1:
Bilbo Baggins
Frodo Baggins

You will need to mark the resources as designated constrained resources if you wish to use Constrained Mode logic for their capacity calculation. To do this, open any Visual Board which includes a channel for the resource, and right-click their channel heading.

Their Release Group will also need to be in Constrained Mode. You can switch a group to Constrained Mode by right-clicking a buffer section name heading on a Visual Board for that Release Group.
", email1.Body);

			var email2 = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("Mail recipient", 1, email2.Recipients.Count);
			AssertEquals("me@you.com", email2.Recipients[0].Email);
			AssertEquals("Detected Capacity Constrained Resources for buffer2", email2.Subject);
			AssertMultilineASCIIEquals("Email body", @"The following resources have been detected as being persistently overloaded in buffer buffer2:
Bilbo Baggins
Frodo Baggins

You will need to mark the resources as designated constrained resources if you wish to use Constrained Mode logic for their capacity calculation. To do this, open any Visual Board which includes a channel for the resource, and right-click their channel heading.

Their Release Group will also need to be in Constrained Mode. You can switch a group to Constrained Mode by right-clicking a buffer section name heading on a Visual Board for that Release Group.
", email2.Body);

			AssertMultilineASCIIEquals("Log message",
@"
Information|The following resources were detected as being capacity constrained for the Release Gate into buffer buffer1. The threshold to being a CCR in this buffer is 192 hours. Their total time in the Release Gate is listed next to their names below.

Bilbo Baggins: 288 hours
Frodo Baggins: 288 hours
Information|The following resources were detected as being capacity constrained for the Release Gate into buffer buffer2. The threshold to being a CCR in this buffer is 96 hours. Their total time in the Release Gate is listed next to their names below.

Bilbo Baggins: 288 hours
Frodo Baggins: 288 hours

Information|The following resources have been detected as being persistently overloaded in buffer buffer1:
Bilbo Baggins
Frodo Baggins
Information|The following resources have been detected as being persistently overloaded in buffer buffer2:
Bilbo Baggins
Frodo Baggins
", logger.ToString());
		}

		[TestDate(2013, 10, 25, 9, 0, 0)]
		public void TestRun()
		{
			var resource1_nonConstraint = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BIL", "Bilbo Baggins");
			var resource2_constraint = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");

			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "me@you.com";
			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);
			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());

			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3");

			var buffer = BMSTestHelper.CreateBuffer(system, "Mai Buffer");
			buffer.FC_BufferTimeCapacityConstraintThresholdMultiple = 2;

			var subConstraint = BMSTestHelper.CreateConstraint(buffer);

			var link1 = BMSTestHelper.LinkComponents(bucket1, buffer);
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer);
			var link3 = BMSTestHelper.LinkComponents(bucket3, buffer, isReleaseGate: false);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", bucket2);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", bucket3);

			var task1_1 = BMSTestHelper.CreateTask(workflow1, resource1_nonConstraint.GS_Code, lowEstMinutes: 10);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, resource2_constraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes);
			var task1_3 = BMSTestHelper.CreateTask(workflow1, resource1_nonConstraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes * 4, taskStatus: ProcessTaskStatusCodeList.Codes.Closed); // Really big task, but it's closed, so doesn't count.

			var task2_1 = BMSTestHelper.CreateTask(workflow2, resource1_nonConstraint.GS_Code, lowEstMinutes: 10);
			var task2_2 = BMSTestHelper.CreateTask(workflow2, resource2_constraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes);

			var task3_1 = BMSTestHelper.CreateTask(workflow3, resource1_nonConstraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes * 4);

			Factory.Save();

			var serviceTask = GetNewServiceTask();
			BMSRegistry.Instance.AllowAutomaticUnmarkingOfCCRs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var logger = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var links = Factory.Load<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, new[] { resource1_nonConstraint.GS_Code, resource2_constraint.GS_Code }));
			AssertEquals("Should create one link for the constrained resource", 1, links.Length);

			var resourceComponentLink = links[0];
			AssertEquals("Component-resource link code", resource2_constraint.GS_Code, resourceComponentLink.FD_GS_NKResource);
			AssertEquals("The detection time should be set", new ZDateTime(2013, 10, 25, 9, 0, 0), resourceComponentLink.FD_CapacityConstraintDetectedUtc);
			AssertEquals("Should not be marked as a constraint - a human has to do that", false, resourceComponentLink.FD_IsCapacityConstrained);
			AssertEquals("We don't want to limit resource capacity!", (byte)100, resourceComponentLink.FD_CapacityLimitPercent);

			// Run the task again later - should not change the detection date
			TestDateAttribute.Date = new DateTime(2013, 10, 26, 9, 0, 0);

			serviceTask.RunTask();

			links = Factory.Load<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, new[] { resource1_nonConstraint.GS_Code, resource2_constraint.GS_Code }));
			AssertEquals("Should still be just one link for the constrained resource", 1, links.Length);
			resourceComponentLink = links[0];

			AssertEquals("The detection time should be the same - the resource is still a constraint", new ZDateTime(2013, 10, 25, 9, 0, 0), resourceComponentLink.FD_CapacityConstraintDetectedUtc);

			// Setting the component as a constraint manually, but closing the tasks that caused it to be considered a constraint.
			resourceComponentLink.FD_IsCapacityConstrained = true;
			task1_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			BufferCapacityCache.Clear();

			serviceTask.RunTask();

			links = Factory.Load<BMComponentResourceLink>(new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, new[] { resource1_nonConstraint.GS_Code, resource2_constraint.GS_Code }));
			AssertEquals("Should still be just one link for the constrained resource", 1, links.Length);
			resourceComponentLink = links[0];

			AssertEquals("Should reset constraint detection time", ZDateTime.Empty, resourceComponentLink.FD_CapacityConstraintDetectedUtc);
			AssertEquals("Should mark as non-constrained", false, resourceComponentLink.FD_IsCapacityConstrained);

			AssertMultilineASCIIEquals("Service task log",
@"
Information|The following resources were detected as being capacity constrained for the Release Gate into buffer Mai Buffer. The threshold to being a CCR in this buffer is 192 hours. Their total time in the Release Gate is listed next to their names below.

Frodo Baggins: 288 hours

Information|The following resources were detected as no longer being capacity constrained for the Release Gate into buffer Mai Buffer. The threshold to being a CCR in this buffer is 192 hours. Their total time in the Release Gate is listed next to their names below.

Frodo Baggins: 144 hours
", logger.ToString());

			AssertEquals("Emails should only be created for persistently overloaded resources", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2017, 01, 30, 9, 0, 0)]
		public void TestAutoUnmarkingOfCCRs_WhenDisabled_CCRsAreNotAutomaticallyUnmarked()
		{
			var testConfig = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var componentLink = testConfig.Buffer.GetOrCreateResourceLink(testConfig.CCR.GS_Code);
			Assert("Precondition: this registry item should be 'false' by default", !BMSRegistry.Instance.AllowAutomaticUnmarkingOfCCRs.Value);
			AssertEquals("This component link should be capacity constrained and yet...", true, componentLink.FD_IsCapacityConstrained);

			var task = GetNewServiceTask();
			Factory.Save();

			var logger = InitialiseTaskSchedule(task);
			task.RunTask();
			AssertEquals("Running the service task should not unmark the CCR and yet...", true, componentLink.FD_IsCapacityConstrained);

			BMSRegistry.Instance.AllowAutomaticUnmarkingOfCCRs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			task.RunTask();

			AssertEquals("Running the service task should unmark the CCR", false, componentLink.FD_IsCapacityConstrained);
		}

		public void TestRun_BufferWithNoIncomingLinks_ShouldNotThrowException()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Frodo Baggins";
			resource.GS_Code = "FRO";

			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.CreateConstraint(buffer, offsetMinutes: 3840);

			Factory.Save();

			var task = GetNewServiceTask();
			var logger = InitialiseTaskSchedule(task);
			AssertNoExceptionThrown(() => task.RunTask());
		}

		public void TestRun_WithSubBuffer_ShouldNotThrowException()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_FullName = "Frodo Baggins";
			resource.GS_Code = "FRO";

			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer);
			var link1 = BMSTestHelper.LinkComponents(bucket1, buffer);

			Factory.Save();

			var task = GetNewServiceTask();
			var logger = InitialiseTaskSchedule(task);
			AssertNoExceptionThrown(() => task.RunTask());
		}

		[TestDate(2013, 10, 25, 9, 0, 0)]
		public void TestDisablingAutounmarkingOfCCRs_DoesNotAffectPersistentlyOverloaded()
		{
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BIL", "Bilbo Baggins");

			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "me@you.com";
			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);
			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1");
			buffer1.FC_BufferTimeCapacityConstraintThresholdMultiple = 2;

			var subConstraint1 = BMSTestHelper.CreateConstraint(buffer1);

			BMSTestHelper.LinkComponents(bucket, buffer1);

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			var workflow2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow2.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, buffer1.FC_BufferTimespanInMinutes * 2);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, buffer1.FC_BufferTimespanInMinutes * 2);

			Factory.Save();

			Assert("Precondition: this registry item should be 'false' by default", !BMSRegistry.Instance.AllowAutomaticUnmarkingOfCCRs.Value);

			var task = GetNewServiceTask();
			var logger = InitialiseTaskSchedule(task);
			task.RunTask();

			var now = ZDateTime.UtcNow;
			var query = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, buffer1.PK);
			var resourceLinks = new BusinessObjectFactory().Load<BMComponentResourceLink>(query);
			AssertEquals(2, resourceLinks.Length);

			AssertEquals(now, resourceLinks[0].FD_CapacityConstraintDetectedUtc);
			AssertEquals(now, resourceLinks[1].FD_CapacityConstraintDetectedUtc);
			AssertEquals("Should be detected as CCR candidate, but not marked as persistently overloaded yet", false, resourceLinks[0].FD_IsPersistentlyOverloaded);
			AssertEquals("Should be detected as CCR candidate, but not marked as persistently overloaded yet", false, resourceLinks[1].FD_IsPersistentlyOverloaded);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(buffer1.FC_BufferTimespanInMinutes);
			task.RunTask();

			resourceLinks = new BusinessObjectFactory().Load<BMComponentResourceLink>(query);
			AssertEquals(2, resourceLinks.Length);

			AssertEquals(now, resourceLinks[0].FD_CapacityConstraintDetectedUtc);
			AssertEquals(now, resourceLinks[1].FD_CapacityConstraintDetectedUtc);
			AssertEquals("Enough time has passed to be classified as persistently overloaded", true, resourceLinks[0].FD_IsPersistentlyOverloaded);
			AssertEquals("Enough time has passed to be classified as persistently overloaded", true, resourceLinks[1].FD_IsPersistentlyOverloaded);

			task1.P9_Status = "CLS";
			task2.P9_Status = "CLS";

			Factory.Save();

			BufferCapacityCache.Clear();

			task.RunTask();

			resourceLinks = new BusinessObjectFactory().Load<BMComponentResourceLink>(query);
			AssertEquals(2, resourceLinks.Length);

			AssertEquals(ZDateTime.Empty, resourceLinks[0].FD_CapacityConstraintDetectedUtc);
			AssertEquals(ZDateTime.Empty, resourceLinks[1].FD_CapacityConstraintDetectedUtc);
			AssertEquals("Shouldn't be marked as persistently overloaded anymore", false, resourceLinks[0].FD_IsPersistentlyOverloaded);
			AssertEquals("Shouldn't be marked as persistently overloaded anymore", false, resourceLinks[1].FD_IsPersistentlyOverloaded);
			AssertMultilineASCIIEquals("Service task log",
@"
Information|The following resources were detected as being capacity constrained for the Release Gate into buffer buffer1. The threshold to being a CCR in this buffer is 192 hours. Their total time in the Release Gate is listed next to their names below.

Bilbo Baggins: 288 hours
Frodo Baggins: 288 hours

Information|The following resources have been detected as being persistently overloaded in buffer buffer1:
Bilbo Baggins
Frodo Baggins

Information|The following resources were detected as no longer being capacity constrained for the Release Gate into buffer buffer1. The threshold to being a CCR in this buffer is 192 hours. Their total time in the Release Gate is listed next to their names below.

Bilbo Baggins: 0 hours
Frodo Baggins: 0 hours
", logger.ToString());
		}

		[TestDate(2017, 01, 30, 9, 0, 0)]
		public void TestRun_WhenCapacityCalculationDisabled_ShouldDoNothing()
		{
			BMSRegistry.Instance.AllowAutomaticUnmarkingOfCCRs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testConfig = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var componentLink = testConfig.Buffer.GetOrCreateResourceLink(testConfig.CCR.GS_Code);
			AssertEquals("This component link should be capacity constrained and yet...", true, componentLink.FD_IsCapacityConstrained);

			var task = GetNewServiceTask();
			Factory.Save();

			var logger = InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals("Running the service task should not unmark the CCR because it's not meant to be running at all.", true, componentLink.FD_IsCapacityConstrained);
			AssertMultilineASCIIEquals("Debug|Service task not run because the [Disable Capacity Calculations] registry item is enabled.", logger.ToString());
		}

		#region Hosted Service Requirements

		public void TestServiceTask_WhenCapacityCalculationDisabled_ShouldNotMeetHostedServiceRequirement()
		{
			AssertEquals("Should be enabled by default if buffer management is enabled.", string.Empty, CapacityConstrainedResourceStatusServiceTask.CheckCapacityCalculationsNotDisabled());

			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations' requires a value other than 'True'.", CapacityConstrainedResourceStatusServiceTask.CheckCapacityCalculationsNotDisabled());
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Environment Errors

		public void TestRun_BufferToBuffer_WhenResourceHasNoHomeBranchSet_ShouldNotReportError()
		{
			var buffer1 = BMSTestHelper.CreateBuffer(system, "First buffer");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Second buffer");
			BMSTestHelper.CreateConstraint(buffer2, offsetMinutes: 3840);
			BMSTestHelper.LinkComponents(buffer1, buffer2);

			var resource = BMSTestHelper.CreateStaff(Factory, "CQD");
			resource.GS_GB_HomeBranch = ZGuid.Empty;

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", buffer1);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, buffer2.FC_BufferTimespanInMinutes * 2);

			Factory.Save();

			var task = GetNewServiceTask();
			InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals("Running this service task when a resource has no home branch shouldn't report errors.", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestRun_WhenServiceTaskCreatesGlbStaffStmALog_ShouldNotReportError()
		{
			var buffer1 = BMSTestHelper.CreateBuffer(system, "First buffer");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Second buffer");
			BMSTestHelper.CreateConstraint(buffer2, offsetMinutes: 3840);
			BMSTestHelper.LinkComponents(buffer1, buffer2);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			AssertNotEquals("We're testing something different than the above test, so this shouldn't be empty.", ZGuid.Empty, resource.GS_GB_HomeBranch);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", buffer1);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, buffer2.FC_BufferTimespanInMinutes * 2);

			Factory.Save();

			var task = (CapacityConstrainedResourceStatusServiceTask_ForTest)GetNewServiceTask();
			task.SetOnFactoryLoaded((sender, args) =>
			{
				foreach (var staff in args.NewObjects.OfType<GlbStaff>())
				{
					// This will cause the creation of a StmALog row on save, which is what was causing the error report.
					// In production this only happens under very specific circumstances, including being an EDI client hook.
					// There are too many variables that could change, so simulating it in this test to ensure that the requirement is protected.
					staff.GS_City = "Halifax";
				}
			});

			InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals("Running this service task when a resource has no home branch shouldn't report errors.", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestRun_ShouldSwitchContextsOncePerBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var buffer1 = BMSTestHelper.CreateBuffer(system, "First buffer");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Second buffer");
			var buffer3 = BMSTestHelper.CreateBuffer(system, "Third buffer");

			BMSTestHelper.CreateConstraint(buffer1, offsetMinutes: 3840);
			BMSTestHelper.CreateConstraint(buffer2, offsetMinutes: 3840);
			BMSTestHelper.CreateConstraint(buffer3, offsetMinutes: 3840);

			buffer1.FC_GB_AgingBranch = branch1.PK;
			buffer2.FC_GB_AgingBranch = branch2.PK;
			buffer3.FC_GB_AgingBranch = branch1.PK;

			Factory.Save();

			var factorySavedCount = 0;
			var contextSwitches = 0;
			var task = (CapacityConstrainedResourceStatusServiceTask_ForTest)GetNewServiceTask();

			task.SetOnFactorySaved((sender, args) => factorySavedCount++);

			InitialiseTaskSchedule(task);

			Env.Instance.UserContextChanging += (sender, args) => contextSwitches++;

			task.RunTask();

			// 2 to switch to branch, and 2 to undo switch.
			AssertEquals("We should group buffers by branch when processing them so we can minimize the number of switches. Without the grouping, this would be 6 (2 per buffer)", 4, contextSwitches);
			AssertEquals("We should not loop through the buffers more than once.", 3, factorySavedCount);
		}

		#endregion

		protected BMSystem system;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			BMSTestHelper.EnableBMSInRegistry();
			system = BMSTestHelper.CreateSystem(Factory, "ORG");
			LocalSetUp();
		}

		protected abstract void LocalSetUp();

		protected abstract bool UsingNewCalculator { get; }

		protected abstract bool UsingSimpleQuery { get; }

		protected override CapacityConstrainedResourceStatusServiceTask GetNewServiceTask()
		{
			return new CapacityConstrainedResourceStatusServiceTask_ForTest();
		}

		class CapacityConstrainedResourceStatusServiceTask_ForTest : CapacityConstrainedResourceStatusServiceTask
		{
			protected override void RunTaskCore(CancellationToken token)
			{
				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTaskCore(token);
				}
			}

			protected override BusinessObjectFactory GetNewFactory()
			{
				var factory = base.GetNewFactory();

				if (factoryLoadedAction != null)
				{
					factory.Loaded += factoryLoadedAction;
				}

				if (factorySavedAction != null)
				{
					factory.Saved += factorySavedAction;
				}

				return factory;
			}

			EventHandler<LoadedEventArgs> factoryLoadedAction;
			BusinessObjectFactory.SavedEventHandler factorySavedAction;

			public void SetOnFactoryLoaded(EventHandler<LoadedEventArgs> actionForFactoryLoaded)
			{
				factoryLoadedAction = actionForFactoryLoaded;
			}

			public void SetOnFactorySaved(BusinessObjectFactory.SavedEventHandler actionForFactorySaved)
			{
				factorySavedAction = actionForFactorySaved;
			}
		}
	}
}
