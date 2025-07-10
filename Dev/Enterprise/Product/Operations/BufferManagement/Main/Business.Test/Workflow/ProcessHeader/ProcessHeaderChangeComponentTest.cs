using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderChangeComponentTest : BMSTestCaseWithFactory
	{
		public void TestLastTransferType_ManualTransfer()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");

			AssertEquals("The workflow's last transfer type should be the default when it is first created/added to its starting bucket. SAD!", "NA", workflow.FH_LastTransferType);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;

			AssertEquals("Manually transferring the workflow should have set the LastTransferType correctly. SAD!", TransferTypeList.Codes.ManualTransfer, workflow.FH_LastTransferType);
		}

		public void TestLastTransferType_Defer()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(workflow, Enumerable.Empty<ZGuid>());
			viewModel.Defer();

			AssertEquals("Deferring the workflow should have set the LastTransferType correctly. SAD!", TransferTypeList.Codes.Defer, workflow.FH_LastTransferType);
		}

		public void TestLastTransferType_SchematicTransfer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");

			Factory.Save();

			var runner = new TestTransferRuleRunner(system, new LoggerForTest());
			runner.Process_ForTest();

			AssertEquals("The workflow should have been transferred. SAD!", bucket2.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertEquals("The Transfer Rule Runner should have set the LastTransferType correctly. SAD!", TransferTypeList.Codes.SchematicTransfer, workflow.FH_LastTransferType);
		}

		public void TestLastTransferType_ResponsiveTransfer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			Factory.Save();

			var mock = new Mock<ITransferRuleRunnerParams>();
			mock.Setup(b => b.IsResponsive).Returns(true);
			mock.Setup(b => b.IsCdcEnabled).Returns(true);

			var runner = new TestTransferRuleRunner(system, new LoggerForTest(), mock.Object);
			runner.Process_ForTest();

			AssertEquals("The workflow should have been transferred. SAD!", bucket2.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertEquals("The Transfer Rule Runner should have set the LastTransferType correctly. SAD!", TransferTypeList.Codes.ResponsiveTransfer, workflow.FH_LastTransferType);
		}

		public void TestLastTransferType_ReleaseGate()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Bucket.FromMeToOthersLinks.Single().FL_IsReleaseGateRuleApplied = true;
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ZZZ", "Sir Rupert Everton");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, lowEstMinutes: 5);
			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);
			workflow.Reload();

			AssertEquals("The workflow should have been transferred. SAD!", config.Buffer.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertEquals("The Release Gate Runner should have set the LastTransferType correctly. SAD!", TransferTypeList.Codes.ReleaseGate, workflow.FH_LastTransferType);
		}

		public void TestComponentChangeMode_AndTransferTypeList_IsCoveredByProcessHeaderChangeComponentTests()
		{
			var expectedCodes = new[]
			{
				"DFR",
				"MAN",
				"REL",
				"XFR",
				"RXR"
			};

			const string failureMessage = "It looks like you've added a new code or enum to TransferTypeList or ComponentChangeMode. Please add a test for this new mode to the the place where this test failed to cover the new case. Cheers!";

			var transferTypeListCodes = new TransferTypeList().GetAllCodes();
			Assertion.AssertContainsExactElementsInAnyOrder(failureMessage, expectedCodes, transferTypeListCodes);

			var componentChangeModeCodes = new List<string>();

			foreach (ComponentChangeMode mode in Enum.GetValues(typeof(ComponentChangeMode)))
			{
				componentChangeModeCodes.Add(mode.ToCode());
			}

			Assertion.AssertContainsExactElementsInAnyOrder(failureMessage, expectedCodes, componentChangeModeCodes);
		}

		public void TestTransferTypeList_ShouldNotContainDefaultValueForTransferType()
		{
			var actualCodes = new TransferTypeList().GetAllCodes();
			AssertCollectionNotContains("The code 'NA' is the default value to indicate that a workflow hasn't been transferred (or its last transfer couldn't be determined by the online transformation), so please don't use it as a valid option for a transfer type.",
				BMConstants.LastTransferTypeDefaultCode, actualCodes);
		}

		protected override void SetUp()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			base.SetUp();

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}
	}
}
