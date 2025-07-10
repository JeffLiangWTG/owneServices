using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BMNCNShapeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJobType_ShouldHaveListValidation()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var shape = Factory.New<BMNCNShape>();
			shape.BNS_JobType = "___";

			AssertHasError(shape.BNS_JobTypeInfo, "Enter a valid Job Type.");

			shape.BNS_JobType = "ORG";
			AssertNoErrors(shape.BNS_JobTypeInfo);
		}

		[TestDate(2014, 6, 3)]
		public void TestScheduledCompletionTime_WhenAfterAgreedDeliveryDate()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_AgreedDeliveryDate = ZDateTime.Today.AddDays(2);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape = NetworkTestCase.CreateShape(workflow, diagram, "shape");

			network.SwitchToScaled();

			shape = network.Entities.ShapeEntities.Single(); // Because approval recreates shapes
			diagram.Validation.ValidateAll();

			AssertEquals(ZDateTime.Today.AddDays(3), shape.Shape.ScheduledFinishTimeUtc);
			shape.Validation.ValidateAll();
			AssertHasRowWarning(shape, "This shape is scheduled to finish after the Agreed Delivery Date of its job. This would require completion with no buffer consumption.");

			shape.Width = CCPMConstants.ScaledModeDiagramPixelsPerScaleUnit * 2;
			shape.Validation.ValidateAll();
			AssertNoRowWarnings(shape);
		}

		[TestDate(2014, 6, 3)]
		public void TestScheduledCompletionTime_WhenAgreedDeliveryDateLessThan150PercentOfCompletionOffset()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.FH_AgreedDeliveryDate = ZDateTime.Today.AddDays(9); // 7 working days, inside the required duration of 6 days + 1.5 days (50% of the buffer of 3 days)

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var arrow = NetworkTestCase.CreateDependencyAttachment(diagram, link, shape1, shape2);

			network.ScaleAndRefresh();

			shape2 = network.Entities.ShapeEntities.Single(s => s.PK == shape2.PK); // Because approval recreates shapes

			AssertEquals(ZDateTime.Today.AddDays(8), shape2.Shape.ScheduledFinishTimeUtc);
			shape2.Validation.ValidateAll();
			AssertNoRowMessageErrors("We only validate ADD + 1.5 diagram offset if there are buffers", shape2);

			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);
			var bufferShape = network.Shapes.Single(s => s.IsBufferShape);
			bufferShape.Active = true;
			network.Refresh(RefreshType.RedrawDiagram);

			shape2.Validation.ValidateAll();
			AssertHasRowMessageError(shape2, "This shape's scheduled finish plus reasonable buffer consumption is after the Agreed Delivery Date of its workflow. This could lead to scheduling conflict.");

			workflow2.FH_AgreedDeliveryDate = ZDateTime.Today.AddDays(20);
			network.Refresh(RefreshType.RedrawDiagram);

			shape2.Validation.ValidateAll();
			AssertNoRowMessageErrors(shape2);
		}

		[TestDate(2014, 6, 3)]
		public void TestScheduledStartingTime_WhenBeforeDoNotStartBeforeDate()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_DoNotStartBeforeDate = ZDateTime.Today.AddDays(1);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape = NetworkTestCase.CreateShape(workflow, diagram, "shape");
			network.ScaleAndRefresh();

			shape = network.Entities.ShapeEntities.Single(); // Because approval recreates shapes

			AssertEquals(ZDateTime.Today, shape.Shape.ScheduledStartTimeUtc);
			shape.Shape.Validation.ValidateAll();
			AssertHasRowMessageError(shape.Shape, "This shape is scheduled to start before the Earliest Start Date of its workflow. This could lead to delays.");

			var notification = ((INetworkEntity)shape).EntityNotifications.Single();
			AssertEquals(EntityNotifcationType.Message, notification.NotificationType);
			AssertContains("This shape is scheduled to start before the Earliest Start Date of its workflow. This could lead to delays.", notification.Message);

			AssertEquals(true, shape.EntityState.HasFlag(EntityState.HasMessages));

			shape.X = CCPMConstants.ScaledModeDiagramPixelsPerScaleUnit * 3;
			AssertNoRowMessageErrors(shape.Shape);

			AssertEquals(0, ((INetworkEntity)shape).EntityNotifications.Count());
			AssertEquals(false, shape.EntityState.HasFlag(EntityState.HasMessages));
		}

		public void TestDuplicateWorkflowsExistOnDiagram()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			diagram.ScrollPosition = ScrollPositionList.Codes.Default;
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow1, diagram, "shape2");

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			AssertNoExceptionThrown(() => diagram.Validation.ValidateAll());
			AssertNoErrors(diagram);
		}

		public void TestBackInTimeHiddenDependencies_ForScaledDiagram_WithAnnotations()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "shape2", ShapeTypeList.Codes.Annotation);
			var shape3 = NetworkTestCase.CreateShape(diagram, "shape2", ShapeTypeList.Codes.Annotation);

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			AssertNoExceptionThrown(() => diagram.Validation.ValidateAll());
		}

		public void TestBackInTimeHiddenDependencies_ForScaledDiagram()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			network.DiagramEntity.Validation.ValidateAll();

			var entity1 = network.Entities.GetInstance(shape1);
			var entity2 = network.Entities.GetInstance(shape2);

			AssertHasRowWarning(entity1, "There is a hidden dependency involving this shape that would cause the post-requisite to be scheduled to begin before its pre-requisite is scheduled to complete.");
			AssertHasRowWarning(entity2, "There is a hidden dependency involving this shape that would cause the post-requisite to be scheduled to begin before its pre-requisite is scheduled to complete.");

			shape2.AsEntity(network).X = 300.0;
			network.DiagramEntity.Validation.ValidateAll();

			AssertNoWarnings(entity1);
			AssertNoWarnings(entity2);
		}

		public void TestBackInTimeHiddenDependencies_ForNonScaledDiagram()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);

			var network = NetworkTestCase.CreateNetwork(diagram);

			AssertEquals(0.0, shape1.AsEntity(network).X);
			AssertEquals(0.0, shape2.AsEntity(network).X);

			var entity1 = network.Entities.GetInstance(shape1);
			var entity2 = network.Entities.GetInstance(shape2);
			AssertNoWarnings(entity1);
			AssertNoWarnings(entity2);
		}

		public void TestName_ShouldBeMandatoryForAllShapes()
		{
			foreach (ICodeDescription shapeType in new ShapeTypeList())
			{
				var shape = Factory.New<BMNCNShape>();
				shape.BNS_ShapeType = shapeType.Code;
				shape.BNS_Name = ZString.Empty;

				AssertMandatoryValidationError(shape.BNS_NameInfo, isExpectingError: true);
			}
		}

		public void TestIsScaled_ShouldBeReadonly()
		{
			var shape = NetworkTestCase.CreateDiagram(Factory);
			AssertEquals(true, shape.IsScaledInfo.ReadOnly);
		}

		public void TestInactiveProcessHeader_ShouldNotHaveErrors()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var shape = NetworkTestCase.CreateDiagram(Factory, name: "Blah");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");

			workflow.FH_IsActive = false;

			shape.BNS_RelatedEntityID = workflow.PK;

			AssertNoErrors(shape);
			AssertNoErrors(workflow);
		}

		public void TestInactiveBuffer()
		{
			var shape = Factory.New<BMNCNShape>();
			shape.BNS_ShapeType = ShapeTypeList.Codes.Buffer;
			shape.Active = false;

			AssertHasError(shape.ActiveInfo, "This buffer has not been accepted yet. Please accept the buffer or hide it from the diagram.");

			shape.Active = true;
			AssertNoErrors(shape.ActiveInfo);

			shape.BNS_ShapeType = ShapeTypeList.Codes.Shape;
			shape.Active = false;
			AssertNoErrors(shape.ActiveInfo);

			shape.Active = true;
			AssertNoErrors(shape.ActiveInfo);
		}

		public void TestScrollPosition()
		{
			var shape = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory)).DiagramEntity;
			AssertEquals(true, shape.ScrollPositionInfo.ReadOnly);

			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScrollPositionInfo);

			shape.ScrollPosition = ZString.Empty;
			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScrollPositionInfo);

			shape.ScrollPosition = "XXX";
			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScrollPositionInfo);

			shape.Shape.IsScaled = true;
			shape.Validation.ValidateAll();
			AssertEquals(false, shape.ScrollPositionInfo.ReadOnly);
			AssertHasError(shape.ScrollPositionInfo, "Enter a valid Scroll Position.");

			shape.ScrollPosition = ZString.Empty;
			shape.Validation.ValidateAll();
			AssertHasError(shape.ScrollPositionInfo, "Please enter a Scroll Position.");

			shape.ScrollPosition = ScrollPositionList.Codes.Current;
			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScrollPositionInfo);

			shape.ScrollPosition = ScrollPositionList.Codes.First;
			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScrollPositionInfo);

			shape.ScrollPosition = ScrollPositionList.Codes.Start;
			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScrollPositionInfo);

			shape.ScrollPosition = ScrollPositionList.Codes.Default;
			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScrollPositionInfo);
		}

		public void TestValidateScheduledStartAgainstFinishDate()
		{
			var shape = Factory.New<BMNCNShape>();
			shape.Validation.ValidateAll();
			AssertNoErrors(shape.ScheduledFinishTimeUtcInfo);
			AssertNoErrors(shape.ScheduledStartTimeUtcInfo);

			shape.ScheduledFinishTimeUtc = ZDateTime.Now.AddDays(-5);
			AssertNoErrors("Scheduled Finish Date cannot be less than Scheduled Start Date.", shape.ScheduledFinishTimeUtcInfo);

			shape.ScheduledStartTimeUtc = ZDateTime.Now;
			AssertHasErrors("Scheduled Start Date cannot be greater than Scheduled Finish Date.", shape.ScheduledStartTimeUtcInfo);

			shape.ScheduledFinishTimeUtc = ZDateTime.Now.AddDays(5);
			shape.Validation.ValidateScheduledStartTimeUtc();
			AssertNoErrors("Scheduled Start Date cannot be greater than Scheduled Finish Date.", shape.ScheduledStartTimeUtcInfo);

			shape.ScheduledFinishTimeUtc = ZDateTime.Now.AddDays(-3);
			AssertHasErrors("Scheduled Finish Date cannot be less than Scheduled Start Date.", shape.ScheduledFinishTimeUtcInfo);
		}

		[TestDate(2019, 1, 1)]
		public void TestScheduledStartAndFinishTimeChanges_WhenLinkedToScaledDiagram_AndVariousOtherFactorsAreTrue_ShouldHaveMessageNotification()
		{
			var scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Scaled Diagram 1", scheduledStartTimeUTC: ZDateTime.UtcNow);
			var scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Scaled Diagram 2");
			var scaledDiagram3 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Scaled Diagram 3");
			var nonScaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Non-Scaled Diagram");

			var shapeOnDiagram1 = NetworkTestCase.CreateShape(scaledDiagram1, name: "Shape on Diagram 1");
			var shapeOnDiagram2 = NetworkTestCase.CreateShape(scaledDiagram2, name: "Shape on Diagram 2");

			var network1 = NetworkTestCase.CreateNetwork(scaledDiagram1);
			var network2 = NetworkTestCase.CreateNetwork(scaledDiagram2);
			var network3 = NetworkTestCase.CreateNetwork(scaledDiagram3);

			network1.RefreshSchedules();
			network2.RefreshSchedules();
			network3.RefreshSchedules();

			AssertNoRowMessageErrors("No linked diagram, so no messages to be shown", shapeOnDiagram1);
			AssertNoRowMessageErrors("No linked diagram, so no messages to be shown", shapeOnDiagram2);

			NetworkTestCase.LinkToRelatedDiagram(shapeOnDiagram1, scaledDiagram3);
			NetworkTestCase.LinkToRelatedDiagram(shapeOnDiagram2, scaledDiagram3);

			AssertHasRowMessageError(shapeOnDiagram1, "When saving this diagram, the Scheduled Start and Scheduled Finish of this shape will be propagated to the diagram to which this shape is linked.");
			AssertNoRowMessageErrors("Scaled diagram #2 has no Scheduled Start time set, which means there is no date information to copy to the linked diagram.", shapeOnDiagram2);

			PropagateLinkedDiagramSchedule(shapeOnDiagram1);

			shapeOnDiagram1.Validation.ValidateAll();
			shapeOnDiagram2.Validation.ValidateAll();

			AssertNoRowMessageErrors("Diagram is saved, so changes must have been propagated", shapeOnDiagram1);
			AssertNoRowMessageErrors("Diagram is saved, so changes must have been propagated", shapeOnDiagram2);

			shapeOnDiagram1.ScheduledStartTimeLocal = shapeOnDiagram1.ScheduledStartTimeLocal.AddDays(1);

			AssertHasRowMessageError(shapeOnDiagram1, "When saving this diagram, the Scheduled Start and Scheduled Finish of this shape will be propagated to the diagram to which this shape is linked.");

			PropagateLinkedDiagramSchedule(shapeOnDiagram1);
			shapeOnDiagram1.Validation.ValidateAll();
			AssertNoRowMessageErrors("Diagram is saved, so changes must have been propagated", shapeOnDiagram1);

			shapeOnDiagram1.ScheduledFinishTimeLocal = shapeOnDiagram1.ScheduledStartTimeLocal.AddDays(1);
			AssertHasRowMessageError(shapeOnDiagram1, "When saving this diagram, the Scheduled Start and Scheduled Finish of this shape will be propagated to the diagram to which this shape is linked.");

			PropagateLinkedDiagramSchedule(shapeOnDiagram1);
			NetworkTestCase.LinkToRelatedDiagram(shapeOnDiagram1, nonScaledDiagram);
			AssertNoRowMessageErrors("Shape is linked to a non-scaled diagram, so we won't propagate date information.", shapeOnDiagram1);

			shapeOnDiagram1.ScheduledFinishTimeLocal = shapeOnDiagram1.ScheduledFinishTimeLocal.AddDays(1);
			NetworkTestCase.LinkToRelatedDiagram(shapeOnDiagram1, scaledDiagram3);
			AssertHasRowMessageError(shapeOnDiagram1, "When saving this diagram, the Scheduled Start and Scheduled Finish of this shape will be propagated to the diagram to which this shape is linked.");

			shapeOnDiagram1.ShouldSynchroniseScheduleWithLinkedEntity = false;
			AssertNoRowMessageErrors("Shape is linked now marked as not having its schedule synced, so we won't propagate date information.", shapeOnDiagram1);
		}

		public void TestScheduledStartAndFinishTimeChanges_WhenLinkedToScaledDiagram_WhichContainsPinnedShapes_ShouldHaveWarning()
		{
			var scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Scaled Diagram 1", scheduledStartTimeUTC: ZDateTime.UtcNow);
			var scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Scaled Diagram 2");

			var shapeOnDiagram1 = NetworkTestCase.CreateShape(scaledDiagram1, name: "Shape on Diagram 1");
			var shapeOnDiagram2 = NetworkTestCase.CreateShape(scaledDiagram2, name: "Shape on Diagram 2");

			var networkViewModel1 = NetworkTestCase.CreateNetworkViewModel(scaledDiagram1);
			var networkViewModel2 = NetworkTestCase.CreateNetworkViewModel(scaledDiagram2);

			var network1 = networkViewModel1.GetJobNetwork();
			var network2 = networkViewModel2.GetJobNetwork();

			shapeOnDiagram2.PinShape(networkViewModel2);

			NetworkTestCase.SetShapeOffset(shapeOnDiagram1, scaledDiagram1, network1, 0, 0);
			NetworkTestCase.SetShapeSize(network1, shapeOnDiagram1, scaledDiagram1, 300, 100);

			network1.RefreshSchedules();
			network2.RefreshSchedules();

			NetworkTestCase.LinkToRelatedDiagram(shapeOnDiagram1, scaledDiagram2);
			PropagateLinkedDiagramSchedule(shapeOnDiagram1);
			shapeOnDiagram1.Validation.ValidateAll();

			AssertNoRowMessageErrors("No changes to propagate, so no messages to be shown", shapeOnDiagram1);
			AssertNoRowWarnings("No changes to propagate, so no warnings to be shown", shapeOnDiagram1);

			NetworkTestCase.SetShapeSize(network1, shapeOnDiagram1, scaledDiagram1, 400, 100);

			const string messageErrorText = "When saving this diagram, the Scheduled Start and Scheduled Finish of this shape will be propagated to the diagram to which this shape is linked.";
			const string warningText = "The linked diagram has one or more pinned shapes. When propagating schedules, the pinned shapes will remain in the same position, but the Scheduled Start time will change. Please ensure this is appropriate and consider adjusting the pinned shape positions after the schedules have propagated.";

			AssertHasRowMessageError(shapeOnDiagram1, messageErrorText);
			AssertNoRowWarnings("Changing the finish time of the shape linked to another diagram won't affect pins on that diagram, so no warnings to be shown", shapeOnDiagram1);

			NetworkTestCase.SetShapeOffset(shapeOnDiagram1, scaledDiagram1, network1, 100, 0);

			AssertHasRowMessageError(shapeOnDiagram1, messageErrorText);
			AssertHasRowWarning("Now that the shape has been moved, we show the warning because the pinned shape's schedule will be affected.", shapeOnDiagram1, warningText);

			shapeOnDiagram2.UnPinShape(networkViewModel2);
			shapeOnDiagram1.Validation.ValidateAll();

			AssertNoRowWarnings("No pinned shapes to worry about anymore, so no warnings to be shown", shapeOnDiagram1);
		}

		void PropagateLinkedDiagramSchedule(BMNCNShape shapeMaybeLinkedToAnotherDiagram)
		{
			var linkedDiagram = shapeMaybeLinkedToAnotherDiagram.LinkedEntity as BMNCNRootDiagramShape;

			if (linkedDiagram != null)
			{
				// Simulate what happens when the shape schedule is propagated to the linked diagram.

				linkedDiagram.ScheduledStartTimeUtc = shapeMaybeLinkedToAnotherDiagram.ScheduledStartTimeUtc;
				linkedDiagram.ScheduledFinishTimeUtc = shapeMaybeLinkedToAnotherDiagram.ScheduledFinishTimeUtc;
			}
		}

		#region Light Validation

		public void TestShouldSupportLightValidation()
		{
			var shape = Factory.New<BMNCNShape>();
			Assert(shape is ILightValidationInternals);
		}

		#endregion
	}
}
