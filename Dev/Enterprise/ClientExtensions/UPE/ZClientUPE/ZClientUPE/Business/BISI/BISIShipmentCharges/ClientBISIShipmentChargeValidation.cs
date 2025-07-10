//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientBISIShipmentChargeValidation
//
//    This class should be used for overriding validation in AutoClientBISIShipmentChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientBISIShipmentChargeValidation : AutoClientBISIShipmentChargeValidation
	{
		public ClientBISIShipmentChargeValidation(AutoClientBISIShipmentCharge parent)
			: base(parent)
		{
		}
	}
}
