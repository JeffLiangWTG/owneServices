//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCountryValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCountryValidation : AutoCMRInstrumentCountryValidation
	{
		public CMRInstrumentCountryValidation(AutoCMRInstrumentCountry parent)
			: base(parent)
		{
		}
	}
}
