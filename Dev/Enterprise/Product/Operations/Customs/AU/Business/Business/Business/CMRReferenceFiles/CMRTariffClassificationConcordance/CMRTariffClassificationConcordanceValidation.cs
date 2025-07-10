//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTariffClassificationConcordanceValidation
//
//    This class should be used for overriding validation in AutoCMRTariffClassificationConcordanceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationConcordanceValidation : AutoCMRTariffClassificationConcordanceValidation
	{
		public CMRTariffClassificationConcordanceValidation(AutoCMRTariffClassificationConcordance parent)
			: base(parent)
		{
		}
	}
}
