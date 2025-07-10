//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccConsolidationBatchLookups
//
//    This class should be used for overriding collections in AutoAccConsolidationBatchLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class AccConsolidationBatchLookups : AutoAccConsolidationBatchLookups
	{
		public AccConsolidationBatchLookups(AutoAccConsolidationBatch parent) : base(parent)
		{
		}
	}
}