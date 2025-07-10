//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientPWSHeaderValidation
//
//    This class should be used for overriding validation in AutoClientPWSHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientPWSHeaderValidation : AutoClientPWSHeaderValidation
	{
		public ClientPWSHeaderValidation(AutoClientPWSHeader parent) : base(parent)
		{
		}
	}
}
