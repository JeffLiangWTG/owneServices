//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewShipmentConsolAndMasterBillNumbersLookups
//
//    This class should be used for overriding collections in AutoViewShipmentConsolAndMasterBillNumbersLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ShipmentConsolAndMasterBillNumbers
{
	public class ViewShipmentConsolAndMasterBillNumbersLookups : AutoViewShipmentConsolAndMasterBillNumbersLookups
	{
		public ViewShipmentConsolAndMasterBillNumbersLookups(AutoViewShipmentConsolAndMasterBillNumbers parent) : base(parent)
		{
		}
	}
}

