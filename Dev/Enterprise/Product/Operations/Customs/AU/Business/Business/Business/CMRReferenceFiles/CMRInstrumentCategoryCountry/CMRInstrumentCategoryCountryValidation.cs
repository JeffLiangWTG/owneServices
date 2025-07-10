//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCategoryCountryValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCategoryCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryCountryValidation : AutoCMRInstrumentCategoryCountryValidation
	{
		public CMRInstrumentCategoryCountryValidation(AutoCMRInstrumentCategoryCountry parent)
			: base(parent)
		{
		}
	}
}
