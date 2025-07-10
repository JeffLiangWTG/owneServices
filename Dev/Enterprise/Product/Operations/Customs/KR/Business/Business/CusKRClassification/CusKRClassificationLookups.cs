//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusKRClassificationLookups
//
//    This class should be used for overriding collections in AutoCusKRClassificationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusKRClassificationLookups : AutoCusKRClassificationLookups
	{
		public CusKRClassificationLookups(AutoCusKRClassification parent) : base(parent)
		{
		}
		public CodeDescriptionPairList CountryOfOriginLabelLocationCodeList => Factory.GetCachedValue<Messaging.CountryOfOriginLabelLocationCodeList>();
	}
}
