//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuDeliveryRestrictionValidation
//
//    This class should be used for overriding validation in AutoStmMenuDeliveryRestrictionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDeliveryRestrictionValidation : AutoStmMenuDeliveryRestrictionValidation
	{
		public StmMenuDeliveryRestrictionValidation(AutoStmMenuDeliveryRestriction parent) : base(parent)
		{
		}
	}
}
