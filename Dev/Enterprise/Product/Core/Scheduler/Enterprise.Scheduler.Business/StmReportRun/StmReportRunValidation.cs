//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmReportRunValidation
//
//    This class should be used for overriding validation in AutoStmReportRunValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Scheduler.Business
{
	public class StmReportRunValidation : AutoStmReportRunValidation
	{
		public StmReportRunValidation(AutoStmReportRun parent) : base(parent)
		{
		}
	}
}
