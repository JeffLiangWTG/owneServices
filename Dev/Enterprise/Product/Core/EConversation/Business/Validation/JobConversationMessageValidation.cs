//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConversationMessageValidation
//
//    This class should be used for overriding validation in AutoJobConversationMessageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.EConversation.Business
{
	public class JobConversationMessageValidation : AutoJobConversationMessageValidation
	{
		public JobConversationMessageValidation(AutoJobConversationMessage parent) : base(parent)
		{
		}
	}
}