//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessageAttachValidation
//
//    This class should be used for overriding validation in AutoEDIMessageAttachValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Messaging.Business
{
	public class EDIMessageAttachValidation : AutoEDIMessageAttachValidation
	{
		public EDIMessageAttachValidation(AutoEDIMessageAttach parent) : base(parent)
		{
		}
	}
}
