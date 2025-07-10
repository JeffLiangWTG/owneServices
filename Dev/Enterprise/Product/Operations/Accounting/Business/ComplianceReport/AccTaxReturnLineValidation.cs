//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxReturnLineValidation
//
//    This class should be used for overriding validation in AutoAccTaxReturnLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccTaxReturnLineValidation : AutoAccTaxReturnLineValidation
	{
		public AccTaxReturnLineValidation(AutoAccTaxReturnLine parent) : base(parent)
		{
		}
	}
}