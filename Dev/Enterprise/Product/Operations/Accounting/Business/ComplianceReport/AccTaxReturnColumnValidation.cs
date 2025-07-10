//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxReturnColumnValidation
//
//    This class should be used for overriding validation in AutoAccTaxReturnColumnValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccTaxReturnColumnValidation : AutoAccTaxReturnColumnValidation
	{
		public AccTaxReturnColumnValidation(AutoAccTaxReturnColumn parent) : base(parent)
		{
		}
	}
}