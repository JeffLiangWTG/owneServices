//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmPrintServerValidation
//
//    This class should be used for overriding validation in AutoStmPrintServerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintServerValidation : AutoStmPrintServerValidation
	{
		public StmPrintServerValidation(AutoStmPrintServer parent) : base(parent)
		{
		}
	}
}
