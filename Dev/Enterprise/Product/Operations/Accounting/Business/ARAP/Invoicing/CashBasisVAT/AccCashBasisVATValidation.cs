//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCashBasisVATValidation
//
//    This class should be used for overriding validation in AutoAccCashBasisVATValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AccCashBasisVATValidation : AutoAccCashBasisVATValidation
	{
		public AccCashBasisVATValidation(AutoAccCashBasisVAT parent) : base(parent)
		{
		}
	}
}
