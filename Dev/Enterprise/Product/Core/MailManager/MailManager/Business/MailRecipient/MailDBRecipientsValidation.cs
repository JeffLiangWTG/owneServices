//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMailDBRecipientsValidation
//
//    This class should be used for overriding validation in AutoMailDBRecipientsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MailManager.Business
{
	public class MailDBRecipientsValidation : AutoMailDBRecipientsValidation
	{
		public MailDBRecipientsValidation(AutoMailDBRecipients parent) : base(parent)
		{
		}
	}
}
