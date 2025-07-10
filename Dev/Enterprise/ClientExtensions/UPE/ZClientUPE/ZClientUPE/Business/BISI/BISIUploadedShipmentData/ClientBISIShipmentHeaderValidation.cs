//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientBISIShipmentHeaderValidation
//
//    This class should be used for overriding validation in AutoClientBISIShipmentHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientBISIShipmentHeaderValidation : AutoClientBISIShipmentHeaderValidation
	{
		public ClientBISIShipmentHeaderValidation(AutoClientBISIShipmentHeader parent)
			: base(parent)
		{
		}
	}
}
