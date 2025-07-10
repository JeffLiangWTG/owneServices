//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmPrintJobQueueValidation
//
//    This class should be used for overriding validation in AutoStmPrintJobQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Scheduler.Business;

public class StmPrintJobQueueValidation : AutoStmPrintJobQueueValidation
{
	public StmPrintJobQueueValidation(AutoStmPrintJobQueue parent) : base(parent)
	{
	}
}
