using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Business.Workflow;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test.Workflow
{
	public class ProcessHeaderLinkCopierForUniversalCopyTest : BMSTestCaseWithFactory
	{
		public void TestCopyProcessHeaderLinksForUniversalCopy_ClonesInnerProcessHeaderLinks()
		{
			// Arrange
			VisualBoards.Business.Test.VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var sourceJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJobHeader = BMSTestHelper.CreateJobHeader(sourceJob);
			var sourceWorkflow1 = CreateWorkflow(template, sourceJobHeader);
			var sourceWorkflow2 = CreateWorkflow(template, sourceJobHeader);
			var sourceWorkflow3 = CreateWorkflow(template, sourceJobHeader);

			var targetJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob);
			var targetWorkflow1 = CreateWorkflow(template, targetJobHeader);
			var targetWorkflow2 = CreateWorkflow(template, targetJobHeader);
			var targetWorkflow3 = CreateWorkflow(template, targetJobHeader);

			var link1 = BMSTestHelper.CreateDependencyLink(template, sourceWorkflow1, sourceWorkflow2);

			var link2 = BMSTestHelper.CreateDependencyLink(template, sourceWorkflow2, sourceWorkflow3);

			var copiedEntities = new Dictionary<object, object>() {
				{ sourceWorkflow1, targetWorkflow1 },
				{ sourceWorkflow2, targetWorkflow2 },
				{ sourceWorkflow3, targetWorkflow3 },
				{ sourceJobHeader, targetJobHeader },
				{ sourceJob, targetJob },
			};

			var processHeaderLinkCopier = new ProcessHeaderLinkCopierForUniversalCopy();

			// Act
			processHeaderLinkCopier.FinishCopyAction(copiedEntities);

			// Assert
			var copiedLinks = copiedEntities.Values
				.Where(v => v is ProcessHeaderLink)
				.Select(l => (ProcessHeaderLink)l).ToArray();
			AssertEquals("Assert internal dependencies are cloned", 2, copiedLinks.Length);

			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow1.PK, copiedLinks[0].FP_FH_HeaderFrom);
			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow2.PK, copiedLinks[0].FP_FH_HeaderTo);

			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow2.PK, copiedLinks[1].FP_FH_HeaderFrom);
			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow3.PK, copiedLinks[1].FP_FH_HeaderTo);
		}

		public void TestCopyProcessHeaderLinksForUniversalCopy_MultipleWorkflowProviders_ClonesAllInnerProcessHeaderLinks()
		{
			// Arrange
			VisualBoards.Business.Test.VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var sourceJob1 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJob1Header = BMSTestHelper.CreateJobHeader(sourceJob1);
			var sourceWorkflow1 = CreateWorkflow(template, sourceJob1Header);
			var sourceWorkflow2 = CreateWorkflow(template, sourceJob1Header);

			var sourceJob2 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJob2Header = BMSTestHelper.CreateJobHeader(sourceJob2);
			var sourceWorkflow3 = CreateWorkflow(template, sourceJob2Header);
			var sourceWorkflow4 = CreateWorkflow(template, sourceJob2Header);

			var targetJob1 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJob1Header = BMSTestHelper.CreateJobHeader(targetJob1);
			var targetWorkflow1 = CreateWorkflow(template, targetJob1Header);
			var targetWorkflow2 = CreateWorkflow(template, targetJob1Header);

			var targetJob2 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJob2Header = BMSTestHelper.CreateJobHeader(targetJob2);
			var targetWorkflow3 = CreateWorkflow(template, targetJob2Header);
			var targetWorkflow4 = CreateWorkflow(template, targetJob2Header);

			var link1 = BMSTestHelper.CreateDependencyLink(template, sourceWorkflow1, sourceWorkflow2);

			var link2 = BMSTestHelper.CreateDependencyLink(template, sourceWorkflow3, sourceWorkflow4);

			var copiedEntities = new Dictionary<object, object>() {
				{ sourceWorkflow1, targetWorkflow1 },
				{ sourceWorkflow2, targetWorkflow2 },
				{ sourceWorkflow3, targetWorkflow3 },
				{ sourceWorkflow4, targetWorkflow4 },
				{ sourceJob1Header, targetJob1Header },
				{ sourceJob2Header, targetJob2Header },
				{ sourceJob1, targetJob1 },
				{ sourceJob2, targetJob2 },
			};

			var processHeaderLinkCopier = new ProcessHeaderLinkCopierForUniversalCopy();

			// Act
			processHeaderLinkCopier.FinishCopyAction(copiedEntities);

			// Assert
			var copiedLinks = copiedEntities.Values
				.Where(v => v is ProcessHeaderLink)
				.Select(l => (ProcessHeaderLink)l).ToArray();
			AssertEquals("Assert internal dependencies are cloned", 2, copiedLinks.Length);

			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow1.PK, copiedLinks[0].FP_FH_HeaderFrom);
			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow2.PK, copiedLinks[0].FP_FH_HeaderTo);

			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow3.PK, copiedLinks[1].FP_FH_HeaderFrom);
			AssertEquals("Assert internal dependencies are cloned with respective target workflows", targetWorkflow4.PK, copiedLinks[1].FP_FH_HeaderTo);
		}

		public void TestCopyProcessHeaderLinksForUniversalCopy_DoesNotCloneExternalDependencies()
		{
			// Arrange
			VisualBoards.Business.Test.VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var sourceJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJobHeader = BMSTestHelper.CreateJobHeader(sourceJob);
			var sourceWorkflow1 = CreateWorkflow(template, sourceJobHeader);

			var targetJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob);
			var targetWorkflow1 = CreateWorkflow(template, targetJobHeader);

			var externalJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var externalJobHeader = BMSTestHelper.CreateJobHeader(externalJob);
			var externalWorkflow1 = CreateWorkflow(template, externalJobHeader);
			var externalWorkflow2 = CreateWorkflow(template, externalJobHeader);

			var link1 = BMSTestHelper.CreateDependencyLink(template, sourceWorkflow1, externalWorkflow1);

			var link2 = BMSTestHelper.CreateDependencyLink(template, externalWorkflow2, sourceWorkflow1);

			var copiedEntities = new Dictionary<object, object>() {
				{ sourceWorkflow1, targetWorkflow1 },
				{ sourceJobHeader, targetJobHeader },
				{ sourceJob, targetJob },
			};

			var processHeaderLinkCopier = new ProcessHeaderLinkCopierForUniversalCopy();

			// Act
			processHeaderLinkCopier.FinishCopyAction(copiedEntities);

			// Assert
			var copiedLinks = copiedEntities.Values
				.Where(v => v is ProcessHeaderLink)
				.Select(l => (ProcessHeaderLink)l).ToArray();
			AssertEquals("Assert external dependencies not cloned", 0, copiedLinks.Length);
		}

		public void TestCopyProcessHeaderLinksForUniversalCopy_DoesNotCloneInterWorkflowProviderDependencies()
		{
			// Arrange
			VisualBoards.Business.Test.VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var sourceJob1 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJob1Header = BMSTestHelper.CreateJobHeader(sourceJob1);
			var sourceWorkflow1 = CreateWorkflow(template, sourceJob1Header);

			var sourceJob2 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJob2Header = BMSTestHelper.CreateJobHeader(sourceJob2);
			var sourceWorkflow2 = CreateWorkflow(template, sourceJob2Header);

			var targetJob1 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJob1Header = BMSTestHelper.CreateJobHeader(targetJob1);
			var targetWorkflow1 = CreateWorkflow(template, targetJob1Header);

			var targetJob2 = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJob2Header = BMSTestHelper.CreateJobHeader(targetJob2);
			var targetWorkflow2 = CreateWorkflow(template, targetJob2Header);

			var externalJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var externalJobHeader = BMSTestHelper.CreateJobHeader(externalJob);
			var externalWorkflow1 = CreateWorkflow(template, externalJobHeader);
			var externalWorkflow2 = CreateWorkflow(template, externalJobHeader);

			var link = BMSTestHelper.CreateDependencyLink(template, sourceWorkflow1, sourceWorkflow2);

			var copiedEntities = new Dictionary<object, object>() {
				{ sourceWorkflow1, targetWorkflow1 },
				{ sourceWorkflow2, targetWorkflow2 },
				{ sourceJob1Header, targetJob1Header },
				{ sourceJob2Header, targetJob2Header },
				{ sourceJob1, targetJob1 },
				{ sourceJob2, targetJob2 }
			};

			var processHeaderLinkCopier = new ProcessHeaderLinkCopierForUniversalCopy();

			// Act
			processHeaderLinkCopier.FinishCopyAction(copiedEntities);

			// Assert
			var copiedLinks = copiedEntities.Values
				.Where(v => v is ProcessHeaderLink)
				.Select(l => (ProcessHeaderLink)l).ToArray();
			AssertEquals("Assert inter-WorkflowProvider dependencies not cloned", 0, copiedLinks.Length);
		}

		public void TestCopyProcessHeaderLinksForUniversalCopy_DoesNotCloneAlreadyClonedDependencies()
		{
			// Arrange
			VisualBoards.Business.Test.VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var sourceJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJobHeader = BMSTestHelper.CreateJobHeader(sourceJob);
			var sourceWorkflow1 = CreateWorkflow(template, sourceJobHeader);
			var sourceWorkflow2 = CreateWorkflow(template, sourceJobHeader);

			var targetJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob);
			var targetWorkflow1 = CreateWorkflow(template, targetJobHeader);
			var targetWorkflow2 = CreateWorkflow(template, targetJobHeader);

			var link = BMSTestHelper.CreateDependencyLink(template, sourceWorkflow1, sourceWorkflow2);

			var clonedLink = BMSTestHelper.CreateDependencyLink(template, targetWorkflow1, targetWorkflow2);

			var copiedEntities = new Dictionary<object, object>() {
				{ sourceWorkflow1, targetWorkflow1 },
				{ sourceWorkflow2, targetWorkflow2 },
				{ sourceJobHeader, targetJobHeader },
				{ sourceJob, targetJob },
				{ link, clonedLink }
			};

			var processHeaderLinkCopier = new ProcessHeaderLinkCopierForUniversalCopy();

			// Act
			processHeaderLinkCopier.FinishCopyAction(copiedEntities);

			// Assert
			var copiedLinks = copiedEntities.Values
				.Where(v => v is ProcessHeaderLink)
				.Select(l => (ProcessHeaderLink)l).ToArray();
			AssertEquals("Assert dependencies not cloned again", 1, copiedLinks.Length);
		}

		public void TestCopyProcessHeaderLinksForUniversalCopy_DoesNotCloneChildParentLinks()
		{
			// Arrange
			VisualBoards.Business.Test.VisualBoardsTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSRegistry.Instance.WorkflowManagementMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var sourceJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var sourceJobHeader = BMSTestHelper.CreateJobHeader(sourceJob);
			var sourceWorkflow1 = CreateWorkflow(template, sourceJobHeader);
			var sourceWorkflow2 = CreateWorkflow(template, sourceJobHeader);

			var targetJob = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob);
			var targetWorkflow1 = CreateWorkflow(template, targetJobHeader);
			var targetWorkflow2 = CreateWorkflow(template, targetJobHeader);

			var link = BMSTestHelper.CreateParentChildLink(template, sourceWorkflow1, sourceWorkflow2);

			var copiedEntities = new Dictionary<object, object>() {
				{ sourceWorkflow1, targetWorkflow1 },
				{ sourceWorkflow2, targetWorkflow2 },
				{ sourceJobHeader, targetJobHeader },
				{ sourceJob, targetJob },
			};

			var processHeaderLinkCopier = new ProcessHeaderLinkCopierForUniversalCopy();

			// Act
			processHeaderLinkCopier.FinishCopyAction(copiedEntities);

			// Assert
			var copiedLinks = copiedEntities.Values
				.Where(v => v is ProcessHeaderLink)
				.Select(l => (ProcessHeaderLink)l).ToArray();
			AssertEquals("Assert child parent links not cloned", 0, copiedLinks.Length);
		}

		Business.ProcessHeader CreateWorkflow(ProcessTaskTemplate template, ProcessJobHeader jobHeader)
		{
			var workflow = BMSTestHelper.CreateWorkflow(template);
			workflow.FH_FH_ParentHeader = jobHeader.PK;
			workflow.FH_ParentId = jobHeader.FH_ParentId;
			workflow.FH_ParentTableCode = jobHeader.FH_ParentTableCode;
			return workflow;
		}
	}
}
