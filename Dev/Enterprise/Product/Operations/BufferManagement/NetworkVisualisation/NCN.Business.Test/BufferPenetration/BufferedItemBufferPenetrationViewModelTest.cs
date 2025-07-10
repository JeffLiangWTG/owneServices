using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
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
	class BufferedItemBufferPenetrationViewModelTest : NetworkTestCase
	{
		[TestDate(2014, 9, 30)]
		public void TestOverflowPenetration()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;

			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 6); // 6 day task
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 6); // 6 day task
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay); // 1 day task

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var shape3 = CreateShape(workflow3, diagram, "shape3");

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape3.MakeVisiblePrerequisiteOf(shape2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffers = network.Shapes.OfType<BMNCNBufferShape>().ToArray();
			AssertEquals(2, buffers.Length);

			var viewModel = new BufferPenetrationViewModel(shape3);
			AssertEquals("shape3 affects feeding buffer and project buffer", 2, viewModel.PenetrationItemsCollection.Count);

			var feedingBufferViewModel = viewModel.PenetrationItemsCollection.Cast<BufferedItemBufferPenetrationViewModel>().Single(x => x.BufferName == "shape3 Feeding Buffer");
			var projectBufferViewModel = viewModel.PenetrationItemsCollection.Cast<BufferedItemBufferPenetrationViewModel>().Single(x => x.BufferName == "Project Buffer");

			AssertEquals("shape3 directly penetrates feeding buffer", false, feedingBufferViewModel.IsOverflow);
			AssertEquals("The project is on time, so there should be no overflow penetration from feeding path", false, projectBufferViewModel.IsOverflow);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(20);
			Factory.Save(); // To flush buffer penetration cache

			viewModel = new BufferPenetrationViewModel(shape3);
			AssertEquals("shape3 affects feeding buffer and project buffer", 2, viewModel.PenetrationItemsCollection.Count);

			feedingBufferViewModel = viewModel.PenetrationItemsCollection.Cast<BufferedItemBufferPenetrationViewModel>().Single(x => x.BufferName == "shape3 Feeding Buffer");
			projectBufferViewModel = viewModel.PenetrationItemsCollection.Cast<BufferedItemBufferPenetrationViewModel>().Single(x => x.BufferName == "Project Buffer");

			AssertEquals("shape3 directly penetrates feeding buffer", false, feedingBufferViewModel.IsOverflow);
			AssertEquals("The project is running late now, so there should be overflow penetration from feeding path", true, projectBufferViewModel.IsOverflow);

			viewModel = new BufferPenetrationViewModel(shape2);
			AssertEquals("shape2 should affect only the project buffer", 1, viewModel.PenetrationItemsCollection.Count);

			projectBufferViewModel = viewModel.PenetrationItemsCollection.Cast<BufferedItemBufferPenetrationViewModel>().Single(x => x.BufferName == "Project Buffer");
			AssertEquals(false, projectBufferViewModel.IsOverflow);
		}

		public void TestBufferedItemRelatedEntityName_ForBufferedItemWithRelatedEntity()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			job.OH_FullName = "Mai Org Syd";

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var diagram = CreateDiagram(jobHeader, name: "Dat Diagram");
			var buffer = Factory.New<BMNCNBufferShape>();
			buffer.ExplicitDurationMinutes = 10;

			var viewModel = new BufferedItemBufferPenetrationViewModel(buffer, diagram);
			AssertEquals("Mai Org Syd", viewModel.BufferedItemRelatedEntityName);
		}

		public void TestBufferedItemRelatedEntityName_ForBufferedItem()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			var viewModel = new BufferedItemBufferPenetrationViewModel(config.Buffer, jobHeader);
			AssertEquals(ZString.Empty, viewModel.BufferedItemRelatedEntityName);
		}

		[TestDate(2014, 9, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestTimeSinceStartable_ShouldUseLocalTimeConversion()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-20);
			var workflow = CreateWorkflow(jobHeader, "workflow");
			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3, estVariationFactor: 1);

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram, "workflowShape");
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			var buffer = network.Shapes.OfType<IBuffer>().Single();
			var bufferedItem = (IBufferedItem)shape;
			var penetrationBizo = new BufferedItemBufferPenetrationViewModel(buffer, bufferedItem);

			var expectedPenetrationPercent = (decimal)(penetrationBizo.TimeSinceStartable.GetMinutesFromDateTimeSpan() - bufferedItem.PlannedDurationInMinutes + bufferedItem.RemainingEstimateInMinutes) / buffer.SizeInMinutes;
			AssertEquals(expectedPenetrationPercent, BufferPenetrationCalculator.CalculatePenetrationPercentage(bufferedItem, WorkingTimeContext.Create(diagram), Factory).Penetration);
		}

		[TestDate(2014, 8, 22)]
		public void TestProperties()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-1);
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3, estVariationFactor: 1);

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram, "workflowShape");
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
			var penetrationBizo = new BufferedItemBufferPenetrationViewModel(buffer, shape);

			AssertEquals("Project Buffer", penetrationBizo.BufferName);
			AssertEquals("workflowShape", penetrationBizo.BufferedItemName);
			AssertEquals("50 %", penetrationBizo.PenetrationPercent);
			AssertEquals(task.P9_EstDuration, penetrationBizo.PlannedDuration);
			AssertEquals(task.P9_EstDuration, penetrationBizo.RemainingEstimatedDuration);
			AssertEquals(jobHeader.FH_DoNotStartBeforeDate, penetrationBizo.ScheduledStartTime);
			AssertEquals((ZDateTime)TimeSpan.FromHours(8), penetrationBizo.TimeSinceStartable);
			AssertEquals(new ZInt(60 * BMConstants.WorkingHoursPerDay * 2).GetDateTimeFromMinutes(), penetrationBizo.BufferDuration);
			AssertEquals(nameof(WorkStatus.Startable), penetrationBizo.Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1); // To force update of workflow status
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(0.0, penetrationBizo.RemainingEstimatedDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(nameof(WorkStatus.Complete), penetrationBizo.Status);
		}

		protected override void SetUp()
		{
			base.SetUp();
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
		}
	}

	[TestedType(typeof(BufferedItemBufferPenetrationViewModel))]
	class BufferedItemBufferPenetrationNonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var shape = Factory.NewWithValidTestData<BMNCNShape>();
			var buffer = Factory.NewWithValidTestData<BMNCNBufferShape>();
			buffer.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;

			return new BufferedItemBufferPenetrationViewModel(buffer, shape);
		}
	}
}
