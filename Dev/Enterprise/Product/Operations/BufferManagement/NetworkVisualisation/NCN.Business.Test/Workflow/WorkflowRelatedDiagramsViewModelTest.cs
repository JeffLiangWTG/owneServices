using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test.Workflow
{
	[TestedType(typeof(WorkflowRelatedDiagramsViewModel))]
	class WorkflowRelatedDiagramsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowRelatedDiagramsViewModel(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory));
		}

		public void TestRelatedDiagrams_ShouldIncludeAllDiagrams()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();

			var diagram1 = NetworkTestCase.CreateDiagram(jobHeader);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram1);

			var viewModel = new WorkflowRelatedDiagramsViewModel(workflow1);

			AssertEquals(1, viewModel.RelatedDiagrams.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shape1 }, viewModel.RelatedDiagrams);

			var diagram2 = NetworkTestCase.CreateDiagram(Factory);
			var shape2 = NetworkTestCase.CreateShape(workflow1, diagram2);

			AssertEquals(2, viewModel.RelatedDiagrams.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shape1, shape2 }, viewModel.RelatedDiagrams);
		}
	}
}
