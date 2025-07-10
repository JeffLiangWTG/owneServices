using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(JobRelatedDiagramsViewModel))]
	class JobRelatedDiagramsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobRelatedDiagramsViewModel(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory));
		}

		#region Related Diagrams

		public void TestRelatedDiagrams_ShouldIncludeAllNonDefaultDiagramsForJob()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2 = jobHeader1.ProcessHeaders.AddNew();

			var diagram1 = NetworkTestCase.CreateDiagram(jobHeader1, "Diagram 1");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram1, "Shape 1");

			var defaultDiagram = jobHeader1.GetDefaultDiagram();
			defaultDiagram.BNS_Name = "Default Diagram";

			AssertEquals("Precondition", jobHeader1.PK, diagram1.BNS_RelatedEntityID);
			AssertEquals("Precondition", workflow1.PK, shape1.BNS_RelatedEntityID);
			AssertEquals("Precondition", jobHeader1.PK, defaultDiagram.BNS_RelatedEntityID);
			AssertEquals("Precondition", ShapeTypeList.Codes.DefaultDiagram, defaultDiagram.BNS_ShapeType);

			var unrelatedJob = Factory.NewWithValidTestData<OrgHeader>();
			var unrelatedJobHeader = ProcessJobHeader.GetForParent(unrelatedJob, Factory);
			var unrelatedWorkflow = unrelatedJobHeader.ProcessHeaders.AddNew();

			var unrelatedDiagram = NetworkTestCase.CreateDiagram(unrelatedJobHeader, "Unrelated Diagram");
			var unrelatedShape = NetworkTestCase.CreateShape(unrelatedWorkflow, unrelatedDiagram, "Unrelate Shape");

			var notlinkeddDiagram = NetworkTestCase.CreateDiagram(Factory);
			var notLinkedShape = NetworkTestCase.CreateShape(notlinkeddDiagram);

			var viewModel = new JobRelatedDiagramsViewModel(jobHeader1);

			AssertContainsExactElementsInAnyOrder(new[] { diagram1, shape1 }, viewModel.RelatedDiagrams);

			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram1, "Shape 2");

			AssertContainsExactElementsInAnyOrder(new[] { diagram1, shape1, shape2 }, viewModel.RelatedDiagrams);
		}

		public void TestRelatedDiagrams_ShouldIncludeDiagramForAddedWorkflows()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();

			var diagram1 = NetworkTestCase.CreateDiagram(jobHeader1, "Diagram 1");
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram1, "Shape 1");

			var unrelatedJob = Factory.NewWithValidTestData<OrgHeader>();
			var unrelatedJobHeader = ProcessJobHeader.GetForParent(unrelatedJob, Factory);
			var unrelatedWorkflow = unrelatedJobHeader.ProcessHeaders.AddNew();

			var unrelatedDiagram = NetworkTestCase.CreateDiagram(unrelatedJobHeader, "Unrelated Diagram");
			var unrelatedShape = NetworkTestCase.CreateShape(unrelatedWorkflow, unrelatedDiagram, "Unrelate Shape");

			var viewModel = new JobRelatedDiagramsViewModel(jobHeader1);

			AssertContainsExactElementsInAnyOrder(new[] { diagram1, shape1 }, viewModel.RelatedDiagrams);

			var workflow2 = jobHeader1.ProcessHeaders.AddNew();

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, "Diagram 2");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram2, "Shape 2");

			AssertContainsExactElementsInAnyOrder(new[] { diagram1, shape1, shape2 }, viewModel.RelatedDiagrams);
		}

		#endregion
	}
}
