//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionApprovalRequestItemValidation
//
//    This class should be used for overriding validation in AutoAccCommissionApprovalRequestItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestItemValidation : AutoAccCommissionApprovalRequestItemValidation
	{
		public AccCommissionApprovalRequestItemValidation(AutoAccCommissionApprovalRequestItem parent) : base(parent)
		{
		}
	}
}
