
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(RelatedParentChildWorkflowsViewModel))]
	public class RelatedParentChildWorkflowsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RelatedParentChildWorkflowsViewModel(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory), RelationshipDirection.From);
		}

		public void TestParentWorkflows()
		{
			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			var internalParentWorkflow = BMSTestHelper.CreateWorkflow(targetJobHeader, "Internal Parent Workflow");
			var targetWorkflow0 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 0");
			var targetWorkflow1 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var parentjob = Factory.NewWithValidTestData<OrgHeader>();
			var parentJobHeader = BMSTestHelper.CreateJobHeader(parentjob, description: "Parent Job Header");
			var parentWorkflow1 = BMSTestHelper.CreateWorkflow(parentJobHeader, "Parent Workflow 1");

			var internalLink = BMSTestHelper.CreateParentChildLink(internalParentWorkflow, targetWorkflow0);
			var link1 = BMSTestHelper.CreateParentChildLink(parentWorkflow1, targetWorkflow1);

			// to ensure dependencies do not mix up with parents and children
			var dependencyjob = Factory.NewWithValidTestData<OrgHeader>();
			var dependencyJobHeader = BMSTestHelper.CreateJobHeader(dependencyjob, description: "Dependency Job Header");
			var prereqWorkflow = BMSTestHelper.CreateWorkflow(dependencyJobHeader, "Prereq Workflow");
			var postreqWorkflow = BMSTestHelper.CreateWorkflow(dependencyJobHeader, "Postreq Workflow");

			var prereqLink = BMSTestHelper.CreateDependencyLink(prereqWorkflow, targetWorkflow0);
			var postreqLink = BMSTestHelper.CreateDependencyLink(targetWorkflow1, postreqWorkflow);

			var viewModel = new RelatedParentChildWorkflowsViewModel(targetJobHeader, RelationshipDirection.From);

			AssertContainsExactElementsInAnyOrder(new[] { link1 }, viewModel.ExternalWorkflowLinks);

			var targetWorkflow2 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 2");
			var targetWorkflow3 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 3");

			var parentWorkflow2 = BMSTestHelper.CreateWorkflow(parentJobHeader, "Parent Workflow 2");

			var link2 = BMSTestHelper.CreateParentChildLink(parentWorkflow2, targetWorkflow2);
			var link3 = BMSTestHelper.CreateParentChildLink(parentWorkflow2, targetWorkflow3);

			AssertContainsExactElementsInAnyOrder(new[] { link1, link2, link3 }, viewModel.ExternalWorkflowLinks);

			link1.Delete();

			AssertContainsExactElementsInAnyOrder(new[] { link2, link3 }, viewModel.ExternalWorkflowLinks);

			var parentWorkflow3 = BMSTestHelper.CreateWorkflow(parentJobHeader, "Parent Workflow 3");

			var jobHeaderLink = BMSTestHelper.CreateParentChildLink(parentWorkflow3, targetJobHeader);

			AssertContainsExactElementsInAnyOrder(new[] { link2, link3, jobHeaderLink }, viewModel.ExternalWorkflowLinks);
		}

		public void TestChildWorkflows()
		{
			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			var internalChildWorkflow = BMSTestHelper.CreateWorkflow(targetJobHeader, "Internal Parent Workflow");
			var targetWorkflow0 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 0");
			var targetWorkflow1 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var childjob = Factory.NewWithValidTestData<OrgHeader>();
			var childJobHeader = BMSTestHelper.CreateJobHeader(childjob, description: "Child Job Header");
			var childWorkflow1 = BMSTestHelper.CreateWorkflow(childJobHeader, "Child Workflow 1");

			var internalLink = BMSTestHelper.CreateParentChildLink(targetWorkflow0, internalChildWorkflow);
			var link1 = BMSTestHelper.CreateParentChildLink(targetWorkflow1, childWorkflow1);

			// to ensure dependencies do not mix up with parents and children
			var dependencyjob = Factory.NewWithValidTestData<OrgHeader>();
			var dependencyJobHeader = BMSTestHelper.CreateJobHeader(dependencyjob, description: "Dependency Job Header");
			var prereqWorkflow = BMSTestHelper.CreateWorkflow(dependencyJobHeader, "Prereq Workflow");
			var postreqWorkflow = BMSTestHelper.CreateWorkflow(dependencyJobHeader, "Postreq Workflow");

			var prereqLink = BMSTestHelper.CreateDependencyLink(prereqWorkflow, targetWorkflow0);
			var postreqLink = BMSTestHelper.CreateDependencyLink(targetWorkflow1, postreqWorkflow);

			var viewModel = new RelatedParentChildWorkflowsViewModel(targetJobHeader, RelationshipDirection.To);

			AssertContainsExactElementsInAnyOrder(new[] { link1 }, viewModel.ExternalWorkflowLinks);

			var targetWorkflow2 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 2");
			var targetWorkflow3 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 3");

			var childWorkflow2 = BMSTestHelper.CreateWorkflow(childJobHeader, "Child Workflow 2");

			var link2 = BMSTestHelper.CreateParentChildLink(targetWorkflow2, childWorkflow2);
			var link3 = BMSTestHelper.CreateParentChildLink(targetWorkflow3, childWorkflow2);

			AssertContainsExactElementsInAnyOrder(new[] { link1, link2, link3 }, viewModel.ExternalWorkflowLinks);

			link1.Delete();

			AssertContainsExactElementsInAnyOrder(new[] { link2, link3 }, viewModel.ExternalWorkflowLinks);

			var childWorkflow3 = BMSTestHelper.CreateWorkflow(childJobHeader, "Child Workflow 3");

			var jobHeaderLink = BMSTestHelper.CreateParentChildLink(targetJobHeader, childWorkflow3);

			AssertContainsExactElementsInAnyOrder(new[] { link2, link3, jobHeaderLink }, viewModel.ExternalWorkflowLinks);
		}
	}
}
