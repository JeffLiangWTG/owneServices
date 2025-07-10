//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientOrgRematchValidation
//
//    This class should be used for overriding validation in AutoClientOrgRematchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientOrgRematchValidation : AutoClientOrgRematchValidation
	{
		public ClientOrgRematchValidation(AutoClientOrgRematch parent)
			: base(parent)
		{
		}
	}
}
