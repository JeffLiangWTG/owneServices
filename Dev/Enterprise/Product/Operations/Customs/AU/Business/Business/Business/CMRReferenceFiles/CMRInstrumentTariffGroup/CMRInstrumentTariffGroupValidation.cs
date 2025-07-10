//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRInstrumentTariffGroupValidation
//
//    This class should be used for overriding validation in AutoCMRInstrumentTariffGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentTariffGroupValidation : AutoCMRInstrumentTariffGroupValidation
	{
		public CMRInstrumentTariffGroupValidation(AutoCMRInstrumentTariffGroup parent)
			: base(parent)
		{
		}
	}
}
