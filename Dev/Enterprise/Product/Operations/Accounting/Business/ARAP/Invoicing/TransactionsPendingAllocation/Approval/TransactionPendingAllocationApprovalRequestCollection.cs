using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationApprovalRequestCollection : TransactionApprovalRequestCollection<TransactionPendingAllocationApprovalRequest>
	{
		public TransactionPendingAllocationApprovalRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransactionPendingAllocationApprovalRequestCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override string[] GetApprovalType()
		{
			return new[] { Constants.GenApprovalRequestApprovalType.TransactionPendingAllocation };
		}
	}
}
