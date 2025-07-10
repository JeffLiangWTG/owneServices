using System;
using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class ShapeNetworkEntityValidationTest : BusinessObjectValidationTestCase
	{
		#region Network Scaling

		public void TestIsScaled()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.IsScaled = true;

			var entity = NetworkTestCase.CreateNetwork(diagram).DiagramEntity;
			AssertEquals(true, entity.IsDiagramScaled);

			entity.Validation.ValidateAll();
			AssertNoErrors("Scale is set by default when a BMNCNSchedule is initialised, meaning that it should not give validation errors before being set", entity.ScaleInfo);
			AssertNoErrors("ResolutionIncrement is set by default when a BMNCNSchedule is initialised, meaning that it should not give validation errors before being set", entity.ResolutionIncrementInfo);
		}

		public void TestIsScaled_ChildShapesDontMind()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.IsScaled = true;

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var entity = network.DiagramEntity;
			AssertEquals(true, entity.IsDiagramScaled);
			entity.Validation.ValidateAll();
			AssertNoErrors(entity.ScaleInfo);
			AssertNoErrors(entity.ResolutionIncrementInfo);

			entity.Scale = new ZInt(120).GetDateTimeFromMinutes();
			entity.ResolutionIncrement = new ZInt(240).GetDateTimeFromMinutes();

			AssertNoErrors(entity.ScaleInfo);
			AssertNoErrors(entity.ResolutionIncrementInfo);

			var child = networkViewModel.CreateNewShape(entity.Shape);
			Assert(child.IsScaled);

			child.ResolutionIncrement = ZDateTime.Invalid;
			child.Scale = ZDateTime.Invalid;

			AssertNoErrors(child.ScaleInfo);
			AssertNoErrors(child.ResolutionIncrementInfo);
		}

		public void TestScalingValues_ComplainWhenSetToZero()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.IsScaled = true;

			var entity = NetworkTestCase.CreateNetwork(diagram).DiagramEntity;
			AssertEquals(true, entity.IsDiagramScaled);

			entity.ResolutionIncrement = new ZDateTime(new DateTime(1, 1, 1, 0, 0, 0));
			entity.Scale = new ZDateTime(new DateTime(1, 1, 1, 0, 0, 0));

			entity.Validation.ValidateAll();
			AssertHasError("Scale should not be 0, since 0 in calculations is just always terrible", entity.ScaleInfo, "Please enter a Scale greater than 0.");
			AssertHasError("ResolutionIncrement should not be 0, since 0 in calculations is just always terrible", entity.ResolutionIncrementInfo, "Please enter a Resolution Increment greater than 0.");
		}

		#endregion

		#region Lööps

		public void TestCyclicNetworkValidation()
		{
			var diagram = NetworkTestCase.CreateJobAndDiagram(Factory);
			diagram.IsScaled = true;
			var shape1 = NetworkTestCase.CreateShape(diagram, "Cider");
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.Refresh(RefreshType.RedrawDiagram);

			var entity = shape1.AsEntity(network);
			entity.Schedule.IsCyclic = true;
			entity.Validation.ValidateAll();
			AssertHasRowError(entity, "This shape has a circular dependency");

			entity.Schedule.IsCyclic = false;
			entity.Validation.ValidateAll();
			AssertNoRowError(entity, "This shape has a circular dependency");
		}

		public void TestShouldValidateProcessHeaderLoops_WhenDiagramIsNotSavedOnly()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";

			var link1to2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2to3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3to1 = workflow3.GetOrCreateDependencyLink(workflow1); //Circular dependency - should anger validation

			var workflowShape1 = NetworkTestCase.CreateShape(workflow1, diagramShape, "workflowShape1");
			var workflowShape2 = NetworkTestCase.CreateShape(workflow2, diagramShape, "workflowShape2");
			var workflowShape3 = NetworkTestCase.CreateShape(workflow3, diagramShape, "workflowShape3");

			var network = networkViewModel.GetJobNetwork();
			var diagramEntity = network.DiagramEntity;

			var workflowEntity1 = (ShapeNetworkEntity)network.Entities[0];
			var workflowEntity2 = (ShapeNetworkEntity)network.Entities[1];
			var workflowEntity3 = (ShapeNetworkEntity)network.Entities[2];

			workflowEntity1.Validation.ValidateProcessHeaderLoops();
			workflowEntity2.Validation.ValidateProcessHeaderLoops();
			workflowEntity3.Validation.ValidateProcessHeaderLoops();
			diagramEntity.Validation.ValidateProcessHeaderLoops();

			AssertEquals("Should have validation errors", true, diagramEntity.HasErrors);
			Assert("Should have circular dependency errors", diagramEntity.GetErrors().Any(e => e.Message.Contains("A looped dependency presents on the diagram")));

			Factory.Save();

			Assert("Precondition", diagramShape.IsInDatabase);
			Assert("Precondition", workflowShape1.IsInDatabase);
			Assert("Precondition", workflowShape2.IsInDatabase);
			Assert("Precondition", workflowShape3.IsInDatabase);
			Assert("Precondition: should have no changes", !diagramShape.HasChanges);

			diagramEntity.ClearAllNotifications();
			workflowEntity1.ClearAllNotifications();
			workflowEntity2.ClearAllNotifications();
			workflowEntity3.ClearAllNotifications();
			AssertEquals(false, diagramEntity.HasErrors);

			workflowEntity1.Validation.ValidateProcessHeaderLoops();
			workflowEntity2.Validation.ValidateProcessHeaderLoops();
			workflowEntity3.Validation.ValidateProcessHeaderLoops();
			diagramEntity.Validation.ValidateProcessHeaderLoops();

			AssertEquals("Should have no validation errors as process header loops validation should be suppressed when the diagram is saved", false, diagramEntity.HasErrors);

			diagramShape.Name = "Let's change the diagram";
			Assert("Precondition: diagram should have changes", diagramShape.HasChanges);

			workflowEntity1.Validation.ValidateProcessHeaderLoops();
			workflowEntity2.Validation.ValidateProcessHeaderLoops();
			workflowEntity3.Validation.ValidateProcessHeaderLoops();
			diagramEntity.Validation.ValidateProcessHeaderLoops();

			AssertEquals("Process header loops validation should be still suppressed even when the diagram has changes", false, diagramEntity.HasErrors);
		}

		#endregion

		#region Back In Time

		public void TestBackInTime_Pins()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "Cole", isScaled: true);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var shape1 = NetworkTestCase.CreateShape(diagram, name: "Foaly");
			var shape2 = NetworkTestCase.CreateShape(diagram, name: "Barnes");

			shape1.X = 0;
			shape1.Y = 0;
			shape1.Width = 100;

			shape2.X = 0;
			shape2.Y = 0;
			shape2.Width = 100;

			var attachment = shape1.MakeVisiblePrerequisiteOf(shape2);

			shape2.Shape.PinShape(viewModel);

			var networkEntity = network.Entities.GetInstance(attachment);
			networkEntity.Validation.ValidateBackInTimeArrows();

			AssertHasRowWarning(networkEntity, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
		}

		public void TestBackInTimeArrows_CallingValidateTwiceIsSafe()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "shape2");
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			shape1.AsEntity(network).X = 0d;
			shape2.AsEntity(network).X = 0d;

			var networkEntity = network.Entities.GetInstance(arrow);
			networkEntity.Validation.ValidateBackInTimeArrows();

			AssertHasRowWarning(networkEntity, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");

			networkEntity.Validation.ValidateBackInTimeArrows();

			AssertHasRowWarning(networkEntity, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
		}

		public void TestBackInTimeArrows_ForScaledDiagram()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var arrow = NetworkTestCase.CreateDependencyAttachment(diagram, link, shape1, shape2);

			network.SwitchToScaled();

			var networkEntity = network.Entities.GetInstance(arrow);
			networkEntity.Validation.ValidateBackInTimeArrows();

			AssertEquals(0.0, shape1.X);
			AssertEquals(300.0, shape2.X);
			AssertNoWarnings(networkEntity);

			networkEntity.To.X = 0.0;
			AssertHasRowWarning(networkEntity, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");

			networkEntity.To.X = 400.0;
			AssertNoWarnings(networkEntity);
		}

		public void TestBackInTimeArrows_ForNonScaledDiagram()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);
			var arrow = NetworkTestCase.CreateDependencyAttachment(diagram, link, shape1, shape2);

			var network = NetworkTestCase.CreateNetwork(diagram);

			var networkEntity = network.Entities.GetInstance(arrow);
			networkEntity.Validation.ValidateBackInTimeArrows();

			AssertEquals(0.0, shape1.AsEntity(network).X);
			AssertEquals(0.0, shape2.AsEntity(network).X);
			AssertNoWarnings(networkEntity);
		}

		public void TestBackInTimeArrow_WhenDecoupled()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var arrow = network.CreateRelationship(shape1, shape2).AsAttachment();

			network.SwitchToScaled();

			AssertEquals(0.0, shape1.X);
			AssertEquals(300.0, shape2.X);

			var networkEntity = network.Entities.GetInstance(arrow);

			networkEntity.To.X = 200;
			AssertHasRowWarning(networkEntity, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");

			networkEntity.IsDecouple = true;
			AssertNoRowWarningContaining(networkEntity, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete."); // Back in time decoupled arrows have no impact on scheduling, so don't bother warning
		}

		public void TestBackInTimeArrow_ForNonScheduledSection_ShouldNotHaveError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			shape1.IsNonScheduled = true;
			shape2.IsNonScheduled = true;
			var arrow = network.CreateRelationship(shape1, shape2).AsAttachment();

			var networkEntity = network.Entities.GetInstance(arrow);

			networkEntity.From.X = 300;
			networkEntity.To.X = 200;
			AssertNoRowWarnings("The warning should not appear on nonscheduled shapes. SAD!", networkEntity);
		}

		public void TestNonScheduledSection_HasValidationErrors_WhenDiagramHasNonScheduledItems()
		{
			var diagram = Factory.New<BMNCNRootDiagramShape>();
			diagram.IsScaled = true;
			diagram.ShouldShowNonScheduledSection = true;

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			shape1.IsNonScheduled = true;
			shape2.IsNonScheduled = false;
			var entity = network.DiagramEntity;

			entity.Validation.ValidateAll();
			AssertNoRowErrors("Should not have any errors", entity);

			shape2.Validation.ValidateAll();
			AssertNoRowErrors("Shape is not affected by Non Scheduled validation", shape2);

			diagram.ShouldShowNonScheduledSection = false;
			entity.Validation.ValidateAll();
			AssertHasRowError(entity, "Unable to hide Non-Scheduled section, as there are non-scheduled items on the diagram");

			shape1.IsNonScheduled = false;
			entity.Validation.ValidateAll();
			AssertNoRowErrors("Should not have any errors", entity);
		}

		#endregion

		#region Scheduled Start and Finish

		public void TestValidationWhenShapesOutsideDiagramDateRange()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "diagram");
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-10);
			diagram.ScheduledFinishTimeLocal = ZDateTime.Now.AddDays(10);

			var subDiagram = NetworkTestCase.CreateShape(diagram, "Sub-Diagram");
			var shape1 = NetworkTestCase.CreateShape(diagram, "shape1");

			var shape2 = NetworkTestCase.CreateShape(diagram, "shape2");

			var networkEntity = network.Entities.GetInstance(diagramShape);
			networkEntity.Validation.ValidateScheduleDateViolations();
			AssertNoErrors(networkEntity);

			shape1.ScheduledFinishTimeLocal = ZDateTime.Now.AddDays(12);
			shape2.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-12);
			networkEntity.Validation.ValidateScheduleDateViolations();

			AssertHasRowError(networkEntity, @"There are shapes scheduled to finish after the Diagram Scheduled Finish Date:
- shape1.

This leads to scheduling violations and needs to be fixed.");

			AssertHasRowError(networkEntity, @"There are shapes scheduled to start before the Diagram Scheduled Start Date:
- shape2.

This leads to scheduling violations and needs to be fixed.");

			shape1.ScheduledFinishTimeLocal = ZDateTime.Empty;
			shape1.SetWidthForDuration(2800);
			shape2.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(4);
			networkEntity.Validation.ValidateScheduleDateViolations();
			AssertHasRowError(networkEntity, @"There are shapes scheduled to finish after the Diagram Scheduled Finish Date:
- shape1.

This leads to scheduling violations and needs to be fixed.");

			AssertNoRowError(networkEntity, @"There are shapes scheduled to start before the Diagram Scheduled Start Date:
- shape2.

This leads to scheduling violations and needs to be fixed.");

			subDiagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-10);
			subDiagram.ScheduledFinishTimeLocal = ZDateTime.Now.AddDays(10);
			var subDiagram_shape1 = NetworkTestCase.CreateShape(subDiagram, "subDiagram shape1");
			subDiagram_shape1.ScheduledFinishTimeLocal = ZDateTime.Now.AddDays(12);
			subDiagram_shape1.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-12);
			subDiagram.Validation.ValidateScheduleDateViolations();
			AssertNoRowErrorContaining(subDiagram, "There are shapes scheduled to finish after the Diagram Scheduled Finish Date:");
			AssertNoRowErrorContaining(subDiagram, "There are shapes scheduled to start before the Diagram Scheduled Start Date:");
		}

		[TestDate(2019, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestScheduledFinish_WhenVaryingAmountsOfRoomForAllShapes_ForDiagramFinishingWithAnnotation_ShouldValidateAgainstFixingDiagramSurface()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Figura Ūnus");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Annotātiōnem", shapeType: ShapeTypeList.Codes.Annotation);

			var network = NetworkTestCase.CreateNetwork(diagram);
			var diagramEntity = network.Entities.GetInstance(diagram);

			NetworkTestCase.SetShapeRectangle(network, shape1, diagram, 300, 100, 0, 10);
			NetworkTestCase.SetShapeRectangle(network, shape2, diagram, 300, 100, 300, 10);

			diagramEntity.ScheduledStartTimeLocal = ZDateTime.Now;
			network.RefreshSchedules();

			diagramEntity.Validation.ValidateAll();
			AssertNoRowErrors(diagramEntity);

			diagramEntity.ScheduledFinishTimeLocal = ZDateTime.Now.AddYears(1);
			diagramEntity.Validation.ValidateAll();

			AssertHasRowError(diagramEntity, "The rightmost shape in this diagram is an annotation. This is not permitted when entering both a Scheduled Start and Scheduled Finish time. Please move the annotation so the rightmost shape is not an annotation.");

			diagramEntity.ScheduledStartTimeLocal = ZDateTime.Empty;
			diagramEntity.Validation.ValidateAll();

			AssertNoRowErrors(diagramEntity);

			diagramEntity.ScheduledStartTimeLocal = ZDateTime.Now;
			diagramEntity.Validation.ValidateAll();

			AssertHasRowError(diagramEntity, "The rightmost shape in this diagram is an annotation. This is not permitted when entering both a Scheduled Start and Scheduled Finish time. Please move the annotation so the rightmost shape is not an annotation.");

			NetworkTestCase.SetShapeRectangle(network, shape2, diagram, 300, 100, 0, 10);
			diagramEntity.Validation.ValidateAll();

			AssertNoRowErrors(diagramEntity);

			NetworkTestCase.SetShapeRectangle(network, shape2, diagram, 300, 100, 300, 10);
			diagram.ScheduledFinishTimeLocal = ZDateTime.Now;
			diagramEntity.Validation.ValidateAll();

			CombineAssertions("We should show all possible row notifications that are relevant", () =>
			{
				AssertHasRowError("Notification for annotation being the last shape", diagramEntity, "The rightmost shape in this diagram is an annotation. This is not permitted when entering both a Scheduled Start and Scheduled Finish time. Please move the annotation so the rightmost shape is not an annotation.");
				AssertHasRowError("Notification for shape not fitting within the scheduled start/finish size", diagramEntity,
	@"There are shapes scheduled to finish after the Diagram Scheduled Finish Date:
- Figura Ūnus.

This leads to scheduling violations and needs to be fixed.");
			});
		}

		[TestDate(2019, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestScheduledFinish_WhenVaryingAmountsOfRoomForAllShapes_ForDiagramFinishingWithShape()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);
			Factory.Save();

			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape1 = NetworkTestCase.CreateShape(diagram, "Figura Ūnus");
			var shape2 = NetworkTestCase.CreateShape(diagram, "Figura Duo");

			var network = NetworkTestCase.CreateNetwork(diagram);
			var diagramEntity = network.Entities.GetInstance(diagram);

			NetworkTestCase.SetShapeRectangle(network, shape1, diagram, 300, 100, 0, 10);
			NetworkTestCase.SetShapeRectangle(network, shape2, diagram, 300, 100, 300, 10);

			diagramEntity.ScheduledStartTimeLocal = ZDateTime.Now;
			network.RefreshSchedules();

			AssertEquals("Pre-condition: scheduled time should be set on shape1", ZDateTime.UtcNow, shape1.ScheduledStartTimeUtc);
			AssertEquals("Pre-condition: scheduled time should be set on shape2", ZDateTime.UtcNow.AddDays(3), shape2.ScheduledStartTimeUtc);

			diagramEntity.ScheduledFinishTimeLocal = ZDateTime.Now.AddYears(1);
			diagramEntity.Validation.ValidateAll();

			AssertNoRowErrors("The diagram is scheduled to finish a year from now, which is more than enough space for two shapes.", diagramEntity);

			diagramEntity.ScheduledFinishTimeLocal = ZDateTime.Now.AddDays(7);
			diagramEntity.Validation.ValidateAll();

			AssertHasRowError("Now that the diagram surface has insufficient space for the last shape, there should be a row error.", diagramEntity,
@"There are shapes scheduled to finish after the Diagram Scheduled Finish Date:
- Figura Duo.

This leads to scheduling violations and needs to be fixed.");

			diagramEntity.ScheduledFinishTimeLocal = ZDateTime.Now.AddDays(8);
			diagramEntity.Validation.ValidateAll();

			AssertNoRowErrors("Now that the diagram surface has just enough space for the last shape, there should be no row errors.", diagramEntity);
		}

		#endregion

		#region Leveling Rules

		[TestDate(2019, 1, 7)]
		public void TestConcurrencyLevelingRule()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagramShape.BackColor = string.Empty;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagramEntity = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1, name: "Con the Fruiterer");

			var childShape1 = NetworkTestCase.CreateShape(diagramEntity, "Shape 1");
			childShape1.Width = 100;
			childShape1.Height = 100;
			childShape1.X = 0;
			childShape1.Y = 300;

			var childShape2 = NetworkTestCase.CreateShape(diagramEntity, "Shape 2");
			childShape2.Width = 100;
			childShape2.Height = 100;
			childShape2.X = 0;
			childShape2.Y = 300;

			descriptor.GetScaleSetForColumns(10);

			var childEntity1 = network.Entities.Single(e => e.Name == "Shape 1");
			var childEntity2 = network.Entities.Single(e => e.Name == "Shape 2");

			CombineAssertions("Both nodes mutually violate each other", () =>
			{
				AssertEquals(1, childEntity1.EntityNotifications.Count());
				AssertEquals(1, childEntity2.EntityNotifications.Count());

				AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity1.EntityNotifications.First().Message);
				AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity2.EntityNotifications.First().Message);
			});

			rule.BNR_RuleValue = 2;
			descriptor.GetScaleSetForColumns(10);

			AssertEquals(0, childEntity1.EntityNotifications.Count());
			AssertEquals(0, childEntity2.EntityNotifications.Count());
		}

		[TestDate(2019, 1, 7)]
		public void TestStartLevelingRule()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagramShape.BackColor = string.Empty;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagramEntity = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 2, name: "Kylie Mole");

			var childShape1 = NetworkTestCase.CreateShape(diagramEntity, "Shape 1");
			childShape1.Width = 200;
			childShape1.Height = 100;
			childShape1.X = 0;
			childShape1.Y = 100;

			var childShape2 = NetworkTestCase.CreateShape(diagramEntity, "Shape 2");
			childShape2.Width = 100;
			childShape2.Height = 100;
			childShape2.X = 0;
			childShape2.Y = 300;

			descriptor.GetScaleSetForColumns(10);

			var childEntity1 = network.Entities.Single(e => e.Name == "Shape 1");
			var childEntity2 = network.Entities.Single(e => e.Name == "Shape 2");

			CombineAssertions("Both nodes mutually violate each other", () =>
			{
				AssertEquals(1, childEntity1.EntityNotifications.Count());
				AssertEquals(1, childEntity2.EntityNotifications.Count());

				AssertEquals("Warning - Leveling Rule: The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].", childEntity1.EntityNotifications.First().Message);
				AssertEquals("Warning - Leveling Rule: The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].", childEntity2.EntityNotifications.First().Message);
			});

			childShape2.Width = 200;
			childShape2.X = 100;
			childShape2.Y = 100;

			descriptor.GetScaleSetForColumns(10);

			CombineAssertions("Second node now only violates the first", () =>
			{
				AssertEquals(0, childEntity1.EntityNotifications.Count());
				AssertEquals(1, childEntity2.EntityNotifications.Count());

				AssertEquals("Warning - Leveling Rule: The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].", childEntity2.EntityNotifications.First().Message);
			});

			childShape2.X = 200;
			descriptor.GetScaleSetForColumns(10);

			AssertEquals(0, childEntity1.EntityNotifications.Count());
			AssertEquals(0, childEntity2.EntityNotifications.Count());
		}

		[TestDate(2019, 1, 7)]
		public void TestGapLevelingRule()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagramShape.BackColor = string.Empty;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagramEntity = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1, name: "Uncle Arthur");

			var childShape1 = NetworkTestCase.CreateShape(diagramEntity, "Shape 1");
			childShape1.Width = 200;
			childShape1.Height = 100;
			childShape1.X = 0;
			childShape1.Y = 100;

			var childShape2 = NetworkTestCase.CreateShape(diagramEntity, "Shape 2");
			childShape2.Width = 100;
			childShape2.Height = 100;
			childShape2.X = 200;
			childShape2.Y = 300;

			descriptor.GetScaleSetForColumns(10);

			var childEntity1 = network.Entities.Single(e => e.Name == "Shape 1");
			var childEntity2 = network.Entities.Single(e => e.Name == "Shape 2");

			CombineAssertions("Second node only violates the first", () =>
			{
				AssertEquals(0, childEntity1.EntityNotifications.Count());
				AssertEquals(1, childEntity2.EntityNotifications.Count());

				AssertEquals("Warning - Leveling Rule: The distance between the end of one or more shapes and the start of this shape is too small, as specified in the Leveling Rule named [Uncle Arthur].", childEntity2.EntityNotifications.First().Message);
			});

			childShape2.Width = 200;
			childShape2.X = 300;
			childShape2.Y = 100;
			descriptor.GetScaleSetForColumns(10);

			AssertEquals(0, childEntity1.EntityNotifications.Count());
			AssertEquals(0, childEntity2.EntityNotifications.Count());
		}

		[TestDate(2019, 1, 7)]
		public void TestValidationChanges_WhenShapeStatusChanges()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagramShape.BackColor = string.Empty;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagramEntity = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule1 = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1, name: "Con the Fruiterer", colorName: Color.Aqua.Name);
			var rule2 = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 1, name: "Kylie Mole", colorName: Color.Brown.Name);
			var rule3 = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1, name: "Uncle Arthur", colorName: Color.Chartreuse.Name);

			var childShape1 = NetworkTestCase.CreateShape(diagramEntity, "Shape 1");
			childShape1.Width = 200;
			childShape1.Height = 100;
			childShape1.X = 0;
			childShape1.Y = 100;

			var childShape2 = NetworkTestCase.CreateShape(diagramEntity, "Shape 2");
			childShape2.Width = 200;
			childShape2.Height = 100;
			childShape2.X = 0;
			childShape2.Y = 300;

			var childShape3 = NetworkTestCase.CreateShape(diagramEntity, "Shape 3");
			childShape3.Width = 200;
			childShape3.Height = 100;
			childShape3.X = 200;
			childShape3.Y = 500;

			descriptor.GetScaleSetForColumns(10);

			var childEntity1 = network.Entities.Single(e => e.Name == "Shape 1");
			var childEntity2 = network.Entities.Single(e => e.Name == "Shape 2");
			var childEntity3 = network.Entities.Single(e => e.Name == "Shape 3");

			CombineAssertions("No closed shapes, all shapes should violate", () =>
			{
				AssertEquals(1, childEntity1.EntityNotifications.Count(n => n.Message.Contains("There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].")));
				AssertEquals(1, childEntity1.EntityNotifications.Count(n => n.Message.Contains("The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].")));

				AssertEquals(1, childEntity2.EntityNotifications.Count(n => n.Message.Contains("There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].")));
				AssertEquals(1, childEntity2.EntityNotifications.Count(n => n.Message.Contains("The distance between the start of shapes is too small, as specified in the Leveling Rule named [Kylie Mole].")));

				AssertEquals(1, childEntity3.EntityNotifications.Count(n => n.Message.Contains("The distance between the end of one or more shapes and the start of this shape is too small, as specified in the Leveling Rule named [Uncle Arthur].")));
			});

			childShape2.Shape.BNS_Status = ShapeStatusList.Codes.Closed;

			descriptor.GetScaleSetForColumns(10);
			network.Refresh(RefreshType.RefreshButton, childShape1, childShape2, childShape3);

			CombineAssertions("Second shape closed, only third shape should violate", () =>
			{
				AssertEquals(0, childEntity1.EntityNotifications.Count());
				AssertEquals(0, childEntity2.EntityNotifications.Count());
				AssertEquals(1, childEntity3.EntityNotifications.Count(n => n.Message.Contains("The distance between the end of one or more shapes and the start of this shape is too small, as specified in the Leveling Rule named [Uncle Arthur].")));
			});
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
