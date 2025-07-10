//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewMatchGroupLookups
//
//    This class should be used for overriding collections in AutoViewMatchGroupLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ViewMatchGroupLookups : AutoViewMatchGroupLookups
	{
		public ViewMatchGroupLookups(AutoViewMatchGroup parent)
			: base(parent)
		{
		}
	}
}
