using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.TransactionApproval.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Security.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalBulk))]
	public class TransactionPendingAllocationApprovalBulkTest : TransactionApprovalBulkTest<TransactionPendingAllocationApprovalBulk, TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails, InvoicingBase>
	{
		public override void TestPostApprovalsAndRemovePosted()
		{
			Assert("Functionality is not supported.", true);
		}

		public override void TestStatusAndParentNullInPostApprovalsAndRemovePosted()
		{
			Assert("Functionality is not supported.", true);
		}

		public override void TestParentValidationErrorInPostApprovalsAndRemovePosted()
		{
			Assert("Functionality is not supported.", true);
		}

		public override void TestTwiceClickingPostApprovalsAndRemovePosted()
		{
			Assert("Functionality is not supported.", true);
		}

		public override void TestParentPostedAndRemoveFromApprovals()
		{
			Assert("Functionality is not supported.", true);
		}

		public override void TestGUIProviderOfPostApprovalsAndRemovePosted()
		{
			Assert("Functionality is not supported.", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewApprovalBulk(new DefaultAccessSecurityProvider(), Factory.NewWithValidTestData<TransactionPendingAllocationApprovalRequest>());
		}

		protected override TransactionPendingAllocationApprovalBulk GetNewApprovalBulk(ISecurityOverrideProvider interactiveSecurityOverrideProvider, params TransactionPendingAllocationApprovalRequest[] approvalRequests)
		{
			return new TransactionPendingAllocationApprovalBulk(Factory, interactiveSecurityOverrideProvider, approvalRequests);
		}

		protected override void SetupSecurity(bool disallowSecondLevel = false, bool disallowTwoLevels = false, Guid? branchPK = null, Guid? departmentPK = null)
		{
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = !disallowSecondLevel && !disallowTwoLevels;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SecurityTestObject.CreateTestUser(true, Env.Security.TransactionsPendingAllocation.Code, "tst", "newuser", "password");
		}
	}
}
