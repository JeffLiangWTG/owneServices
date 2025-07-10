//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBROrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoBROrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BROrgImpAddInfoLookups : AutoBROrgImpAddInfoLookups
	{
		public BROrgImpAddInfoLookups(AutoBROrgImpAddInfo parent) : base(parent)
		{
		}

		public GlbStaffCollection StaffList => new GlbStaffCollection(Factory);
	}
}
