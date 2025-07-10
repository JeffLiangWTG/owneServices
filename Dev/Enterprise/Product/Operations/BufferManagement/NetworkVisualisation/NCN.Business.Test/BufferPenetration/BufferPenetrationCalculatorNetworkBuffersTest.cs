using System;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestDate(2014, 9, 1)]
	class BufferPenetrationCalculatorNetworkBuffersTest : NetworkTestCase
	{
		#region Future Start Date

		public void TestBufferPenetration_ForProjectStartingInFuture()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Kurt found this defect.", releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60 * 8);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(jobHeader, diagram);
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddHours(2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			shape.AsEntity(network).Width = 100; // One scale unit = 8 hours. Workflow std est is 12 hours.

			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			networkViewModel.GetJobController().TriggerSaveAction();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals("Job starts in the future, so there should be less buffer penetration than would otherwise be the case", 0.25m, buffer.BufferPenetration);
		}

		public void TestBufferPenetration_ForProjectStartingInPast()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Kurt found this defect.", releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60 * 8);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(jobHeader, diagram);
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddHours(-2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			shape.AsEntity(network).Width = 100; // One scale unit = 8 hours. Workflow std est is 12 hours.

			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			networkViewModel.GetJobController().TriggerSaveAction();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals("Job starts in the past, so buffer penetration should be calculated", 0.625m, buffer.BufferPenetration);
		}

		#endregion

		#region Downstream Propagation

		public void TestCalculatePenetrationPercentage_ForNetworkBuffers_ShouldPropagateZoneZeroness()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-15);

			var workflow1_cc = CreateWorkflow(jobHeader, "workflow1_cc");
			var workflow2_cc = CreateWorkflow(jobHeader, "workflow2_cc");
			var workflow3_cc = CreateWorkflow(jobHeader, "workflow3_cc");
			var workflow4_cc = CreateWorkflow(jobHeader, "workflow4_cc");
			var workflow5_nonCC = CreateWorkflow(jobHeader, "workflow5_nonCC");
			var workflow6_nonCC = CreateWorkflow(jobHeader, "workflow6_nonCC");
			var workflow7_nonCC = CreateWorkflow(jobHeader, "workflow7_nonCC");
			var workflow8_nonCC = CreateWorkflow(jobHeader, "workflow8_nonCC");

			CreateTask(workflow1_cc, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			CreateTask(workflow2_cc, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			CreateTask(workflow3_cc, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			CreateTask(workflow4_cc, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			CreateTask(workflow5_nonCC, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay);
			CreateTask(workflow6_nonCC, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay);
			CreateTask(workflow7_nonCC, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay);
			CreateTask(workflow8_nonCC, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay);

			var diagram = CreateDiagram(jobHeader);
			var shape1_cc = CreateShape(workflow1_cc, diagram, "shape1_cc");
			var shape2_cc = CreateShape(workflow2_cc, diagram, "shape2_cc");
			var shape3_cc = CreateShape(workflow3_cc, diagram, "shape3_cc");
			var shape4_cc = CreateShape(workflow4_cc, diagram, "shape4_cc");

			var shape5_nonCC = CreateShape(workflow5_nonCC, diagram, "shape5_nonCC");
			var shape6_nonCC = CreateShape(workflow6_nonCC, diagram, "shape6_nonCC");
			var shape7_nonCC = CreateShape(workflow7_nonCC, diagram, "shape7_nonCC");
			var shape8_nonCC = CreateShape(workflow8_nonCC, diagram, "shape8_nonCC");

			// CC chain
			shape1_cc.MakeVisiblePrerequisiteOf(shape2_cc, diagram);
			shape2_cc.MakeVisiblePrerequisiteOf(shape3_cc, diagram);
			shape3_cc.MakeVisiblePrerequisiteOf(shape4_cc, diagram);

			// Feeding chain 1 hits CC at #2
			shape5_nonCC.MakeVisiblePrerequisiteOf(shape6_nonCC, diagram);
			shape6_nonCC.MakeVisiblePrerequisiteOf(shape2_cc, diagram);

			// Feeding chain 2 hits CC at #4
			shape7_nonCC.MakeVisiblePrerequisiteOf(shape8_nonCC, diagram);
			shape8_nonCC.MakeVisiblePrerequisiteOf(shape4_cc, diagram);

			// 1 --------> 2 -> 3 ---> 4
			// 5 -> 6 -||> 2
			//             7 -> 8 -||> 4

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			shape1_cc.AsEntity(network).Width = 1000;
			shape2_cc.AsEntity(network).Width = 1000;
			shape3_cc.AsEntity(network).Width = 1000;
			shape4_cc.AsEntity(network).Width = 1000;
			shape5_nonCC.AsEntity(network).Width = 100;
			shape6_nonCC.AsEntity(network).Width = 100;
			shape7_nonCC.AsEntity(network).Width = 100;
			shape8_nonCC.AsEntity(network).Width = 100;

			networkViewModel.PushAsLateAsPossible();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var projectBuffer = network.Shapes.OfType<IBuffer>().Single(b => b.Type == BufferType.Project);
			var feedingBuffer1 = network.Shapes.OfType<IBuffer>().Single(b => b.Name == "shape6_nonCC Feeding Buffer");
			var feedingBuffer2 = network.Shapes.OfType<IBuffer>().Single(b => b.Name == "shape8_nonCC Feeding Buffer");

			CombineAssertions("Buffer penetrations", () =>
			{
				AssertEquals("projectBuffer - should get excess zone zero from feedingBuffer1, without re-considering planned/actual differential", 0.13125m, BufferPenetrationCalculator.CalculatePenetrationPercentage(projectBuffer, context, Factory).Penetration);
				AssertEquals("feedingBuffer1", 3.625m, BufferPenetrationCalculator.CalculatePenetrationPercentage(feedingBuffer1, context, Factory).Penetration);
				AssertEquals("feedingBuffer2 - starts in the future so estimate to remaining differential is absorbed", 0.0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(feedingBuffer2, context, Factory).Penetration);
			});
		}

		#endregion

		#region Root Diagram Hidden Entities

		[TestDate(2019, 1, 1)]
		public void TestBufferPenetration_WorkflowWithinJobLinkedToRootDiagram_ShouldUsePlannedDurationOfDiagram()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.ReleaseGroup.Staff.Add(Factory.NewWithValidTestData<GlbStaff>()); // to avoid release group validation errors on shape's workflow

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1_shownOnDiagram = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow the First", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2_shownOnDiagram = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow the Second", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3_notShownOnDiagram = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow the Third", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK, releaseDateTime: ZDateTime.UtcNow.AddDays(-6));

			BMSTestHelper.CreateTask(workflow1_shownOnDiagram, lowEstMinutes: 60 * 3 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1);
			BMSTestHelper.CreateTask(workflow2_shownOnDiagram, lowEstMinutes: 60 * 3 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1);
			BMSTestHelper.CreateTask(workflow3_notShownOnDiagram, lowEstMinutes: 60 * 3 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var shape1 = NetworkTestCase.CreateShape(workflow1_shownOnDiagram, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2_shownOnDiagram, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-3);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var projectBuffer = networkViewModel.SuggestAndAcceptAllBuffers().Single();
			networkViewModel.ToggleApproval();

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			RunBufferPenetrationUpdater();

			AssertCcpmRtrTagApplied(jobHeader);
			AssertCcpmRtrTagApplied(workflow1_shownOnDiagram);
			AssertCcpmRblTagApplied(workflow2_shownOnDiagram);

			AssertEquals("Workflow that is not shown on the diagram, but whose job is linked to the root diagram, should find the diagram as its approved shape.", diagram, workflow3_notShownOnDiagram.ApprovedShape);

			AssertEquals("Project buffer penetration", 0.3333m, projectBuffer.BufferPenetration.Round(4));
			AssertBufferPenetration(workflow1_shownOnDiagram, Tuple.Create(projectBuffer, 0.3333m));
			AssertBufferPenetration(workflow2_shownOnDiagram, Tuple.Create(projectBuffer, 0m));
			AssertBufferPenetration(workflow3_notShownOnDiagram, Tuple.Create(config.Buffer, 0.0833m));
		}

		#endregion

		#region Edge Case Handling

		public void TestCalculatePenetration_WhenCircularDependencyExists_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.ReleaseGroup.Staff.Add(Factory.NewWithValidTestData<GlbStaff>()); // to avoid release group validation errors on shape's workflow

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "The earth spins on its axis", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "One man struggles", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "While another relaxes", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2);
			BMSTestHelper.CreateTask(workflow3);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram);

			var arrow1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var arrow2 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();
			network.SwitchToScaled();

			var buffer1 = network.Entities.GetInstance(arrow1).CreateBuffer();
			var buffer2 = network.Entities.GetInstance(arrow2).CreateBuffer();

			buffer1.Name += " Yeahyeahyeahyeah";
			buffer2.Name += " Dadadadadada";

			viewModel.PushAsEarlyAsPossible();
			viewModel.ToggleApproval();

			// Make circular dependency
			shape3.MakeVisiblePrerequisiteOf(shape1, diagram);

			Factory.Save();

			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow1, context, Factory).Penetration);
			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow2, context, Factory).Penetration);
			AssertEquals(0m, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow3, context, Factory).Penetration);
		}

		#endregion

		#region Implementation

		WorkingTimeContext context;

		protected override void SetUp()
		{
			base.SetUp();

			context = WorkingTimeContext.Create(Factory);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
		}

		#endregion
	}
}
