//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKREntryLineDetailsViewLookups
//
//    This class should be used for overriding collections in AutoKREntryLineDetailsViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryLineDetailsViewLookups : AutoKREntryLineDetailsViewLookups
	{
		public KREntryLineDetailsViewLookups(AutoKREntryLineDetailsView parent) : base(parent)
		{
		}
		public ConsigneeCollection ConsigneeList => new ConsigneeCollection(Factory);
	}
}
