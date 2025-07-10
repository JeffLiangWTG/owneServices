//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiLegacyConversationMessageValidation
//
//    This class should be used for overriding validation in AutoEdiLegacyConversationMessageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiLegacyConversationMessageValidation : AutoEdiLegacyConversationMessageValidation
	{
		public EdiLegacyConversationMessageValidation(AutoEdiLegacyConversationMessage parent) : base(parent)
		{
		}
	}
}

