using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.BufferManagement.Service.Test
{
	public class ResponsiveTransferRuleRunnerTest : BMSTestCaseWithFactory
	{
		public void TestGetWorkflowsQuery_ShouldLoadOnlyTransferables()
		{
			var system = CreateSystem();
			var bucketFrom = CreateBucket(system, "from");
			var bucketTo = CreateBucket(system, "to");
			var link = LinkComponents(bucketFrom, bucketTo);
			var workflows = CreateWorkflows(bucketFrom, 4);

			Factory.Save();

			var dummyTransferablePKs = new Guid[] { workflows[0].PK.ToGuid(), workflows[1].PK.ToGuid() };
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, loggerMock.Object, dummyTransferablePKs);
			var workflowQuery = responsiveTransferRuleRunner.GetWorkflowQueryForTest(link, shouldProcessWorkflowsWithNextComponentSet: true);

			var loadedProcessHeaders = Factory.Load<ProcessHeader>(workflowQuery);

			AssertContainsExactElementsInAnyOrder(dummyTransferablePKs, loadedProcessHeaders.Select(ph => ph.PK.ToGuid()).ToArray());
			AssertEquals(true, workflowQuery.AllowTableValuedParameters);
		}

		public void TestLogInformation_ShouldAddUsingResponsiveTransferRuleRunner()
		{
			var system = CreateSystem();
			system.FS_Name = "My system";

			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, loggerMock.Object, Array.Empty<Guid>());

			responsiveTransferRuleRunner.LogInformationForTest("log me!");
			responsiveTransferRuleRunner.LogInformationForTest("log me again!");
			AssertContainsExactElementsInAnyOrder(new[]
			{
				(LogType.Debug, "My system: 0 workflows to process [ResponsiveTransferRuleRunner]"),
				(LogType.Information, "My system: log me! [ResponsiveTransferRuleRunner]"),
				(LogType.Information, "My system: log me again! [ResponsiveTransferRuleRunner]")
			}, logs.ToArray());
		}

		public void TestCreateStartingFactory_ShouldCreateStartingFactoryWithRefreshEnabled()
		{
			var notRefreshFactory = new BusinessObjectFactory();

			AssertEquals(false, notRefreshFactory.RefreshEnabled);

			var system = notRefreshFactory.NewWithValidTestData<BMSystem>();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, loggerMock.Object, Array.Empty<Guid>());
			var startingFactory = responsiveTransferRuleRunner.CurrentFactory_ExposedForTest;

			AssertEquals(true, startingFactory.RefreshEnabled);
		}

		#region Setting Dedicated Buffer

		public void TestDedicatedBuffer_ShouldBeSetFromSuitableActiveComponentLink_WithLowerLinkSequenceFirst()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var candidateComponent4 = BMSTestHelper.CreateBuffer(system, "Candidate component 4");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			var link4 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent4, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 5;
			link3.FL_Sequence = 7;
			link4.FL_Sequence = 2;
			candidateComponent1.FC_DisplaySequence = 1;
			candidateComponent2.FC_DisplaySequence = 2;
			candidateComponent3.FC_DisplaySequence = 3;
			candidateComponent4.FC_DisplaySequence = 4;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link4.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Red",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Should take Component To from the suitable link with the lowest FL_Sequence", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("The workflow should stay in the same component - not transferred/released", currentComponent.PK, workflow.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2, Sequence: 5]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 3, Sequence: 7]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestDedicatedBuffer_ShouldBeSetFromSuitableComponentLink_WithLowerComponentToDisplaySequenceNext()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var candidateComponent4 = BMSTestHelper.CreateBuffer(system, "Candidate component 4");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			var link4 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent4, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 10;
			link3.FL_Sequence = 10;
			link4.FL_Sequence = 10;
			candidateComponent1.FC_DisplaySequence = 7;
			candidateComponent2.FC_DisplaySequence = 5;
			candidateComponent3.FC_DisplaySequence = 3;
			candidateComponent4.FC_DisplaySequence = 2;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link4.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Red",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Should take Component To from the suitable link with the lowest FC_DisplaySequence on To components when link sequences are the same", candidateComponent3.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("The workflow should stay in the same component - not transferred/released", currentComponent.PK, workflow.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 3 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 3, Sequence: 10]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestHavingDedicatedBufferSetBeforeRun_ShouldNotPreventUpdatingDedicatedBufferByLinksRequiringReleaseGate()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBucket(system, "Initial dedicated buffer");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("The dedicated buffer should be updated even when being set before run", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestHavingDedicatedBufferSetBeforeRun_ShouldNotPreventTransferByLinksNotRequiringReleaseGate()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBuffer(system, "Initial dedicated buffer");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false); // does not require release gate

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("The workflow should be transferred even having dedicated buffer set before run", candidateComponent1.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("The dedicated buffer should be reset after the transfer", Guid.Empty, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Moved workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) from component Current component to Candidate component 1 [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: 1 workflow transferred [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestSettingDedicatedBuffer_ShouldExcludeFurtherTransfersOrSettingDedicatedBufferAgainInTheSameRun()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1 (Buffer)");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2 (Buffer)");
			var candidateComponent3 = BMSTestHelper.CreateBucket(system, "Candidate component 3 (Bucket)");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: false);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;
			link3.FL_Sequence = 2;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "ABC",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "AB",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "A",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = currentComponent.PK;
			workflow1.FH_CompletionStatement = "ABC___";

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = currentComponent.PK;
			workflow2.FH_CompletionStatement = "AB___";

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = currentComponent.PK;
			workflow3.FH_CompletionStatement = "A___";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow1.PK.ToGuid(), workflow2.PK.ToGuid(), workflow3.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();

			CombineAssertions("When running the first time", () =>
			{
				AssertEquals("Workflow1 should stay in the same component - not transferred", currentComponent.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals("Should set dedicated buffer for workflow1 using the first link", candidateComponent1.PK, workflow1.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow2 should stay in the same component - not transferred", currentComponent.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals("Should set dedicated buffer for workflow2 using the second link", candidateComponent2.PK, workflow2.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow3 should be transferred", candidateComponent3.PK, workflow3.FH_FC_CurrentComponent);
				AssertEquals("Should not set dedicated buffer for workflow3", ZGuid.Empty, workflow3.FH_FC_DedicatedBuffer);

				AssertMultilineASCIIEquals($@"Debug - Test System: 3 workflows to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 1 (Buffer) on workflow ABC___ (PK = {workflow1.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1 (Buffer), Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 2 (Buffer) on workflow AB___ (PK = {workflow2.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2 (Buffer), Sequence: 1]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Moved workflow A___ (PK = {workflow3.PK}, Job = Dummy Business Object Default) from component Current component to Candidate component 3 (Bucket) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 3 (Bucket), Sequence: 2]: 1 workflow transferred [ResponsiveTransferRuleRunner]
", logger.ToString());
			});

			logger = new BufferManagementLogger();
			responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow1.PK.ToGuid(), workflow2.PK.ToGuid(), workflow3.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();

			CombineAssertions("When running the second time", () =>
			{
				AssertEquals("Workflow1 should stay in the same component - not transferred", currentComponent.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals("The dedicated buffer for workflow1 should stay the same", candidateComponent1.PK, workflow1.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow2 should stay in the same component - not transferred", currentComponent.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals("The dedicated buffer for workflow2 should stay the same", candidateComponent2.PK, workflow2.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow3 should stay in the same component - it already transferred", candidateComponent3.PK, workflow3.FH_FC_CurrentComponent);
				AssertEquals("Should not set dedicated buffer for workflow3", ZGuid.Empty, workflow3.FH_FC_DedicatedBuffer);

				AssertMultilineASCIIEquals($@"Debug - Test System: 3 workflows to process [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1 (Buffer), Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 0 workflows [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2 (Buffer), Sequence: 1]: calculated dedicated buffer for 1 workflow, updated on 0 workflows [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 3 (Bucket), Sequence: 2]: 0 workflows transferred [ResponsiveTransferRuleRunner]
", logger.ToString());
			});
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreInactiveComponentLinks()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 9;
			link3.FL_Sequence = 8;

			link3.FL_TransferRulesEnabled = false;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Should ignore inactive component links", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2, Sequence: 9]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreLinksLeadingToInactiveComponents()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 11;
			link3.FL_Sequence = 12;

			candidateComponent1.FC_IsActive = false;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Should ignore links that lead to inactive components", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2, Sequence: 11]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 3, Sequence: 12]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreInactiveWorkflows()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";
			workflow.FH_IsActive = false;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Inactive workflow should not update its dedicated buffer", Guid.Empty, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
", logger.ToString());

			workflow.FH_IsActive = true;
			Factory.Save();

			logger = new BufferManagementLogger();
			responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Active workflow should update its dedicated buffer", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreWorkflowsWithNoCurrentComponent()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent = BMSTestHelper.CreateBuffer(system, "Candidate component");
			var link = BMSTestHelper.LinkComponents(currentComponent, candidateComponent, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = ZGuid.Empty;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			AssertEquals("A workflow with no current component set should not update its dedicated buffer", Guid.Empty, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
", logger.ToString());

			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			Factory.Save();

			logger = new BufferManagementLogger();
			responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("A workflow with a current component set should update its dedicated buffer", candidateComponent.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestDedicatedBuffer_ShouldStayUnchanged_WhileBMSIsNotLive()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Should take Component To from the suitable link with the lowest FL_Sequence", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2, Sequence: 1]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
", logger.ToString());

			system.FS_IsLive = false;

			Factory.Save();

			link1.FL_Sequence = 1;
			link2.FL_Sequence = 0;

			Factory.Save();

			logger = new BufferManagementLogger();
			responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Dedicated buffer should stay untouched when the system is deactivated", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
", logger.ToString());

			system.FS_IsLive = true;

			Factory.Save();

			logger = new BufferManagementLogger();
			responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Dedicated buffer should be taken from another link now as the system is active again", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 2, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 1]: calculated dedicated buffer for 0 workflows, updated on 0 workflows [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestSettingDedicatedBuffer_ShouldNotCauseWorkflowDeactivation()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0, isReleaseGate: false);
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer, sequence: 1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertEquals("Should transfer", bucket2.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("Should determine the dedicated buffer", buffer.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("Should stay active", true, workflow.FH_IsActive);
		}

		public void TestDedicatedBuffer_ShouldBeSetToNullAfterTransfer()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var buffer = CreateBuffer(system, "Buffer");

			LinkComponents(bucket1, bucket2);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			workflow.FH_FC_DedicatedBuffer = buffer.PK;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();
			AssertEquals(bucket2.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertEquals(ZGuid.Empty, workflow.FH_FC_DedicatedBuffer);
		}

		public void TestShouldSetDedicatedBufferToNull_WhenThereIsNoSuitableLink()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBuffer(system, "Initial dedicated buffer");
			var candidateComponent = BMSTestHelper.CreateBucket(system, "Candidate component");
			var link = BMSTestHelper.LinkComponents(currentComponent, candidateComponent);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Red";
			workflow1.FH_FC_CurrentComponent = currentComponent.PK;
			workflow1.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow1.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow1.Reload();
			AssertEquals("Should set dedicated buffer to Guid.Empty (null in the database) as there is no suitable link", Guid.Empty, workflow1.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow1.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for 1 workflow [ResponsiveTransferRuleRunner]", logger.ToString());
		}

		#region Save Concurrency Exceptions

		public void TestShouldSuccessfullySetDedicatedBufferToNull_AfterTwoSaveConcurrencyExceptions_WithoutLogging()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBucket(system, "Initial dedicated buffer");
			var candidateComponent = BMSTestHelper.CreateBucket(system, "Candidate component");
			var link = BMSTestHelper.LinkComponents(currentComponent, candidateComponent);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Red"; // not eligible for transfer

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			var saveAttempts = 0;
			responsiveTransferRuleRunner.OnNullSettingBatchSaving_ForTest = () =>
			{
				if (saveAttempts < 2)
				{
					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
					loadedWorkflow.FH_IsStandby = !loadedWorkflow.FH_IsStandby; // this will generate save concurrency exception
					newFactory.Save();
				}
				saveAttempts++;
			};

			Assert("Precondition", !responsiveTransferRuleRunner.HadConcurrencyException_ForTest);

			responsiveTransferRuleRunner.Process(new CancellationToken());

			Assert(responsiveTransferRuleRunner.HadConcurrencyException_ForTest);
			AssertEquals("Should successfully set dedicated buffer to Guid.Empty (effectively null in the database) after up to two save concurrency exceptions when trying to do that", Guid.Empty, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for 1 workflow [ResponsiveTransferRuleRunner]", logger.ToString());
			AssertNotContains("CONCURRENCY Error Saving Record", logger.ToString(), ignoreCase: true);
		}

		public void TestShouldGiveUpSettingDedicatedBufferToNull_AfterTwoSaveConcurrencyExceptions_WithLogging()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBucket(system, "Initial dedicated buffer");
			var candidateComponent = BMSTestHelper.CreateBucket(system, "Candidate component");
			var link = BMSTestHelper.LinkComponents(currentComponent, candidateComponent);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Red"; // not eligible for transfer

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			var saveAttempts = 0;
			responsiveTransferRuleRunner.OnNullSettingBatchSaving_ForTest = () =>
			{
				if (saveAttempts < 3)
				{
					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
					loadedWorkflow.FH_IsStandby = !loadedWorkflow.FH_IsStandby; // this will generate save concurrency exception
					newFactory.Save();
				}
				saveAttempts++;
			};

			Assert("Precondition", !responsiveTransferRuleRunner.HadConcurrencyException_ForTest);

			responsiveTransferRuleRunner.Process(new CancellationToken());

			Assert(responsiveTransferRuleRunner.HadConcurrencyException_ForTest);
			AssertEquals("Should give up setting dedicated buffer to Guid.Empty (effectively null in the database) after three save concurrency exceptions and leave dedicated buffer unchanged", initialDedicatedBuffer.PK, workflow.FH_FC_DedicatedBuffer);

#if NETFRAMEWORK
			var expectedMessage = $@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Warning - Test System: Concurrency error: CargoWise.PAVE.Common.Interfaces.SaveConcurrencyException: Cannot save due to save concurrency error ---> CargoWise.EntityFramework.ZSaveConcurrencyException: 
**CONCURRENCY Error Saving Record **";
#else
			var expectedMessage = $@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Warning - Test System: Concurrency error: CargoWise.PAVE.Common.Interfaces.SaveConcurrencyException: Cannot save due to save concurrency error
 ---> CargoWise.EntityFramework.ZSaveConcurrencyException: 
**CONCURRENCY Error Saving Record **";
#endif

			AssertContains(expectedMessage, logger.ToString(), ignoreCase: true);
		}

		#endregion

		public void TestShouldNotSetDedicatedBufferToNull_WhenResettingIsDisabledInRegistry()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BMSRegistry.Instance.ResetDedicatedBufferOnWorkflowsNotMatchingComponentLinks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBuffer(system, "Initial dedicated buffer");
			var candidateComponent = BMSTestHelper.CreateBucket(system, "Candidate component");
			var link = BMSTestHelper.LinkComponents(currentComponent, candidateComponent);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Red";
			workflow1.FH_FC_CurrentComponent = currentComponent.PK;
			workflow1.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow1.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow1.Reload();
			AssertEquals("Should not set dedicated buffer to Guid.Empty as disabled in the registry", initialDedicatedBuffer.PK, workflow1.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Debug - Test System: Skipped resetting dedicated buffers as disabled in the registry [ResponsiveTransferRuleRunner]", logger.ToString());

			BMSRegistry.Instance.ResetDedicatedBufferOnWorkflowsNotMatchingComponentLinks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			logger = new BufferManagementLogger();
			responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow1.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow1.Reload();
			AssertEquals("Should set dedicated buffer to Guid.Empty (null in the database) as there is no suitable link", Guid.Empty, workflow1.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow1.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for 1 workflow [ResponsiveTransferRuleRunner]", logger.ToString());
		}

		public void TestDedicatedBuffer_ShouldNotResetOnWorkflowsRelatedToAnotherBMS()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system1 = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system1.FS_Name = "Test System 1";
			var bucket1 = BMSTestHelper.CreateBucket(system1, "Bucket 1");
			var buffer1 = BMSTestHelper.CreateBuffer(system1, "Buffer 1");
			var link1 = BMSTestHelper.LinkComponents(bucket1, buffer1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var system2 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system2.FS_Name = "Test System 2";
			var bucket2 = BMSTestHelper.CreateBucket(system2, "Bucket 2");
			var buffer2 = BMSTestHelper.CreateBuffer(system2, "Buffer 2");
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer2, isReleaseGate: true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader1.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			workflow1.FH_CompletionStatement = "Green";

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow2 = jobHeader2.ProcessHeaders[0];
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			workflow2.FH_CompletionStatement = "Workflow belonging to System 2";
			workflow2.FH_FC_DedicatedBuffer = buffer2.PK;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system1, logger, [workflow1.PK.ToGuid(), workflow2.PK.ToGuid()]);
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow1.Reload();

			AssertEquals("Should set dedicated buffer on a workflow belonging to the target system", buffer1.PK, workflow1.FH_FC_DedicatedBuffer);
			AssertEquals("Should not reset dedicated buffer on a workflow not belonging to the target system", buffer2.PK, workflow2.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Debug - Test System 1: 2 workflows to process [ResponsiveTransferRuleRunner]
Test System 1: Set dedicated buffer Buffer 1 on workflow Green (PK = {workflow1.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System 1: Component link [Bucket 1 -> Buffer 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		#endregion

		#region Logging

		public void TestLog_Transfer_LogWorkflowInfoOnTransferEnabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Moved workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) from component Current component to Candidate component 1 [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: 1 workflow transferred [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestLog_Transfer_LogWorkflowInfoOnTransferDisabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: 1 workflow transferred [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestLog_SettingDedicatedBuffer_LogWorkflowInfoOnTransferEnabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestLog_SettingDedicatedBuffer_LogWorkflowInfoOnTransferDisabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestLog_ResettingDedicatedBuffer_LogWorkflowInfoOnTransferEnabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBuffer(system, "Initial dedicated buffer");
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Red";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for workflow Red (PK = {workflow.PK}, Job = Dummy Business Object Default) [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		public void TestLog_ResettingDedicatedBuffer_LogWorkflowInfoOnTransferDisabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBuffer(system, "Initial dedicated buffer");
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Red";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(system, logger, new[] { workflow.PK.ToGuid() });
			responsiveTransferRuleRunner.Process(new CancellationToken());

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Debug - Test System: 1 workflow to process [ResponsiveTransferRuleRunner]
Test System: Resetting Dedicated Buffer for active workflows no longer eligible for release: [ResponsiveTransferRuleRunner]
Test System: Reset dedicated buffer for 1 workflow [ResponsiveTransferRuleRunner]
", logger.ToString());
		}

		#endregion

		#region Db Hits

		public void TestDbHits_WorkflowTransferWithoutDeactivation()
		{
			const int batchSize = 10;
			BMSRegistry.Instance.TransferRuleBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var currentComponent1 = BMSTestHelper.CreateBucket(system, "Current component 1", sequence: 0);
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1", sequence: 1);
			var candidateComponent2 = BMSTestHelper.CreateBucket(system, "Candidate component 2", sequence: 2);
			var link1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent1, isReleaseGate: false);
			var link2 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent2, isReleaseGate: false);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});

			const int workflowBatchesForLink1Count = 4;
			const int workflowsForLink1Count = workflowBatchesForLink1Count * batchSize;

			var workflowsForLink1 = new ProcessHeader[workflowsForLink1Count];

			for (int i = 0; i < workflowsForLink1Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate1_{i}";
				workflowsForLink1[i] = workflow;
			}

			const int workflowBatchesForLink2Count = 2;
			const int workflowsForLink2Count = workflowBatchesForLink2Count * batchSize;
			var workflowsForLink2 = new ProcessHeader[workflowsForLink2Count];

			for (int i = 0; i < workflowsForLink2Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate2_{i}";
				workflowsForLink2[i] = workflow;
			}

			const int batchCount = workflowBatchesForLink1Count + workflowBatchesForLink2Count;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(newFactory.Load<BMSystem>(system.PK), logger, workflowsForLink1.Union(workflowsForLink2).Select(w => w.PK.ToGuid()).ToArray());

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentLinkSchema.Constants.TableName, 0 },

					// 3 hits per batch:
					// 1) LinksProcessorWithDeactivation.ProcessLink - loading a batch
					// 2) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.LoadWorkflowsToTransferCore
					// 3) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.AddFetchHintsForWorkflowsToTransfer
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 3 },

					// 2 hits per batch:
					// 1) TransferRuleRunnerDataAccessor.LoadWorkflowsToTransfer - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 2 },

					// 1 hits per batch in TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer() for updating Planned Duration on save in ProcessHeader.OnSavingForService
				{ ProcessTasksSchema.Constants.TableName, batchCount },

					// 1 hit per batch: TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer - SavePendingWorkflows - ProcessHeader.UpdateDatesFromJob - Parent
				{ DummyBizoSchema.Constants.TableName, batchCount },
				{ TagLinkSchema.Constants.TableName, 0 }
			};

			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				responsiveTransferRuleRunner.Process(new CancellationToken());
			}

			foreach (var workflow in workflowsForLink1)
			{
				AssertEquals(candidateComponent1.PK, workflow.FH_FC_CurrentComponent);
				AssertEquals("Should not be deactivated", true, workflow.FH_IsActive);
			}

			foreach (var workflow in workflowsForLink2)
			{
				AssertEquals(candidateComponent2.PK, workflow.FH_FC_CurrentComponent);
				AssertEquals("Should not be deactivated", true, workflow.FH_IsActive);
			}
		}

		public void TestDbHits_WorkflowTransferAndDeactivation()
		{
			const int batchSize = 10;
			BMSRegistry.Instance.TransferRuleBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var currentComponent1 = BMSTestHelper.CreateBucket(system, "Current component 1", sequence: 0);
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1", sequence: 1);
			var candidateComponent2 = BMSTestHelper.CreateBucket(system, "Candidate component 2", sequence: 2);
			var link1_1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent1, isReleaseGate: false);
			var link1_2 = BMSTestHelper.LinkComponents(candidateComponent1, currentComponent1, isReleaseGate: false); // loop
			var link2_1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent2, isReleaseGate: false);
			var link2_2 = BMSTestHelper.LinkComponents(candidateComponent2, currentComponent1, isReleaseGate: false); // loop
			link1_1.FL_Sequence = 0;
			link2_1.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1_1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2_1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2_2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});

			const int workflowBatchesForLinks1Count = 4;
			const int workflowsForLinks1Count = workflowBatchesForLinks1Count * batchSize;
			var workflowsForLink1 = new ProcessHeader[workflowsForLinks1Count];

			for (int i = 0; i < workflowsForLinks1Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate1_{i}";
				workflowsForLink1[i] = workflow;
			}

			const int workflowBatchesForLinks2Count = 2;
			const int workflowsForLink2Count = workflowBatchesForLinks2Count * batchSize;
			var workflowsForLink2 = new ProcessHeader[workflowsForLink2Count];

			for (int i = 0; i < workflowsForLink2Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate2_{i}";
				workflowsForLink2[i] = workflow;
			}

			const int batchCount = (workflowBatchesForLinks1Count + workflowBatchesForLinks2Count) * 2; // each workflow is processed by two links (back and forth)

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(newFactory.Load<BMSystem>(system.PK), logger, workflowsForLink1.Union(workflowsForLink2).Select(w => w.PK.ToGuid()).ToArray());

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentLinkSchema.Constants.TableName, 0 },
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 3 + 2 },

					// 2 hits per batch:
					// 1) TransferRuleRunnerDataAccessor.LoadWorkflowsToTransfer - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo

					// Plus 2 extra db hits:
					// 1) TransferRuleRunnerDataAccessor.LoadWorkflowsToDeactivate - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 2 + 2 },

					// 1 hits per batch in TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer() for updating Planned Duration on save in ProcessHeader.OnSavingForService
					// Plus 1 extra db hit for doing the same when deactivating workflows
				{ ProcessTasksSchema.Constants.TableName, batchCount + 1 },

					// 1 hit per batch: TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer - SavePendingWorkflows - ProcessHeader.UpdateDatesFromJob - Parent
					// Plus one extra db hit for displaying job names in logs when deactivating workflows: TransferRuleRunner.ShouldSaveOnProcessedFunc - ProcessHeader.ParentJobDescription
				{ DummyBizoSchema.Constants.TableName, batchCount + 1 },
				{ TagLinkSchema.Constants.TableName, 0 }
			};

			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				responsiveTransferRuleRunner.Process(new CancellationToken());
			}

			foreach (var workflow in workflowsForLink1.Union(workflowsForLink2))
			{
				AssertEquals(currentComponent1.PK, workflow.FH_FC_CurrentComponent);
				AssertEquals("Should be deactivated", false, workflow.FH_IsActive);
			}
		}

		public void TestDbHits_SettingDedicatedBuffer()
		{
			const int batchSize = 10;
			BMSRegistry.Instance.TransferRuleBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var currentComponent1 = BMSTestHelper.CreateBucket(system, "Current component 1");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var link1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent2, isReleaseGate: true);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});

			const int workflowBatchesForLink1Count = 4;
			const int workflowsForLink1Count = workflowBatchesForLink1Count * batchSize;
			var workflowsForLink1 = new ProcessHeader[workflowsForLink1Count];

			for (int i = 0; i < workflowsForLink1Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate1_{i}";
				workflowsForLink1[i] = workflow;
			}

			const int workflowBatchesForLink2Count = 2;
			const int workflowsForLink2Count = workflowBatchesForLink2Count * batchSize;
			var workflowsForLink2 = new ProcessHeader[workflowsForLink2Count];

			for (int i = 0; i < workflowsForLink2Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate2_{i}";
				workflowsForLink2[i] = workflow;
			}

			const int batchCount = workflowBatchesForLink1Count + workflowBatchesForLink2Count;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var logger = new BufferManagementLogger();
			var responsiveTransferRuleRunner = new DummyResponsiveTransferRuleRunner(newFactory.Load<BMSystem>(system.PK), logger, workflowsForLink1.Union(workflowsForLink2).Select(w => w.PK.ToGuid()).ToArray());

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentLinkSchema.Constants.TableName, 0 },

					// 3 hits per batch:
					// 1) LinksProcessorWithDeactivation.ProcessLink - loading a batch
					// 2) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.LoadWorkflowsToTransferCore
					// 3) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.AddFetchHintsForWorkflowsToTransfer
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 3 },

					// 2 hits per batch:
					// 1) TrasnferRuleRunnerDataAccessor.LoadWorkflowsToTransfer - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 2 },

					// 1 hits per batch in TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer() for updating Planned Duration on save in ProcessHeader.OnSavingForService
				{ ProcessTasksSchema.Constants.TableName, batchCount },

					// 1 hit per batch: TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer - SavePendingWorkflows - ProcessHeader.UpdateDatesFromJob - Parent
				{ DummyBizoSchema.Constants.TableName, batchCount }
			};

			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				responsiveTransferRuleRunner.Process(new CancellationToken());
			}
		}

		#endregion

		#region Setup

		#region Dummies

		class DummyResponsiveTransferRuleRunner : ResponsiveTransferRuleRunner
		{
			public DummyResponsiveTransferRuleRunner(BMSystem system, ILogger logger, IReadOnlyCollection<Guid> transferablesPKs)
				: this(system, new ResponsiveTransferRuleRunnerLogger(logger, system), transferablesPKs)
			{
			}

			DummyResponsiveTransferRuleRunner(BMSystem system, ResponsiveTransferRuleRunnerLogger logger, IReadOnlyCollection<Guid> transferablesPKs)
				: this(system, logger, new ResponsiveTransferRuleRunnerDataAccessor(logger, transferablesPKs), transferablesPKs)
			{
			}

			DummyResponsiveTransferRuleRunner(BMSystem system, ResponsiveTransferRuleRunnerLogger logger, ResponsiveTransferRuleRunnerDataAccessor dataAccessor, IReadOnlyCollection<Guid> transferablesPKs)
				: base(system, dataAccessor, logger, transferablesPKs)
			{
				responsiveTransferRuleRunnerDataAccessor = dataAccessor;
			}

			public ZQuery GetWorkflowQueryForTest(BMComponentLink link, bool shouldProcessWorkflowsWithNextComponentSet)
			{
				var (query, queryName) = responsiveTransferRuleRunnerDataAccessor.GetWorkflowQuery_ExposedForTest(link);
				return query;
			}

			readonly ResponsiveTransferRuleRunnerDataAccessor responsiveTransferRuleRunnerDataAccessor;

			public BusinessObjectFactory CurrentFactory_ExposedForTest => responsiveTransferRuleRunnerDataAccessor.Current;

			public void LogInformationForTest(string message)
			{
				Logger.Log(message);
			}
		}

		#endregion

		Mock<ILogger> loggerMock;
		readonly List<(LogType type, string message)> logs = new List<(LogType, string)>();

		protected override void SetUp()
		{
			base.SetUp();

			Globals.IsWebService = true;
			loggerMock = new Mock<ILogger>();
			loggerMock.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType type, string message) => logs.Add((type, message)));

			BMSTestHelper.CreateServiceTask_WithoutAssemblyReference(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			Globals.IsWebService = false;
		}

		#endregion
	}
}
