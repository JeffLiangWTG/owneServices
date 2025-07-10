//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientPWSChargeValidation
//
//    This class should be used for overriding validation in AutoClientPWSChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientPWSChargeValidation : AutoClientPWSChargeValidation
	{
		public ClientPWSChargeValidation(AutoClientPWSCharge parent) : base(parent)
		{
		}
	}
}
