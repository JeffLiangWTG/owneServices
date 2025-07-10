//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKRAddInfoLookups
//
//    This class should be used for overriding collections in AutoKRAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.KR.Business
{
	public class KRAddInfoLookups : AutoKRAddInfoLookups
	{
		public KRAddInfoLookups(AutoKRAddInfo parent) : base(parent)
		{
		}
		public new KRAddInfo Parent => (KRAddInfo)base.Parent;
	}
}
