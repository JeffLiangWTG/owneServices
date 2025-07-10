//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPromptSkipValidation
//
//    This class should be used for overriding validation in AutoEdiPromptSkipValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiPromptSkipValidation : AutoEdiPromptSkipValidation
	{
		public EdiPromptSkipValidation(AutoEdiPromptSkip parent) : base(parent)
		{
		}
	}
}