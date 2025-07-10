using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRWorkflowDescriptor))]
	class JPAFRHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<JPAFRWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", JPAFRWorkflowDescriptor.Constants.Code, WorkflowDescriptor.Code);
		}

		public void TestClientName()
		{
			AssertEquals("Carrier", WorkflowDescriptor.ClientName);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JPAFRWorkflowDescriptor.Constants.Description, WorkflowDescriptor.Description);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new[] { Factory.New<JPAFRHeader>() };

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
	}
}
