using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class AccComplianceDocumentWorkflowDescriptorTest<T> : WorkflowDescriptorTestCase<T> where T : AccComplianceDocumentWorkflowDescriptor, new()
	{
		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(AccComplianceDocumentHeader), WorkflowDescriptor.WorkflowProviderType);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy; }
		}
	}
}
