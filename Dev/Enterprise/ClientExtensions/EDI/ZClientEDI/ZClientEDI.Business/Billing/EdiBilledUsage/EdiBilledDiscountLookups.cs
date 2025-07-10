//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiBilledDiscountLookups
//
//    This class should be used for overriding collections in AutoEdiBilledDiscountLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBilledDiscountLookups : AutoEdiBilledDiscountLookups
	{
		public EdiBilledDiscountLookups(AutoEdiBilledDiscount parent) : base(parent)
		{
		}
	}
}

