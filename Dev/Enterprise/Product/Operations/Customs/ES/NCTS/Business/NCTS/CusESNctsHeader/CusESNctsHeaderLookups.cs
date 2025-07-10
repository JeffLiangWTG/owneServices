//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusESNctsHeaderLookups
//
//    This class should be used for overriding collections in AutoCusESNctsHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class CusESNctsHeaderLookups : AutoCusESNctsHeaderLookups
	{
		public CusESNctsHeaderLookups(AutoCusESNctsHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SummaryTypeList => Factory.GetCachedValue<SummaryTypeList>();
	}
}
