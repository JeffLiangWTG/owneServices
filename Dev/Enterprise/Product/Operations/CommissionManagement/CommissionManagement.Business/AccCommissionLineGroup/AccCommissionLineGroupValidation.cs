//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionLineGroupValidation
//
//    This class should be used for overriding validation in AutoAccCommissionLineGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionLineGroupValidation : AutoAccCommissionLineGroupValidation
	{
		public AccCommissionLineGroupValidation(AutoAccCommissionLineGroup parent) : base(parent)
		{
		}
	}
}
