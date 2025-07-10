//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCNAddInfoLookups
//
//    This class should be used for overriding collections in AutoCNAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CN.Business
{
	public class CNAddInfoLookups : AutoCNAddInfoLookups
	{
		public CNAddInfoLookups(AutoCNAddInfo parent)
			: base(parent)
		{
		}

		public new AutoCNAddInfo Parent => (AutoCNAddInfo)base.Parent;
	}
}
