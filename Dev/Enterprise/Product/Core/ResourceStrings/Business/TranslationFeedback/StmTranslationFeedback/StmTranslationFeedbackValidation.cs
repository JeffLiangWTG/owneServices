//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmTranslationFeedbackValidation
//
//    This class should be used for overriding validation in AutoStmTranslationFeedbackValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedbackValidation : AutoStmTranslationFeedbackValidation
	{
		public StmTranslationFeedbackValidation(AutoStmTranslationFeedback parent) : base(parent)
		{
		}

		protected override void CheckXT_SuggestedTranslation()
		{
			ParameterConsistencyChecker.Check(Parent.XT_Source, Parent.XT_SuggestedTranslation, Parent.XT_SuggestedTranslationInfo);
		}
	}
}
