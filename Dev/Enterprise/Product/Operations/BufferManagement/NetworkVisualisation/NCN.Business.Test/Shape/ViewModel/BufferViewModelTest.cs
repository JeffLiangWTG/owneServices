using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BufferViewModelTest : NetworkTestCase
	{
		#region Constructor

		[TestDate(2017, 10, 11)]
		public void TestConstructor_ShouldNotHitDb_ExceptToInitialiseProperties()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = CreateNetworkViewModel(diagram, nodeViewModelProvider: nodeViewModelProvider);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBuffer = newFactory.Load<BMNCNBufferShape>(buffer.PK);

			var startingShapeHitCount = newFactory.GetTableHitCount(BMNCNShapeSchema.Constants.TableName);
			var startingAttachmentHitCount = newFactory.GetTableHitCount(BMNCNAttachmentSchema.Constants.TableName);

			var viewModel = nodeViewModelProvider.Create(loadedBuffer.AsEntity(network), networkViewModel);

			AssertDbHits(new Dictionary<string, int>
			{
				{ BMNCNShapeSchema.Constants.TableName, startingShapeHitCount },
				{ BMNCNAttachmentSchema.Constants.TableName, startingAttachmentHitCount },
			}, newFactory);
		}

		public void TestConstructor_ShouldNotAccess_BNS_ShapeType_AfterBufferIsDeleted()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = CreateNetworkViewModel(diagram, nodeViewModelProvider: nodeViewModelProvider);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			Factory.Save();

			buffer.Delete();

			AssertNoExceptionThrown(() => nodeViewModelProvider.Create(buffer, networkViewModel));
		}

		[TestDate(2021, 09, 01)]
		public void TestConstructor_WhenNoBranchOrDepartment_ShouldNotThrowException()
		{
			BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = CreateNetworkViewModel(diagram, nodeViewModelProvider: nodeViewModelProvider);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var bufferShape = network.Shapes.OfType<BMNCNBufferShape>().Single();

			diagram.ScheduleBizo.BNC_GB_Branch = Guid.Empty;
			diagram.ScheduleBizo.BNC_GE_Department = Guid.Empty;
			shape.ScheduleBizo.BNC_GB_Branch = Guid.Empty;
			shape.ScheduleBizo.BNC_GE_Department = Guid.Empty;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var diagramFromNewFactory = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);
			var bufferShapeFromNewFactory = newFactory.Load<BMNCNBufferShape>(bufferShape.PK);

			networkViewModel = CreateNetworkViewModel(diagramFromNewFactory, nodeViewModelProvider: nodeViewModelProvider);

			BufferViewModel bufferViewModel = null;

			AssertNoExceptionThrown(() => bufferViewModel = new BufferViewModel(bufferShapeFromNewFactory.AsEntity(network), networkViewModel));
			AssertNotNull(bufferViewModel);

			var reloadDiagramShape = bufferViewModel.NetworkViewModel.Network.DiagramEntity.AsShape();

			AssertHasError(reloadDiagramShape.ScheduleBizo.BNC_GB_BranchInfo, "Please enter a Branch.");
			AssertHasError(reloadDiagramShape.ScheduleBizo.BNC_GE_DepartmentInfo, "Please enter a Department.");
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		#endregion

		public void TestContentVisibility()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork
			{
				DiagramEntity = diagramEntity
			};

			var buffer = new Entity
			{
				ShapeType = ShapeTypeList.Codes.Buffer
			};
			network.Entities.Add(buffer);

			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = new NetworkViewModel(network, nodeViewModelProvider: nodeViewModelProvider);
			var viewModel = nodeViewModelProvider.Create(buffer, networkViewModel);
			AssertEquals(false, viewModel.IsCompletionCriteriaVisible);
		}

		public void TestStatusColor()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork
			{
				DiagramEntity = diagramEntity
			};

			var buffer = new Entity
			{
				ShapeType = ShapeTypeList.Codes.Buffer
			};
			network.Entities.Add(buffer);

			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = new NetworkViewModel(network, nodeViewModelProvider: nodeViewModelProvider);
			var viewModel = nodeViewModelProvider.Create(buffer, networkViewModel);
			var zoneColorsAndOffsets = new[]
				{
					Tuple.Create(Color.DodgerBlue, 0.3), Tuple.Create(Color.Yellow, 0.3333), Tuple.Create(Color.Yellow, 0.6667), Tuple.Create(Color.Red, 0.7)
				};
			AssertColors(viewModel.StatusColors.colors, zoneColorsAndOffsets);
		}

		[TestDate(2014, 7, 23)]
		public void TestStatusColor_WithBufferPenetration()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-3);

			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = CreateNetworkViewModel(diagram, nodeViewModelProvider: nodeViewModelProvider);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			networkViewModel.GetJobController().TriggerSaveAction();

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals("Should be some buffer penetration", 0.66m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage((IBuffer)projectBuffer, WorkingTimeContext.Create(diagram), Factory).Penetration, 2));

			var viewModel = (BufferViewModel)nodeViewModelProvider.Create(projectBuffer.AsEntity(network), networkViewModel);
			AssertEquals("66 %", viewModel.PenetrationPercentLabel);
			AssertEquals(0.66, viewModel.LowerBar.percent, 0.01);
			var zoneColorsAndOffsets = new[]
				{
					Tuple.Create(Color.DodgerBlue, 0.3), Tuple.Create(Color.Yellow, 0.3333), Tuple.Create(Color.Yellow, 0.6667), Tuple.Create(Color.Red, 0.7)
				};
			AssertColors(viewModel.StatusColors.colors, zoneColorsAndOffsets);
		}

		public void TestStatusBrush_WhenComplete()
		{
			var network = new DummyNetwork
			{
				DiagramEntity = new Entity()
			};
			var buffer = new Entity
			{
				ShapeType = ShapeTypeList.Codes.Buffer,
				Status = WorkStatus.Complete
			};
			network.Entities.Add(buffer);

			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = new NetworkViewModel(network, nodeViewModelProvider: nodeViewModelProvider);
			var viewModel = nodeViewModelProvider.Create(buffer, networkViewModel);

			var fadedBlue = Color.DodgerBlue.FadeTowardsWhite(1.5f);
			var fadedYellow = Color.Yellow.FadeTowardsWhite(1.5f);
			var fadedRed = Color.Red.FadeTowardsWhite(1.5f);
			var zoneColorsAndOffsets = new[]
				{
					Tuple.Create(fadedBlue, 0.3), Tuple.Create(fadedYellow, 0.3333), Tuple.Create(fadedYellow, 0.6667), Tuple.Create(fadedRed, 0.7)
				};
			AssertColors(viewModel.StatusColors.colors, zoneColorsAndOffsets);
		}

		public void TestSupportedThings()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var nodeViewModelProvider = new JobNetworkNodeViewModelProvider();
			var networkViewModel = CreateNetworkViewModel(diagram, nodeViewModelProvider: nodeViewModelProvider);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			var buffer = networkViewModel.SuggestAndAcceptAllBuffers().Single();

			var bufferViewModel = nodeViewModelProvider.Create(network.Entities.GetInstance(buffer), networkViewModel);
			var entityViewModel = nodeViewModelProvider.Create(network.Entities.GetInstance(shape), networkViewModel);

			AssertEquals(false, bufferViewModel.SupportsAffinities);
			AssertEquals(true, bufferViewModel.SupportsEditEntity);

			AssertEquals(true, entityViewModel.SupportsAffinities);
			AssertEquals(true, entityViewModel.SupportsEditEntity);
		}

		public void TestShowScheduleDetails()
		{
			var entity = new Entity
			{
				Width = 200,
				Height = 100
			};
			var network = new DummyNetwork
			{
				DiagramEntity = entity
			};
			var networkViewModel = new NetworkViewModel(network);

			var buffer = new Entity
			{
				Width = 200,
				Height = 100
			};
			var bufferViewModel = new BufferViewModel(buffer, networkViewModel);

			AssertEquals(false, bufferViewModel.ShowScheduleDetails);
		}

		public void TestProjectBuffer_ShouldUpdateDisplayedPercentage_WhenBufferedWorkChanges()
		{
			var diagram = CreateDiagram(Factory);
			diagram.SwitchToScaled();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var buffer = Factory.NewWithValidTestData<BMNCNBufferShape>();
			buffer.Name = "ProjectBuffer";
			AssertEquals("PRE: We made a project buffer, and not some other random shape", ShapeTypeList.Codes.Buffer, buffer.BNS_ShapeType);

			var schedule = buffer.GetOrCreateSchedule();
			var bufferEntity = buffer.AsEntity(networkViewModel.GetJobNetwork());

			AssertNewPercentageValues(bufferEntity, networkViewModel, schedule, 100);
			AssertNewPercentageValues(bufferEntity, networkViewModel, schedule, 400);
			AssertNewPercentageValues(bufferEntity, networkViewModel, schedule, 30);
			AssertNewPercentageValues(bufferEntity, networkViewModel, schedule, -40);
			AssertNewPercentageValues(bufferEntity, networkViewModel, schedule, -500);
		}

		void AssertNewPercentageValues(ShapeNetworkEntity bufferEntity, NetworkViewModel networkViewModel, BMNCNSchedule schedule, int newPercentage)
		{
			schedule.BNC_BufferPenetrationPercent = newPercentage;
			var bufferVM = new BufferViewModel(bufferEntity, networkViewModel);
			AssertEquals("Our viewmodel percentage changes to matche the schedule", $"{newPercentage} %", bufferVM.PenetrationPercentLabel);
		}
	}
}
