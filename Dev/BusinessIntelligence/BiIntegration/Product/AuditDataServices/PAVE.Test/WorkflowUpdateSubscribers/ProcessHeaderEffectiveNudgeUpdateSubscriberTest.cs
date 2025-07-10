using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.PAVE.Subscribers;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(ProcessHeaderEffectiveNudgeUpdateSubscriber))]
	class ProcessHeaderEffectiveNudgeUpdateSubscriberTest : PAVESubscriberBaseTest
	{
		protected override string ExpectedCode => "PHN";

		protected override ITableSchema ExpectedTable => ProcessHeaderSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSpecificColumns => [ProcessHeaderSchema.FH_VoteUpDownAmount, ProcessHeaderSchema.FH_IsActive, ProcessHeaderSchema.FH_Status];

		public override void TestCustomFilter()
		{
			var subscriber = new ProcessHeaderEffectiveNudgeUpdateSubscriber();

			var table = CreateAuditProcessHeaderTable();
			var rowsAndNames = new List<(DataRow Row, string Name)>
			{
				(CreateAuditProcessHeaderRow(table, parentHeaderPK: Guid.NewGuid()), "row1Workflow"),
				(CreateAuditProcessHeaderRow(table, parentHeaderPK: null), "row2JobHeader"),
				(CreateAuditProcessHeaderRow(table, parentHeaderPK: Guid.NewGuid(), templateId: Guid.NewGuid()), "row3TemplateWorkflow"),
			};
			table.AcceptChanges();

			foreach (var rowAndName in rowsAndNames)
			{
				RunCustomFilter(rowAndName.Row, subscriber);
			}
			table.AcceptChanges();

			var matchedRows = table.Rows.Cast<DataRow>().ToArray();
			var matchedRowsAndNames = rowsAndNames.Where(rn => matchedRows.Contains(rn.Row)).ToArray();
			AssertContainsExactElementsInAnyOrder(["row1Workflow", "row2JobHeader"], matchedRowsAndNames.Select(rn => rn.Name));
		}

		public override void TestIsEnabled()
		{
			void AssertDisabled()
			{
				var changeTable = CreateAuditProcessHeaderTable(5);
				ProcessChanges(changeTable);

				AssertMultilineASCIIEquals(@"5 changes were skipped as the PHN subscriber is disabled.
", Logger.ToString());
				ResetLogger();
			}

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			var changeTable = CreateAuditProcessHeaderTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());

			// other flags should not affect
			BMSRegistry.EnablePaveExperimentalFeatures = false;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = false;
			changeTable = CreateAuditProcessHeaderTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());
		}

		public void TestShouldCalculateEffectiveNudge_OnCreatedWorkflow()
		{
			SetupConfig();
			var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_VoteUpDownAmount = 100;

			factory.Save();

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow.PK);
			ProcessChanges(changeTable);

			workflow.Reload();
			AssertEquals("Should calculate effective nudge based on the most up-to-date values", 100m, workflow.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
			ResetLogger();
		}

		public void TestShouldRecalculateEffectiveNudge_WhenSpecificPropertiesChangedWorkflow()
		{
			SetupConfig();
			var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_VoteUpDownAmount = 100;
			workflow.FH_EffectiveNudge = 5; // some previous random value

			factory.Save();

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow.PK);
			ProcessChanges(changeTable);

			workflow.Reload();
			AssertEquals("Should recalculate effective nudge based on the most up-to-date values", 100m, workflow.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
		}

		public void TestShouldResetEffectiveNudge_WhenSpecificPropertiesChangedOnInactiveWorkflow()
		{
			SetupConfig();
			var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			workflow.FH_IsActive = false;
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_VoteUpDownAmount = 200;
			workflow.FH_EffectiveNudge = 6; // some previous random value

			factory.Save();

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow.PK);
			ProcessChanges(changeTable);

			workflow.Reload();
			AssertEquals("Should reset effective nudge for inactive workflows", 0m, workflow.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
		}

		public void TestShouldResetEffectiveNudge_WhenSpecificPropertiesChangedOnClosedWorkflow()
		{
			SetupConfig();
			var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			workflow.CancelAllTasksAndCompletionStatements("Because");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_VoteUpDownAmount = 300;
			workflow.FH_EffectiveNudge = 7; // some previous random value

			factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow.FH_Status);

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow.PK);
			ProcessChanges(changeTable);

			workflow.Reload();
			AssertEquals("Should reset effective nudge for closed workflows", 0m, workflow.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
		}

		public void TestShouldResetEffectiveNudge_WhenSpecificPropertiesChangedOnClosedWorkflowWithOpenPrerequisites()
		{
			SetupConfig();
			var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			var prereqWorkflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			prereqWorkflow.MakePrerequisiteOf(workflow);
			workflow.CancelAllTasksAndCompletionStatements("Because");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_VoteUpDownAmount = 400;
			workflow.FH_EffectiveNudge = 8; // some previous random value

			factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow.FH_Status);

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow.PK);
			ProcessChanges(changeTable);

			workflow.Reload();
			AssertEquals("Should reset effective nudge for closed workflows (with open prerequisites)", 0m, workflow.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
		}

		public void TestResetEffectiveNudge_WhenSpecificPropertiesChangedOnWorkflowWithDedicatedBufferReset()
		{
			SetupConfig();
			var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			workflow.FH_VoteUpDownAmount = 200;
			workflow.FH_EffectiveNudge = 6; // some previous random value

			factory.Save();

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow.PK);
			ProcessChanges(changeTable);

			workflow.Reload();
			AssertEquals("Should reset effective nudge for workflows with dedicated buffer reset", 0m, workflow.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
		}

		public void TestShouldRecalculateEffectiveNudgeOnWorkflows_WhenSpecificPropertiesChangedOnJLW()
		{
			SetupConfig();
			var jobHeader = (ProcessJobHeader)testHelper.CreateJobHeader<DummyWithWorkflow>(factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3Inactive = jobHeader.ProcessHeaders.AddNew();
			var workflow4Closed = jobHeader.ProcessHeaders.AddNew();
			var workflow5ClosedWithOpenPrerequisites = jobHeader.ProcessHeaders.AddNew();
			var prereqWorkflow = jobHeader.ProcessHeaders.AddNew();
			var workflow6WithDedicatedBufferReset = jobHeader.ProcessHeaders.AddNew();

			jobHeader.FH_VoteUpDownAmount = 1000;

			workflow1.FH_CompletionStatement = "Workflow 1";
			testHelper.CreateTask((IProcessHeader)workflow1);
			workflow1.FH_FC_DedicatedBuffer = buffer.PK;
			workflow1.FH_VoteUpDownAmount = 100;
			workflow1.FH_EffectiveNudge = 5; // some previous random value

			workflow2.FH_CompletionStatement = "Workflow 2";
			testHelper.CreateTask((IProcessHeader)workflow2);
			workflow2.FH_FC_DedicatedBuffer = buffer.PK;
			workflow2.FH_VoteUpDownAmount = 200;
			workflow2.FH_EffectiveNudge = 6; // some previous random value

			workflow3Inactive.FH_CompletionStatement = "Workflow 3 (inactive)";
			workflow3Inactive.FH_IsActive = false;
			testHelper.CreateTask((IProcessHeader)workflow3Inactive);
			workflow3Inactive.FH_FC_DedicatedBuffer = buffer.PK;
			workflow3Inactive.FH_VoteUpDownAmount = 300;
			workflow3Inactive.FH_EffectiveNudge = 7; // some previous random value

			workflow4Closed.FH_CompletionStatement = "Workflow 4 (closed)";
			workflow4Closed.FH_FC_DedicatedBuffer = buffer.PK;
			workflow4Closed.FH_VoteUpDownAmount = 400;
			workflow4Closed.FH_EffectiveNudge = 8; // some previous random value

			workflow5ClosedWithOpenPrerequisites.FH_CompletionStatement = "Workflow 5 (closed with open prereqs)";
			prereqWorkflow.MakePrerequisiteOf(workflow5ClosedWithOpenPrerequisites);
			testHelper.CreateTask((IProcessHeader)prereqWorkflow);
			workflow5ClosedWithOpenPrerequisites.FH_FC_DedicatedBuffer = buffer.PK;
			workflow5ClosedWithOpenPrerequisites.FH_VoteUpDownAmount = 500;
			workflow5ClosedWithOpenPrerequisites.FH_EffectiveNudge = 9; // some previous random value

			workflow6WithDedicatedBufferReset.FH_CompletionStatement = "Workflow 6 (with dedicated buffer reset)";
			testHelper.CreateTask((IProcessHeader)workflow6WithDedicatedBufferReset);
			Assert("Precondition", workflow6WithDedicatedBufferReset.FH_FC_DedicatedBuffer.IsEmpty);
			workflow6WithDedicatedBufferReset.FH_VoteUpDownAmount = 600;
			workflow6WithDedicatedBufferReset.FH_EffectiveNudge = 10; // some previous random value

			factory.Save();

			workflow1.Reload();
			workflow2.Reload();
			workflow3Inactive.Reload();
			workflow4Closed.Reload();
			workflow5ClosedWithOpenPrerequisites.Reload();
			workflow6WithDedicatedBufferReset.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow3Inactive.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow4Closed.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow5ClosedWithOpenPrerequisites.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow6WithDedicatedBufferReset.FH_Status);

			AssertEquals("Precondition", 5m, workflow1.FH_EffectiveNudge);
			AssertEquals("Precondition", 6m, workflow2.FH_EffectiveNudge);
			AssertEquals("Precondition: for inactive workflows dedicated buffer should be reset on save which also resets effective nudge", 0m, workflow3Inactive.FH_EffectiveNudge);
			AssertEquals("Precondition", 8m, workflow4Closed.FH_EffectiveNudge);
			AssertEquals("Precondition", 9m, workflow5ClosedWithOpenPrerequisites.FH_EffectiveNudge);
			AssertEquals("Precondition", 10m, workflow6WithDedicatedBufferReset.FH_EffectiveNudge);

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, jobHeader.PK);
			ProcessChanges(changeTable);

			jobHeader.Reload();
			workflow1.Reload();
			workflow2.Reload();
			workflow3Inactive.Reload();
			workflow4Closed.Reload();
			workflow5ClosedWithOpenPrerequisites.Reload();
			workflow6WithDedicatedBufferReset.Reload();
			AssertEquals("Should always be zero on JLW", 0m, jobHeader.FH_EffectiveNudge);
			AssertEquals("Should recalculate effective nudge on child workflows based on the most up-to-date values - workflow1", 1100m, workflow1.FH_EffectiveNudge);
			AssertEquals("Should recalculate effective nudge on child workflows based on the most up-to-date values - workflow2", 1200m, workflow2.FH_EffectiveNudge);
			AssertEquals("Should reset effective nudge on inactive child workflows", 0m, workflow3Inactive.FH_EffectiveNudge);
			AssertEquals("Should reset effective nudge on closed child workflows", 0m, workflow4Closed.FH_EffectiveNudge);
			AssertEquals("Should reset effective nudge on closed child workflows with open prerequisites", 0m, workflow5ClosedWithOpenPrerequisites.FH_EffectiveNudge);
			AssertEquals("Should reset effective nudge on child workflows with dedicated buffers reset", 0m, workflow6WithDedicatedBufferReset.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
		}

		public void TestShouldRecalculateEffectiveNudgeOnJustOneJLW_WhenVoteUpDownAmountChangedOnMultipleWorkflowsWithinJob()
		{
			BMSRegistry.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;

			SetupConfig();
			var jobHeader = (ProcessJobHeader)testHelper.CreateJobHeader<DummyWithWorkflow>(factory);
			jobHeader.FH_VoteUpDownAmount = 1000;

			var workflow1 = jobHeader.ProcessHeaders[0];
			testHelper.CreateTask((IProcessHeader)workflow1);
			workflow1.FH_FC_DedicatedBuffer = buffer.PK;
			workflow1.FH_VoteUpDownAmount = 100;
			workflow1.FH_EffectiveNudge = 5; // some previous random value

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			testHelper.CreateTask((IProcessHeader)workflow2);
			workflow2.FH_FC_DedicatedBuffer = buffer.PK;
			workflow2.FH_VoteUpDownAmount = 200;
			workflow2.FH_EffectiveNudge = 5; // some previous random value

			factory.Save();

			workflow1.Reload();
			workflow2.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow2.FH_Status);

			var changeTable = CreateAuditProcessHeaderTable();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, jobHeader.PK);
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow1.PK);
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow2.PK);
			ProcessChanges(changeTable);

			jobHeader.Reload();
			workflow1.Reload();
			workflow2.Reload();
			AssertEquals("Should always be zero on JLW", 0m, jobHeader.FH_EffectiveNudge);
			AssertEquals("Should recalculate effective nudge on child workflows based on the most up-to-date values - workflow1", 1100m, workflow1.FH_EffectiveNudge);
			AssertEquals("Should recalculate effective nudge on child workflows based on the most up-to-date values - workflow2", 1200m, workflow2.FH_EffectiveNudge);

			AssertMultilineASCIIEquals($@"Updated effective nudge on Job Dummy Business Object Default is complete. (PK = {jobHeader.PK}, Job = Dummy Business Object Default)
Updated effective nudge on 1 workflow in a batch (1 workflow in total in 1 batch)", Logger.ToString());
		}

		public void TestShouldNotUpdateEffectiveNudgeOnDeletedWorkflows()
		{
			BMSRegistry.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;

			var changeTable = CreateAuditProcessHeaderTable();
			var nonExistentWorkflowPK = ZGuid.NewZGuid();
			CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, nonExistentWorkflowPK);
			ProcessChanges(changeTable);

			AssertMultilineASCIIEquals($@"Updated effective nudge on 0 workflows in a batch (0 workflows in total in 1 batch)", Logger.ToString());
			ResetLogger();
		}

		public void TestShouldSkipNudgeUpdates_WhenBookingConcurrencyErrorOccurs()
		{
			SetupConfig();

			var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_VoteUpDownAmount = 100;
			factory.Save();

			var subscriber = new ProcessHeaderEffectiveNudgeUpdateSubscriber();

			var bookingConversionException = new ZConcurrencyCheckFailureException(
				"Another user has converted the booking into a shipment.",
				"Booking concurrency error",
				true);

			var isDetected = subscriber.IsBookingConvertedToShipment(bookingConversionException);
			Assert("Should detect booking conversion exception", isDetected);

			if (isDetected)
			{
				Logger.Information("Skipping nudge updates for workflows - booking has been converted to shipment.");
			}

			var logs = Logger.ToString();
			AssertContains("Skipping nudge updates for workflows - booking has been converted to shipment.", logs);
		}

		public void TestDbHitsAndLogs_WhenUpdatingWorkflows_AndLoggingIndividualWorkflowDetails()
		{
			const int batchSize = 10;
			const int workflowCount = 15;
			const int batchCount = 2;

			BMSRegistry.ResponsiveWorkflowUpdatesBatchSize = batchSize;
			BMSRegistry.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;

			SetupConfig();

			var workflowPKs = new List<ZGuid>();
			var changeTable = CreateAuditProcessHeaderTable();

			for (var i = 0; i < workflowCount; i++)
			{
				var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, $"Workflow{i}");
				workflow.FH_FC_DedicatedBuffer = buffer.PK;

				workflowPKs.Add(workflow.PK);
				CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, workflow.PK);
			}

			factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, batchCount },

					// 2 hits per batch:
					// 1) loading workflows in the batch
					// 2) loading job headers for the workflows
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 2 },
				{ TagLinkSchema.Constants.TableName, batchCount },
			};

			using (testHelper.TemporarilyDisableTableCachingInUberFactory([BMComponentSchema.Constants.TableName, BMSystemSchema.Constants.TableName])) // preparation for removing these tables from UberFactory
			using (TestCaseWithFactory.AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				ProcessChanges(changeTable);
			}

			var logs = logger.ToString();
			AssertContains($@"Updated effective nudge on Workflow0 (PK = {workflowPKs[0]}, Job = Organization (XVBQP68SIYXQ))", logs);
			AssertContains($@"Updated effective nudge on Workflow1 (PK = {workflowPKs[1]}, Job = Organization (H5ZX52PAMCOI))", logs);
			AssertContains($@"Updated effective nudge on Workflow2 (PK = {workflowPKs[2]}, Job = Organization (ZGP5LX5SQPEB))", logs);
			AssertContains($@"Updated effective nudge on Workflow3 (PK = {workflowPKs[3]}, Job = Organization (IRED1TLAV242))", logs);
			AssertContains($@"Updated effective nudge on Workflow4 (PK = {workflowPKs[4]}, Job = Organization (013LIP1SZFVU))", logs);
			AssertContains($@"Updated effective nudge on Workflow5 (PK = {workflowPKs[5]}, Job = Organization (KCSSYKIA3SMN))", logs);
			AssertContains($@"Updated effective nudge on Workflow6 (PK = {workflowPKs[6]}, Job = Organization (2MIZFGXS85CF))", logs);
			AssertContains($@"Updated effective nudge on Workflow7 (PK = {workflowPKs[7]}, Job = Organization (LX67UBEADJ26))", logs);
			AssertContains($@"Updated effective nudge on Workflow8 (PK = {workflowPKs[8]}, Job = Organization (47WFB6USHWTZ))", logs);
			AssertContains($@"Updated effective nudge on Workflow9 (PK = {workflowPKs[9]}, Job = Organization (NILNR2ABMAKR))", logs);
			AssertContains($@"Updated effective nudge on 10 workflows in a batch (10 workflows in total in 1 batch)", logs);
			AssertContains($@"Updated effective nudge on Workflow10 (PK = {workflowPKs[10]}, Job = Organization (5TBU7XQSQNBJ))", logs);
			AssertContains($@"Updated effective nudge on Workflow11 (PK = {workflowPKs[11]}, Job = Organization (O3Z2OT6BU00C))", logs);
			AssertContains($@"Updated effective nudge on Workflow12 (PK = {workflowPKs[12]}, Job = Organization (7EOA3OMSZER3))", logs);
			AssertContains($@"Updated effective nudge on Workflow13 (PK = {workflowPKs[13]}, Job = Organization (QOEIKK2B3RIV))", logs);
			AssertContains($@"Updated effective nudge on Workflow14 (PK = {workflowPKs[14]}, Job = Organization (8Z2P0FJS748O))", logs);
			AssertContains($@"Updated effective nudge on 5 workflows in a batch (15 workflows in total in 2 batches)", logs);
		}

		public void TestDbHitsAndLogs_WhenUpdatingJLWs_AndLoggingIndividualWorkflowDetails()
		{
			const int batchSize = 10;
			const int workflowCount = 15;
			const int batchCount = 2;

			BMSRegistry.ResponsiveWorkflowUpdatesBatchSize = batchSize;
			BMSRegistry.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;

			SetupConfig();

			var jobHeaderPKs = new List<ZGuid>();
			var changeTable = CreateAuditProcessHeaderTable();

			for (var i = 0; i < workflowCount; i++)
			{
				var workflow = (ProcessHeader)testHelper.CreateWorkflowAndTask(factory, $"Workflow{i}");
				workflow.FH_FC_DedicatedBuffer = buffer.PK;

				var jobHeader = workflow.JobHeader;
				jobHeader.FH_CompletionStatement = $"JLW{i}";
				jobHeaderPKs.Add(jobHeader.PK);

				CreateAuditProcessHeaderRowMatchingSpecificColumns(changeTable, jobHeader.PK);
			}

			factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, batchCount },

					// 2 hits per batch:
					// 1) loading workflows (job headers) in the batch
					// 2) loading child workflows for the job headers
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 2 },
				{ TagLinkSchema.Constants.TableName, batchCount },
			};

			using (testHelper.TemporarilyDisableTableCachingInUberFactory([BMComponentSchema.Constants.TableName, BMSystemSchema.Constants.TableName])) // preparation for removing these tables from UberFactory
			using (TestCaseWithFactory.AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				ProcessChanges(changeTable);
			}

			var logs = logger.ToString();
			AssertContains($@"Updated effective nudge on JLW0 (PK = {jobHeaderPKs[0]}, Job = Organization (XVBQP68SIYXQ))", logs);
			AssertContains($@"Updated effective nudge on JLW1 (PK = {jobHeaderPKs[1]}, Job = Organization (H5ZX52PAMCOI))", logs);
			AssertContains($@"Updated effective nudge on JLW2 (PK = {jobHeaderPKs[2]}, Job = Organization (ZGP5LX5SQPEB))", logs);
			AssertContains($@"Updated effective nudge on JLW3 (PK = {jobHeaderPKs[3]}, Job = Organization (IRED1TLAV242))", logs);
			AssertContains($@"Updated effective nudge on JLW4 (PK = {jobHeaderPKs[4]}, Job = Organization (013LIP1SZFVU))", logs);
			AssertContains($@"Updated effective nudge on JLW5 (PK = {jobHeaderPKs[5]}, Job = Organization (KCSSYKIA3SMN))", logs);
			AssertContains($@"Updated effective nudge on JLW6 (PK = {jobHeaderPKs[6]}, Job = Organization (2MIZFGXS85CF))", logs);
			AssertContains($@"Updated effective nudge on JLW7 (PK = {jobHeaderPKs[7]}, Job = Organization (LX67UBEADJ26))", logs);
			AssertContains($@"Updated effective nudge on JLW8 (PK = {jobHeaderPKs[8]}, Job = Organization (47WFB6USHWTZ))", logs);
			AssertContains($@"Updated effective nudge on JLW9 (PK = {jobHeaderPKs[9]}, Job = Organization (NILNR2ABMAKR))", logs);
			AssertContains($@"Updated effective nudge on 10 workflows in a batch (10 workflows in total in 1 batch)", logs);
			AssertContains($@"Updated effective nudge on JLW10 (PK = {jobHeaderPKs[10]}, Job = Organization (5TBU7XQSQNBJ))", logs);
			AssertContains($@"Updated effective nudge on JLW11 (PK = {jobHeaderPKs[11]}, Job = Organization (O3Z2OT6BU00C))", logs);
			AssertContains($@"Updated effective nudge on JLW12 (PK = {jobHeaderPKs[12]}, Job = Organization (7EOA3OMSZER3))", logs);
			AssertContains($@"Updated effective nudge on JLW13 (PK = {jobHeaderPKs[13]}, Job = Organization (QOEIKK2B3RIV))", logs);
			AssertContains($@"Updated effective nudge on JLW14 (PK = {jobHeaderPKs[14]}, Job = Organization (8Z2P0FJS748O))", logs);
			AssertContains($@"Updated effective nudge on 5 workflows in a batch (15 workflows in total in 2 batches)", logs);
		}

		#region Implementation

		ILogger logger;

		protected override ILogger Logger => logger;

		void ResetLogger()
		{
			logger = new SimpleLogger();
		}

		DataTable CreateAuditProcessHeaderTable(int numEntries = 0)
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(ProcessHeaderSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_VoteUpDownAmount, typeof(ZShort));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_IsActive, typeof(bool));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_Status, typeof(string));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_FH_ParentHeader, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_FC_DedicatedBuffer, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_P0_Template, typeof(Guid));
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			for (var i = 0; i < numEntries; i++)
			{
				var row = CreateAuditProcessHeaderRow(changeTable);
				row.AcceptChanges();
				row.SetModified();
			}

			return changeTable;
		}

		protected override DataTable GetTestDataTable() => CreateAuditProcessHeaderTable();

		DataRow CreateAuditProcessHeaderRowMatchingSpecificColumns(DataTable dataTable, ZGuid? processHeaderPK = null)
		{
			var row = CreateAuditProcessHeaderRow(dataTable, processHeaderPK, voteUpDownAmount: 1);
			row.AcceptChanges();
			row.SetModified();
			row[ProcessHeaderSchema.Constants.FH_VoteUpDownAmount] = (ZShort)2;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FH_VoteUpDownAmount", (ZShort)1, row[ProcessHeaderSchema.Constants.FH_VoteUpDownAmount, DataRowVersion.Original]);
			AssertEquals("Precondition: current FH_VoteUpDownAmount", (ZShort)2, row[ProcessHeaderSchema.Constants.FH_VoteUpDownAmount, DataRowVersion.Current]);

			return row;
		}

		DataRow CreateAuditProcessHeaderRow(DataTable dataTable, ZGuid? processHeaderPK = null, ZShort? voteUpDownAmount = null, bool isActive = true, string status = null, ZGuid? parentHeaderPK = null, ZGuid? dedicatedBuffer = null, Guid? templateId = null, DateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc ?? ZDateTime.UtcNow.ToDateTime();

			row[ProcessHeaderSchema.Constants.PK] = processHeaderPK != null ? processHeaderPK.Value.ToGuid() : Guid.NewGuid();
			row[ProcessHeaderSchema.Constants.FH_VoteUpDownAmount] = voteUpDownAmount ?? 0;
			row[ProcessHeaderSchema.Constants.FH_IsActive] = isActive;
			row[ProcessHeaderSchema.Constants.FH_Status] = status ?? "OPN";
			row[ProcessHeaderSchema.Constants.FH_FH_ParentHeader] = parentHeaderPK != null ? parentHeaderPK.Value.ToGuid() : DBNull.Value;
			row[ProcessHeaderSchema.Constants.FH_FC_DedicatedBuffer] = dedicatedBuffer != null ? dedicatedBuffer.Value.ToGuid() : DBNull.Value;
			row[ProcessHeaderSchema.Constants.FH_P0_Template] = templateId != null ? templateId.Value : DBNull.Value;
			dataTable.Rows.Add(row);

			AssertEquals("Precondition: should be added", DataRowState.Added, row.RowState);
			Assert("Precondition: should have no original version", !row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));

			return row;
		}

		BusinessObjectFactory factory;
		IBMTestHelper testHelper;
		IBMComponent buffer;

		void SetupConfig()
		{
			factory = new BusinessObjectFactory();
			testHelper = ObjectFactory.Get<IBMTestHelper>();
			var system = testHelper.CreateSystem(factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test";
			buffer = testHelper.CreateBuffer(system, "Buffer");
		}

		#endregion

		#region Setup And TearDown

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			ResetLogger();
		}

		#endregion
	}
}
