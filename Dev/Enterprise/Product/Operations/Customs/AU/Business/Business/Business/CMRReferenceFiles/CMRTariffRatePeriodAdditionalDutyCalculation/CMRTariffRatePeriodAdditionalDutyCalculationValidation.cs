//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTariffRatePeriodAdditionalDutyCalculationValidation
//
//    This class should be used for overriding validation in AutoCMRTariffRatePeriodAdditionalDutyCalculationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodAdditionalDutyCalculationValidation : AutoCMRTariffRatePeriodAdditionalDutyCalculationValidation
	{
		public CMRTariffRatePeriodAdditionalDutyCalculationValidation(AutoCMRTariffRatePeriodAdditionalDutyCalculation parent)
			: base(parent)
		{
		}
	}
}
