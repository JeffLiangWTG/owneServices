//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmEventSubscriptionValidation
//
//    This class should be used for overriding validation in AutoStmEventSubscriptionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmEventSubscriptionValidation : AutoStmEventSubscriptionValidation
	{
		public StmEventSubscriptionValidation(AutoStmEventSubscription parent) : base(parent)
		{
		}
	}
}
