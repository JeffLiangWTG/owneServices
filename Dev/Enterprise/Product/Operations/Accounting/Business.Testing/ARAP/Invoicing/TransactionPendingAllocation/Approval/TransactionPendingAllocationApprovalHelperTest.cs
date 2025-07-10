using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Business.TransactionApproval.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class TransactionPendingAllocationApprovalHelperTest : TransactionApprovalHelperTest<TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		protected override TransactionPendingAllocationApprovalRequest GetNewTestApprovalWithoutSetup(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<TransactionPendingAllocationApprovalRequest>();
		}

		protected override TransactionApprovalHelper<TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails> GetNewTransactionApprovalHelper(TransactionPendingAllocationApprovalRequest approval)
		{
			return new TransactionPendingAllocationApprovalHelper(approval);
		}

		protected override void SetupDefaultPostingAction(TransactionPendingAllocationApprovalRequest approval)
		{
			approval.XP_ParentID = DefaultParentID;
		}

		protected override void SetupAnotherPostingAction(TransactionPendingAllocationApprovalRequest approval)
		{
			approval.XP_ParentID = AnotherParentID; //only one request is allowed for the parent ID for Transaction Pending Allocation requests. So only new ParentID can make a request with different details.
		}

		ZGuid DefaultParentID
		{
			get
			{
				if (!defaultParentID.HasValue)
				{
					defaultParentID = ZGuid.NewZGuid();
				}
				return defaultParentID.Value;
			}
		}
		ZGuid? defaultParentID;

		ZGuid AnotherParentID
		{
			get
			{
				if (!anotherParentID.HasValue)
				{
					anotherParentID = ZGuid.NewZGuid();
				}
				return anotherParentID.Value;
			}
		}
		ZGuid? anotherParentID;
	}
}
