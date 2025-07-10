//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewCommissionLineLookups
//
//    This class should be used for overriding collections in AutoViewCommissionLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineLookups : AutoViewCommissionLineLookups
	{
		public ViewCommissionLineLookups(AutoViewCommissionLine parent) : base(parent)
		{
		}

		#region Lists

		public AccChargeCodeCollection AccChargeCodes
		{
			get { return new AccChargeCodeCollection(Factory); }
		}

		public AccCommissionApprovalRequestCollection CommissionApprovalRequests
		{
			get { return new AccCommissionApprovalRequestCollection(Factory); }
		}

		#endregion
	}
}
