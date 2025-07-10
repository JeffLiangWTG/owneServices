using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestDate(2015, 7, 14)]
	class JobNetworkScheduleRefresherTest : NetworkTestCase
	{
		#region When to refresh schedules

		public void TestShouldRecalculateSchedules_WhenRefreshingDiagram()
		{
			var shape = networkViewModel.CreateNewShape(diagram).AsShape();
			shape.ExplicitDurationMinutes = 8 * 60;

			network.RefreshSchedules();

			AssertEquals(0, shape.ScheduleBizo.BNC_EarliestStartOffsetMinutes);

			var entity = network.Entities.GetInstance(shape);
			entity.Schedule.EarliestStartHours = 1m;
			shape.ScheduleBizo.BNC_EarliestStartOffsetMinutes = 60;

			AssertEquals(60, shape.ScheduleBizo.BNC_EarliestStartOffsetMinutes);

			network.Refresh(RefreshType.RefreshButton);

			AssertEquals("Performing a full refresh should update schedules", 0, shape.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
		}

		public void TestShouldRecalculateSchedules_WhenCloningDiagram()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();
			shape1.ExplicitDurationMinutes = 8 * 60;
			shape2.ExplicitDurationMinutes = 8 * 60;

			shape1.Name = "Hawaiian Shirt";
			shape2.Name = "Camo Pants";

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			network.RefreshSchedules();

			AssertEquals(8 * 60, shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes);

			var entity = network.Entities.GetInstance(shape2);
			entity.Schedule.EarliestStartHours += 1m;
			shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes += 60;

			AssertEquals(false, shape2.IsInDatabase);
			AssertEquals(9 * 60, shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes);

			var action = new CloneDiagramAction(networkViewModel);
			action.Execute();

			AssertEquals(false, shape2.IsInDatabase);
			AssertEquals("Original diagram schedules should stay the same", 9 * 60, shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes);

			var cloneDiagram = action.FactoryForSpawnedNetwork_ExposedForTest.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Stevetember (copy)"));
			var cloneShape = cloneDiagram.ChildShapes.Single(s => s.Name == "Camo Pants");

			AssertEquals(8 * 60, cloneShape.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
			controller.Verify(m => m.ViewDiagram(It.IsAny<INetworkEntity>()), Times.Once());
			controller.Verify(m => m.TriggerSaveAction(), Times.Never());
		}

		public void TestShouldRecalculateSchedules_WhenPushingEarlyOrLate()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape3 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 8 * 60;
			shape2.ExplicitDurationMinutes = 8 * 60;
			shape3.ExplicitDurationMinutes = 2 * 60;

			shape1.BNS_Name = "shape1";
			shape2.BNS_Name = "shape2";
			shape3.BNS_Name = "shape3";

			network.CreateRelationship(shape1, shape2);
			network.CreateRelationship(shape3, shape2);

			network.RefreshSchedules();

			AssertEquals(true, shape1.ScheduleBizo.BNC_IsCriticalPath);
			AssertEquals(true, shape2.ScheduleBizo.BNC_IsCriticalPath);
			AssertEquals(false, shape3.ScheduleBizo.BNC_IsCriticalPath);

			AssertEquals(ZDateTime.UtcNow, shape3.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow, shape3.EarliestStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddHours(6), shape3.LatestStartTimeUtc);

			networkViewModel.PushAsLateAsPossible();

			AssertEquals(ZDateTime.UtcNow.AddHours(6), shape3.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow, shape3.EarliestStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddHours(6), shape3.LatestStartTimeUtc);

			var entity = network.Entities.GetInstance(shape3);
			entity.Schedule.EarliestStartHours += 1m;
			shape3.ScheduleBizo.BNC_EarliestStartOffsetMinutes += 60;

			networkViewModel.PushAsEarlyAsPossible();
			AssertEquals("Pushing early should update schedules before executing", ZDateTime.UtcNow, shape3.ScheduledStartTimeUtc);

			entity.Schedule.LatestStartHours -= 1m;
			shape3.ScheduleBizo.BNC_LatestStartOffsetMinutes -= 60;

			networkViewModel.PushAsLateAsPossible();
			AssertEquals("Pushing late should update schedules before executing", ZDateTime.UtcNow.AddHours(6), shape3.ScheduledStartTimeUtc);
		}

		public void TestShouldRecalculateSchedules_WhenApprovingTheDiagram()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 8 * 60;
			shape2.ExplicitDurationMinutes = 8 * 60;

			shape1.BNS_Name = "shape1";
			shape2.BNS_Name = "shape2";

			network.CreateRelationship(shape1, shape2);
			networkViewModel.PushAsEarlyAsPossible();
			network.RefreshSchedules();

			AssertEquals(0, shape1.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
			AssertEquals(shape2.ExplicitDurationMinutes, shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes);

			var entity = network.Entities.GetInstance(shape2);
			entity.Schedule.EarliestStartHours = 0m;
			shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes = 0;

			AssertEquals(false, diagram.IsApproved);

			new ApproveDiagramAction(networkViewModel).Execute();
			AssertEquals(true, diagram.IsApproved);

			AssertEquals(0, shape1.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
			AssertEquals("Approving the diagram should cause schedules to be updated", shape1.ExplicitDurationMinutes, shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
		}

		public void TestShouldRecalculateSchedules_WhenSavingTheDiagram()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 8 * 60;
			shape2.ExplicitDurationMinutes = 8 * 60;

			shape1.BNS_Name = "shape1";
			shape2.BNS_Name = "shape2";

			network.CreateRelationship(shape1, shape2);
			network.RefreshSchedules();

			AssertEquals(0, shape1.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
			AssertEquals(shape2.ExplicitDurationMinutes, shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes);

			var entity = network.Entities.GetInstance(shape2);
			entity.Schedule.EarliestStartHours = 0m;
			shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes = 0;

			network.Refresh(RefreshType.Saving);

			AssertEquals(0, shape1.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
			AssertEquals("Saving the diagram should cause schedules to be updated", shape1.ExplicitDurationMinutes, shape2.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
		}

		#endregion

		#region When NOT to refresh schedules

		public void TestShouldNotRecalculateSchedules_WhenOpeningDiagram()
		{
			var shape = networkViewModel.CreateNewShape(diagram).AsShape();
			network.RefreshSchedules();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSchedule = newFactory.LoadTop1<BMNCNSchedule>(new ZQuery(BMNCNScheduleSchema.BNC_BNS_Shape, shape.PK));

			var newValue = loadedSchedule.BNC_EarliestStartUtc.AddDays(1);

			loadedSchedule.BNC_EarliestStartUtc = newValue;
			newFactory.Save();

			newFactory = newFactory.CreateNewFactory();

			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			network = CreateNetwork(loadedDiagram);
			loadedSchedule = newFactory.LoadTop1<BMNCNSchedule>(new ZQuery(BMNCNScheduleSchema.BNC_BNS_Shape, shape.PK));

			AssertEquals("Should not have re-calculated schedules on loading the diagram", newValue, loadedSchedule.BNC_EarliestStartUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenCreatingShape()
		{
			var shape = networkViewModel.CreateNewShape(diagram).AsShape();

			AssertEquals(ZDateTime.Empty, shape.ScheduleBizo.BNC_EarliestStartUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenDeletingShape()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			shape2.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;

			network.CreateRelationship(shape1, shape2);
			network.RefreshSchedules();

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.DeleteEntity(shape1);

			AssertEquals("Deleting a shape shouldn't update other shapes' schedules. Yet.", ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.Refresh(RefreshType.Saving);
			AssertEquals(ZDateTime.UtcNow, shape2.ScheduleBizo.BNC_EarliestStartUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenResizingShape()
		{
			var shape = networkViewModel.CreateNewShape(diagram).AsShape();

			shape.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			network.RefreshSchedules();

			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape.ScheduleBizo.BNC_EarliestFinishUtc);

			var entity = network.Entities.GetInstance(shape);

			entity.Width *= 2;
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape.ScheduleBizo.BNC_EarliestFinishUtc);

			network.Refresh(RefreshType.EntitySize, entity);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape.ScheduleBizo.BNC_EarliestFinishUtc);

			network.Refresh(RefreshType.Saving);
			AssertEquals(ZDateTime.UtcNow.AddDays(2), shape.ScheduleBizo.BNC_EarliestFinishUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenCreatingArrow()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			shape2.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			network.RefreshSchedules();

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow, shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.CreateRelationship(shape1, shape2);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow, shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.Refresh(RefreshType.Saving);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenDeletingArrow()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			shape2.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;

			var relationship = network.CreateRelationship(shape1, shape2);
			network.RefreshSchedules();

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.DeleteRelationship(relationship);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.Refresh(RefreshType.Saving);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow, shape2.ScheduleBizo.BNC_EarliestStartUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenCreatingResourceDependencyArrow()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			shape2.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			network.RefreshSchedules();

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow, shape2.ScheduleBizo.BNC_EarliestStartUtc);

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteForShapes(shape1, shape2);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow, shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.Refresh(RefreshType.Saving);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenDeletingResourceDependencyArrow()
		{
			var shape1 = networkViewModel.CreateNewShape(diagram).AsShape();
			var shape2 = networkViewModel.CreateNewShape(diagram).AsShape();

			shape1.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			shape2.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteForShapes(shape1, shape2);
			network.RefreshSchedules();

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);

			var relationship = shape1.AllAttachments.Single(a => a.BNA_Type == AttachmentTypeList.Codes.ResourceDependency);
			network.DeleteRelationship(relationship);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape2.ScheduleBizo.BNC_EarliestStartUtc);

			network.Refresh(RefreshType.Saving);

			AssertEquals(ZDateTime.UtcNow, shape1.ScheduleBizo.BNC_EarliestStartUtc);
			AssertEquals(ZDateTime.UtcNow, shape2.ScheduleBizo.BNC_EarliestStartUtc);
		}

		public void TestShouldNotRecalculateSchedules_WhenUnApprovingDiagram()
		{
			var shapeEntity = networkViewModel.CreateNewShape(diagram);
			var shape = shapeEntity.AsShape();

			shape.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;
			network.RefreshSchedules();

			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape.ScheduleBizo.BNC_EarliestFinishUtc);

			new ApproveDiagramAction(networkViewModel).Execute();
			AssertEquals(true, diagram.IsApproved);

			shapeEntity.ExplicitDurationMinutes *= 2;
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape.ScheduleBizo.BNC_EarliestFinishUtc);

			new UnapproveDiagramAction(networkViewModel).Execute();
			AssertEquals(false, diagram.IsApproved);
			AssertEquals(ZDateTime.UtcNow.AddDays(1), shape.ScheduleBizo.BNC_EarliestFinishUtc);

			network.Refresh(RefreshType.Saving);
			AssertEquals(ZDateTime.UtcNow.AddDays(2), shape.ScheduleBizo.BNC_EarliestFinishUtc);
		}

		#endregion

		#region Regression testing

		public void TestOpenDiagram_ShouldPopulateSchedulesOnLoad()
		{
			diagram.EarliestStartTimeUtc = ZDateTime.Empty;
			network.Refresh(RefreshType.Saved);

			var shape = networkViewModel.CreateNewShape(diagram).AsShape();

			shape.ExplicitDurationMinutes = 8 * 60;
			network.RefreshSchedules();

			AssertScheduleInfo(shape, 0m, 8m, 0m, 8m, 8 * 60, 0m, true);
			AssertEquals(8m, shape.ExplicitDurationLabel);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var newNetwork = CreateNetwork(loadedDiagram);

			var loadedShape = newNetwork.Shapes.Single();

			AssertScheduleInfo(shape, 0m, 8m, 0m, 8m, 8 * 60, 0m, true);
		}

		public void TestRefreshSchedules_ShouldNotCreateNewFactories()
		{
			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				var startingTrackedFactories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.ToArray();

				network.RefreshSchedules(forceReCalculation: true);

				AssertSequencesEqual("Refreshing schedules shouldn't create new factories. There isn't any multithreading here anymore, so the network's factory can be used.", startingTrackedFactories, PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest);
			}
		}

		#endregion

		#region Implementation

		BMNCNShape diagram;
		NetworkViewModel networkViewModel;
		IJobNetwork network;
		Mock<IBMNetworkEntityController> controller;

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			diagram = CreateDiagram(Factory, name: "Stevetember");
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			controller = CreateMockableController(Mocks);
			networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			Factory.Save();
		}

		#endregion
	}
}
