using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ProcessHeaderResponsiveActionPerformerTest : TestCaseWithFactory
	{
		public void TestResponsiveMarkForDedicatedBufferUpdate_BUFAction()
		{
			AssertMarksForDedicatedBufferUpdate(actionCode: ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer);
		}

		public void TestResponsiveMarkForDedicatedBufferUpdate_BBDAction()
		{
			AssertMarksForDedicatedBufferUpdate(actionCode: ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		void AssertMarksForDedicatedBufferUpdate(string actionCode)
		{
			BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			const int matchingWorkflowCount = 15;
			const int notMatchingWorkflowCount = 5;

			var matchingWorkflows = new List<ProcessHeader>();
			var notMatchingWorkflows = new List<ProcessHeader>();

			for (var i = 0; i < matchingWorkflowCount; i++)
			{
				var processHeader = BMSTestHelper.CreateWorkflow(Factory, "matching workflow");
				processHeader.FH_IsStandby = true;
				matchingWorkflows.Add(processHeader);
			}

			for (var i = 0; i < notMatchingWorkflowCount; i++)
			{
				var processHeader = BMSTestHelper.CreateWorkflow(Factory, "not matching workflow");
				processHeader.FH_IsStandby = false;
				notMatchingWorkflows.Add(processHeader);
			}

			Factory.Save();

			var latestLastEditTime = notMatchingWorkflows.Last().FH_SystemLastEditTimeUtc;

			Assert("Precondition", matchingWorkflows.Union(notMatchingWorkflows).All(w => w.FH_SystemLastEditTimeUtc <= latestLastEditTime));

			var logger = new BufferManagementLogger();
			var query = new ZQuery(ProcessHeaderSchema.FH_IsStandby, true);
			var performer = new ProcessHeaderResponsiveActionPerformer(logger, query, actionCode);
			performer.PerformActionOnMultipleWorkflows();

			var newFactory = new BusinessObjectFactory();
			var loadedMatchingWorkflows = matchingWorkflows.Select(w => newFactory.Load<ProcessHeader>(w.PK)).ToArray();
			var loadedNotMatchingWorkflows = notMatchingWorkflows.Select(w => newFactory.Load<ProcessHeader>(w.PK)).ToArray();

			Assert("Should mark the matching workflows for dedicated buffer update by changing their last edit time", loadedMatchingWorkflows.All(w => w.FH_SystemLastEditTimeUtc > latestLastEditTime));
			Assert("Should not update not matching workflows", loadedNotMatchingWorkflows.All(w => w.FH_SystemLastEditTimeUtc <= latestLastEditTime));

			AssertMultilineASCIIEquals($@"Debug - Performed {actionCode} responsive action on 10 workflows in a batch (10 workflows in total in 1 batch)
Debug - Performed {actionCode} responsive action on 5 workflows in a batch (15 workflows in total in 2 batches)
", logger.ToString());
		}

		public void TestResponsiveEffectiveBranchAndDepartmentUpdate_EBDAction()
		{
			AssertUpdatesEffectiveBranchAndDepartment(actionCode: ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment);
		}

		public void TestResponsiveEffectiveBranchAndDepartmentUpdate_BBDAction()
		{
			AssertUpdatesEffectiveBranchAndDepartment(actionCode: ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		public void AssertUpdatesEffectiveBranchAndDepartment(string actionCode)
		{
			BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			buffer.FC_GB_AgingBranch = branch1.PK;
			buffer.FC_GE_AgingDepartment = department1.PK;

			BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			const int matchingWorkflowCount = 15;
			const int notMatchingWorkflowCount = 5;

			var matchingWorkflows = new List<ProcessHeader>();
			var notMatchingWorkflows = new List<ProcessHeader>();

			for (var i = 0; i < matchingWorkflowCount; i++)
			{
				var processHeader = BMSTestHelper.CreateWorkflow(Factory, "matching workflow");
				processHeader.FH_IsStandby = true;
				processHeader.FH_FC_DedicatedBuffer = buffer.PK;

				var task = BMSTestHelper.CreateTask(processHeader);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				matchingWorkflows.Add(processHeader);
			}

			for (var i = 0; i < notMatchingWorkflowCount; i++)
			{
				var processHeader = BMSTestHelper.CreateWorkflow(Factory, "not matching workflow");
				processHeader.FH_IsStandby = false;
				processHeader.FH_FC_DedicatedBuffer = buffer.PK;

				var task = BMSTestHelper.CreateTask(processHeader);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				notMatchingWorkflows.Add(processHeader);
			}

			Factory.Save();

			Assert("Precondition", matchingWorkflows.Union(notMatchingWorkflows).All(w => w.FH_GB_EffectiveBranch == branch1.PK && w.FH_GE_EffectiveDepartment == department1.PK));

			// components are cached in Uber Factory cache - we need to ensure the most up-to-date component data is reloaded from db
			var sqlText = $@"UPDATE BMComponent
SET FC_GB_AgingBranch = '{branch2.PK}',
FC_GE_AgingDepartment = '{department2.PK}',
FC_SystemLastEditTimeUtc = getdate(),
FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer.PK}'";
			TestConnection.ExecuteNonQuery(sqlText);

			var logger = new BufferManagementLogger();
			var query = new ZQuery(ProcessHeaderSchema.FH_IsStandby, true);
			var performer = new ProcessHeaderResponsiveActionPerformer(logger, query, actionCode);
			performer.PerformActionOnMultipleWorkflows();

			Assert("Should update matching workflows", matchingWorkflows.All(w => w.FH_GB_EffectiveBranch == branch2.PK && w.FH_GE_EffectiveDepartment == department2.PK));
			Assert("Should not update not matching workflows", notMatchingWorkflows.All(w => w.FH_GB_EffectiveBranch == branch1.PK && w.FH_GE_EffectiveDepartment == department1.PK));

			AssertMultilineASCIIEquals($@"Debug - Performed {actionCode} responsive action on 10 workflows in a batch (10 workflows in total in 1 batch)
Debug - Performed {actionCode} responsive action on 5 workflows in a batch (15 workflows in total in 2 batches)
", logger.ToString());
		}

		#region Extended Logging

		public void TestLogIndividualWorkflowDetails()
		{
			BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			BMSRegistry.Instance.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			const int matchingWorkflowCount = 15;
			var matchingWorkflows = new List<ProcessHeader>();

			for (int i = 0; i < matchingWorkflowCount; i++)
			{
				var processHeader = BMSTestHelper.CreateWorkflow(jobHeader, $"Workflow{i}");
				processHeader.FH_IsStandby = true;
				matchingWorkflows.Add(processHeader);
			}

			Factory.Save();

			var logger = new BufferManagementLogger();
			var query = new ZQuery(ProcessHeaderSchema.FH_IsStandby, true);
			var performer = new ProcessHeaderResponsiveActionPerformer(logger, query, "BUF");
			performer.PerformActionOnMultipleWorkflows();

			var workflowPKs = matchingWorkflows.Select(w => w.PK).ToArray();

			AssertMultilineASCIIEquals($@"Debug - Performed BUF responsive action on Workflow0 (PK = {workflowPKs[0]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow1 (PK = {workflowPKs[1]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow2 (PK = {workflowPKs[2]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow3 (PK = {workflowPKs[3]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow4 (PK = {workflowPKs[4]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow5 (PK = {workflowPKs[5]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow6 (PK = {workflowPKs[6]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow7 (PK = {workflowPKs[7]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow8 (PK = {workflowPKs[8]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow9 (PK = {workflowPKs[9]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on 10 workflows in a batch (10 workflows in total in 1 batch)
Debug - Performed BUF responsive action on Workflow10 (PK = {workflowPKs[10]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow11 (PK = {workflowPKs[11]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow12 (PK = {workflowPKs[12]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow13 (PK = {workflowPKs[13]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on Workflow14 (PK = {workflowPKs[14]}, Job = Organization (XVBQP68SIYXQ))
Debug - Performed BUF responsive action on 5 workflows in a batch (15 workflows in total in 2 batches)
", logger.ToString());
		}

		#endregion

		#region Db Hits

		public void TestResponsiveMarkForDedicatedBufferUpdate_DbHits()
		{
			const int batchSize = 10;
			BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);

			const int batchCount = 3;
			const int workflowCount = batchCount * batchSize;

			var matchingWorkflows = new List<ProcessHeader>();

			for (int i = 0; i < workflowCount; i++)
			{
				var processHeader = BMSTestHelper.CreateWorkflow(Factory, "matching workflow");
				processHeader.FH_IsStandby = true;

				var task = BMSTestHelper.CreateTask(processHeader);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				matchingWorkflows.Add(processHeader);
			}

			Factory.Save();

			var latestLastEditTime = matchingWorkflows.Last().FH_SystemLastEditTimeUtc;

			Assert("Precondition", matchingWorkflows.All(w => w.FH_SystemLastEditTimeUtc <= latestLastEditTime));

			var logger = new BufferManagementLogger();
			var query = new ZQuery(ProcessHeaderSchema.FH_IsStandby, true);
			var performer = new ProcessHeaderResponsiveActionPerformer(logger, query, "BUF");

			var hits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, batchCount },
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 2 },
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 4 },
				{ ProcessTasksSchema.Constants.TableName, batchCount },
			};

			// ignoreHitsFromTablesCachedInUberFactory is set to false, because we want to be sure we don't excessively reload BMComponents ignoring User Factory cache
			using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false))
			{
				performer.PerformActionOnMultipleWorkflows();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedMatchingWorkflows = matchingWorkflows.Select(w => newFactory.Load<ProcessHeader>(w.PK)).ToArray();
			Assert("Should mark the matching workflows for dedicated buffer update by changing their last edit time", loadedMatchingWorkflows.All(w => w.FH_SystemLastEditTimeUtc > latestLastEditTime));
		}

		public void TestResponsiveEffectiveBranchAndDepartmentUpdate_DbHits()
		{
			const int batchSize = 10;
			BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			buffer.FC_GB_AgingBranch = branch1.PK;
			buffer.FC_GE_AgingDepartment = department1.PK;

			BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			const int batchCount = 4;
			const int workflowCount = batchCount * batchSize;

			var matchingWorkflows = new List<ProcessHeader>();

			for (int i = 0; i < workflowCount; i++)
			{
				var processHeader = BMSTestHelper.CreateWorkflow(Factory, $"matching workflow{i}");

				var task = BMSTestHelper.CreateTask(processHeader);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				processHeader.FH_IsStandby = true;
				processHeader.FH_FC_DedicatedBuffer = buffer.PK;
				matchingWorkflows.Add(processHeader);
			}

			Factory.Save();

			Assert("Precondition", matchingWorkflows.All(w => w.FH_GB_EffectiveBranch == branch1.PK && w.FH_GE_EffectiveDepartment == department1.PK));

			buffer.FC_GB_AgingBranch = branch2.PK;
			buffer.FC_GE_AgingDepartment = department2.PK;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var query = new ZQuery(ProcessHeaderSchema.FH_IsStandby, true);
			var performer = new ProcessHeaderResponsiveActionPerformer(logger, query, "EBD");

			var hits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, batchCount },
				{ BMComponentSchema.Constants.TableName, 3 }, // cached in Uber Factory, but also 1 hit for reloading directly from the db
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 2 },
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 4 },
				{ ProcessTasksSchema.Constants.TableName, batchCount },
			};

			// ignoreHitsFromTablesCachedInUberFactory is set to false, because we want to be sure we don't excessively reload BMComponents ignoring User Factory cache
			using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false))
			{
				performer.PerformActionOnMultipleWorkflows();
			}

			Assert("Should update matching workflows", matchingWorkflows.All(w => w.FH_GB_EffectiveBranch == branch2.PK && w.FH_GE_EffectiveDepartment == department2.PK));
		}

		#endregion

		#region Setup And TearDown

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#endregion
	}
}
