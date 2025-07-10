using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class ReceiptPaymentBaseWorkflowDescriptorTest<T> : WorkflowDescriptorTestCase<T> where T : ReceiptPaymentBaseWorkflowDescriptor, new()
	{
		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
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
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestSupportsWorkflowTriggerActionUniversalTransactionBatchXML()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalTransactionBatchXML);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return true; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy; }
		}
	}
}
