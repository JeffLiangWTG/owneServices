//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionApprovalRequestItemLookups
//
//    This class should be used for overriding collections in AutoAccCommissionApprovalRequestItemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestItemLookups : AutoAccCommissionApprovalRequestItemLookups
	{
		public AccCommissionApprovalRequestItemLookups(AutoAccCommissionApprovalRequestItem parent) : base(parent)
		{
		}

		#region CommissionApprovalRequests

		public virtual AccCommissionApprovalRequestCollection CommissionApprovalRequests
		{
			get { return new AccCommissionApprovalRequestCollection(Factory); }
		}

		#endregion

		#region CommissionLines

		public virtual AccCommissionLineCollection CommissionLines
		{
			get { return new AccCommissionLineCollection(Factory); }
		}

		#endregion
	}
}
