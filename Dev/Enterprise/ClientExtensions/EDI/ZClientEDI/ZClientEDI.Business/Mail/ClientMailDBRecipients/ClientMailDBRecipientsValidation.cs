//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientMailDBRecipientsValidation
//
//    This class should be used for overriding validation in AutoClientMailDBRecipientsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Mail.Business
{
	public class ClientMailDBRecipientsValidation : AutoClientMailDBRecipientsValidation
	{
		public ClientMailDBRecipientsValidation(AutoClientMailDBRecipients parent) : base(parent)
		{
		}
	}
}

