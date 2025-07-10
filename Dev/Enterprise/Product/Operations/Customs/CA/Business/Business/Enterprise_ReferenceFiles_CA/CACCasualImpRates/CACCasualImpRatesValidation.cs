//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACCasualImpRatesValidation
//
//    This class should be used for overriding validation in AutoCACCasualImpRatesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACCasualImpRatesValidation : AutoCACCasualImpRatesValidation
	{
		public CACCasualImpRatesValidation(AutoCACCasualImpRates parent) : base(parent)
		{
		}
	}
}
