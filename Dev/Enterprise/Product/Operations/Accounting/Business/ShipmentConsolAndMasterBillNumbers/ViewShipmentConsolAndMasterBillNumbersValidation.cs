//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewShipmentConsolAndMasterBillNumbersValidation
//
//    This class should be used for overriding validation in AutoViewShipmentConsolAndMasterBillNumbersValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ShipmentConsolAndMasterBillNumbers
{
	public class ViewShipmentConsolAndMasterBillNumbersValidation : AutoViewShipmentConsolAndMasterBillNumbersValidation
	{
		public ViewShipmentConsolAndMasterBillNumbersValidation(AutoViewShipmentConsolAndMasterBillNumbers parent) : base(parent)
		{
		}
	}
}

