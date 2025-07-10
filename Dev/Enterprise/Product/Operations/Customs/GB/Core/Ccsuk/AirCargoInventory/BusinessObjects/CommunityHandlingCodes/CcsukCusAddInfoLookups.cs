//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCcsukCusAddInfoLookups
//
//    This class should be used for overriding collections in AutoCcsukCusAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CcsukCusAddInfoLookups : AutoCcsukCusAddInfoLookups
	{
		public CcsukCusAddInfoLookups(AutoCcsukCusAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SpecialHandlingCodes => Factory.GetCachedValue<AWBSpecialHandlingCodeDescriptionPairList>();
	}
}
