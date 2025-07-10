using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkflowDeleterTest : BMSTestCaseWithFactory
	{
		public void TestCanDeleteBusinessObject_WithApprovedShapeOnParent()
		{
			var system = CreateSystem("ORG");
			var org_parent = Factory.NewWithValidTestData<OrgHeader>();
			var org_child = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader_parent = ProcessJobHeader.GetForParent(org_parent, Factory);
			var jobHeader_child = ProcessJobHeader.GetForParent(org_child, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader_child, "workflow1");

			BMSTestHelper.MakeChildOf(jobHeader_child, jobHeader_parent);

			var shape = Factory.New<IBMNCNShape>();
			shape.BNS_RelatedEntityID = jobHeader_parent.PK;
			shape.Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();
			org_child.Delete();

			AssertEquals(true, org_child.IsDeleted);
			AssertEquals(true, jobHeader_child.IsDeleted);
			AssertEquals(true, workflow.IsDeleted);

			AssertEquals(false, jobHeader_parent.IsDeleted);
			AssertEquals(false, ((BusinessObject)shape).IsDeleted);
		}

		public void TestCanDeleteBusinessObject_WithoutApprovedShape()
		{
			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var shape = Factory.New<IBMNCNShape>();
			shape.BNS_RelatedEntityID = jobHeader.PK;

			Factory.Save();
			org.Delete();

			AssertEquals(true, org.IsDeleted);
			AssertEquals(true, jobHeader.IsDeleted);
			AssertEquals(true, workflow.IsDeleted);

			AssertEquals(false, ((BusinessObject)shape).IsDeleted);
			AssertEquals(ZGuid.Empty, shape.BNS_RelatedEntityID);
		}
	}
}
