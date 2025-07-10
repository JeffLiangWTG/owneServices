//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceSchemePeriodCountryValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceSchemePeriodCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemePeriodCountryValidation : AutoCMRPreferenceSchemePeriodCountryValidation
	{
		public CMRPreferenceSchemePeriodCountryValidation(AutoCMRPreferenceSchemePeriodCountry parent)
			: base(parent)
		{
		}
	}
}
