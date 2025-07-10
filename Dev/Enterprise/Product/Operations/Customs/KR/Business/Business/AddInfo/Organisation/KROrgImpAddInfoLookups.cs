//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKROrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoKROrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class KROrgImpAddInfoLookups : AutoKROrgImpAddInfoLookups
	{
		public KROrgImpAddInfoLookups(AutoKROrgImpAddInfo parent) : base(parent)
		{
		}
		public CodeDescriptionPairList BankTypeList => Factory.GetCachedValue<BankTypeList>();
		public CodeDescriptionPairList VATDefermentType => Factory.GetCachedValue<VATDefermentType>();
	}
}

