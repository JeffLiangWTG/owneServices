//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAUTravelDocAddInfoLookups
//
//    This class should be used for overriding collections in AutoAUTravelDocAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUTravelDocAddInfoLookups : AutoAUTravelDocAddInfoLookups
	{
		public AUTravelDocAddInfoLookups(AutoAUTravelDocAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Countries
		{
			get { return Factory.GetCachedValue<CMRICAOCountryCodes>(); }
		}
	}
}
