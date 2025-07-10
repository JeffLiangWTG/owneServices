//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisConcernLookups
//
//    This class should be used for overriding collections in AutoCMRAqisConcernLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisConcernLookups : AutoCMRAqisConcernLookups
	{
		public CMRAqisConcernLookups(AutoCMRAqisConcern parent)
			: base(parent)
		{
		}
	}
}
