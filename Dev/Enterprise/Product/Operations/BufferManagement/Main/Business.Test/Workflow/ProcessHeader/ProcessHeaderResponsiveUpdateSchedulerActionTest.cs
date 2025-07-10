using System;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderResponsiveUpdateSchedulerActionTest : TestCaseWithFactory
	{
		[TestDate(2024, 01, 18)]
		public void TestDedicatedBufferUpdate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket 3");
			BMSTestHelper.LinkComponents(bucket1, bucket2);
			BMSTestHelper.LinkComponents(bucket2, bucket3);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 1", currentComponent: bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 2", currentComponent: bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 3 in another bucket", currentComponent: bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 4 inactive", currentComponent: bucket1);
			workflow4.FH_IsActive = false;
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 5 closed", currentComponent: bucket1, taskStatus: "CLS");
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 6 closed with open prerequisites", currentComponent: bucket1, taskStatus: "CLS");
			BMSTestHelper.CreateDependencyLink(workflow1, workflow6);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 7 blocked", currentComponent: bucket1);
			BMSTestHelper.CreateDependencyLink(workflow1, workflow7);

			Factory.Save();

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow3.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow4.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow5.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow6.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow7.FH_SystemLastEditTimeUtc);

			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow3.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow4.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow5.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow6.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Blocked, workflow7.FH_Status);

			TestDateAttribute.AddHours(1);

			var action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			var logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, bucket1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, ZDateTime.UtcNow));
			AssertContains($"Performing BUF responsive action for component bucket 1 (PK = {bucket1.PK}):", logger.ToString());
			AssertContains($"Debug - Performed BUF responsive action on workflow 1 (PK = {workflow1.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed BUF responsive action on workflow 2 (PK = {workflow2.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed BUF responsive action on workflow 7 blocked (PK = {workflow7.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains("Debug - Performed BUF responsive action on 3 workflows in a batch (3 workflows in total in 1 batch)", logger.ToString());

			AssertNotContains($"Debug - Performed BUF responsive action on workflow 3 in another component", logger.ToString());
			AssertNotContains($"Debug - Performed BUF responsive action on workflow 4 inactive", logger.ToString());
			AssertNotContains($"Debug - Performed BUF responsive action on workflow 5 closed", logger.ToString());
			AssertNotContains($"Debug - Performed BUF responsive action on workflow 6 closed with open prerequisites", logger.ToString());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Should mark workflow1 for dedicated buffer update as it's in the target bucket", ZDateTime.UtcNow, workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals("Should mark workflow2 for dedicated buffer update as it's in the target bucket", ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow3 for dedicated buffer update as it's not in the target bucket", ZDateTime.UtcNow, workflow3.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow4 for dedicated buffer update as it's inactive", ZDateTime.UtcNow, workflow4.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow5 for dedicated buffer update as it's closed", ZDateTime.UtcNow, workflow5.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow6 for dedicated buffer update as it's closed with open prerequisites", ZDateTime.UtcNow, workflow6.FH_SystemLastEditTimeUtc);
			AssertEquals("Should mark workflow7 for dedicated buffer update as it's in the target bucket and not closed (even if it's blocked)", ZDateTime.UtcNow, workflow7.FH_SystemLastEditTimeUtc);

			TestDateAttribute.AddHours(1);
			logger.Clear();

			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, bucket2.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, ZDateTime.UtcNow));
			AssertContains($"Performing BUF responsive action for component bucket 2 (PK = {bucket2.PK}):", logger.ToString());
			AssertContains($"Debug - Performed BUF responsive action on workflow 3 in another bucket (PK = {workflow3.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains("Debug - Performed BUF responsive action on 1 workflow in a batch (1 workflow in total in 1 batch)", logger.ToString());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertNotEquals("Should not mark workflow1 for dedicated buffer update as it's not in the target bucket", ZDateTime.UtcNow, workflow1.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow2 for dedicated buffer update as it's not in the target bucket", ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);
			AssertEquals("Should mark workflow3 for dedicated buffer update as it's in the target bucket", ZDateTime.UtcNow, workflow3.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow4 for dedicated buffer update as it's not in the target bucket (and inactive)", ZDateTime.UtcNow, workflow4.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow5 for dedicated buffer update as it's not in the target bucket (and closed)", ZDateTime.UtcNow, workflow5.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow6 for dedicated buffer update as it's not in the target bucket (and closed with open prerequisites)", ZDateTime.UtcNow, workflow6.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow7 for dedicated buffer update as it's not in the target bucket", ZDateTime.UtcNow, workflow7.FH_SystemLastEditTimeUtc);
		}

		[TestDate(2025, 02, 19)]
		public void TestEffectiveBranchDepartmentUpdate_ForBuffer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer 2");

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			buffer1.FC_GB_AgingBranch = branch1.PK;
			buffer1.FC_GE_AgingDepartment = department1.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 1", currentComponent: buffer1);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 2", currentComponent: buffer1);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 3 in another component", currentComponent: buffer2);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 4 inactive", currentComponent: buffer1);
			workflow4.FH_IsActive = false;
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 5 closed", currentComponent: buffer1, taskStatus: "CLS");
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 6 closed with open prerequisites", currentComponent: buffer1, taskStatus: "CLS");
			BMSTestHelper.CreateDependencyLink(workflow1, workflow6);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 7 blocked", currentComponent: buffer1);
			BMSTestHelper.CreateDependencyLink(workflow1, workflow7);

			Factory.Save();

			// components are cached in Uber Factory cache - we need to ensure the most up-to-date component data is reloaded from db
			var sqlText = $@"UPDATE BMComponent
SET FC_GB_AgingBranch = '{branch2.PK}',
FC_GE_AgingDepartment = '{department2.PK}',
FC_SystemLastEditTimeUtc = getdate(),
FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer1.PK}'";
			TestConnection.ExecuteNonQuery(sqlText);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow3.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow4.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow5.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow6.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Blocked, workflow7.FH_Status);

			AssertEquals("Precondition: active workflow in buffer should have effective branch", branch1.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Precondition: active workflow in buffer should have effective department", department1.PK, workflow1.EffectiveDepartment.PK);
			AssertEquals("Precondition: active workflow in buffer should have effective branch", branch1.PK, workflow2.EffectiveBranch.PK);
			AssertEquals("Precondition: active workflow in buffer should have effective department", department1.PK, workflow2.EffectiveDepartment.PK);
			AssertEquals("Precondition: blocked workflow in buffer should have effective branch", branch1.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Precondition: blocked workflow in buffer should have effective department", department1.PK, workflow7.EffectiveDepartment.PK);

			AssertNull("Precondition: inactive workflow should have null effective branch", workflow4.EffectiveBranch);
			AssertNull("Precondition: inactive workflow should have null effective department", workflow4.EffectiveDepartment);
			AssertNull("Precondition: closed workflow should have null effective branch", workflow5.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow5.EffectiveDepartment);
			AssertNull("Precondition: closed workflow should have null effective branch", workflow6.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow6.EffectiveDepartment);

			TestDateAttribute.AddHours(1);

			var action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			var logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, buffer1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment, ZDateTime.UtcNow));
			AssertContains($"Performing EBD responsive action for component buffer 1 (PK = {buffer1.PK}):", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 1 (PK = {workflow1.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 2 (PK = {workflow2.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 4 inactive", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 7 blocked (PK = {workflow7.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains("Debug - Performed EBD responsive action on 4 workflows in a batch (4 workflows in total in 1 batch)", logger.ToString());

			AssertNotContains($"Debug - Performed EBD responsive action on workflow 3 in another component", logger.ToString());
			AssertNotContains($"Debug - Performed EBD responsive action on workflow 5 closed", logger.ToString());
			AssertNotContains($"Debug - Performed EBD responsive action on workflow 6 closed with open prerequisites", logger.ToString());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();

			AssertEquals("Should update effective branch on workflow1", branch2.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow1", department2.PK, workflow1.EffectiveDepartment.PK);
			AssertEquals("Should update effective branch on workflow2", branch2.PK, workflow2.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow2", department2.PK, workflow2.EffectiveDepartment.PK);
			AssertEquals("Should update effective branch on workflow7", branch2.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow7", department2.PK, workflow7.EffectiveDepartment.PK);

			AssertNull("Should keep effective branch null on workflow4 as it's inactive", workflow4.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow4 as it's inactive", workflow4.EffectiveDepartment);

			AssertNull("Should keep effective branch null on workflow5 because it's closed", workflow5.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow5 because it's closed", workflow5.EffectiveDepartment);
			AssertNull("Should keep effective branch null on workflow6 because it's closed", workflow6.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow6 because it's closed", workflow6.EffectiveDepartment);

			sqlText = $@"UPDATE BMComponent
SET FC_GB_AgingBranch = '{branch1.PK}',
FC_GE_AgingDepartment = '{department1.PK}',
FC_SystemLastEditTimeUtc = getdate(),
FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer1.PK}'";
			TestConnection.ExecuteNonQuery(sqlText);

			TestDateAttribute.AddHours(1);

			action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, buffer1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment, ZDateTime.UtcNow));

			workflow1.Reload();

			AssertEquals("Should update effective branch on workflow1 back to branch1", branch1.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow1 back to department1", department1.PK, workflow1.EffectiveDepartment.PK);
		}

		[TestDate(2025, 02, 19)]
		public void TestEffectiveBranchDepartmentUpdate_ForBucket()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			buffer.FC_GB_AgingBranch = branch1.PK;
			buffer.FC_GE_AgingDepartment = department1.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 1", currentComponent: bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 2", currentComponent: bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 3 in another component", currentComponent: bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 4 inactive", currentComponent: bucket1);
			workflow4.FH_IsActive = false;
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 5 closed", currentComponent: bucket1, taskStatus: "CLS");
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 6 closed with open prerequisites", currentComponent: bucket1, taskStatus: "CLS");
			BMSTestHelper.CreateDependencyLink(workflow1, workflow6);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 7 blocked", currentComponent: bucket1);
			BMSTestHelper.CreateDependencyLink(workflow1, workflow7);

			workflow1.FH_FC_DedicatedBuffer = buffer.PK;
			workflow2.FH_FC_DedicatedBuffer = buffer.PK;
			workflow3.FH_FC_DedicatedBuffer = buffer.PK;
			workflow4.FH_FC_DedicatedBuffer = buffer.PK;
			workflow5.FH_FC_DedicatedBuffer = buffer.PK;
			workflow6.FH_FC_DedicatedBuffer = buffer.PK;
			workflow7.FH_FC_DedicatedBuffer = buffer.PK;

			Factory.Save();

			// components are cached in Uber Factory cache - we need to ensure the most up-to-date component data is reloaded from db
			var sqlText = $@"UPDATE BMComponent
SET FC_GB_AgingBranch = '{branch2.PK}',
FC_GE_AgingDepartment = '{department2.PK}',
FC_SystemLastEditTimeUtc = getdate(),
FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer.PK}'";
			TestConnection.ExecuteNonQuery(sqlText);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow3.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow4.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow5.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow6.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Blocked, workflow7.FH_Status);

			AssertEquals("Precondition: active workflow should have effective branch from dedicated buffer", branch1.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Precondition: active workflow should have effective department from dedicated buffer", department1.PK, workflow1.EffectiveDepartment.PK);
			AssertEquals("Precondition: active workflow should have effective branch from dedicated buffer", branch1.PK, workflow2.EffectiveBranch.PK);
			AssertEquals("Precondition: active workflow should have effective department from dedicated buffer", department1.PK, workflow2.EffectiveDepartment.PK);
			AssertEquals("Precondition: blocked workflow should have effective branch from dedicated buffer", branch1.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Precondition: blocked workflow should have effective department from dedicated buffer", department1.PK, workflow7.EffectiveDepartment.PK);

			AssertNull("Precondition: inactive workflow should have null effective branch", workflow4.EffectiveBranch);
			AssertNull("Precondition: inactive workflow should have null effective department", workflow4.EffectiveDepartment);
			AssertNull("Precondition: closed workflow should have null effective branch", workflow5.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow5.EffectiveDepartment);
			AssertNull("Precondition: closed workflow should have null effective branch", workflow6.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow6.EffectiveDepartment);

			TestDateAttribute.AddHours(1);

			var action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			var logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, bucket1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment, ZDateTime.UtcNow));
			AssertContains($"Performing EBD responsive action for component bucket 1 (PK = {bucket1.PK}):", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 1 (PK = {workflow1.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 2 (PK = {workflow2.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 4 inactive (PK = {workflow4.PK}, Job = Dummy Business Object Default)\r\n", logger.ToString());
			AssertContains($"Debug - Performed EBD responsive action on workflow 7 blocked (PK = {workflow7.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains("Debug - Performed EBD responsive action on 4 workflows in a batch (4 workflows in total in 1 batch)", logger.ToString());

			AssertNotContains($"Debug - Performed EBD responsive action on workflow 3 in another component", logger.ToString());
			AssertNotContains($"Debug - Performed EBD responsive action on workflow 5 closed", logger.ToString());
			AssertNotContains($"Debug - Performed EBD responsive action on workflow 6 closed with open prerequisites", logger.ToString());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();

			AssertEquals("Should update effective branch on workflow1", branch2.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow1", department2.PK, workflow1.EffectiveDepartment.PK);
			AssertEquals("Should update effective branch on workflow2", branch2.PK, workflow2.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow2", department2.PK, workflow2.EffectiveDepartment.PK);
			AssertEquals("Should update effective branch on workflow7", branch2.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow7", department2.PK, workflow7.EffectiveDepartment.PK);

			AssertNull("Inactive workflow should remain null for effective branch", workflow4.EffectiveBranch);
			AssertNull("Inactive workflow should remain null for effective department", workflow4.EffectiveDepartment);
			AssertNull("Closed workflow should remain null for effective branch", workflow5.EffectiveBranch);
			AssertNull("Closed workflow should remain null for effective department", workflow5.EffectiveDepartment);
			AssertNull("Closed workflow should remain null for effective branch", workflow6.EffectiveBranch);
			AssertNull("Closed workflow should remain null for effective department", workflow6.EffectiveDepartment);

			sqlText = $@"UPDATE BMComponent
SET FC_GB_AgingBranch = '{branch1.PK}',
FC_GE_AgingDepartment = '{department1.PK}',
FC_SystemLastEditTimeUtc = getdate(),
FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer.PK}'";
			TestConnection.ExecuteNonQuery(sqlText);

			TestDateAttribute.AddHours(1);

			action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, bucket1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment, ZDateTime.UtcNow));

			workflow1.Reload();
			AssertEquals("Should update effective branch on workflow1 back to branch1", branch1.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow1 back to department1", department1.PK, workflow1.EffectiveDepartment.PK);
		}

		[TestDate(2025, 02, 19)]
		public void TestBothDedicatedBufferAndEffectiveBranchDepartmentUpdate_ForBuffer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer 2");

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			buffer1.FC_GB_AgingBranch = branch1.PK;
			buffer1.FC_GE_AgingDepartment = department1.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 1", currentComponent: buffer1);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 2", currentComponent: buffer1);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 3 in another component", currentComponent: buffer2);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 4 inactive", currentComponent: buffer1);
			workflow4.FH_IsActive = false;
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 5 closed", currentComponent: buffer1, taskStatus: "CLS");
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 6 closed with open prerequisites", currentComponent: buffer1, taskStatus: "CLS");
			BMSTestHelper.CreateDependencyLink(workflow1, workflow6);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 7 blocked", currentComponent: buffer1);
			BMSTestHelper.CreateDependencyLink(workflow1, workflow7);

			Factory.Save();

			buffer1.FC_GB_AgingBranch = branch2.PK;
			buffer1.FC_GE_AgingDepartment = department2.PK;

			Factory.Save();

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow3.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow4.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow5.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow6.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow7.FH_SystemLastEditTimeUtc);

			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow3.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow4.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow5.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow6.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Blocked, workflow7.FH_Status);

			AssertEquals("Precondition: active workflows in buffer should have effective branch", branch1.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Precondition: active workflows in buffer should have effective department", department1.PK, workflow1.EffectiveDepartment.PK);
			AssertEquals("Precondition: active workflows in buffer should have effective branch", branch1.PK, workflow2.EffectiveBranch.PK);
			AssertEquals("Precondition: active workflows in buffer should have effective department", department1.PK, workflow2.EffectiveDepartment.PK);

			AssertNull("Precondition: inactive workflow should have null effective branch", workflow4.EffectiveBranch);
			AssertNull("Precondition: inactive workflow should have null effective department", workflow4.EffectiveDepartment);

			AssertNull("Precondition: closed workflow should have null effective branch", workflow5.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow5.EffectiveDepartment);
			AssertNull("Precondition: closed workflow should have null effective branch", workflow6.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow6.EffectiveDepartment);

			AssertEquals("Precondition: blocked workflow should have effective branch", branch1.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Precondition: blocked workflow should have effective department", department1.PK, workflow7.EffectiveDepartment.PK);

			TestDateAttribute.AddHours(1);

			var action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			var logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, buffer1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment, ZDateTime.UtcNow));
			AssertContains($"Performing BBD responsive action for component buffer 1 (PK = {buffer1.PK}):", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 1 (PK = {workflow1.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 2 (PK = {workflow2.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 7 blocked (PK = {workflow7.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains("Debug - Performed BBD responsive action on 4 workflows in a batch (4 workflows in total in 1 batch)", logger.ToString());

			AssertNotContains($"Debug - Performed BBD responsive action on workflow 3 in another component", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 4 inactive (PK = {workflow4.PK}, Job = Dummy Business Object Default)", logger.ToString());
			AssertNotContains($"Debug - Performed BBD responsive action on workflow 5 closed", logger.ToString());
			AssertNotContains($"Debug - Performed BBD responsive action on workflow 6 closed with open prerequisites", logger.ToString());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Should mark workflow1 for dedicated buffer update as it's in the target component", ZDateTime.UtcNow, workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals("Should mark workflow2 for dedicated buffer update as it's in the target component", ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow3 for dedicated buffer update as it's not in the target component", ZDateTime.UtcNow, workflow3.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow4 for dedicated buffer update as it's inactive (even though it's logged as processed)", ZDateTime.UtcNow, workflow4.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow5 for dedicated buffer update as it's closed", ZDateTime.UtcNow, workflow5.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow6 for dedicated buffer update as it's closed with open prerequisites", ZDateTime.UtcNow, workflow6.FH_SystemLastEditTimeUtc);
			AssertEquals("Should mark workflow7 for dedicated buffer update as it's in the target component and not closed (even if it's blocked)", ZDateTime.UtcNow, workflow7.FH_SystemLastEditTimeUtc);

			AssertEquals("Should update effective branch on workflow1", branch2.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow1", department2.PK, workflow1.EffectiveDepartment.PK);
			AssertEquals("Should update effective branch on workflow2", branch2.PK, workflow2.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow2", department2.PK, workflow2.EffectiveDepartment.PK);

			AssertNull("Should keep effective branch null on workflow4 as it's inactive", workflow4.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow4 as it's inactive", workflow4.EffectiveDepartment);

			AssertEquals("Should update effective branch on workflow7", branch2.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow7", department2.PK, workflow7.EffectiveDepartment.PK);

			AssertNull("Should keep effective branch null on workflow5 because it's closed", workflow5.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow5 because it's closed", workflow5.EffectiveDepartment);
			AssertNull("Should keep effective branch null on workflow6 because it's closed (with open prerequisites)", workflow6.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow6 because it's closed (with open prerequisites)", workflow6.EffectiveDepartment);
		}

		[TestDate(2025, 02, 19)]
		public void TestBothDedicatedBufferAndEffectiveBranchDepartmentUpdate_ForBucket()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			buffer.FC_GB_AgingBranch = branch1.PK;
			buffer.FC_GE_AgingDepartment = department1.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 1", currentComponent: bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 2", currentComponent: bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 3 in another component", currentComponent: bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 4 inactive", currentComponent: bucket1);
			workflow4.FH_IsActive = false;
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 5 closed", currentComponent: bucket1, taskStatus: "CLS");
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 6 closed with open prerequisites", currentComponent: bucket1, taskStatus: "CLS");
			BMSTestHelper.CreateDependencyLink(workflow1, workflow6);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 7 blocked", currentComponent: bucket1);
			BMSTestHelper.CreateDependencyLink(workflow1, workflow7);

			workflow1.FH_FC_DedicatedBuffer = buffer.PK;
			workflow2.FH_FC_DedicatedBuffer = buffer.PK;
			workflow3.FH_FC_DedicatedBuffer = buffer.PK;
			workflow4.FH_FC_DedicatedBuffer = buffer.PK;
			workflow5.FH_FC_DedicatedBuffer = buffer.PK;
			workflow6.FH_FC_DedicatedBuffer = buffer.PK;
			workflow7.FH_FC_DedicatedBuffer = buffer.PK;

			Factory.Save();

			buffer.FC_GB_AgingBranch = branch2.PK;
			buffer.FC_GE_AgingDepartment = department2.PK;

			Factory.Save();

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow3.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow4.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow5.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow6.FH_SystemLastEditTimeUtc);
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow7.FH_SystemLastEditTimeUtc);

			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow3.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow4.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow5.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow6.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Blocked, workflow7.FH_Status);

			AssertNull("Precondition: closed workflow should have null effective branch", workflow5.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow5.EffectiveDepartment);
			AssertNull("Precondition: closed workflow should have null effective branch", workflow6.EffectiveBranch);
			AssertNull("Precondition: closed workflow should have null effective department", workflow6.EffectiveDepartment);

			AssertEquals("Precondition: blocked workflow should have effective branch", branch1.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Precondition: blocked workflow should have effective department", department1.PK, workflow7.EffectiveDepartment.PK);

			AssertNull("Precondition: inactive workflow should have null effective branch", workflow4.EffectiveBranch);
			AssertNull("Precondition: inactive workflow should have null effective department", workflow4.EffectiveDepartment);

			TestDateAttribute.AddHours(1);

			var action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			var logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, bucket1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment, ZDateTime.UtcNow));
			AssertContains($"Performing BBD responsive action for component bucket 1 (PK = {bucket1.PK}):", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 1 (PK = {workflow1.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 2 (PK = {workflow2.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 4 inactive (PK = {workflow4.PK}, Job = Dummy Business Object Default)", logger.ToString());
			AssertContains($"Debug - Performed BBD responsive action on workflow 7 blocked (PK = {workflow7.PK}, Job = Dummy Business Object Default", logger.ToString());
			AssertContains("Debug - Performed BBD responsive action on 4 workflows in a batch (4 workflows in total in 1 batch)", logger.ToString());

			AssertNotContains($"Debug - Performed BBD responsive action on workflow 3 in another component", logger.ToString());
			AssertNotContains($"Debug - Performed BBD responsive action on workflow 5 closed", logger.ToString());
			AssertNotContains($"Debug - Performed BBD responsive action on workflow 6 closed with open prerequisites", logger.ToString());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();
			workflow4.Reload();
			workflow5.Reload();
			workflow6.Reload();
			workflow7.Reload();
			AssertEquals("Should mark workflow1 for dedicated buffer update as it's in the target component", ZDateTime.UtcNow, workflow1.FH_SystemLastEditTimeUtc);
			AssertEquals("Should mark workflow2 for dedicated buffer update as it's in the target component", ZDateTime.UtcNow, workflow2.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow3 for dedicated buffer update as it's not in the target component", ZDateTime.UtcNow, workflow3.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow4 for dedicated buffer update as it's inactive", ZDateTime.UtcNow, workflow4.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow5 for dedicated buffer update as it's closed", ZDateTime.UtcNow, workflow5.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should not mark workflow6 for dedicated buffer update as it's closed with open prerequisites", ZDateTime.UtcNow, workflow6.FH_SystemLastEditTimeUtc);
			AssertEquals("Should mark workflow7 for dedicated buffer update as it's in the target component and not closed (even if it's blocked)", ZDateTime.UtcNow, workflow7.FH_SystemLastEditTimeUtc);

			AssertEquals("Should update effective branch on workflow1", branch2.PK, workflow1.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow1", department2.PK, workflow1.EffectiveDepartment.PK);
			AssertEquals("Should update effective branch on workflow2", branch2.PK, workflow2.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow2", department2.PK, workflow2.EffectiveDepartment.PK);

			AssertNull("Should keep effective branch null on workflow4 as it's inactive", workflow4.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow4 as it's inactive", workflow4.EffectiveDepartment);

			AssertEquals("Should update effective branch on workflow7", branch2.PK, workflow7.EffectiveBranch.PK);
			AssertEquals("Should update effective department on workflow7", department2.PK, workflow7.EffectiveDepartment.PK);

			AssertNull("Should keep effective branch null on workflow5 because it's closed", workflow5.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow5 because it's closed", workflow5.EffectiveDepartment);
			AssertNull("Should keep effective branch null on workflow6 because it's closed (with open prerequisites)", workflow6.EffectiveBranch);
			AssertNull("Should keep effective department null on workflow6 because it's closed (with open prerequisites)", workflow6.EffectiveDepartment);
		}

		[TestDate(2024, 01, 18)]
		public void TestShouldNotProcess_WhenDisabledInRegistry()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow 1", currentComponent: bucket1);

			Factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", ZDateTime.UtcNow, workflow.FH_SystemLastEditTimeUtc);

			TestDateAttribute.AddHours(1);

			var action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			var logger = new BufferManagementLogger();
			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, bucket1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, ZDateTime.UtcNow));
			AssertMultilineASCIIEquals($@"Performing BUF responsive action for component bucket 1 (PK = {bucket1.PK}):
Debug - Performed BUF responsive action on workflow 1 (PK = {workflow.PK}, Job = Dummy Business Object Default)
Debug - Performed BUF responsive action on 1 workflow in a batch (1 workflow in total in 1 batch)", logger.ToString());

			workflow.Reload();
			AssertEquals("Should update", ZDateTime.UtcNow, workflow.FH_SystemLastEditTimeUtc);

			logger.Clear();
			TestDateAttribute.AddHours(1);

			void AssertCancelled()
			{
				AssertEquals("Cancelled", action.Execute(new CancellationToken(), Factory, logger, bucket1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, ZDateTime.UtcNow));
				AssertMultilineASCIIEquals("Debug - Cancelled processing as disabled in the registry", logger.ToString());

				workflow.Reload();
				AssertNotEquals("Should not update", ZDateTime.UtcNow, workflow.FH_SystemLastEditTimeUtc);

				logger.Clear();
				TestDateAttribute.AddHours(1);
			}

			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertCancelled();

			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertCancelled();

			BMSTestHelper.DisableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertCancelled();

			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("Success", action.Execute(new CancellationToken(), Factory, logger, bucket1.PK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, ZDateTime.UtcNow));
			AssertMultilineASCIIEquals($@"Performing BUF responsive action for component bucket 1 (PK = {bucket1.PK}):
Debug - Performed BUF responsive action on workflow 1 (PK = {workflow.PK}, Job = Dummy Business Object Default)
Debug - Performed BUF responsive action on 1 workflow in a batch (1 workflow in total in 1 batch)", logger.ToString());

			workflow.Reload();
			AssertEquals("Should update", ZDateTime.UtcNow, workflow.FH_SystemLastEditTimeUtc);
		}

		public void TestShouldNotProcess_WhenComponentIsDeleted()
		{
			var absentComponentPK = Guid.NewGuid();

			var action = new ProcessHeaderResponsiveUpdateSchedulerAction();
			var logger = new BufferManagementLogger();
			AssertEquals("Cancelled", action.Execute(new CancellationToken(), Factory, logger, absentComponentPK, BMComponentSchema.Constants.Prefix, ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, ZDateTime.UtcNow));
			AssertMultilineASCIIEquals($@"Debug - Cancelled processing as component (PK = {absentComponentPK}) does not exist", logger.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
