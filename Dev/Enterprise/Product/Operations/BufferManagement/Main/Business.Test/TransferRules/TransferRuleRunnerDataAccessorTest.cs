using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;

namespace Enterprise.BufferManagement.Business.Test
{
	class TransferRuleRunnerDataAccessorTest : BMSTestCaseWithFactory
	{
		public void TestShouldIgnoreUberFactoryCache_WhenLoadingComponentLinksToProcess()
		{
			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: true);
			var componentFrom = BMSTestHelper.CreateBucket(system, "from");
			var componentTo1 = BMSTestHelper.CreateBucket(system, "to 1", sequence: 10);
			var componentTo2 = BMSTestHelper.CreateBucket(system, "to 2", sequence: 20);
			var componentTo3 = BMSTestHelper.CreateBucket(system, "to 3", sequence: 30);
			var link1 = BMSTestHelper.LinkComponents(componentFrom, componentTo1);
			var link2 = BMSTestHelper.LinkComponents(componentFrom, componentTo2);
			var link3 = BMSTestHelper.LinkComponents(componentFrom, componentTo3);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 0;
			link3.FL_Sequence = 0;

			var processHeader = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", componentFrom);
			Factory.Save();

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_IsActive = 0,
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{componentTo2.PK}'

UPDATE dbo.BMComponent
SET FC_DisplaySequence = 5,
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{componentTo3.PK}'
"))
			{
				command.ExecuteNonQuery();
			}

			var logger = new Mock<ITransferRuleRunnerLogger>();
			var accessor = new TransferRuleRunnerDataAccessorThatLoadsOneNullWorkflow(logger.Object);
			var links = accessor.GetComponentLinks(system).Cast<BMComponentLink>().ToArray();
			AssertContainsExactElementsInExactOrder("Should not return links with inactive components - should use BMComponent data read from db ignoring the Uber Factory cache",
				[link3.DisplayText, link1.DisplayText], links.Select(l => l.DisplayText));
			AssertContainsExactElementsInExactOrder("Should load component sequence ignoring the Uber Factory cache", [5, 10], links.Select(l => l.ComponentTo.FC_DisplaySequence));

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_IsActive = 1,
	FC_DisplaySequence = 1,
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{componentTo2.PK}'"))
			{
				command.ExecuteNonQuery();
			}

			accessor = new TransferRuleRunnerDataAccessorThatLoadsOneNullWorkflow(logger.Object);
			links = accessor.GetComponentLinks(system).Cast<BMComponentLink>().ToArray();
			AssertContainsExactElementsInExactOrder("Should now return all links ordered by component sequence",
				[link2.DisplayText, link3.DisplayText, link1.DisplayText], links.Select(l => l.DisplayText));
			AssertContainsExactElementsInExactOrder("Should load component sequence ignoring the Uber Factory cache", [1, 5, 10], links.Select(l => l.ComponentTo.FC_DisplaySequence));
		}

		public void TestShouldReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer()
		{
			BMSRegistry.Instance.ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "ORG", isActive: true);
			var component1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var component2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var link = BMSTestHelper.LinkComponents(component1, component2);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";
			var header = ProcessJobHeader.GetForParent(org, Factory);
			var processHeader = header.ProcessHeaders.AddNew();
			processHeader.FH_CompletionStatement = "Workflow";
			processHeader.FH_FC_CurrentComponent = component1.PK;

			Factory.Save();

			var logger = new Mock<ITransferRuleRunnerLogger>();
			var accessor = new TransferRuleRunnerDataAccessor(logger.Object);

			accessor.AddingFetchHintsForWorkflowsToTransfer_ForTest += (s, workflows) =>
			{
				AssertEquals(1, workflows.Length);
				var workflow = workflows.First();
				AssertEquals(workflow.PK, processHeader.PK);
				DeleteWorkflow_ToAppearInTheCallStack(workflow);
			};

			IEnumerable<ITransferrableProcessHeader> loadedWorkflows = null;
			AssertNoExceptionThrown(() => loadedWorkflows = accessor.LoadWorkflowsToTransfer(new Guid[] { processHeader.PK.ToGuid() }, link));
			AssertNotNull(loadedWorkflows);
			AssertEquals(0, loadedWorkflows.Count());

			AssertEquals("TransferRuleRunnerDataAccessor:AddingFetchHintsToDeletedWorkflows", ErrorReporter.LastKeyReported);
			Assert(ErrorReporter.LastMessageReported.Contains("Trying to add fetch hints to a deleted workflows."));
			Assert(ErrorReporter.LastMessageReported.Contains("Deletion callstack:"));
			Assert(ErrorReporter.LastMessageReported.Contains("at Enterprise.BufferManagement.Business.Test.TransferRuleRunnerDataAccessorTest.DeleteWorkflow_ToAppearInTheCallStack(ProcessHeader workflow) in"));
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestReportDeletedWorkflow_ShouldTurnOnRelevantRegistryItem()
		{
			AssertEquals("The registry item should be off by default.", false, BMSRegistry.Instance.ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer.Value);

			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: true);
			var component1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var component2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var link = BMSTestHelper.LinkComponents(component1, component2);

			var processHeader = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", component1);

			Factory.Save();

			var logger = new Mock<ITransferRuleRunnerLogger>();
			var accessor = new TransferRuleRunnerDataAccessor(logger.Object);

			accessor.AddingFetchHintsForWorkflowsToTransfer_ForTest += (s, workflows) =>
			{
				AssertEquals(1, workflows.Length);
				var workflow = workflows.First();
				AssertEquals(workflow.PK, processHeader.PK);
				DeleteWorkflow_ToAppearInTheCallStack(workflow);
			};

			IEnumerable<ITransferrableProcessHeader> loadedWorkflows = null;
			AssertNoExceptionThrown(() => loadedWorkflows = accessor.LoadWorkflowsToTransfer(new[] { processHeader.PK.ToGuid() }, link));
			AssertNotNull(loadedWorkflows);
			AssertEquals(0, loadedWorkflows.Count());

			AssertEquals("TransferRuleRunnerDataAccessor:AddingFetchHintsToDeletedWorkflows", ErrorReporter.LastKeyReported);
			Assert(ErrorReporter.LastMessageReported.Contains("Trying to add fetch hints to a deleted workflows."));
			Assert(ErrorReporter.LastMessageReported.Contains("Deletion callstack:"));
			Assert(ErrorReporter.LastMessageReported.Contains("Call stack not yet available, either because ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer isn't enabled in the registry, or because Delete() wasn't actually called on the ProcessHeader. This registry item has now been turned on for this customer system, so you should see the actual call stack here in the next occurrence."));
			Assert("The registry item hasn't been enabled so the call stack should not appear until next time.", !ErrorReporter.LastMessageReported.Contains("DeleteWorkflow_ToAppearInTheCallStack"));
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();

			AssertEquals("The error was encountered on this customer's system so the registry item should have been enabled so that the call stack will be reported next time.", true,
				BMSRegistry.Instance.ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer.Value);
		}

		public void TestAddFetchHintsForWorkflowsToTransfer_ShouldIgnoreNullWorkflows()
		{
			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: true);
			var component1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var component2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var link = BMSTestHelper.LinkComponents(component1, component2);

			var processHeader = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", component1);

			Factory.Save();

			var logger = new Mock<ITransferRuleRunnerLogger>();
			var accessor = new TransferRuleRunnerDataAccessorThatLoadsOneNullWorkflow(logger.Object);

			AssertNoExceptionThrown(() => accessor.LoadWorkflowsToTransfer(new[] { processHeader.PK.ToGuid() }, link));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestDataAccessor_ShouldHaveTypeInfoInQueryName()
		{
			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: true);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var logger = new Mock<ITransferRuleRunnerLogger>();
			var accessor = new TransferRuleRunnerDataAccessor(logger.Object);

			var (query, queryName) = accessor.GetWorkflowQuery_ExposedForTest(link);

			Assert(queryName.Contains("Transfer Rules"));
		}

		static void DeleteWorkflow_ToAppearInTheCallStack(ProcessHeader workflow)
		{
			workflow.Delete();
		}

		public void TestDataAccessor_ShouldIgnoreInactiveWorkflows()
		{
			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: true);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var activeWorkflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "activeWorkflow", bucket1);
			var inactiveWorkflow1 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "inactiveWorkflow1", bucket1);
			var inactiveWorkflow2 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "inactiveWorkflow2", bucket1);
			inactiveWorkflow1.FH_IsActive = false;
			inactiveWorkflow2.FH_IsActive = false;

			Factory.Save();

			var logger = new Mock<ITransferRuleRunnerLogger>();
			var accessor = new TransferRuleRunnerDataAccessor(logger.Object);

			var (query, queryName) = accessor.GetWorkflowQuery_ExposedForTest(link);

			var workflows = Factory.Load<ProcessHeader>(query);

			AssertEquals(activeWorkflow.PK, workflows.Single().PK);
		}

		#region Implementation

		class TransferRuleRunnerDataAccessorThatLoadsOneNullWorkflow : TransferRuleRunnerDataAccessor
		{
			protected override ProcessHeader[] LoadWorkflowsToTransferCore(ZGuid[] pks, ZQuery additionalFilter)
			{
				return new ProcessHeader[] { null };
			}

			public TransferRuleRunnerDataAccessorThatLoadsOneNullWorkflow(ITransferRuleRunnerLogger logger)
				: base(logger)
			{
			}
		}

		#endregion
	}
}
