//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentPreferenceSchemeValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentPreferenceSchemeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentPreferenceSchemeValidation : AutoCMRInstrumentPreferenceSchemeValidation
	{
		public CMRInstrumentPreferenceSchemeValidation(AutoCMRInstrumentPreferenceScheme parent)
			: base(parent)
		{
		}
	}
}
