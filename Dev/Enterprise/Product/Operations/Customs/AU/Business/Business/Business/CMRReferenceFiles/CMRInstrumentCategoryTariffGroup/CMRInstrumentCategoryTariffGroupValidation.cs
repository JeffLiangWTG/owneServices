//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentCategoryTariffGroupValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentCategoryTariffGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryTariffGroupValidation : AutoCMRInstrumentCategoryTariffGroupValidation
	{
		public CMRInstrumentCategoryTariffGroupValidation(AutoCMRInstrumentCategoryTariffGroup parent)
			: base(parent)
		{
		}
	}
}
