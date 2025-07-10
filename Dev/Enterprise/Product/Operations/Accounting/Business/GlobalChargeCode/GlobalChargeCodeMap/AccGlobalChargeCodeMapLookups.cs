//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGlobalChargeCodeMapLookups
//
//    This class should be used for overriding collections in AutoAccGlobalChargeCodeMapLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class AccGlobalChargeCodeMapLookups : AutoAccGlobalChargeCodeMapLookups
	{
		public AccGlobalChargeCodeMapLookups(AutoAccGlobalChargeCodeMap parent) : base(parent)
		{
		}
	}
}

