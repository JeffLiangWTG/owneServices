//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmPrintJobValidation
//
//    This class should be used for overriding validation in AutoStmPrintJobValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintJobValidation : AutoStmPrintJobValidation
	{
		public StmPrintJobValidation(AutoStmPrintJob parent) : base(parent)
		{
		}
	}
}
