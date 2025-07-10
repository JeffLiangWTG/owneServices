using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkflowToDeferBusinessObjectCollection))]
	class WorkflowToDeferBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WorkflowToDeferBusinessObjectCollection>
	{
		protected override WorkflowToDeferBusinessObjectCollection GetCollectionToTest()
		{
			return new WorkflowToDeferBusinessObjectCollection(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();
			var link = workflow.LinksFromMeToOthers_ForBinding.AddNew();

			return new WorkflowToDeferBusinessObject(workflow, link, false);
		}
	}
}
