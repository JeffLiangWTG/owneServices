//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionHeaderNettingLinkLookups
//
//    This class should be used for overriding collections in AutoAccTransactionHeaderNettingLinkLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class AccTransactionHeaderNettingLinkLookups : AutoAccTransactionHeaderNettingLinkLookups
	{
		public AccTransactionHeaderNettingLinkLookups(AutoAccTransactionHeaderNettingLink parent) : base(parent)
		{
		}
	}
}
