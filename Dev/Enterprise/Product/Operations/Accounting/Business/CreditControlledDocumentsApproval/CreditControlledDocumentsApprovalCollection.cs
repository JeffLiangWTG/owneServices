using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class CreditControlledDocumentsApprovalCollection : ActiveBusinessObjectCollection<CreditControlledDocumentsApproval>
	{
		public CreditControlledDocumentsApprovalCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CreditControlledDocumentsApprovalCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var additionalFilter = new ZQuery();
			additionalFilter.AddToFilter(base.CreateRelationshipFilter());
			additionalFilter.AddToFilter(GenApprovalRequestSchema.XP_GB_RequestingBranch, GlbCompany.CurrentCompany.Branches.GetPKs());
			additionalFilter.AddToFilter(GenApprovalRequestSchema.XP_SubSystem, Constants.GenApprovalRequestSubSystem.Accounting);
			additionalFilter.AddToFilter(GenApprovalRequestSchema.XP_ApprovalType, Constants.GenApprovalRequestApprovalType.ARCreditControlledDocuments);
			return additionalFilter;
		}
	}
}