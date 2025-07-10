//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionApprovalRequestLookups
//
//    This class should be used for overriding collections in AutoAccCommissionApprovalRequestLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestLookups : AutoAccCommissionApprovalRequestLookups
	{
		public AccCommissionApprovalRequestLookups(AutoAccCommissionApprovalRequest parent) : base(parent)
		{
		}

		public override GlbStaffCollection ApprovingStaff1s
		{
			get { return new CommissionAuthorizationStaffCollection(Factory, x => x.CommissionAuthorizationLevel1); }
		}

		public override GlbStaffCollection ApprovingStaff2s
		{
			get { return new CommissionAuthorizationStaffCollection(Factory, x => x.CommissionAuthorizationLevel2); }
		}
	}
}
