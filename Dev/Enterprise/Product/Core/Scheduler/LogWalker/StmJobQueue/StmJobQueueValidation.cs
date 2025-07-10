//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmJobQueueValidation
//
//    This class should be used for overriding validation in AutoStmJobQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.LogWalker
{
	public class StmJobQueueValidation : AutoStmJobQueueValidation
	{
		public StmJobQueueValidation(AutoStmJobQueue parent) : base(parent)
		{
		}
	}
}
