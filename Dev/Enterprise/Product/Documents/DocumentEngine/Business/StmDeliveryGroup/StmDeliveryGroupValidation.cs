//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDeliveryGroupValidation
//
//    This class should be used for overriding validation in AutoStmDeliveryGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmDeliveryGroupValidation : AutoStmDeliveryGroupValidation
	{
		public StmDeliveryGroupValidation(AutoStmDeliveryGroup parent) : base(parent)
		{
		}
	}
}
