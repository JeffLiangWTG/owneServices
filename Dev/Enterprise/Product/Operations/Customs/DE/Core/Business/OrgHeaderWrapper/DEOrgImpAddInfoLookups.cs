//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDEOrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoDEOrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class DEOrgImpAddInfoLookups : AutoDEOrgImpAddInfoLookups
	{
		public DEOrgImpAddInfoLookups(AutoDEOrgImpAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList VATClaimBackList => Factory.GetCachedValue<YesNoList>();
	}
}
