using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	public abstract class NetworkTestCase : BMSTestCaseWithFactory
	{
		#region Helper Methods

		#region Creating Infrastructure

		public static JobNetworkRefresher CreateRefresher() => new JobNetworkRefresher();

		public static IBMNetworkEntityController CreateController(BusinessObjectFactory factory, JobNetworkRefresher refresher)
		{
			return CreateController(factory, MakeSaveAction(factory, refresher), progressReporterProvider: null);
		}

		public static IBMNetworkEntityController CreateController(BusinessObjectFactory factory, JobNetworkRefresher refresher, IProgressReporterProvider progressReporterProvider)
		{
			return CreateController(factory, MakeSaveAction(factory, refresher), progressReporterProvider);
		}

		public static IBMNetworkEntityController CreateController(BusinessObjectFactory factory, JobNetworkRefresher refresher, IBMNetworkUserInteractionImplementor interactionImplementor)
		{
			return CreateController(factory, MakeSaveAction(factory, refresher), interactionImplementor);
		}

		public static IBMNetworkEntityController CreateController(BusinessObjectFactory factory, NetworkControllerSaveAction saveAction)
		{
			return ObjectFactory.Get<IBMNetworkEntityController>(nameof(IBMNetworkEntityController), CreateInteractionImplementor(null), saveAction);
		}

		public static IBMNetworkEntityController CreateController(BusinessObjectFactory factory, NetworkControllerSaveAction saveAction, IProgressReporterProvider progressReporterProvider)
		{
			return ObjectFactory.Get<IBMNetworkEntityController>(nameof(IBMNetworkEntityController), CreateInteractionImplementor(progressReporterProvider), saveAction);
		}

		public static IBMNetworkEntityController CreateController(BusinessObjectFactory factory, NetworkControllerSaveAction saveAction, IBMNetworkUserInteractionImplementor interactionImplementor)
		{
			return ObjectFactory.Get<IBMNetworkEntityController>(nameof(IBMNetworkEntityController), interactionImplementor, saveAction);
		}

		public static Mock<IBMNetworkEntityController> CreateDummyController()
		{
			return CreateMockableController(new MockRepository(MockBehavior.Default), CreateInteractionImplementor());
		}

		public static Mock<IBMNetworkEntityController> CreateMockableController(MockRepository mocks)
		{
			return CreateMockableController(mocks, CreateInteractionImplementor());
		}

		public static Mock<IBMNetworkEntityController> CreateMockableControllerWithMockableInteractionImplementor(MockRepository mocks)
		{
			var userInteractionImplementor = mocks.Create<IBMNetworkUserInteractionImplementor>();
			userInteractionImplementor.Setup(m => m.ProgressReporterProvider).Returns(new DummyProgressReporterProvider());
			return CreateMockableController(mocks, userInteractionImplementor.Object);
		}

		static IBMNetworkUserInteractionImplementor CreateInteractionImplementor(IProgressReporterProvider progressReporterProvider = null)
		{
			progressReporterProvider = progressReporterProvider ?? new DummyProgressReporterProvider();
			return ObjectFactory.Get<IBMNetworkUserInteractionImplementor>(nameof(IBMNetworkUserInteractionImplementor), progressReporterProvider);
		}

		static Mock<IBMNetworkEntityController> CreateMockableController(MockRepository mocks, IBMNetworkUserInteractionImplementor userInteractionImplementor)
		{
			var controller = mocks.Create<IBMNetworkEntityController>();
			controller.Setup(m => m.UserInteractionImplementor).Returns(userInteractionImplementor);
			return controller;
		}

		public static JobNetwork CreateNetwork(BMNCNShape diagram, BMNetworkViewModel viewModel = null, IBMNetworkEntityController controller = null, bool refreshSchedules = false, JobNetworkRefresher refresher = null, IJobNetworkValidator validator = null)
		{
			refresher = refresher ?? CreateRefresher();
			var network = JobNetwork.Create_ForTest(viewModel ?? new BMNetworkViewModel(diagram), controller ?? CreateController(diagram.Factory, refresher), refresher, validator);
			refresher.AddedToNetwork(network);
			var notUsed = network.Entities; // To ensure entities are loaded (doing so when adding a new shape later can cause duplicate shapes in tests)

			if (refreshSchedules)
			{
				network.RefreshSchedules();
			}

			return network;
		}

		public static NetworkViewModel CreateNetworkViewModel(BMNCNShape diagram, BMNetworkViewModel viewModel = null, IBMNetworkEntityController controller = null, bool refreshSchedules = false, NodeViewModelProvider nodeViewModelProvider = null, IProgressReporterProvider progressReporterProvider = null, bool createNodeViewModels = true, IJobNetworkValidator validator = null)
		{
			var refresher = CreateRefresher();
			controller = controller ?? CreateController(diagram.Factory, refresher, progressReporterProvider);
			var network = CreateNetwork(diagram, viewModel, controller, refreshSchedules, refresher, validator);
			var networkViewModel = new NetworkViewModel(network, nodeViewModelProvider);

			if (createNodeViewModels)
			{
				networkViewModel.CreateNodeViewModelsForEntitiesToLetNetworkActionsWork();
			}

			return networkViewModel;
		}

		static NetworkControllerSaveAction MakeSaveAction(BusinessObjectFactory factory, JobNetworkRefresher refresher)
		{
			return () =>
			{
				refresher.Refresh(new RefreshArgs(RefreshType.Saving));
				factory.Save();
				return ContinueWithSave.Yes;
			};
		}

		#endregion

		public static void LinkToRelatedDiagram(BMNCNShape shapeReferencingOtherDiagram, BMNCNRootDiagramShape otherDiagram)
		{
			shapeReferencingOtherDiagram.BNS_RelatedEntityID = otherDiagram.PK;
		}

		public static void SetDimensionsForShape(BMNCNShape shape, double? left = null, double? top = null, double width = 100, double height = 100)
		{
			if (left != null)
			{
				shape.Left = (double)left;
			}

			if (top != null)
			{
				shape.Top = (double)top;
			}

			shape.Width = (double)width;
			shape.Height = (double)height;
		}

		public static void SetDimensionsForEntity(ShapeNetworkEntity shape, double? x = null, double? y = null, double? width = null, double? height = null)
		{
			if (x != null)
			{
				shape.X = (double)x;
			}

			if (y != null)
			{
				shape.Y = (double)y;
			}

			if (width != null)
			{
				shape.Width = (double)width;
			}

			if (height != null)
			{
				shape.Height = (double)height;
			}
		}

		#region Creating Diagrams

		public static BMNCNShape CreateJobAndDiagram(BusinessObjectFactory factory, string name = null)
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);

			return CreateDiagram(jobHeader, name);
		}

		public static BMNCNRootDiagramShape CreateDiagram(BusinessObjectFactory factory, string name = null, bool? isScaled = null, ZDateTime? scheduledStartTimeUTC = null)
		{
			return CreateShapeCore<BMNCNRootDiagramShape>(factory, ShapeTypeList.Codes.Diagram, null, name, isScaled: isScaled, scheduledStartTimeUTC: scheduledStartTimeUTC);
		}

		public static BMNCNRootDiagramShape CreateDiagram(ProcessHeader workflow, string name = null, bool? isScaled = null, ZDateTime? scheduledStartTimeUTC = null)
		{
			return CreateShapeCore<BMNCNRootDiagramShape>(workflow.Factory, ShapeTypeList.Codes.Diagram, null, name, isScaled, scheduledStartTimeUTC, workflow);
		}

		public static BMNCNShapeDefaultDiagram CreateDefaultDiagram(ProcessJobHeader jobHeader, string name = null)
		{
			var diagram = jobHeader.GetDefaultDiagram();

			if (name != null)
			{
				diagram.Name = name;
			}

			return diagram;
		}

		#endregion

		#region Creating Shapes

		public static ShapeNetworkEntity CreateShapeAtLocation(ProcessHeader workflow, ShapeNetworkEntity parentEntity, double x, double y, double width, double height, string name = null, string shapeType = ShapeTypeList.Codes.Shape)
		{
			var shape = CreateShapeCore<BMNCNShape>(workflow.Factory, shapeType, parentEntity.Shape, name, workflow: workflow);
			var entity = parentEntity.Network.Entities.GetInstance(shape);

			SetShapeOffset(entity, parentEntity, x, y);
			SetShapeSize(entity, parentEntity, width, height);

			return entity;
		}

		public static ShapeNetworkEntity CreateShape(ShapeNetworkEntity parentEntity, string name = null, string shapeType = ShapeTypeList.Codes.Shape, bool? isScaled = null)
		{
			var shape = CreateShapeCore<BMNCNShape>(parentEntity.Factory, shapeType, parentEntity.Shape, name, isScaled);

			return parentEntity.Network.Entities.GetInstance(shape);
		}

		public static ShapeNetworkEntity CreateShape(ProcessHeader workflow, ShapeNetworkEntity parentEntity, string name = null, string shapeType = ShapeTypeList.Codes.Shape, bool? isScaled = null, int? explicitDurationMinutes = null, ZDateTime? scheduledStartTimeUTC = null)
		{
			var shape = CreateShapeCore<BMNCNShape>(workflow.Factory, shapeType, parentEntity.Shape, name, isScaled, scheduledStartTimeUTC, workflow, explicitDurationMinutes);

			return parentEntity.Network.Entities.GetInstance(shape);
		}

		public static BMNCNShape CreateShape(BMNCNShape parentShape, string name = null, string shapeType = ShapeTypeList.Codes.Shape, bool? isScaled = null, int? explicitDurationMinutes = null, ZDateTime? scheduledStartTimeUTC = null)
		{
			return CreateShapeCore<BMNCNShape>(parentShape.Factory, shapeType, parentShape, name, isScaled, scheduledStartTimeUTC, explicitDurationMinutes: explicitDurationMinutes);
		}

		public static BMNCNShape CreateShape(ProcessHeader workflow, BMNCNShape parentShape, string name = null, string shapeType = ShapeTypeList.Codes.Shape, bool? isScaled = null, int? explicitDurationMinutes = null, ZDateTime? scheduledStartTimeUTC = null)
		{
			return CreateShapeCore<BMNCNShape>(workflow.Factory, shapeType, parentShape, name, isScaled, scheduledStartTimeUTC, workflow, explicitDurationMinutes);
		}

		public static BMNCNShape CreateShapeFromTypeString(string shapeType, BusinessObjectFactory factory, BMNCNShape parentShape = null)
		{
			return CreateShapeCore<BMNCNShape>(factory, shapeType, parentShape);
		}

		public BMNCNShape CreateShape(string shapeType)
		{
			return CreateShapeCore<BMNCNShape>(Factory, shapeType);
		}

		public static BMNCNShape CreateDefaultDiagramWorkflowShape(ProcessHeader workflow)
		{
			var diagram = CreateDefaultDiagram(workflow.JobHeader);

			return workflow.GetDefaultShape(diagram);
		}

		#endregion

		#region Shape Creation Implementation

		static T CreateShapeCore<T>(BusinessObjectFactory factory, string shapeType, BMNCNShape parentShape = null, string name = null, bool? isScaled = null, ZDateTime? scheduledStartTimeUTC = null, ProcessHeader workflow = null, int? explicitDurationMinutes = null)
			where T : BMNCNShape
		{
			var shape = factory.New<T>();
			shape.BNS_ShapeType = shapeType == ShapeTypeList.Codes.Diagram && parentShape != null ? ShapeTypeList.Codes.Shape : shapeType; // Diagrams can't be children of other shapes.

			if (name != null)
			{
				shape.BNS_Name = name;
			}

			if (scheduledStartTimeUTC != null)
			{
				shape.ScheduledStartTimeUtc = scheduledStartTimeUTC.Value;
			}

			if (explicitDurationMinutes != null)
			{
				shape.ExplicitDurationMinutes = explicitDurationMinutes.Value;
			}

			if (workflow != null)
			{
				shape.BNS_RelatedEntityID = workflow.PK;
			}

			SetShapeParent(parentShape, shape);

			if (isScaled.HasValue && isScaled.Value && !shape.IsScaled)
			{
				JobNetwork.SwitchToScaled(shape);
			}

			return shape;
		}

		#endregion

		static void SetShapeParent(BMNCNShape parentShape, BMNCNShape shape)
		{
			if (parentShape != null)
			{
				shape.MakeChildOf(parentShape);
			}
		}

		public static void SetShapeOffset(BMNCNShape shape, BMNCNShape parent, IJobNetwork network, double x, double y, int? zIndex = null)
		{
			var entity = network.Entities.GetInstance(shape);
			var parentEntity = network.Entities.GetInstance(parent);

			SetShapeOffset(entity, parentEntity, x, y, zIndex);
		}

		public static void SetShapeOffset(ShapeNetworkEntity shape, ShapeNetworkEntity owner, double x, double y, int? zIndex = null)
		{
			EnsureShapeIsChildOfParent(shape, owner);

			shape.X = x + owner.X;
			shape.Y = y + owner.Y;

			if (zIndex != null)
			{
				shape.ZIndex = zIndex.Value;
			}
		}

		public static void SetShapeSize(IJobNetwork network, BMNCNShape shape, BMNCNShape owner, double width, double height)
		{
			SetShapeSize(shape.AsEntity(network), owner.AsEntity(network), width, height);
		}

		public static void SetShapeSize(ShapeNetworkEntity shape, ShapeNetworkEntity owner, double width, double height)
		{
			EnsureShapeIsChildOfParent(shape, owner);

			shape.Width = width;
			shape.Height = height;
		}

		static void EnsureShapeIsChildOfParent(ShapeNetworkEntity shapeEntity, ShapeNetworkEntity ownerEntity)
		{
			AttachChildToParent(shapeEntity.Shape, ownerEntity.Shape);
		}

		public static void SetShapeRectangle(ShapeNetworkEntity shape, ShapeNetworkEntity owner, double width, double height, double x, double y, int? zIndex = null)
		{
			SetShapeOffset(shape, owner, x, y, zIndex);

			shape.Width = width;
			shape.Height = height;
		}

		public static void SetShapeRectangle(IJobNetwork network, BMNCNShape shape, BMNCNShape owner, double width, double height, double x, double y, int? zIndex = null)
		{
			SetShapeRectangle(shape.AsEntity(network), owner.AsEntity(network), width, height, x, y, zIndex);
		}

		public static void AttachChildToParent(BMNCNShape childShape, BMNCNShape parentShape)
		{
			if (!childShape.IsChildOf(parentShape))
			{
				childShape.MakeChildOf(parentShape);
			}
		}

		public static BMNCNAttachment CreateDependencyAttachment(INetworkEntity ownerEntity, ProcessHeaderLink link, INetworkEntity fromEntity, INetworkEntity toEntity)
		{
			var owner = ownerEntity.AsShape();

			var attachment = owner.Factory.New<BMNCNAttachment>();
			attachment.BNA_FP_ProcessHeaderLink = link.PK;
			attachment.BNA_BNS_Owner = owner.PK;
			attachment.BNA_BNS_FromShape = fromEntity.EntityPK;
			attachment.BNA_BNS_ToShape = toEntity.EntityPK;
			attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;

			return attachment;
		}

		public static IBMBoardSection CreateNetworkBoardSection(BMNCNShape diagram)
		{
			var system = diagram.Factory.New<IBMSystem>(); // TODO: remove the need for a system on BMBoard
			system.FS_Name = "system";
			var board = diagram.Factory.New<IBMBoard>();
			board.MB_FS_System = system.Identifier;
			var section = diagram.Factory.New<IBMBoardSection>();
			section.MS_MB_Board = board.Identifier;
			section.MS_SectionType = CCPMConstants.NetworkDiagramSectionType;

			((DiagramBoardSectionConfiguration)section.Configuration).DiagramPK = diagram.PK;

			return section;
		}

		public static ShapeAffinity CreateAffinity(BMNCNShape diagram, string name = "Lemon", string colour = "Red")
		{
			var affinity = diagram.ShapeAffinities.AddNew();
			affinity.Name = name;
			affinity.Color = colour;
			return affinity;
		}

		public static void LinkAffinity(IJobNetwork network, BMNCNShape shapeToWhichAffinityWillBeLinked, ShapeAffinity affinity)
		{
			((IDiagramEntity)network.DiagramEntity).CreateAffinityLink(shapeToWhichAffinityWillBeLinked, affinity);
		}

		public static ViewApprovedWorkflowSchedule GetScheduleInNewFactory(ZGuid workflowPK)
		{
			var factory = new BusinessObjectFactory();
			return factory.Load<ViewApprovedWorkflowSchedule>(workflowPK);
		}

		protected void NetworkEntityControllerCreateJobMocker(ProcessJobHeader jobHeaderForCreateJob, ProcessJobHeader jobHeaderForShowNewForm, Action action)
		{
			var controller = new Mock<IBMNetworkEntityController>();
			controller.Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<string>())).Returns(jobHeaderForCreateJob);
			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(jobHeaderForShowNewForm);

			using (ObjectFactory.Substitute(controller))
			{
				action();
			}
		}

		public static NodeViewModel CreateNodeViewModel(BMNCNShape shape, IJobNetwork network, NetworkViewModel networkViewModel)
		{
			return CreateNodeViewModel(network.Entities.GetInstance(shape), networkViewModel);
		}

		public static NodeViewModel CreateNodeViewModel(ShapeNetworkEntity entity, NetworkViewModel networkViewModel)
		{
			return new NodeViewModel(entity, networkViewModel);
		}

		public static BMNCNBufferShape CreateBufferOnArrow(BMNCNAttachment arrow, NetworkViewModel viewModel, string name = null)
		{
			var shape = arrow.FromShape;
			var action = new AddBufferAction(viewModel);
			var network = viewModel.GetJobNetwork();

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(viewModel, shape))
			{
				action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == arrow.DisplayText).AsJobNetworkAction().Execute();
			}

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.PreRequisiteLinks.Any(l => l.From.IsSameEntity(shape)));

			if (name != null)
			{
				buffer.Name = name;
			}

			NetworkVisualisationTestHelper.CreateNodeAndAddToNetworkViewModel(viewModel, buffer.AsEntity(network));

			return buffer;
		}

		public static BMNCNChannel CreateChannel(BMNCNRootDiagramShape diagram, string name = "Discovery Channel", int? sequence = null, string color = null, int? height = null)
		{
			var channel = diagram.Channels.AddNew();

			channel.FillWithValidTestData();

			if (name != null)
			{
				channel.BNL_Name = name;
			}

			if (sequence.HasValue)
			{
				channel.BNL_Sequence = sequence.Value;
			}

			if (color != null)
			{
				channel.Color = color;
			}

			if (height != null)
			{
				channel.BNL_Height = height.Value;
			}

			return channel;
		}

		public static BMNCNLevelingRule CreateLevelingRule(BMNCNRootDiagramShape diagram, string type = LevelingRuleTypeList.Codes.MaximumConcurrentEntities, string name = "PAVE Team is the best team. Believe me!", string colorName = "Chartreuse", int value = 1)
		{
			var rule = diagram.LevelingRules.AddNew();

			rule.FillWithValidTestData();

			if (name != null)
			{
				rule.BNR_Name = name;
			}

			if (colorName != null)
			{
				rule.ColorName = colorName;
			}

			if (type != null)
			{
				rule.BNR_Type = type;
			}

			rule.BNR_RuleValue = value;

			return rule;
		}

		public static BMNCNLevelingRuleChannelLink CreateLevelingRuleChannelLink(BMNCNLevelingRule rule, BMNCNChannel channel)
		{
			var link = rule.ChannelLinks.AddNew();

			link.BNK_BNL_Channel = channel.PK;

			return link;
		}

		#endregion

		#region Assertion

		public static void AssertCoordinates(ShapeNetworkEntity shape, double? x = null, double? y = null, double? width = null, double? height = null)
		{
			AssertCoordinates("Expected shape to have coordinates.", shape, x, y, width, height);
		}

		public static void AssertCoordinates(string message, ShapeNetworkEntity shape, double? x = null, double? y = null, double? width = null, double? height = null)
		{
			var name = shape.Name;
			CombineAssertions(message, () =>
			{
				AssertIfNotNull(name, "X", x, shape.X);
				AssertIfNotNull(name, "Y", y, shape.Y);
				AssertIfNotNull(name, "Width", width, shape.Width);
				AssertIfNotNull(name, "Height", height, shape.Height);
			});
		}

		static void AssertIfNotNull(string objectName, string propertyName, double? expected, double actual)
		{
			if (expected.HasValue)
			{
				var message = string.Format("Comparing [{0}] {1}", objectName, propertyName);
				AssertEquals(message, expected.Value, actual);
			}
		}

		public static void AssertDiagramHasDependencyAttachment(BMNCNShape diagramShape, ProcessHeaderLink link, IBMNCNShape entityFromShape, IBMNCNShape entityToShape)
		{
			AssertCollectionContains(diagramShape.ChildDependencyAttachments, a => a.BNA_FP_ProcessHeaderLink == link.PK && a.BNA_BNS_FromShape == entityFromShape.Identifier && a.BNA_BNS_ToShape == entityToShape.Identifier);
		}

		public static void AssertDiagramHasChildShape(BMNCNShape diagramShape, BMNCNShape childShape)
		{
			AssertCollectionContains(childShape.PK, diagramShape.ChildShapes.Select(x => x.PK));
		}

		public static void AssertDiagramHasChildShapeForProcessHeader(BMNCNShape diagramShape, ProcessHeader processHeader)
		{
			AssertCollectionContains(diagramShape.ChildShapes, s => s.BNS_RelatedEntityID == processHeader.PK);
		}

		public static void AssertShapeOffset(ShapeNetworkEntity shape, ShapeNetworkEntity owner, double x, double y)
		{
			AssertShapeOffset(string.Empty, shape, owner, x, y);
		}

		public static void AssertShapeOffset(string message, ShapeNetworkEntity shape, ShapeNetworkEntity owner, double x, double y)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("X", x, shape.X - owner.X);
				AssertEquals("Y", y, shape.Y - owner.Y);
			});
		}

		public static void AssertShapeSize(ShapeNetworkEntity shape, double width, double height)
		{
			AssertShapeSize(string.Empty, shape, width, height);
		}

		public static void AssertShapeSize(string message, ShapeNetworkEntity shape, double width, double height)
		{
			CombineAssertions(() =>
			{
				AssertEquals(message, width, shape.Width);
				AssertEquals(message, height, shape.Height);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertShapeScheduleDurations(BMNCNShape shape, decimal earliestStart, decimal earliestFinish, decimal latestStart, decimal latestFinish, decimal floatHours, bool isCriticalPath)
		{
			CombineAssertions("Schedule for shape: " + shape.Name, () =>
			{
				AssertEquals("EarliestStartHours", earliestStart, shape.EarliestStartHours);
				AssertEquals("EarliestFinishHours", earliestFinish, shape.EarliestFinishHours);
				AssertEquals("LatestStartHours", latestStart, shape.LatestStartHours);
				AssertEquals("LatestFinishHours", latestFinish, shape.LatestFinishHours);

				AssertEquals("FloatHours", floatHours, shape.FloatHours);
				AssertEquals("IsCriticalPath", isCriticalPath, shape.IsCriticalPath);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertShapeScheduleTimes(BMNCNShape shape, ZDateTime earliestStart, ZDateTime earliestFinish, ZDateTime latestStart, ZDateTime latestFinish, decimal floatHours, bool isCriticalPath, ZDateTime? scheduledStart = null, ZDateTime? scheduledFinish = null)
		{
			CombineAssertions("Schedule for shape: " + shape.Name, () =>
			{
				AssertEquals("EarliestStartTimeUtc", earliestStart, shape.EarliestStartTimeUtc);
				AssertEquals("EarliestFinishTimeUtc", earliestFinish, shape.EarliestFinishTimeUtc);
				AssertEquals("LatestStartTimeUtc", latestStart, shape.LatestStartTimeUtc);
				AssertEquals("LatestFinishTimeUtc", latestFinish, shape.LatestFinishTimeUtc);

				AssertEquals("FloatHours", floatHours, shape.FloatHours);
				AssertEquals("IsCriticalPath", isCriticalPath, shape.IsCriticalPath);

				if (scheduledStart != null)
				{
					AssertEquals("ScheduledStartTimeUtc", scheduledStart.Value, shape.ScheduledStartTimeUtc);
				}

				if (scheduledFinish != null)
				{
					AssertEquals("ScheduledFinishTimeUtc", scheduledFinish.Value, shape.ScheduledFinishTimeUtc);
				}
			});
		}

		public static void AssertDurationInDays(IBufferedItem entity, double days)
		{
			var message = string.Format("Duration in days for shape [{0}]", entity.Name);
			var actualDays = entity.PlannedDurationInMinutes / 60.0 / BMConstants.WorkingHoursPerDay;

			AssertEquals(message, days, actualDays);
		}

		public static void RunBufferPenetrationUpdaterAndAssertTasksInCell(BMBoardSection section, BMBoardSectionViewModel viewModel, ProcessTask[] tasks, params Tuple<int, int, ProcessTask[]>[] tasksInCells)
		{
			using (DisableAsyncBehaviour())
			{
				RunBufferPenetrationUpdater();

				var service = new ApprovedShapeBufferPenetrationService(tasks.Select(t => t.GetProcessHeader()).Distinct().ToArray());

				section.Factory.ServiceContainer.RemoveService<ApprovedShapeBufferPenetrationService>();
				section.Factory.ServiceContainer.AddService(service);

				if (tasks.Length > 0)
				{
					tasks[0].Factory.ServiceContainer.RemoveService<ApprovedShapeBufferPenetrationService>();
					tasks[0].Factory.ServiceContainer.AddService(service);
				}

				AssertTasksInCell(section, viewModel, tasks, tasksInCells);
			}
		}

		public static void RunBufferPenetrationUpdater()
		{
			var task = new BufferPenetrationUpdaterServiceTask();
			task.ServiceLogger = new DummyLogger();

			task.RunTask(CancellationToken.None);
		}

		public static void AssertColors(IEnumerable<ColorOffset> colors, params Tuple<Color, double>[] colorAndOffsets)
		{
			AssertEquals(colorAndOffsets.Length, colors.Count());
			var colorsList = colors.ToList();
			for (var i = 0; i < colorAndOffsets.Length; i++)
			{
				var color = colorsList[i];
				var tuple = colorAndOffsets[i];

				CombineAssertions("Color and offset for gradient stop at index " + i, () =>
				{
					AssertEquals(tuple.Item1.A, color.color.A);
					AssertEquals(tuple.Item1.R, color.color.R);
					AssertEquals(tuple.Item1.G, color.color.G);
					AssertEquals(tuple.Item1.B, color.color.B);
					AssertEquals("Offset", Math.Round(tuple.Item2, 4), Math.Round(color.offset, 4)); // This is a unit test. Go away.
				});
			}
		}

		public static void AssertRelatedBuffers<T>(ShapeNetworkEntity entity, params T[] expectedRelatedBuffers)
			where T : IBuffer
		{
			AssertRelatedBuffers(entity.Shape, expectedRelatedBuffers);
		}

		public static void AssertRelatedBuffers<T>(string message, ShapeNetworkEntity entity, params T[] expectedRelatedBuffers)
			where T : IBuffer
		{
			AssertRelatedBuffers(message, entity.Shape, expectedRelatedBuffers);
		}

		protected void AssertBufferPenetration<T>(string message, ShapeNetworkEntity entity, params Tuple<T, decimal>[] bufferPenetrations)
			where T : IBuffer
		{
			AssertBufferPenetration(message, entity.Shape, bufferPenetrations);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertScheduleInfo(BMNCNShape entity, ZDecimal earliestStart, ZDecimal earliestFinish, ZDecimal latestStart, ZDecimal latestFinish, int durationMinutes, ZDecimal @float, bool isCriticalPath)
		{
			CombineAssertions("Scheduling info for " + entity.Name, () =>
			{
				AssertEquals(nameof(earliestStart), earliestStart, entity.EarliestStartHours);
				AssertEquals(nameof(earliestFinish), earliestFinish, entity.EarliestFinishHours);
				AssertEquals(nameof(latestStart), latestStart, entity.LatestStartHours);
				AssertEquals(nameof(latestFinish), latestFinish, entity.LatestFinishHours);
				AssertEquals(nameof(durationMinutes), durationMinutes, (int)entity.ExplicitDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(nameof(@float), @float, entity.FloatHours);
				AssertEquals(nameof(isCriticalPath), isCriticalPath, entity.IsCriticalPath);
			});
		}

		#endregion

		#region Sample Networks

		public static IJobNetwork CreateVeryComplexNetwork(BusinessObjectFactory factory)
		{
			return CreateVeryComplexNetwork(factory, new ZDateTime(2014, 8, 20));
		}

		public static IJobNetwork CreateVeryComplexNetwork(BusinessObjectFactory factory, ZDateTime startTime)
		{
			// This network was designed with JRP to express most/all permutations on buffer penetration.
			// See WI00060103 eDocs (VeryComplexNetwork.tif) for a screenshot of how it looks.
			// Or, save the diagram in a non-transactioned test case and it's yours to play with :)

			#region Create workflow network

			var config = TestConfigsHelper.CreateSchematicTestConfig(factory);

			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_DoNotStartBeforeDate = startTime;

			// CC workflows
			var hldWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "HLD", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 3);
			var designWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Design", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 2);
			var codingWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Coding", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 9);
			var reviewWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Review", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 2);
			var publishWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Publish", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 6);

			// Sub-diagram

			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory, addDefaultProcessHeaderIfNone: false);

			var subDiagramWorkflow1 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow1", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay);
			var subDiagramWorkflow2 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow2", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay);
			var subDiagramWorkflow3 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow3", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay);
			var subDiagramWorkflow4 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow4", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay);
			var subDiagramWorkflow5 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow5", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay);

			// Satellite workflows

			var unbufferedWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Unbuffered Workflow", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 2);
			var resourceConflictWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Resource Conflict Workflow", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 3);
			var ccBranchWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "CC Branch Workflow", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 4);
			var workflowWotEntersHalfway = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Workflow Wot Enters Halfway", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 3);

			// Non-CC chain workflows

			var nonCCWorkflow1 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Non-CC Branch 1", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 3);
			var nonCCWorkflow2 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Non-CC Branch 2", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 5);
			var nonCCWorkflow3 = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Non-CC Branch 3", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 4);
			var unapprovedWorkflow = VisualBoardsTestHelper.CreateWorkflowAndTask(jobHeader1, "Unapproved Workflow", staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay * 4);

			#endregion

			#region Create shape network

			var diagram = CreateDiagram(jobHeader1, name: "Integrated zone capacity age thing");

			// CC workflows
			var hldShape = CreateShape(hldWorkflow, diagram, "HLD");
			var designShape = CreateShape(designWorkflow, diagram, "Design");
			var codingShape = CreateShape(codingWorkflow, diagram, "Coding");
			var reviewShape = CreateShape(reviewWorkflow, diagram, "Review");
			var publishShape = CreateShape(publishWorkflow, diagram, "Publish");

			hldShape.MakeVisiblePrerequisiteOf(designShape, diagram);
			var designToCodingArrow = designShape.MakeVisiblePrerequisiteOf(codingShape, diagram);
			codingShape.MakeVisiblePrerequisiteOf(reviewShape, diagram);
			reviewShape.MakeVisiblePrerequisiteOf(publishShape, diagram);

			// Sub-diagram

			var subDiagram = CreateShape(jobHeader2, diagram, name: "Sub-diagram");

			var subDiagramShape1 = CreateShape(subDiagramWorkflow1, subDiagram, "Workflow1");
			var subDiagramShape2 = CreateShape(subDiagramWorkflow2, subDiagram, "Workflow2");
			var subDiagramShape3 = CreateShape(subDiagramWorkflow3, subDiagram, "Workflow3");
			var subDiagramShape4 = CreateShape(subDiagramWorkflow4, subDiagram, "Workflow4");
			var subDiagramShape5 = CreateShape(subDiagramWorkflow5, subDiagram, "Workflow5");

			subDiagramShape1.MakeVisiblePrerequisiteOf(subDiagramShape2, diagram);
			subDiagramShape2.MakeVisiblePrerequisiteOf(subDiagramShape3, diagram);
			subDiagramShape3.MakeVisiblePrerequisiteOf(subDiagramShape4, diagram);
			subDiagramShape4.MakeVisiblePrerequisiteOf(subDiagramShape5, diagram);

			// Satellite workflows

			var unbufferedShape = CreateShape(unbufferedWorkflow, diagram, "Unbuffered Workflow");
			var resourceConflictShape = CreateShape(resourceConflictWorkflow, diagram, "Resource Conflict Workflow");
			var ccBranchShape = CreateShape(ccBranchWorkflow, diagram, "CC Branch Workflow");
			var shapeWotEntersHalfway = CreateShape(workflowWotEntersHalfway, diagram, "Workflow Wot Enters Halfway");

			unbufferedShape.MakeVisiblePrerequisiteOf(designShape, diagram);
			unbufferedShape.MakeVisiblePrerequisiteOf(reviewShape, diagram);
			resourceConflictShape.MakeVisiblePrerequisiteOf(publishShape, diagram);
			designShape.MakeVisiblePrerequisiteOf(ccBranchShape, diagram);
			var ccBranchArrow = ccBranchShape.MakeVisiblePrerequisiteOf(subDiagram, diagram);
			var subDiagramArrow = subDiagram.MakeVisiblePrerequisiteOf(reviewShape, diagram);
			shapeWotEntersHalfway.MakeVisiblePrerequisiteOf(subDiagramShape3, diagram);

			// Non-CC chain workflows

			var nonCCShape1 = CreateShape(nonCCWorkflow1, diagram, "Non-CC Branch 1");
			var nonCCShape2 = CreateShape(nonCCWorkflow2, diagram, "Non-CC Branch 2");
			var nonCCShape3 = CreateShape(nonCCWorkflow3, diagram, "Non-CC Branch 3");

			nonCCShape1.MakeVisiblePrerequisiteOf(nonCCShape2, diagram);
			nonCCShape1.MakeVisiblePrerequisiteOf(nonCCShape3, diagram);
			var nonCCShape2Arrow = nonCCShape2.MakeVisiblePrerequisiteOf(publishShape, diagram);
			var nonCCShape3Arrow = nonCCShape3.MakeVisiblePrerequisiteOf(publishShape, diagram);

			#endregion

			#region Create network and buffers

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.ScaleAndRefresh();

			BufferCreator.AddProjectBuffer(network);
			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
			projectBuffer.Active = true;

			var subDiagramFeedingBuffer = network.Entities.GetInstance(subDiagramArrow).CreateBuffer();
			subDiagramFeedingBuffer.Name = "Sub-diagram Feeding Buffer";

			var nonCCBranch2FeedingBuffer = network.Entities.GetInstance(nonCCShape2Arrow).CreateBuffer();
			nonCCBranch2FeedingBuffer.Name = "Non-CC Branch 2 Feeding Buffer";

			var nonCCBranch3FeedingBuffer = network.Entities.GetInstance(nonCCShape3Arrow).CreateBuffer();
			nonCCBranch3FeedingBuffer.Name = "Non-CC Branch 3 Feeding Buffer";

			var iterationBuffer = network.Entities.GetInstance(designToCodingArrow).CreateBuffer();
			iterationBuffer.Name = "Iteration Buffer";

			var midFeedingPathFeedingBuffer = network.Entities.GetInstance(ccBranchArrow).CreateBuffer();
			midFeedingPathFeedingBuffer.Name = "CC Branch Workflow Feeding Buffer";

			new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite).ExecuteForShapes(reviewShape, resourceConflictShape);

			#endregion

			#region Position entities

			// CC shapes
			SetShapeRectangle(network, hldShape, diagram, width: 300, height: 200, x: 0, y: 300);
			SetShapeRectangle(network, designShape, diagram, width: 200, height: 200, x: 300, y: 200);
			SetShapeRectangle(network, iterationBuffer.Shape, diagram, width: 600, height: 70, x: 700, y: 250);
			SetShapeRectangle(network, codingShape, diagram, width: 900, height: 200, x: 1300, y: 200);
			SetShapeRectangle(network, reviewShape, diagram, width: 200, height: 200, x: 2400, y: 400);
			SetShapeRectangle(network, publishShape, diagram, width: 600, height: 200, x: 2900, y: 600);

			SetShapeRectangle(network, projectBuffer, diagram, width: 1200, height: 100, x: 3500, y: 650);

			// Satellite shapes
			SetShapeRectangle(network, unbufferedShape, diagram, width: 200, height: 200, x: 100, y: 0);
			SetShapeRectangle(network, resourceConflictShape, diagram, width: 300, height: 200, x: 2600, y: 0);

			// CC branch shapes
			SetShapeRectangle(network, ccBranchShape, diagram, width: 400, height: 200, x: 500, y: 600);
			SetShapeRectangle(network, midFeedingPathFeedingBuffer.Shape, diagram, width: 300, height: 70, x: 900, y: 700);
			SetShapeRectangle(network, subDiagram, diagram, width: 600, height: 600, x: 1200, y: 650);
			SetShapeRectangle(network, subDiagramFeedingBuffer.Shape, diagram, width: 600, height: 70, x: 1800, y: 600);
			SetShapeRectangle(network, shapeWotEntersHalfway, diagram, width: 300, height: 200, x: 1100, y: 1300);

			// Sub-diagram children
			SetShapeRectangle(network, subDiagramShape1, subDiagram, width: 100, height: 200, x: 0, y: 300, zIndex: 2);
			SetShapeRectangle(network, subDiagramShape2, subDiagram, width: 100, height: 200, x: 100, y: 200, zIndex: 2);
			SetShapeRectangle(network, subDiagramShape3, subDiagram, width: 100, height: 200, x: 200, y: 100, zIndex: 2);
			SetShapeRectangle(network, subDiagramShape4, subDiagram, width: 100, height: 200, x: 300, y: 300, zIndex: 2);
			SetShapeRectangle(network, subDiagramShape5, subDiagram, width: 100, height: 200, x: 400, y: 200, zIndex: 2);

			// Non-CC branch
			SetShapeRectangle(network, nonCCShape1, diagram, width: 300, height: 200, x: 800, y: 1600);
			SetShapeRectangle(network, nonCCShape2, diagram, width: 500, height: 200, x: 1100, y: 1800);
			SetShapeRectangle(network, nonCCShape3, diagram, width: 400, height: 200, x: 2000, y: 1600);
			SetShapeRectangle(network, nonCCBranch2FeedingBuffer.Shape, diagram, width: 1300, height: 70, x: 1600, y: 2100);
			SetShapeRectangle(network, nonCCBranch3FeedingBuffer.Shape, diagram, width: 500, height: 70, x: 2400, y: 1600);

			networkViewModel.TogglePinnedStatus(codingShape);
			networkViewModel.TogglePinnedStatus(reviewShape);

			#endregion

			#region Approve diagram and add non-approved things

			UnitTestUserNotification.Instance.AddOKAnswer();
			networkViewModel.ToggleApproval();

			var unapprovedShape = CreateShape(unapprovedWorkflow, network.DiagramEntity, "Unapproved Workflow");
			unapprovedShape.MakeVisiblePrerequisiteOf(nonCCShape2.AsEntity(network));
			SetShapeRectangle(unapprovedShape, network.DiagramEntity, width: 400, height: 300, x: 700, y: 2000);

			network.Entities.Add(unapprovedShape);

			#endregion

			return network;
		}

		#endregion
	}

	#region Dummy Classes

	public class DummyProgressReporterProvider : IProgressReporterProvider
	{
		IProgressReporter IProgressReporterProvider.CreateProgressReporter(string message, int numberOfItemsToProcess, bool allowCancel)
		{
			return new DummyProgressReporter();
		}
	}

	public class DummyProgressReporter : IProgressReporter
	{
		public bool IsCancelled { get; set; }
		public bool IsDisposed { get; set; }
		public int ItemsProcessed { get; set; }
		public int CancelAfterItemNumber { get; set; }

		public void Report(int value)
		{
			ItemsProcessed = value;

			if (value == CancelAfterItemNumber)
			{
				IsCancelled = true;
			}
		}

		public void Dispose()
		{
			IsDisposed = true;
		}

		public void ShowForm(IComponent parentFormToShowModallyTo, string initialMessage)
		{
		}
	}

	#endregion

	#region PropertyChangedTracker

	public class PropertyChangedTracker
	{
		public PropertyChangedTracker(INotifyPropertyChanged propertyChanged)
		{
			propertyChanged.PropertyChanged += OnPropertyChanged;
		}

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			List.Add(e.PropertyName);
		}

		public List<string> List { get; } = new List<string>();
	}

	#endregion

	#region NetworkTestCaseExtensions

	public static class NetworkTestCaseExtensions
	{
		#region PropertyChanged

		public static PropertyChangedTracker MakePropertyChangedTracker(this INotifyPropertyChanged propertyChanged)
		{
			return new PropertyChangedTracker(propertyChanged);
		}

		#endregion

		public static bool IsPrerequisiteOf(this BMNCNShape shape, BMNCNShape other)
		{
			return shape.IsPrerequisiteOf(other, new BMNCNShapeDescendantsStrategy());
		}

		public static IDiagramEntity AsDiagramEntity(this BMNCNShape diagram)
		{
			return NetworkTestCase.CreateNetwork(diagram).DiagramEntity;
		}

		public static ShapeNetworkEntity AsEntity(this BMNCNShape shape, IJobNetwork network)
		{
			return network.Entities.GetInstance(shape);
		}

		public static ShapeNetworkEntity AsEntity(this BMNCNShape shape, ShapeNetworkEntity networkEntity)
		{
			return shape.AsEntity(networkEntity.Network);
		}

		public static NetworkAttachment AsEntity(this BMNCNAttachment attachment, IJobNetwork network)
		{
			return network.Entities.GetInstance(attachment);
		}

		public static void ScaleAndRefresh(this IJobNetwork network)
		{
			network.SwitchToScaled();
			network.Refresh(RefreshType.RedrawDiagram);
		}

		static BMNCNAttachment GetAttachmentToSibling(this BMNCNShape fromShape, BMNCNShape toShape, BMNCNShape owner)
		{
			return fromShape.AllAttachments.FirstOrDefault(a => (owner == null || a.BNA_BNS_Owner == owner.PK) && a.BNA_BNS_ToShape == toShape.PK);
		}

		public static BMNCNAttachment MakeVisiblePrerequisiteOf(this ShapeNetworkEntity fromShape, ShapeNetworkEntity toShape)
		{
			return MakeVisiblePrerequisiteOf(fromShape.Shape, toShape.Shape, fromShape.Owner?.Shape);
		}

		public static BMNCNAttachment MakeVisiblePrerequisiteOf(this BMNCNShape fromShape, BMNCNShape toShape, BMNCNShape owner)
		{
			var attachment = fromShape.GetAttachmentToSibling(toShape, owner);
			if (attachment == null)
			{
				attachment = fromShape.Factory.New<BMNCNAttachment>();
				attachment.BNA_BNS_ToShape = toShape.PK;
				attachment.BNA_BNS_FromShape = fromShape.PK;
				if (owner != null)
				{
					attachment.BNA_BNS_Owner = owner.PK;
				}
			}

			if (toShape.ProcessHeader != null && fromShape.ProcessHeader != null)
			{
				var toProcessHeader = toShape.ProcessHeader;
				var link = fromShape.ProcessHeader.GetOrCreateDependencyLink(toProcessHeader);

				attachment.BNA_FP_ProcessHeaderLink = link.PK;
			}

			return attachment;
		}

		#region Push Entities

		public static void PushAsEarlyAsPossible(this NetworkViewModel networkViewModel)
		{
			var network = networkViewModel.GetJobNetwork();
			new PushAllEntitiesAction(networkViewModel).GetChildActionsAfterActivatingEntity_ForTest(network.DiagramEntity).First().AsJobNetworkAction().ExecuteAfterActivatingEntity_ForTest(network.DiagramEntity);
		}

		public static void PushAsLateAsPossible(this NetworkViewModel networkViewModel)
		{
			var network = networkViewModel.GetJobNetwork();
			new PushAllEntitiesAction(networkViewModel).GetChildActionsAfterActivatingEntity_ForTest(network.DiagramEntity).Last().AsJobNetworkAction().ExecuteAfterActivatingEntity_ForTest(network.DiagramEntity);
		}

		#endregion

		public static void ToggleApproval(this NetworkViewModel networkViewModel, bool setStartDateToNowWhenApproving = true, bool ensureIsNowApproved = true)
		{
			var network = networkViewModel.GetJobNetwork();
			if (setStartDateToNowWhenApproving && !network.DiagramShape.IsApproved && network.DiagramShape.ScheduledStartTimeUtc.IsEmpty && network.DiagramShape.ScheduledFinishTimeUtc.IsEmpty)
			{
				network.DiagramShape.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			}

			network.Refresh(RefreshType.RefreshButton);
			network.RefreshSchedules();

			if (!network.DiagramShape.IsApproved)
			{
				new ApproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(network.DiagramEntity);

				if (ensureIsNowApproved && !network.DiagramShape.IsApproved)
				{
					BMSTestCaseWithFactory.AssertNoConfirmationNotificationShown("This diagram should be approved, but it couldn't be because of the notifications shown. Consider using UnitTestUserNotification.Instance.AddOKAnswer() if there are warnings which are OK to bypass in this test.");
				}
			}
			else
			{
				new UnapproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(network.DiagramEntity);
			}
		}

		public static void PinShape(this BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			new PinShapeAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(shape);
		}

		public static void UnPinShape(this BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			new UnpinShapeAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(shape);
		}

		public static BMNCNBufferShape[] SuggestAndAcceptAllBuffers(this NetworkViewModel networkViewModel)
		{
			var network = networkViewModel.GetJobNetwork();
			var existingShapes = new HashSet<BMNCNShape>(network.Shapes);

			network.Refresh(RefreshType.RefreshButton);

			new SuggestBufferAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(network.DiagramEntity);

			foreach (var buffer in network.Shapes.Where(s => s.IsBufferShape && !s.Active).ToArray())
			{
				if (networkViewModel.GetNodeForEntity(buffer) == null)
				{
					NetworkVisualisationTestHelper.CreateNodeAndAddToNetworkViewModel(networkViewModel, buffer.AsEntity(network));
				}

				new AcceptBufferAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(buffer);
			}

			return network.Shapes.Where(s => !existingShapes.Contains(s)).Cast<BMNCNBufferShape>().ToArray();
		}

		public static void TogglePinnedStatus(this NetworkViewModel networkViewModel, INetworkEntity entity)
		{
			var network = networkViewModel.GetJobNetwork();
			var shapeEntity = network.Entities.GetInstance(entity);

			if (!shapeEntity.IsPinned)
			{
				shapeEntity.Shape.PinShape(networkViewModel);
			}
			else
			{
				shapeEntity.Shape.UnPinShape(networkViewModel);
			}
		}

		public static void SetCoordinates(this ShapeNetworkEntity shape, double width, double height, double x, double y)
		{
			shape.Width = width;
			shape.Height = height;
			shape.X = x;
			shape.Y = y;
		}

		#region CreateNewEntity

		public static ShapeNetworkEntity CreateNewShape(this NetworkViewModel networkViewModel, ShapeNetworkEntity parentShape)
		{
			return CreateNewShape(networkViewModel, parentShape.Shape);
		}

		public static ShapeNetworkEntity CreateNewWorkflow(this NetworkViewModel networkViewModel, ShapeNetworkEntity parentShape)
		{
			return CreateNewWorkflow(networkViewModel, parentShape.Shape);
		}

		public static ShapeNetworkEntity CreateNewAnnotation(this NetworkViewModel networkViewModel, ShapeNetworkEntity parentShape)
		{
			return CreateNewAnnotation(networkViewModel, parentShape.Shape);
		}

		public static ShapeNetworkEntity CreateNewShape(this NetworkViewModel networkViewModel, IBMNCNShape parentShape)
		{
			var entity = CreateNewEntity<CreateShapeAction>(networkViewModel, parentShape);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			return entity;
		}

		public static ShapeNetworkEntity CreateNewWorkflow(this NetworkViewModel networkViewModel, IBMNCNShape parentShape)
		{
			var entity = networkViewModel.GetJobNetwork().DiagramShape.IsDefaultDiagram
				? CreateNewEntity<CreateDefaultWorkflowAction>(networkViewModel, parentShape)
				: CreateNewEntity<CreateWorkflowAction>(networkViewModel, parentShape);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			return entity;
		}

		public static ShapeNetworkEntity CreateNewJobShape(this NetworkViewModel networkViewModel, IBMNCNShape parentShape)
		{
			var entity = CreateNewEntity<CreateJobAction>(networkViewModel, parentShape);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			return entity;
		}

		public static ShapeNetworkEntity CreateNewAnnotation(this NetworkViewModel networkViewModel, IBMNCNShape parentShape)
		{
			var entity = CreateNewEntity<CreateAnnotationAction>(networkViewModel, parentShape);
			NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			return entity;
		}

		static ShapeNetworkEntity CreateNewEntity<T>(NetworkViewModel networkViewModel, IBMNCNShape parentShape)
			where T : CreateEntityActionBase
		{
			var network = networkViewModel.GetJobNetwork();
			var parent = network.Entities.GetInstance(parentShape);
			var action = networkViewModel.GetCreateEntityActions().Where(t => t != null && t.GetType() == typeof(T)).Cast<T>().Single();

			var isEnabled = action.IsEnabledForEntity(parent);

			if (!isEnabled.IsAllowed)
			{
				var message = "Cannot execute " + action.GetType() + " because it is disabled: " + isEnabled;

				if (action is CreateWorkflowActionBase)
				{
					message += " Ensure Buffer Management is enabled in the registry within the test and a Buffer Management System is created and enabled for your type of jobs.";
				}

				throw new InvalidOperationException(message);
			}

			var entity = (ShapeNetworkEntity)action.ExecuteForEntityWithoutAccessCheck(parent);
			var node = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			network.Refresh(RefreshType.EntityAdded, entity);
			return entity;
		}

		public static void CreateNodeViewModelsForEntitiesToLetNetworkActionsWork(this NetworkViewModel networkViewModel) => networkViewModel.Refresh();

		#endregion

		#region Custom Network Actions

		public static IEnumerable<INetworkAction> GetCoreCustomNetworkActions_ForTesting(this NetworkViewModel networkViewModel)
		{
			return networkViewModel.GetCustomNetworkActions().SingleOrDefault(a => a.GetName() == "Actions")?.GetChildActions().ToArray();
		}

		public static IEnumerable<INetworkAction> GetApplicableCoreCustomNetworkActions_ForTesting(this NetworkViewModel networkViewModel, INetworkEntity entity)
		{
			return networkViewModel.GetCoreCustomNetworkActions_ForTesting().Where(a => a.IsApplicableToEntity(entity).IsAllowed).ToArray();
		}

		public static T GetCoreCustomNetworkAction_ForTesting<T>(this NetworkViewModel networkViewModel) where T : INetworkAction
		{
			return networkViewModel.GetCoreCustomNetworkActions_ForTesting().OfType<T>().SingleOrDefault();
		}

		public static bool IsCoreCustomNetworkActionApplicableToEntity<T>(this NetworkViewModel networkViewModel, INetworkEntity entity) where T : INetworkAction
		{
			return networkViewModel.GetCoreCustomNetworkAction_ForTesting<T>().IsApplicableToEntity(entity).IsAllowed;
		}

		public static bool IsCoreCustomNetworkActionEnabledForEntity<T>(this NetworkViewModel networkViewModel, INetworkEntity entity) where T : INetworkAction
		{
			return networkViewModel.GetCoreCustomNetworkAction_ForTesting<T>().IsEnabledForEntity(entity).IsAllowed;
		}

		#endregion

		#region Create Entity Network Actions

		public static IEnumerable<INetworkAction> GetApplicableCreateEntityActions_ForTesting(this NetworkViewModel networkViewModel, INetworkEntity entity)
		{
			return networkViewModel.GetCreateEntityActions().Where(a => a.IsApplicableToEntity(entity).IsAllowed).ToArray();
		}

		#endregion

		#region Network Action Menu Items

		static NetworkActionMenuItem[] GetFirstLevelNetworkActionsMenuItems_ForTesting(this NetworkViewModel networkViewModel, INetworkEntity entity)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, entity))
			{
				var node = networkViewModel.GetNodeForEntity(entity);
				node.ReloadMenuItems();
				return node.MenuItems.ToArray();
			}
		}

		public static NetworkActionMenuItem GetFirstLevelNetworkActionsMenuItem_ForTesting(this NetworkViewModel networkViewModel, INetworkEntity entity, string menuItemName)
		{
			return networkViewModel.GetFirstLevelNetworkActionsMenuItems_ForTesting(entity).SingleOrDefault(i => i?.Name == menuItemName);
		}

		public static NetworkActionMenuItem[] GetSecondLevelNetworkActionsMenuItems_ForTesting(this NetworkViewModel networkViewModel, INetworkEntity entity, string firstLevelMenuItemName)
		{
			return networkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(entity, firstLevelMenuItemName)?.Items.ToArray();
		}

		public static NetworkActionMenuItem GetSecondLevelNetworkActionsMenuItem_ForTesting(this NetworkViewModel networkViewModel, INetworkEntity entity, string firstLevelMenuItemName, string secondLevelMenuItemName)
		{
			return networkViewModel.GetSecondLevelNetworkActionsMenuItems_ForTesting(entity, firstLevelMenuItemName).SingleOrDefault(i => i?.Name == secondLevelMenuItemName);
		}

		#endregion
	}

	#endregion
}
