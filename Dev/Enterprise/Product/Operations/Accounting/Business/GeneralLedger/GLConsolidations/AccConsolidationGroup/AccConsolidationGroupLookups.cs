//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccConsolidationGroupLookups
//
//    This class should be used for overriding collections in AutoAccConsolidationGroupLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class AccConsolidationGroupLookups : AutoAccConsolidationGroupLookups
	{
		public AccConsolidationGroupLookups(AutoAccConsolidationGroup parent)
			: base(parent)
		{
		}

		public AccConsolidationGroupCollection ConsolidationGroups
		{
			get { return new AccConsolidationGroupCollection(Parent.Factory); }
		}
	}
}