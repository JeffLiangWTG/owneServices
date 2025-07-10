using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusLPCOHeaderWorkflowDescriptor))]
	sealed class CusLPCOHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusLPCOHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CusBRLPCOHeaderWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("BR LPCO", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			Assert("RequiresPort1 should be FALSE", !WorkflowDescriptor.RequiresPort1);
			Assert("RequiresPort2 should be FALSE", !WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			Assert("RequiresClient should be TRUE", WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			Assert("RequiresBranch should be FALSE", !WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			Assert("RequiresDepartament should be FALSE", !WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			Assert("SupportsEventTracking should be TRUE", WorkflowDescriptor.SupportsEventTracking);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { Factory.New<CusLPCOHeader>() };
	}
}
