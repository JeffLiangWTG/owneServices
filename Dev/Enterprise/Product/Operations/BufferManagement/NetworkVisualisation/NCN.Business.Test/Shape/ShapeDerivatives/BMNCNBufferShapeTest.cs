using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNBufferShape))]
	class BMNCNBufferShapeTest : BaseShapeTestCase
	{
		[GuiTest, TestDate(2014, 11, 30)]
		public void TestCachedBufferPenetration_ShouldUpdateWhenSavingParentWithChanges()
		{
			VisualBoardsTestCase.EnableBMSInRegistry();

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = VisualBoardsTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, BMConstants.WorkingHoursPerDay * 8 * 60);

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-5);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var shape = NetworkTestCase.CreateShape(workflow, diagram);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			Factory.Save();

			var serviceTask = new BufferPenetrationUpdaterServiceTask();
			serviceTask.ServiceLogger = new BufferManagementLogger();
			serviceTask.RunTask(CancellationToken.None);

			var newFactory = Factory.CreateNewFactory();
			var loadedBuffer = newFactory.Load<BMNCNBufferShape>(buffer.PK);

			AssertEquals(1.7917m, loadedBuffer.BufferPenetration.Round(4));

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			network.EditEntity(network.DiagramEntity); // To cause schedules to update
			networkViewModel.GetJobController().TriggerSaveAction();

			loadedBuffer = newFactory.CreateNewFactory().Load<BMNCNBufferShape>(buffer.PK);
			AssertEquals(3.2917m, loadedBuffer.BufferPenetration.Round(4));
		}

		public void TestStatus_ShouldBeCompleteWhenPrereqsAreAllComplete()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();

			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = VisualBoardsTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);
			var task4 = VisualBoardsTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 60);
			var task5 = VisualBoardsTestHelper.CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 60);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var ccShape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var ccShape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var ccShape3 = NetworkTestCase.CreateShape(workflow3, diagram, "shape3");
			var ccShape4 = NetworkTestCase.CreateShape(workflow4, diagram, "shape4");

			var nonCCShape = NetworkTestCase.CreateShape(workflow5, diagram, "shape5");

			ccShape1.MakeVisiblePrerequisiteOf(ccShape2);
			ccShape2.MakeVisiblePrerequisiteOf(ccShape3);
			ccShape3.MakeVisiblePrerequisiteOf(ccShape4);

			ccShape1.MakeVisiblePrerequisiteOf(nonCCShape);
			nonCCShape.MakeVisiblePrerequisiteOf(ccShape4);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();

			AssertEquals(true, ccShape1.IsCriticalPath);
			AssertEquals(true, ccShape1.IsCriticalPath);
			AssertEquals(true, ccShape1.IsCriticalPath);
			AssertEquals(true, ccShape1.IsCriticalPath);
			AssertEquals(false, nonCCShape.IsCriticalPath);

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(s => s.BufferType == BufferTypeList.Codes.Project);
			var feedingBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(s => s.BufferType == BufferTypeList.Codes.Feeding);

			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkStatus.None, ((IProposedNetworkEntity)projectBuffer).Status);
			AssertEquals("Item on the CC pre-feeding path is still open", WorkStatus.None, ((IProposedNetworkEntity)feedingBuffer).Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkStatus.None, ((IProposedNetworkEntity)projectBuffer).Status);
			AssertEquals(WorkStatus.Complete, ((IProposedNetworkEntity)feedingBuffer).Status);

			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkStatus.None, ((IProposedNetworkEntity)projectBuffer).Status);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkStatus.None, ((IProposedNetworkEntity)projectBuffer).Status);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkStatus.Complete, ((IProposedNetworkEntity)projectBuffer).Status);
		}

		public void TestSetBufferType_AndLoad()
		{
			var buffer = Factory.NewWithValidTestData<BMNCNBufferShape>();
			buffer.BufferType = BufferTypeList.Codes.Project;

			Factory.Save();

			var loadedBuffer = Factory.CreateNewFactory().Load<BMNCNBufferShape>(buffer.PK);
			AssertEquals(BufferType.Project, ((IBuffer)loadedBuffer).Type);
		}

		public void TestBufferShapeDoesntPlaceShapeOnCC()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, name: "Life");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram, name: "Exersize Daily");
			var shape2 = NetworkTestCase.CreateShape(diagram, name: "Eat Healthily");
			var shape3 = NetworkTestCase.CreateShape(diagram, name: "Die Anyway");

			shape1.MakeVisiblePrerequisiteOf(shape3);
			shape2.MakeVisiblePrerequisiteOf(shape3);

			network.SwitchToScaled();
			shape1.Width = 200;
			shape2.Width = 100;
			shape3.Width = 200;

			network.RefreshSchedules();
			var buffer = networkViewModel.SuggestAndAcceptAllBuffers().Single(s => s.BufferType == BufferTypeList.Codes.Feeding).AsEntity(network);

			buffer.Width = 6000;

			network.RefreshSchedules();

			AssertEquals(true, shape1.IsCriticalPath);
			AssertEquals(true, shape3.IsCriticalPath);
			AssertEquals(false, shape2.IsCriticalPath);
			AssertEquals(false, buffer.IsCriticalPath);
		}

		public void TestBufferType_ReadOnly()
		{
			var shape = Factory.New<BMNCNShape>();
			var buffer = Factory.New<BMNCNBufferShape>();

			AssertEquals(true, shape.BufferTypeInfo.ReadOnly);
			AssertEquals(false, buffer.BufferTypeInfo.ReadOnly);
		}

		[TestDate(2014, 7, 26)]
		public void TestBufferPenetration_ShouldBeWorstCaseOfPrereqs()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-5);

			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow5");

			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 7 * BMConstants.WorkingHoursPerDay * 60, estVariationFactor: 1);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60, estVariationFactor: 1);
			var task3 = VisualBoardsTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60, estVariationFactor: 1);
			var task4 = VisualBoardsTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60, estVariationFactor: 1);
			var task5 = VisualBoardsTestHelper.CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60, estVariationFactor: 1);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram, "shape3");
			var shape4 = NetworkTestCase.CreateShape(workflow4, diagram, "shape4");
			var shape5 = NetworkTestCase.CreateShape(workflow5, diagram, "shape5");

			// Critical chain dependencies
			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);

			// Non-CC dependencies
			shape4.MakeVisiblePrerequisiteOf(shape5);
			shape5.MakeVisiblePrerequisiteOf(shape3);

			network.ScaleAndRefresh();
			networkViewModel.PushAsLateAsPossible();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			AssertEquals(true, shape1.IsCriticalPath);
			AssertEquals(true, shape2.IsCriticalPath);
			AssertEquals(true, shape3.IsCriticalPath);
			AssertEquals(false, shape4.IsCriticalPath);
			AssertEquals(false, shape5.IsCriticalPath);

			var buffers = network.Shapes.OfType<BMNCNBufferShape>().ToArray();
			AssertEquals(2, buffers.Length);

			var projectBuffer = buffers.Single(b => b.BufferType == BufferTypeList.Codes.Project);
			var feedingBuffer = buffers.Single(b => b.BufferType == BufferTypeList.Codes.Feeding);

			var context = WorkingTimeContext.Create(diagram.Shape);

			CombineAssertions("Individual shape buffer penetrations of project buffer", () =>
			{
				AssertEquals("shape1", 0.70m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage(shape1.Shape, projectBuffer, context, Factory).Penetration, 2));
				AssertEquals("shape2", 0.0m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage(shape2.Shape, projectBuffer, context, Factory).Penetration, 2));
				AssertEquals("shape3", 0.0m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage(shape3.Shape, projectBuffer, context, Factory).Penetration, 2));
				AssertEquals("shape4", 0.12m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage(shape4.Shape, projectBuffer, context, Factory).Penetration, 2));
				AssertEquals("shape5 - all aging is contained within feeding buffer", 0.0m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage(shape5.Shape, projectBuffer, context, Factory).Penetration, 2));
			});

			AssertEquals("Overall buffer penetration should be worst case of relevant prereqs", 0.70m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage((IBuffer)projectBuffer, context, Factory).Penetration, 2));

			CombineAssertions("Individual shape buffer penetrations of feeding buffer", () =>
			{
				AssertEquals("shape4", 1.29m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage(shape4.Shape, feedingBuffer, context, Factory).Penetration, 2));
				AssertEquals("shape5", 0.29m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage(shape5.Shape, feedingBuffer, context, Factory).Penetration, 2));
			});

			AssertEquals("Overall buffer penetration should be worst case of relevant prereqs", 1.29m, decimal.Round(BufferPenetrationCalculator.CalculatePenetrationPercentage((IBuffer)feedingBuffer, context, Factory).Penetration, 2));
		}

		#region Implementation

		protected override IEnumerable<string> XmlMemberNames
		{
			get { return BMNCNShapeTest.BMNCNShape_XmlMemberNames; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
