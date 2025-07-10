//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMailDBAttachmentsValidation
//
//    This class should be used for overriding validation in AutoMailDBAttachmentsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MailManager.Business
{
	public class MailDBAttachmentsValidation : AutoMailDBAttachmentsValidation
	{
		public MailDBAttachmentsValidation(AutoMailDBAttachments parent) : base(parent)
		{
		}
	}
}
