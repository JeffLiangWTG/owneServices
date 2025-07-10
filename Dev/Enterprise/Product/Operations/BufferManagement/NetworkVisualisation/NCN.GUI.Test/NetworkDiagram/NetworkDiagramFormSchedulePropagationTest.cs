using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	[TestUtcOffset(10, 0, 0)]
	[TestDate(2019, 1, 1)]
	class NetworkDiagramFormSchedulePropagationTest : NetworkGUITestCase
	{
		public void TestProjectBufferPenetration_ShouldUpdate_WhenUpstreamShapesAreChanged()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var task1 = CreateOpenTaskWithTimeLeftForWorkflow(workflow);
			var task2 = CreateOpenTaskWithTimeLeftForWorkflow(workflow);

			// make the business-layer networkdiagram stuff
			var diagramStartTime = ZDateTime.Now.AddDays(-5);
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.SwitchToScaled();
			diagramShape.ScheduledStartTimeLocal = diagramStartTime;

			Factory.Save();

			var shape1Start = diagramStartTime.AddDays(1);
			var shape2Start = diagramStartTime.AddDays(3);
			var shape1 = NetworkTestCase.CreateShape(diagramShape, "shape1", scheduledStartTimeUTC: shape1Start);
			var shape2 = NetworkTestCase.CreateShape(workflow, diagramShape, "shape2", scheduledStartTimeUTC: shape2Start);
			shape1.MakeVisiblePrerequisiteOf(shape2, diagramShape);

			shape1.Validation.ValidateAll();
			shape2.Validation.ValidateAll();
			AssertNoErrors(shape1);
			AssertNoErrors(shape2);

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagramShape))
			{
				diagramForm.Show();
				Application.DoEvents();

				var networkViewModel = diagramForm.NetworkViewModel;
				var network = (JobNetwork)networkViewModel.Network;
				var diagram = network.DiagramEntity;

				diagramForm.Show();
				Application.DoEvents();

				new SuggestBufferAction(networkViewModel).Execute();
				var buffer = diagramShape.ChildShapes.OfType<BMNCNBufferShape>().First();
				new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(buffer);
				new ApproveDiagramAction(networkViewModel).Execute();

				diagramForm.FireSaveButton();

				AssertEquals("Our project buffer's penetration % is the sum of the two upflow workflows", 1.66667, (double)buffer.BufferPenetration);

				var firstShape = (ShapeNetworkEntity)network.Entities.First();

				firstShape.Shape.BNS_Status = "CLS";
				network.Refresh(RefreshType.EntityStatusChanged, network.Entities.Select(entity => (ShapeNetworkEntity)entity).ToArray());
				diagramForm.FireSaveButton();

				AssertEquals("Our project buffer's penetration % is now just the value of shape with two open tasks", 0.45, (double)buffer.BufferPenetration);
			}

			// close a task outside of the diagram
			task1.P9_Status = "CLS";
			Factory.Save();

			// return to the diagram to check the progress of our project
			using (var diagramForm = new NetworkDiagramForm(diagramShape))
			{
				diagramForm.Show();
				Application.DoEvents();

				var network = (JobNetwork)diagramForm.NetworkViewModel.Network;
				var buffer = diagramShape.ChildShapes.OfType<BMNCNBufferShape>().First();

				AssertEquals("Our project buffer's penetration % is now just the value of a single shape's single tasks's penetration", 0.03333, (double)buffer.BufferPenetration);
			}

			// close the last task outside of the diagram
			task2.P9_Status = "CLS";
			Factory.Save();

			// return to the diagram to check the progress of our project
			using (var diagramForm = new NetworkDiagramForm(diagramShape))
			{
				diagramForm.Show();
				Application.DoEvents();

				var buffer = diagramShape.ChildShapes.OfType<BMNCNBufferShape>().First();

				AssertEquals("Our project buffer's penetration should now be 0, as all work is closed", 0, (int)buffer.BufferPenetration);
			}

			ProcessTask CreateOpenTaskWithTimeLeftForWorkflow(ProcessHeader wf)
			{
				var task = BMSTestHelper.CreateTask(wf);
				task.P9_EstimatedTimeToComplete = ZDateTime.Now;
				task.P9_Description = "An tasku";
				task.P9_Type = "UDF";

				return task;
			}
		}

		public void TestResizeShape_WhenLinkedToScaledDiagram_AndLinkedDiagramResizeIsInvalid_ShouldDisplayErrorBeforePropagatingSchedule_WithOptionToOpenLinkedDiagramForm_AnsweringYes()
		{
			using (var diagramForm = new NetworkDiagramForm(scaledDiagram1))
			{
				diagramForm.Show();
				Application.DoEvents();

				var network1 = (JobNetwork)diagramForm.NetworkViewModel.Network;
				network1.LinkEntity(shapeOnDiagram1, scaledDiagram2);

				AssertShapeAndLinkedDiagramSchedule("Pre-condition: shape has schedule dates set, but they're not yet propagated to the linked diagram",
					ZDateTime.Now, ZDateTime.Now.AddDays(4),
					shouldLinkedDiagramBeTheSame: false, ZDateTime.Empty, ZDateTime.Empty);

				VisualBoardsTestCase.AssertSaved(diagramForm.FireSaveButton());

				AssertShapeAndLinkedDiagramSchedule("Now that the network has been saved, schedule should be propagated to the linked diagram",
					ZDateTime.Now, ZDateTime.Now.AddDays(4),
					shouldLinkedDiagramBeTheSame: true);

				NetworkTestCase.SetShapeSize(network1, shapeOnDiagram1, scaledDiagram1, 300, 200);
				var saveResult = diagramForm.FireSaveButton();

				using (var linkedDiagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault(f => f.BusinessEntity.Identifier == scaledDiagram2.PK))
				{
					Application.DoEvents();

					AssertHasRowError(network1.DiagramEntity, "Could not save the diagram due to scheduling conflicts with linked diagrams. Please correct those conflicts before saving the form.");
					AssertEquals(ContinueWithSave.No, saveResult);

					AssertShapeAndLinkedDiagramSchedule("After resizing a shape and saving the network, the moved shape's schedule should be updated, but it should not be propagated to the linked diagram because that would cause the diagram surface to be too small for its largest shape",
						ZDateTime.Now, ZDateTime.Now.AddDays(3),
						shouldLinkedDiagramBeTheSame: false, ZDateTime.Now, ZDateTime.Now.AddDays(4));

					AssertEquals("Cannot Propagate Scheduling Changes", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(
@"Cannot propagate scheduling changes to linked diagrams because it would introduce conflicts. The diagram with the conflict is named [Anastasia Beaverhausen].

The system attempted to update the diagram with the following information:
• Scheduled Start Time: 01 Jan 2019 10:00
• Scheduled Finish Time: 04 Jan 2019 10:00

Would you like to open this diagram to attempt to fix the conflict?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNotNull("The linked diagram should have been opened to give the user the opportunity to make necessary edits so the propagation can work.", linkedDiagramForm);

					var network = (JobNetwork)linkedDiagramForm.NetworkViewModel.Network;
					var diagram = network.DiagramShape;
					var shape = diagram.ChildShapes.Single();

					NetworkTestCase.SetShapeSize(network, shape, diagram, 300, 200);

					VisualBoardsTestCase.AssertSaved(linkedDiagramForm.FireSaveButton(), linkedDiagramForm.BusinessEntity);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				VisualBoardsTestCase.AssertSaved(diagramForm.FireSaveButton());

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertShapeAndLinkedDiagramSchedule("After correcting the linked diagram's scheduling conflict, the original change should not be propagated to the linked diagram",
					ZDateTime.Now, ZDateTime.Now.AddDays(3),
					shouldLinkedDiagramBeTheSame: true);
			}
		}

		public void TestResizeShape_WhenLinkedToScaledDiagram_AndLinkedDiagramResizeIsInvalid_ShouldDisplayErrorBeforePropagatingSchedule_WithOptionToOpenLinkedDiagramForm_AnsweringNo()
		{
			using (var diagramForm = new NetworkDiagramForm(scaledDiagram1))
			{
				diagramForm.Show();
				Application.DoEvents();

				var network1 = (JobNetwork)diagramForm.NetworkViewModel.Network;
				network1.LinkEntity(shapeOnDiagram1, scaledDiagram2);

				AssertShapeAndLinkedDiagramSchedule("Pre-condition: shape has schedule dates set, but they're not yet propagated to the linked diagram",
					ZDateTime.Now, ZDateTime.Now.AddDays(4),
					shouldLinkedDiagramBeTheSame: false, ZDateTime.Empty, ZDateTime.Empty);

				VisualBoardsTestCase.AssertSaved(diagramForm.FireSaveButton());

				AssertShapeAndLinkedDiagramSchedule("Now that the network has been saved, schedule should be propagated to the linked diagram",
					ZDateTime.Now, ZDateTime.Now.AddDays(4),
					shouldLinkedDiagramBeTheSame: true);

				NetworkTestCase.SetShapeSize(network1, shapeOnDiagram1, scaledDiagram1, 300, 200);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var saveResult = diagramForm.FireSaveButton();

				using (var linkedDiagramForm = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault(f => f.BusinessEntity.Identifier == scaledDiagram2.PK))
				{
					AssertNull("The linked diagram should NOT have been opened since the user declined to do so.", linkedDiagramForm);
				}

				AssertHasRowError(network1.DiagramEntity, "Could not save the diagram due to scheduling conflicts with linked diagrams. Please correct those conflicts before saving the form.");
				AssertEquals(ContinueWithSave.No, saveResult);

				AssertShapeAndLinkedDiagramSchedule("After resizing a shape and saving the network, the moved shape's schedule should be updated, but it should not be propagated to the linked diagram because that would cause the diagram surface to be too small for its largest shape",
					ZDateTime.Now, ZDateTime.Now.AddDays(3),
					shouldLinkedDiagramBeTheSame: false, ZDateTime.Now, ZDateTime.Now.AddDays(4));

				AssertEquals("Cannot Propagate Scheduling Changes", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(
@"Cannot propagate scheduling changes to linked diagrams because it would introduce conflicts. The diagram with the conflict is named [Anastasia Beaverhausen].

The system attempted to update the diagram with the following information:
• Scheduled Start Time: 01 Jan 2019 10:00
• Scheduled Finish Time: 04 Jan 2019 10:00

Would you like to open this diagram to attempt to fix the conflict?", UnitTestUserNotification.Instance.LastMessage.Text);

				// Now let's fix the problem manually.
				var newFactory = Factory.CreateNewFactory();
				newFactory.RefreshEnabled = false; // Ensures the schedule propagation happens in a new factory rather than using the form's factory. That would be bad for memory usage.

				var loadedDiagram2 = newFactory.Load<BMNCNRootDiagramShape>(scaledDiagram2.PK);
				var network2 = NetworkTestCase.CreateNetwork(loadedDiagram2);
				var shapeOnDiagram2 = loadedDiagram2.ChildShapes.Single();

				NetworkTestCase.SetShapeSize(network2, shapeOnDiagram2, loadedDiagram2, 300, 200);
				newFactory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				VisualBoardsTestCase.AssertSaved(diagramForm.FireSaveButton());

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertShapeAndLinkedDiagramSchedule("After correcting the linked diagram's scheduling conflict, the original change should not be propagated to the linked diagram",
					ZDateTime.Now, ZDateTime.Now.AddDays(3),
					shouldLinkedDiagramBeTheSame: true);
			}
		}

		void AssertShapeAndLinkedDiagramSchedule(string assertionMessage, ZDateTime expectedShapeStartTime, ZDateTime expectedShapeFinishTime, bool shouldLinkedDiagramBeTheSame, ZDateTime? expectedLinkedDiagramStartTime = null, ZDateTime? expectedLinkedDiagramFinishTime = null)
		{
			CombineAssertions(assertionMessage, () =>
			{
				AssertEquals("Shape Scheduled Start", expectedShapeStartTime, shapeOnDiagram1.ScheduledStartTimeLocal);
				AssertEquals("Shape Scheduled Finish", expectedShapeFinishTime, shapeOnDiagram1.ScheduledFinishTimeLocal);

				AssertEquals("Linked diagram Scheduled Start", shouldLinkedDiagramBeTheSame ? expectedShapeStartTime : expectedLinkedDiagramStartTime.Value, scaledDiagram2.ScheduledStartTimeLocal);
				AssertEquals("Linked diagram Scheduled Finish", shouldLinkedDiagramBeTheSame ? expectedShapeFinishTime : expectedLinkedDiagramFinishTime.Value, scaledDiagram2.ScheduledFinishTimeLocal);
			});
		}

		public void TestProjectBufferPenetration_ShouldAlwaysMatchInternalBufferPenetration()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagramStartTime = ZDateTime.Now.AddDays(-5);
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.SwitchToScaled();
			diagramShape.ScheduledStartTimeLocal = diagramStartTime;

			Factory.Save();

			var shape1Start = diagramStartTime.AddDays(1);
			var shape2Start = diagramStartTime.AddDays(3);
			var shape1 = NetworkTestCase.CreateShape(diagramShape, "shape1", scheduledStartTimeUTC: shape1Start);
			var shape2 = NetworkTestCase.CreateShape(diagramShape, "shape2", scheduledStartTimeUTC: shape2Start);
			shape1.MakeVisiblePrerequisiteOf(shape2, diagramShape);

			shape1.Validation.ValidateAll();
			shape2.Validation.ValidateAll();
			AssertNoErrors(shape1);
			AssertNoErrors(shape2);

			Factory.Save();

			using (var diagramForm = new NetworkDiagramForm(diagramShape))
			{
				diagramForm.Show();
				Application.DoEvents();

				var networkViewModel = diagramForm.NetworkViewModel;
				var network = (JobNetwork)networkViewModel.Network;
				var diagram = network.DiagramEntity;

				diagramForm.Show();
				Application.DoEvents();

				new SuggestBufferAction(networkViewModel).Execute();
				var bufferTheBackend = diagramShape.ChildShapes.OfType<BMNCNBufferShape>().First();
				new AcceptBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(bufferTheBackend);
				new ApproveDiagramAction(networkViewModel).Execute();

				diagramForm.FireSaveButton();
			}

			using (var diagramForm = new NetworkDiagramForm(diagramShape))
			{
				diagramForm.Show();
				Application.DoEvents();

				var networkViewModel = diagramForm.NetworkViewModel;
				var network = (JobNetwork)networkViewModel.Network;
				var diagram = network.DiagramEntity;
				var bufferTheBackend = diagramShape.ChildShapes.OfType<BMNCNBufferShape>().First();
				var bufferTheFrontend = networkViewModel.ScheduledNodes.OfType<BufferViewModel>().First();

				AssertBufferPenetrationValues("PRE: Our buffer shapes have the correct initial value", 1.66667, bufferTheBackend, bufferTheFrontend);

				var firstShape = (ShapeNetworkEntity)network.Entities.First();

				firstShape.Shape.BNS_Status = "CLS";
				network.Refresh(RefreshType.EntityStatusChanged, network.Entities.Select(entity => (ShapeNetworkEntity)entity).ToArray());

				AssertBufferPenetrationValues("Our buffer shapes shouldn't be updated yet, since we haven't saved, and penetration calculation requires saving for now", 1.66667, bufferTheBackend, bufferTheFrontend);

				diagramForm.FireSaveButton();

				diagramForm.Show();
				Application.DoEvents();

				// ALL of these have to be re-initialised, because reloading a diagram doesn't update a viewmodel, it replaces it :/
				networkViewModel = diagramForm.NetworkViewModel;
				bufferTheBackend = diagramShape.ChildShapes.OfType<BMNCNBufferShape>().First();
				bufferTheFrontend = networkViewModel.ScheduledNodes.OfType<BufferViewModel>().First();

				AssertBufferPenetrationValues("Our buffer shapes should both update to the new value", 0.51667, bufferTheBackend, bufferTheFrontend);
			}
		}

		void AssertBufferPenetrationValues(string message, double penetration, BMNCNBufferShape shape, BufferViewModel entityViewModel)
		{
			var labelValue = Math.Round(shape.BufferPenetration * 100);
			CombineAssertions(message, () =>
			{
				AssertEquals(penetration, (double)shape.BufferPenetration);
				AssertEquals(labelValue + " %", entityViewModel.PenetrationPercentLabel);
			});
		}

		#region Setup Stuff

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);
			Factory.Save();

			scaledDiagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow, name: "Karen Walker");
			shapeOnDiagram1 = NetworkTestCase.CreateShape(scaledDiagram1, name: "Shape on Diagram 1");

			scaledDiagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Anastasia Beaverhausen");
			var shapeOnDiagram2 = NetworkTestCase.CreateShape(scaledDiagram2, name: "Shape on Diagram 2");

			var network1 = NetworkTestCase.CreateNetwork(scaledDiagram1);
			NetworkTestCase.SetShapeSize(network1, shapeOnDiagram1, scaledDiagram1, 400, 200);
			var network2 = NetworkTestCase.CreateNetwork(scaledDiagram2);
			NetworkTestCase.SetShapeSize(network2, shapeOnDiagram2, scaledDiagram2, 400, 200);

			network1.RefreshSchedules();
			network2.RefreshSchedules();

			Factory.Save();
		}

		BMNCNRootDiagramShape scaledDiagram1, scaledDiagram2;
		BMNCNShape shapeOnDiagram1;

		#endregion
	}
}
