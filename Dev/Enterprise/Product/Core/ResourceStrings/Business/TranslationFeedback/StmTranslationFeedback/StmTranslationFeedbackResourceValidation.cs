//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmTranslationFeedbackResourceValidation
//
//    This class should be used for overriding validation in AutoStmTranslationFeedbackResourceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedbackResourceValidation : AutoStmTranslationFeedbackResourceValidation
	{
		public StmTranslationFeedbackResourceValidation(AutoStmTranslationFeedbackResource parent) : base(parent)
		{
		}
	}
}
