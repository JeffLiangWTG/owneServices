//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxReturnValidation
//
//    This class should be used for overriding validation in AutoAccTaxReturnValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccTaxReturnValidation : AutoAccTaxReturnValidation
	{
		public AccTaxReturnValidation(AutoAccTaxReturn parent) : base(parent)
		{
		}
	}
}