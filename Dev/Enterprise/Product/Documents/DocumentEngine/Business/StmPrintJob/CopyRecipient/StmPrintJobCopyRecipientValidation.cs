//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmPrintJobCopyRecipientValidation
//
//    This class should be used for overriding validation in AutoStmPrintJobCopyRecipientValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintJobCopyRecipientValidation : AutoStmPrintJobCopyRecipientValidation
	{
		public StmPrintJobCopyRecipientValidation(AutoStmPrintJobCopyRecipient parent) : base(parent)
		{
		}
	}
}
