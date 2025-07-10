//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACTaxRateValidation
//
//    This class should be used for overriding validation in AutoCACTaxRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACTaxRateValidation : AutoCACTaxRateValidation
	{
		public CACTaxRateValidation(AutoCACTaxRate parent) : base(parent)
		{
		}
	}
}
