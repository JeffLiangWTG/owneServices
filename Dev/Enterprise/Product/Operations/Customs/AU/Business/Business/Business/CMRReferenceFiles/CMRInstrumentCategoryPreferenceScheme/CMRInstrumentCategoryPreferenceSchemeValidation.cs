//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCategoryPreferenceSchemeValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCategoryPreferenceSchemeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryPreferenceSchemeValidation : AutoCMRInstrumentCategoryPreferenceSchemeValidation
	{
		public CMRInstrumentCategoryPreferenceSchemeValidation(AutoCMRInstrumentCategoryPreferenceScheme parent)
			: base(parent)
		{
		}
	}
}
