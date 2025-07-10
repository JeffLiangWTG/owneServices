//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientXPLDUploadLogValidation
//
//    This class should be used for overriding validation in AutoClientXPLDUploadLogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientXPLDUploadLogValidation : AutoClientXPLDUploadLogValidation
	{
		public ClientXPLDUploadLogValidation(AutoClientXPLDUploadLog parent) : base(parent)
		{
		}
	}
}
