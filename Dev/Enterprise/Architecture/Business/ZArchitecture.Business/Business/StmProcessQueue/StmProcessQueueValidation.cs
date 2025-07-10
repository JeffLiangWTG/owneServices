//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmProcessQueueValidation
//
//    This class should be used for overriding validation in AutoStmProcessQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ZArchitecture.Business
{
	public class StmProcessQueueValidation : AutoStmProcessQueueValidation
	{
		public StmProcessQueueValidation(AutoStmProcessQueue parent) : base(parent)
		{
		}
	}
}
