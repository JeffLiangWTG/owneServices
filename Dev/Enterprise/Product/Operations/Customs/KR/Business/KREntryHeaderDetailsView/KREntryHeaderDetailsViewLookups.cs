//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKREntryHeaderDetailsViewLookups
//
//    This class should be used for overriding collections in AutoKREntryHeaderDetailsViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryHeaderDetailsViewLookups : AutoKREntryHeaderDetailsViewLookups
	{
		public KREntryHeaderDetailsViewLookups(AutoKREntryHeaderDetailsView parent) : base(parent)
		{
		}

		public OrgHeaderCollection OrganisationList => new OrgHeaderCollection(Factory);
		public ConsigneeCollection ConsigneeList => new ConsigneeCollection(Factory);
	}
}
