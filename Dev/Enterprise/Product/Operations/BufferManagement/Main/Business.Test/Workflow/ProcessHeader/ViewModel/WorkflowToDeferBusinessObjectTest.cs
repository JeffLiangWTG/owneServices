using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkflowToDeferBusinessObject))]
	class WorkflowToDeferBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeferJob_ShouldNotIncludeRemovePostreqAction()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var bizo = new WorkflowToDeferBusinessObject(jobHeader, null, false);
			AssertContainsExactElementsInAnyOrder(new WorkflowDeferalActionList(), bizo.PrerequisiteDeferalActions);

			bizo = new WorkflowToDeferBusinessObject(jobHeader, null, true);
			var expected = new WorkflowDeferalActionList();
			expected.RemoveCode(WorkflowDeferalActionList.Codes.RemovePrerequisite);
			AssertContainsExactElementsInAnyOrder(expected, bizo.PrerequisiteDeferalActions);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var link = workflow.LinksFromMeToOthers_ForBinding.AddNew();

			return new WorkflowToDeferBusinessObject(workflow, link, false);
		}
	}
}
