using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public abstract class TransactionApprovalRequestCollection<RequestType> : ActiveBusinessObjectCollection<RequestType>
		where RequestType : GenApprovalRequest
	{
		public TransactionApprovalRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransactionApprovalRequestCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery additionalFilter = new ZQuery();
			additionalFilter.AddToFilter(base.CreateRelationshipFilter());
			additionalFilter.AddToFilter(AccountingUtils.GenerateCompanyFilter(GenApprovalRequestSchema.XP_GB_RequestingBranch, Factory));
			additionalFilter.AddToFilter(GenApprovalRequestSchema.XP_SubSystem, Constants.GenApprovalRequestSubSystem.Accounting);
			additionalFilter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalType, GetApprovalType());

			return additionalFilter;
		}

		protected abstract string[] GetApprovalType();
	}
}