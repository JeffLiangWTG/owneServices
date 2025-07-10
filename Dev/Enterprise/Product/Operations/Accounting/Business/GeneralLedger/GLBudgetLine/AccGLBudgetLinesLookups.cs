//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGLBudgetLinesLookups
//
//    This class should be used for overriding collections in AutoAccGLBudgetLinesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public class AccGLBudgetLinesLookups : AutoAccGLBudgetLinesLookups
	{
		public AccGLBudgetLinesLookups(AutoAccGLBudgetLines parent)
			: base(parent)
		{
		}
	}
}
