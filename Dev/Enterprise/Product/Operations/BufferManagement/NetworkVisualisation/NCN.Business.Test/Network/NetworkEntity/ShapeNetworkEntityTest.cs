using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ShapeNetworkEntity))]
	class ShapeNetworkEntityTest : NonPersistentBusinessObjectTestCase
	{
		#region Owner Considerations

		public void TestOwner_ShouldConsiderNetworkScope()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var childShape = NetworkTestCase.CreateShape(diagram);
			var grandchildShape = NetworkTestCase.CreateShape(childShape);

			var network1 = NetworkTestCase.CreateNetwork(diagram);
			var network2 = NetworkTestCase.CreateNetwork(childShape);

			AssertNull(network1.Entities.GetInstance(diagram).Owner);
			AssertEquals(diagram, network1.Entities.GetInstance(childShape).Owner.Shape);
			AssertEquals(childShape, network1.Entities.GetInstance(grandchildShape).Owner.Shape);

			AssertNull(network2.Entities.GetInstance(childShape).Owner);
			AssertEquals(childShape, network2.Entities.GetInstance(grandchildShape).Owner.Shape);
		}

		public void TestCoordinates_ShouldConsiderNetworkScope()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram");
			var childShape = NetworkTestCase.CreateShape(diagram, name: "Child Shape");
			var grandchildShape = NetworkTestCase.CreateShape(childShape, name: "Grandchild Shape");

			childShape.Left = 10;
			childShape.Top = 100;
			grandchildShape.Left = 10;
			grandchildShape.Top = 100;

			var network1 = NetworkTestCase.CreateNetwork(diagram);

			AssertEquals("Left offset of childShape + root diagram offset of 0", 10d, network1.Entities.GetInstance(childShape).X);
			AssertEquals("Top offset of childShape + root diagram offset of 0", 100d, network1.Entities.GetInstance(childShape).Y);

			AssertEquals("Left offset of grandchildShape + left offset of childShape", 20d, network1.Entities.GetInstance(grandchildShape).X);
			AssertEquals("Top offset of grandchildShape + top offset of childShape", 200d, network1.Entities.GetInstance(grandchildShape).Y);

			var network2 = NetworkTestCase.CreateNetwork(childShape);

			AssertEquals("Left offset of grandchildShape + 0 since we're looking at childShape as the diagram surface", 10d, network2.Entities.GetInstance(grandchildShape).X);
			AssertEquals("Top offset of grandchildShape + 0 since we're looking at childShape as the diagram surface", 100d, network2.Entities.GetInstance(grandchildShape).Y);
		}

		public void TestMoveShapeOutsideOwnerBounds_ShouldBeBlocked()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram");
			var childShape = NetworkTestCase.CreateShape(diagram, name: "Child Shape");
			var grandchildShape = NetworkTestCase.CreateShape(childShape, name: "Grandchild Shape");
			var greatGrandchildShape = NetworkTestCase.CreateShape(grandchildShape, name: "Great Grandchild Shape");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			childShape.Width = childShape.Height = 300;
			grandchildShape.Width = grandchildShape.Height = 200;
			greatGrandchildShape.Width = greatGrandchildShape.Height = 100;

			var childEntity = network.Entities.GetInstance(childShape);
			var grandChildEntity = network.Entities.GetInstance(grandchildShape);
			var greatGrandChildEntity = network.Entities.GetInstance(greatGrandchildShape);

			var greatGrandChildViewModel = NetworkTestCase.CreateNodeViewModel(greatGrandChildEntity, networkViewModel);

			CombineAssertions("Preconditions: entities should be arranged with offsets as the network is built", () =>
			{
				AssertEquals(0d, childEntity.X);
				AssertEquals(0d, childEntity.Y);

				AssertEquals(0d, grandChildEntity.X);
				AssertEquals(40d, grandChildEntity.Y);

				AssertEquals(0d, greatGrandChildEntity.X);
				AssertEquals(80d, greatGrandChildEntity.Y);
				AssertEquals(100d, greatGrandChildEntity.Width);
				AssertEquals(100d, greatGrandChildEntity.Height);
			});

			var expectedGreatGrandChildEntityY = grandChildEntity.Y + grandChildEntity.Height - greatGrandChildEntity.Height - 5; // 5px of unusable space

			greatGrandChildViewModel.X = 1000;
			greatGrandChildViewModel.Y = 1000;

			AssertEquals(100d, greatGrandChildEntity.X);
			AssertEquals(expectedGreatGrandChildEntityY, greatGrandChildEntity.Y);
			AssertEquals(100d, greatGrandChildEntity.Width);
			AssertEquals(100d, greatGrandChildEntity.Height);
		}

		#endregion

		#region Notifications

		public void TestNotifications()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = networkViewModel.CreateNewShape(diagramShape);
			var shape2 = networkViewModel.CreateNewShape(diagramShape);
			var attachment = (NetworkAttachment)network.CreateRelationship(shape1, shape2);

			shape1.X = 0;
			shape1.Width = 300;
			shape2.X = 300;
			shape2.Width = 300;

			AssertEquals(0, shape1.Notifications.Count());

			var shape1Notified = false;
			var shape2Notified = false;
			shape1.PropertyChanged += (s, e) => shape1Notified = true;
			shape2.PropertyChanged += (s, e) => shape2Notified = true;

			shape2.X = 100;

			Assert(shape1Notified);
			Assert(shape2Notified);
		}

		public void TestShapeNetworkEntity_ShouldBeNotifiedAboutShapeChanged_ByReliableWeakEventManager()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var entity = networkViewModel.CreateNewShape(diagramShape);
			var shape = entity.AsShape();

			var entityNotified = false;
			var propertyName = string.Empty;
			entity.PropertyChanged += (s, e) =>
			{
				entityNotified = true;
				propertyName = e.PropertyName;
			};

			GC.Collect(); // to check that the manager's scope is outside the constructor on the ShapeNetworkEntity and the manager is not GC'ed

			shape.BNS_Status = "SUS";

			Assert("Should notify ShapeNetworkEntity about changes in BMNCNShape", entityNotified);
			AssertEquals("Status", propertyName);
		}

		#endregion

		#region Schedules

		[TestDate(2020, 02, 24)]
		public void TestScheduledTimes_WhenShapeMoved_AndDiagramHasStartAndFinishTime_ShouldUpdateToCorrectTime()
		{
			var diagramStartTime = new ZDateTime(ZDateTime.UtcNow).AddDays(-7);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.ScheduledStartTimeUtc = diagramStartTime;
			diagram.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(7);

			var shape = NetworkTestCase.CreateShape(diagram, "Crentist");

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			shape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			shape.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();

			var diagramShape = network.DiagramEntity;
			var shapeShape = network.Entities.GetInstance(shape);

			shapeShape.X = 0; // move this to the beginning of the diagram

			AssertEquals("Our shape is now at the beginning of the diagram", (double)0, shapeShape.X);
			AssertEquals("Our shape is now starts where the diagram starts", diagramStartTime, shape.ScheduledStartTimeUtc);

			shapeShape.X += 100; // move this to the next column of the diagram

			AssertEquals("Our shape is now in the first column of the diagram", (double)100, shapeShape.X);
			AssertEquals("Our shape is now starts where the diagram starts", diagramStartTime.AddHours(BMConstants.WorkingHoursPerDay), shape.ScheduledStartTimeUtc);

			shapeShape.X += 100; // move this to the next column of the diagram, again

			AssertEquals("Our shape is now in the first column of the diagram", (double)200, shapeShape.X);
			AssertEquals("Our shape is now starts where the diagram starts", diagramStartTime.AddHours(BMConstants.WorkingHoursPerDay * 2), shape.ScheduledStartTimeUtc);
		}

		#endregion

		#region Schedule Info

		public void TestScheduleInfo_ScaledShapeLinkedToWorkflow()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Crentist");
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(workflow, diagram, "Crentist");
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			shape.ScheduleBizo.BNC_ScheduledStartUtc = new ZDateTime(2015, 7, 14);
			shape.ScheduleBizo.BNC_ScheduledFinishUtc = new ZDateTime(2016, 10, 25);

			Factory.Save();

			var entity = network.Entities.GetInstance(shape);

			AssertScheduleInfo(entity, 480, 90, new ZDateTime(2015, 7, 14, 10, 0, 0), new ZDateTime(2016, 10, 25, 10, 0, 0));
		}

		public void TestScheduleInfo_NonScaledShapeLinkedToWorkflow()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Crentist");
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(workflow, diagram, "Crentist");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			var entity = network.Entities.GetInstance(shape);

			AssertScheduleInfo(entity, 180, 90, ZDateTime.Empty, ZDateTime.Empty);
		}

		public void TestScheduleInfo_ScaledShapeNotLinkedToWorkflow()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram, "Crentist");
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			shape.ScheduleBizo.BNC_ScheduledStartUtc = new ZDateTime(2015, 7, 14);
			shape.ScheduleBizo.BNC_ScheduledFinishUtc = new ZDateTime(2016, 10, 25);

			Factory.Save();

			var entity = network.Entities.GetInstance(shape);

			AssertScheduleInfo(entity, 1440, 0, new ZDateTime(2015, 7, 14, 10, 0, 0), new ZDateTime(2016, 10, 25, 10, 0, 0));
		}

		public void TestScheduleInfo_NonScaledShapeNotLinkedToWorkflow()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram, "Crentist");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			var entity = network.Entities.GetInstance(shape);

			AssertScheduleInfo(entity, 0, 0, ZDateTime.Empty, ZDateTime.Empty);
		}

		#endregion

		#region Drag/Drop

		public void TestMoveShape_ShouldRefreshSchedulePropertyBinding()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			var propertiesRefreshed = new List<string>();

			var entity = network.Entities.GetInstance(shape);
			entity.PropertyChanged += (s, e) => propertiesRefreshed.Add(e.PropertyName);

			entity.X += 200;

			AssertContainsExactElementsInAnyOrder(new[] { nameof(ShapeNetworkEntity.X), nameof(NodeViewModel.StartDateReadableText), nameof(NodeViewModel.FinishDateReadableText) }, propertiesRefreshed);
		}

		public void TestResizeBuffer_ShouldRefreshDurationPropertyBinding()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var buffer = networkViewModel.SuggestAndAcceptAllBuffers().Single();

			var bufferPropertiesRefreshed = new List<string>();
			var shapePropertiesRefreshed = new List<string>();

			var bufferEntity = network.Entities.GetInstance(buffer);
			bufferEntity.PropertyChanged += (s, e) => bufferPropertiesRefreshed.Add(e.PropertyName);

			var shapeEntity = network.Entities.GetInstance(shape);
			shapeEntity.PropertyChanged += (s, e) => shapePropertiesRefreshed.Add(e.PropertyName);

			bufferEntity.Width += 200;
			shapeEntity.Width += 200;

			var sharedPropertiesExpectedToRefresh = new[]
			{
				nameof(ShapeNetworkEntity.Width),
				nameof(NodeViewModel.StartDateReadableText),
				nameof(NodeViewModel.FinishDateReadableText),
				nameof(ShapeNetworkEntity.EntityState),
				nameof(ShapeNetworkEntity.HasNotifications),
			};

			AssertContainsExactElementsInAnyOrder(sharedPropertiesExpectedToRefresh, shapePropertiesRefreshed);
			AssertContainsExactElementsInAnyOrder(sharedPropertiesExpectedToRefresh.Append(nameof(ShapeNetworkEntity.AdditionalDetail)), bufferPropertiesRefreshed);
		}

		#endregion

		#region Menu Items

		public void TestCustomNetworkActions_ForTheVariousShapeTypes()
		{
			CombineAssertions("Normal diagram", () =>
			{
				var diagram = NetworkTestCase.CreateDiagram(Factory);

				var shape = NetworkTestCase.CreateShape(diagram);
				var nestedShape = NetworkTestCase.CreateShape(shape);

				var buffer = NetworkTestCase.CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
				var annotation = NetworkTestCase.CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);

				AssertMenuItems("Root shape should NOT have 'Open as Diagram'", networkViewModel, diagram, LinkedEntityAndActionsMenuItems);
				AssertMenuItems("Shape should have 'Open as Diagram'", networkViewModel, shape, MenuItemsWithOpenAsDiagram);
				AssertMenuItems("Nested shape should have 'Open as Diagram'", networkViewModel, nestedShape, MenuItemsWithOpenAsDiagram);

				AssertMenuItems("Buffer shape should never have 'Open as Diagram'", networkViewModel, buffer, ActionsMenuItemOnly);
				AssertMenuItems("Annotation shape should never have 'Open as Diagram'", networkViewModel, annotation, NoMenuItemsAtAll);
			});

			CombineAssertions("Default diagram", () =>
			{
				var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Spare Parts");
				var defaultDiagram = workflow.JobHeader.GetDefaultDiagram();
				var defaultShape = NetworkTestCase.CreateDefaultDiagramWorkflowShape(workflow);
				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(defaultDiagram);

				AssertMenuItems("Default diagram should NOT have 'Open as Diagram'", networkViewModel, defaultDiagram, ActionsMenuItemOnly);
				AssertMenuItems("Default workflow shape should NOT have 'Open as Diagram'", networkViewModel, defaultShape, ActionsMenuItemOnly);
			});
		}

		static string[] NoMenuItemsAtAll => Array.Empty<string>();
		static string[] ActionsMenuItemOnly => new[] { "Actions" };
		static string[] LinkedEntityAndActionsMenuItems => new[] { "Linked Entity" }.Concat(ActionsMenuItemOnly).ToArray();
		static string[] MenuItemsWithOpenAsDiagram => LinkedEntityAndActionsMenuItems.Append("Open as Diagram").ToArray();

		static void AssertMenuItems(string message, NetworkViewModel networkViewModel, BMNCNShape shape, params string[] expectedMenuItemNames)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape))
			{
				var menuItems = networkViewModel.GetCustomNetworkActions().ToMenuItemsGrouped();

				AssertArrayEqualsByElements(message, expectedMenuItemNames, menuItems.Select(a => a?.Name).ToArray());
			}
		}

		public void TestCreateJobCustomAction_ShouldNotBeEnabled_WhenShapeAlreadyLinkedToJob()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Diagram job");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Shape workflow");

			shape.BNS_JobType = "ORG";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			var mocks = new MockRepository(MockBehavior.Default);
			var controller = NetworkTestCase.CreateMockableController(mocks);
			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(loadedJobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			new CreateJobAction(networkViewModel, linkOnly: true).ExecuteForEntityWithoutAccessCheck(shape);

			AssertEquals("Precondition: shape is linked to a job", jobHeader, shape.ProcessHeader);
			Factory.Save();

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape))
			{
				var menuItems = networkViewModel.GetCustomNetworkActions().ToMenuItemsGrouped();
				var createJobActionMenuItem = menuItems.Single(i => i.Name == "Actions").Items.Single(i => i.Name == "Create Job");
				AssertEquals("Should be disabled as the shape is already linked to the job", false, createJobActionMenuItem.Enabled);
			}
		}

		public void TestSingleNetworkActionsShouldExecuteForRightClickedEntity_NotForFirstSelectedEntity()
		{
			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var diagram = Factory.New<BMNCNShape>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "job");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			var mocks = new MockRepository(MockBehavior.Default);
			var controller = NetworkTestCase.CreateMockableController(mocks);
			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(loadedJobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "shape1";
			childShape2.Name = "shape2";
			childShape1.AsShape().BNS_JobType = "ORG";
			childShape2.AsShape().BNS_JobType = "ORG";

			Factory.Save();

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, childShape2))
			{
				var menuItems = networkViewModel.GetCustomNetworkActions().ToMenuItemsGrouped();
				var actionsMenuItem = menuItems.Single(i => i.Name == "Actions");
				var createJobActionMenuItem = actionsMenuItem.Items.Single(i => i != null && i.Name == "Create Job");
				var action = (CreateJobAction)createJobActionMenuItem.Action;
				action.Execute();
				AssertEquals("This is childShape2 which should be linked, not childShape1", jobHeader,
					childShape2.ProcessHeader);
			}
		}

		#endregion

		#region Channels

		public void TestDiagramChannels_WhenChannelsExist_ShouldIncludeNonChanneledAtEnd()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			NetworkTestCase.CreateChannel(diagram, "Kablamo!");
			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = diagram.AsEntity(network);

			AssertSequencesEqual("'Non-channeled' should appear as one of the channel names. SAD!", new[] { "Kablamo!", "Non-channeled" }, entity.DiagramChannels.Select(x => x.Name));
		}

		public void TestDiagramChannels_WhenNoChannelsExist_ShouldBeEmpty()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = diagram.AsEntity(network);

			AssertSequencesEqual("ChannelNames should be empty when there are no channels defined. That includes 'Non-channeled'. SAD!", Array.Empty<string>(), entity.DiagramChannels.Select(x => x.Name));
		}

		public void TestDiagramChannels_ShouldBeSortedBySequence()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			NetworkTestCase.CreateChannel(diagram, "Earliest in the alphabet", 2);
			NetworkTestCase.CreateChannel(diagram, "Zap! Even later than 'Non-channeled'", 1);
			NetworkTestCase.CreateChannel(diagram, "Later in the alphabet", 3);

			AssertSequencesEqual("The channels were created in an order that isn't alphabetical or in order of sequence, either descending or ascending. This is an important precondition to prove that we really are using sequence number for sorting. SAD!",
				new[] { "Earliest in the alphabet", "Zap! Even later than 'Non-channeled'", "Later in the alphabet" }, diagram.Channels.Select(x => x.BNL_Name.ToString()));

			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = diagram.AsEntity(network);

			AssertSequencesEqual("The channel names must be sorted by sequence number, followed by 'Non-channeled'. SAD!", new[] { "Zap! Even later than 'Non-channeled'", "Earliest in the alphabet", "Later in the alphabet", "Non-channeled" }, entity.DiagramChannels.Select(x => x.Name));
		}

		public void TestDiagramChannels_ShouldIncludeCorrectHeightValues()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			NetworkTestCase.CreateChannel(diagram, "Blake");
			NetworkTestCase.CreateChannel(diagram, "Bortals", height: 420);

			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = diagram.AsEntity(network);

			AssertSequencesEqual("The channel heights should be used. SAD!", new[] { "Blake: 600", "Bortals: 420", "Non-channeled: 600" }, entity.DiagramChannels.Select(x => $"{x.Name}: {x.Height}"));
		}

		#endregion

		#region NonScheduled Section

		public void TestShouldShowNonScheduledSection_ForNonRootDiagramShape_ShouldBeFalse()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape = NetworkTestCase.CreateShape(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = shape.AsEntity(network);

			AssertEquals(false, entity.ShouldShowNonScheduledSection);
		}

		public void TestShouldShowNonScheduledSection_ForRootDiagramShape_ShouldReturnValueFromShape()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = diagram.AsEntity(network);

			AssertEquals(false, entity.ShouldShowNonScheduledSection);

			diagram.ShouldShowNonScheduledSection = true;

			AssertEquals(true, entity.ShouldShowNonScheduledSection);
		}

		public void TestShowNonScheduledSection_ForScaledDiagram_ShouldNotBeReadOnly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = diagram.AsEntity(network);

			AssertEquals("The property should be readonly, because only scaled diagrams may use it. SAD!", false, IsNonScheduledPropertyReadOnly(entity));
		}

		public void TestShowNonScheduledSection_ForNonScaledDiagram_ShouldBeReadOnly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: false);
			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = diagram.AsEntity(network);

			AssertEquals("The property should not be readonly, because scaled diagrams may use it. SAD!", true, IsNonScheduledPropertyReadOnly(entity));
		}

		bool IsNonScheduledPropertyReadOnly(ShapeNetworkEntity entity)
		{
			return ((IAccessBusinessObject)entity).IsPropertyReadOnly(nameof(ShapeNetworkEntity.ShouldShowNonScheduledSection));
		}

		public void TestCanHaveSchedule_ForNonScheduledShapes_ShouldBeFalse()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = NetworkTestCase.CreateShape(diagram);

			var network = NetworkTestCase.CreateNetwork(diagram);
			var entity = shape.AsEntity(network);

			AssertEquals(true, entity.CanHaveSchedule);

			shape.IsNonScheduled = true;
			AssertEquals("Non-scheduled shapes should not be able to have schedules. SAD!", false, entity.CanHaveSchedule);
		}

		[TestDate(2019, 7, 19, 8, 30, 01)]
		public void TestShowNonScheduledSection_ShouldNotConsiderNonScheduledEntitiesForSchedulingDateCalculations_WhenNonScheduledFurtherRight()
		{
			AssertAllShapeTypes_WhenInNonScheduledSection_WithSomePosition_DoNotCauseValidationErrors(100);
		}

		[TestDate(2019, 7, 19, 8, 30, 01)]
		public void TestShowNonScheduledSection_ShouldNotConsiderNonScheduledEntitiesForSchedulingDateCalculations_WhenNonScheduledInline()
		{
			AssertAllShapeTypes_WhenInNonScheduledSection_WithSomePosition_DoNotCauseValidationErrors(0);
		}

		[TestDate(2019, 7, 19, 8, 30, 01)]
		public void TestShowNonScheduledSection_ShouldNotConsiderNonScheduledEntitiesForSchedulingDateCalculations_WhenNonScheduledFurtherLeft()
		{
			AssertAllShapeTypes_WhenInNonScheduledSection_WithSomePosition_DoNotCauseValidationErrors(-100);
		}

		readonly string[] ShapeTypes_CannotBePlacedInUnscheduledSection =
		{
			ShapeTypeList.Codes.Buffer // this can be positioned anywhere in a main diagram surface, but can't be created in or imported into a non-scheduled section
		};

		void AssertAllShapeTypes_WhenInNonScheduledSection_WithSomePosition_DoNotCauseValidationErrors(int horizontalOffset)
		{
			const int offsetOfBaseShape = 200;
			var diagramStartTime = ZDateTime.UtcNow.AddDays(-2);
			var diagramEndTime = ZDateTime.UtcNow.AddDays(10);
			var shapeStartTime = ZDateTime.UtcNow;
			var shapeEndTime = ZDateTime.UtcNow.AddHours(1);

			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "The Diagram", isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			diagram.ScheduledStartTimeUtc = diagramStartTime;
			diagram.ScheduledFinishTimeUtc = diagramEndTime;

			var network = NetworkTestCase.CreateNetwork(diagram);
			var diagramEntity = CreateDiagramEntityAndAssertProperlyScaled(diagram, network, diagramStartTime, diagramEndTime);
			diagramEntity.Name = "The DiagramEntity";

			var shape = NetworkTestCase.CreateShape(diagram, name: "The Shape", scheduledStartTimeUTC: shapeStartTime);
			shape.IsNonScheduled = false;
			shape.ScheduledFinishTimeUtc = shapeEndTime;

			var entity = CreateShapeEntityAndAssertProperlyScaled(shape, network, shape.ScheduledStartTimeLocal, shape.ScheduledFinishTimeLocal);
			entity.Name = "The ShapeEntity";
			NetworkTestCase.SetDimensionsForEntity(entity, offsetOfBaseShape, 0, 100, 100);

			Factory.Save();

			diagramEntity.Validation.ValidateAll();
			CombineAssertions("PRE: No errors or warnings yet!", () =>
			{
				AssertNoWarnings(diagramEntity);
				AssertNoErrors(diagramEntity);
			});

			var typeCodeList = new ShapeTypeList().ToArray()
				.Select(pair => pair.Code)
				.Where(code => !ShapeTypes_CannotBePlacedInUnscheduledSection.Contains(code))
				.ToArray();

			foreach (var shapeType in typeCodeList)
			{
				var genericShape = NetworkTestCase.CreateShapeFromTypeString(shapeType, Factory, diagram);
				genericShape.IsNonScheduled = true;

				var newEntity = genericShape.AsEntity(network);
				newEntity.IsNonScheduled = true;
				NetworkTestCase.SetDimensionsForEntity(newEntity, offsetOfBaseShape + horizontalOffset, 0, 100, 100);

				CombineAssertions($"PRE: Create a {shapeType}-codeshape on the non-scheduled side of the diagram", () =>
				{
					AssertEquals("new shape is NonScheduled", true, genericShape.IsNonScheduled);
					AssertEquals("new entity is also NonScheduled", true, newEntity.IsNonScheduled);
					AssertEquals("new entity has diagram as a parent", diagram.PK, newEntity.Root.PK);
				});

				diagramEntity.Validation.ValidateAll();

				string relativeShapePosition = "";

				switch (offsetOfBaseShape + horizontalOffset)
				{
					case int pos when (pos > offsetOfBaseShape):
						relativeShapePosition = "more right than";
						break;
					case int pos when (pos < offsetOfBaseShape):
						relativeShapePosition = "more left than";
						break;
					default: // pos == offsetOfBaseShape
						relativeShapePosition = "inline with";
						break;
				}

				CombineAssertions($"For shapes of type {shapeType} in a non-scheduled section, being {relativeShapePosition} the most right scheduled shape should be ok", () =>
				{
					AssertNoWarnings(diagramEntity);
					AssertNoErrors(diagramEntity);
				});

				newEntity.Delete();
			}
		}

		#endregion

		#region Foreground Color

		public void TestForegroundColor()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var diagramEntity = diagram.AsEntity(network);
			AssertEquals(Color.Black, diagramEntity.ForeColor);

			diagram.ForeColor = "Chartreuse";
			AssertEquals(Color.Chartreuse, diagramEntity.ForeColor);

			AssertColor(Color.Chartreuse, networkViewModel.DiagramNodeViewModel.ForegroundColor);
			AssertColor(Color.Chartreuse, networkViewModel.DiagramNodeViewModel.CompletionCriteriaTextColor);
		}

		void AssertColor(Color expectedColor, Color actualColor)
		{
			AssertEquals(expectedColor.A, actualColor.A);
			AssertEquals(expectedColor.R, actualColor.R);
			AssertEquals(expectedColor.G, actualColor.G);
			AssertEquals(expectedColor.B, actualColor.B);
		}

		#endregion

		#region Branch/Department

		public void TestIBranchDepartmentProviderMembers()
		{
			var otherBranch = Factory.New<GlbBranch>();
			var otherDepartment = Factory.New<GlbDepartment>();

			var shape = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(shape);
			var network = networkViewModel.GetJobNetwork();
			var entity = network.Entities.GetInstance(shape);

			AssertNull("Branch should be null when the shape's schedule is null. SAD!", BMSTestHelper.GetBranch(entity, Factory));
			AssertNull("Department should be null when the shape's schedule is null. SAD!", BMSTestHelper.GetDepartment(entity, Factory));

			var schedule = Factory.New<BMNCNSchedule>();
			schedule.BNC_BNS_Shape = shape.PK;
			schedule.BNC_GB_Branch = ZGuid.Empty;
			schedule.BNC_GE_Department = ZGuid.Empty;

			AssertNull("Branch should be null when the shape's schedule has no branch. SAD!", BMSTestHelper.GetBranch(entity, Factory));
			AssertNull("Department should be null when the shape's schedule has no department. SAD!", BMSTestHelper.GetDepartment(entity, Factory));

			schedule.BNC_GB_Branch = otherBranch.PK;
			schedule.BNC_GE_Department = otherDepartment.PK;

			AssertEquals("The schedule's branch should be used as the shape's branch. SAD!", otherBranch, BMSTestHelper.GetBranch(entity, Factory));
			AssertEquals("The schedule's department should be used as the shape's department. SAD!", otherDepartment, BMSTestHelper.GetDepartment(entity, Factory));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(Factory)).DiagramEntity;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		static void AssertScheduleInfo(IScheduledNetworkEntity entity, int explicitDurationMinutes, int remainingDurationMinutes, ZDateTime scheduledStartTimeLocal, ZDateTime scheduledEndTimeLocal)
		{
			CombineAssertions("Schedule info for " + entity.Name, () =>
			{
				AssertEquals(nameof(explicitDurationMinutes), explicitDurationMinutes, entity.ExplicitDurationMinutes);
				AssertEquals(nameof(remainingDurationMinutes), remainingDurationMinutes, entity.RemainingDurationMinutes);
				AssertEquals(nameof(scheduledStartTimeLocal), scheduledStartTimeLocal, entity.ScheduledStartTimeLocal);
				AssertEquals(nameof(scheduledEndTimeLocal), scheduledEndTimeLocal, entity.ScheduledEndTimeLocal);
			});
		}

		static ShapeNetworkEntity CreateDiagramEntityAndAssertProperlyScaled(BMNCNRootDiagramShape diagram, JobNetwork network, ZDateTime? scheduledStartTimeLocal, ZDateTime? scheduledFinishTimeLocal)
		{
			var diagramEntity = diagram.AsEntity(network);
			diagramEntity.ScheduledStartTimeLocal = scheduledStartTimeLocal ?? ZDateTime.Now;
			diagramEntity.ScheduledFinishTimeLocal = scheduledFinishTimeLocal ?? ZDateTime.Now.AddHours(10);
			var durationInHours = (diagramEntity.ScheduledFinishTimeLocal.ToTimeSpan().TotalHours - diagramEntity.ScheduledStartTimeLocal.ToTimeSpan().TotalHours);
			var durationInDays = durationInHours / BMConstants.WorkingHoursPerDay;
			diagramEntity.Width = durationInDays * CCPMConstants.ScaledModeDiagramPixelsPerScaleUnit;

			CombineAssertions("PRE: Our diagram entity has the correct settings to validate ScheduleDateValidations", () =>
			{
				AssertEquals("IsDiagram", true, diagramEntity.IsDiagram);
				AssertEquals("Owner is null", true, diagramEntity.Owner == null);
				AssertEquals("IsDiagramSurfaceFixed", true, diagramEntity.IsDiagramSurfaceFixed);
			});

			return diagramEntity;
		}

		static ShapeNetworkEntity CreateShapeEntityAndAssertProperlyScaled(BMNCNShape shape, JobNetwork network, ZDateTime scheduledStartTime, ZDateTime scheduledFinishTime)
		{
			var entity = shape.AsEntity(network);
			entity.ScheduledStartTimeLocal = scheduledStartTime;
			entity.ScheduledFinishTimeLocal = scheduledFinishTime;

			AssertEquals("PRE: Create a shape on the scheduled side of the diagram", false, shape.IsNonScheduled);
			AssertEquals("PRE: Create an entity on the scheduled side of the diagram", true, entity.CanHaveSchedule);

			return entity;
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
