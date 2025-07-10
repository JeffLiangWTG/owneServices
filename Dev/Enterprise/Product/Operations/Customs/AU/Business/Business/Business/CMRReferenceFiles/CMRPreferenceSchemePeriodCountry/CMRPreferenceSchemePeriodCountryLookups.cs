//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceSchemePeriodCountryLookups
//
//    This class should be used for overriding collections in AutoCMRPreferenceSchemePeriodCountryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemePeriodCountryLookups : AutoCMRPreferenceSchemePeriodCountryLookups
	{
		public CMRPreferenceSchemePeriodCountryLookups(AutoCMRPreferenceSchemePeriodCountry parent)
			: base(parent)
		{
		}
	}
}
