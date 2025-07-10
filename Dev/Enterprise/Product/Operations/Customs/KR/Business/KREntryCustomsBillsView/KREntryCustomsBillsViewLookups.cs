//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKREntryCustomsBillsViewLookups
//
//    This class should be used for overriding collections in AutoKREntryCustomsBillsViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryCustomsBillsViewLookups : AutoKREntryCustomsBillsViewLookups
	{
		public KREntryCustomsBillsViewLookups(AutoKREntryCustomsBillsView parent) : base(parent)
		{
		}

		public ConsigneeCollection ConsigneeList => new(Factory);
	}
}

