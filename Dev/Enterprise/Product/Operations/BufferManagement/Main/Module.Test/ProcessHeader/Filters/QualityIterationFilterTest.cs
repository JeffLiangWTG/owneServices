using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(QualityIterationFilter))]
	class QualityIterationFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilter_WhenQualityIteration_ShouldShowQualityIterationWorkflowsOnly()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<QualityIterationFilter>(ProcessHeader.ModuleFilterConstants.QualityIteration);
			filter.Property = QualityIterationFilterTypeList.Codes.QualityIteration;

			CombineAssertions("WHEN we select Quality Iteration in the filter strip, THEN it should show all of the quality iteration workflows only.", () =>
			{
				var results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("All of the quality iteration workflows.", new[] { QualityIteration1.Item1, QualityIteration2.Item1, QualityIteration3.Item1, QualityIteration4.Item1 }, results);

				var anotherQualityIteration = CreateQualityIterationWorkflowAndTask(QualityIteration4.Item2);
				Factory.Save();
				results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("Added a new quality iteration, it should be now updated.", new[] { QualityIteration1.Item1, QualityIteration2.Item1, QualityIteration3.Item1, QualityIteration4.Item1, anotherQualityIteration.Item1 }, results);

				var anotherNormalWorkflow = BMSTestHelper.CreateWorkflow(JobHeader1, "another normal workflow");
				Factory.Save();
				results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("Added a non-quality iteration workflow, it should remain the same.", new[] { QualityIteration1.Item1, QualityIteration2.Item1, QualityIteration3.Item1, QualityIteration4.Item1, anotherQualityIteration.Item1 }, results);
			});
		}

		public void TestFilter_WhenNonQualityIteration_ShouldShowNonQualityIterationWorkflowsOnly()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<QualityIterationFilter>(ProcessHeader.ModuleFilterConstants.QualityIteration);
			filter.Property = QualityIterationFilterTypeList.Codes.NonQualityIteration;

			CombineAssertions("WHEN we select Non-Quality Iteration in the filter strip, THEN it should show all of the non-quality iteration workflows only.", () =>
			{
				var results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("All of the non-quality iteration workflows.", new[] { JobHeader1, JobHeader2, Workflow1, Workflow2 }, results);

				var anotherQualityIteration = CreateQualityIterationWorkflowAndTask(QualityIteration4.Item2);
				Factory.Save();
				results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("Added a new quality iteration, it should remain the same.", new[] { JobHeader1, JobHeader2, Workflow1, Workflow2 }, results);

				var anotherNormalWorkflow = BMSTestHelper.CreateWorkflow(JobHeader1, "another normal workflow");
				Factory.Save();
				results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("Added a non-quality iteration workflow, it should now be updated.", new[] { JobHeader1, JobHeader2, Workflow1, Workflow2, anotherNormalWorkflow }, results);
			});
		}

		public void TestFilter_WhenAll_ShouldShowAllWorkflows()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<QualityIterationFilter>(ProcessHeader.ModuleFilterConstants.QualityIteration);
			filter.Property = QualityIterationFilterTypeList.Codes.AllWorkflows;

			CombineAssertions("WHEN we select All in the filter strip, THEN it should not apply the filter.", () =>
			{
				var results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("All of the workflows.", new[] { JobHeader1, JobHeader2, Workflow1, Workflow2, QualityIteration1.Item1, QualityIteration2.Item1, QualityIteration3.Item1, QualityIteration4.Item1 }, results);

				var anotherQualityIteration = CreateQualityIterationWorkflowAndTask(QualityIteration4.Item2);
				Factory.Save();
				results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("Added a new quality iteration, it should now be updated.", new[] { JobHeader1, JobHeader2, Workflow1, Workflow2, QualityIteration1.Item1, QualityIteration2.Item1, QualityIteration3.Item1, QualityIteration4.Item1, anotherQualityIteration.Item1 }, results);

				var anotherNormalWorkflow = BMSTestHelper.CreateWorkflow(JobHeader1, "another normal workflow");
				Factory.Save();
				results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder("Added a non-quality iteration workflow, it should now be updated.", new[] { JobHeader1, JobHeader2, Workflow1, Workflow2, anotherNormalWorkflow, QualityIteration1.Item1, QualityIteration2.Item1, QualityIteration3.Item1, QualityIteration4.Item1, anotherQualityIteration.Item1 }, results);
			});
		}

		#region Implementation

		ProcessJobHeader JobHeader1 { get; set; }
		ProcessJobHeader JobHeader2 { get; set; }
		ProcessHeader Workflow1 { get; set; }
		ProcessHeader Workflow2 { get; set; }
		ProcessTask Task1 { get; set; }
		ProcessTask Task2 { get; set; }
		Tuple<ProcessHeader, ProcessTask> QualityIteration1 { get; set; }
		Tuple<ProcessHeader, ProcessTask> QualityIteration2 { get; set; }
		Tuple<ProcessHeader, ProcessTask> QualityIteration3 { get; set; }
		Tuple<ProcessHeader, ProcessTask> QualityIteration4 { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			JobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader1");
			JobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader2");

			Workflow1 = BMSTestHelper.CreateWorkflow(JobHeader1, "workflow1");
			Workflow2 = BMSTestHelper.CreateWorkflow(JobHeader2, "workflow2");

			Task1 = BMSTestHelper.CreateTask(Workflow1, GlbStaff.CurrentUser.GS_Code, taskType: "QCB");
			Task2 = BMSTestHelper.CreateTask(Workflow2, GlbStaff.CurrentUser.GS_Code, taskType: "QCB");

			QualityIteration1 = CreateQualityIterationWorkflowAndTask(Task1);
			QualityIteration2 = CreateQualityIterationWorkflowAndTask(QualityIteration1.Item2);
			QualityIteration3 = CreateQualityIterationWorkflowAndTask(Task2);
			QualityIteration4 = CreateQualityIterationWorkflowAndTask(QualityIteration3.Item2);

			Factory.Save();
		}

		Tuple<ProcessHeader, ProcessTask> CreateQualityIterationWorkflowAndTask(ProcessTask taskToIterate)
		{
			var qualityIteration = BMSTestCaseWithFactory.CreateQualityIteration(taskToIterate, taskToIterate);
			return Tuple.Create(qualityIteration, qualityIteration.Tasks.Single());
		}

		#endregion
	}
}
