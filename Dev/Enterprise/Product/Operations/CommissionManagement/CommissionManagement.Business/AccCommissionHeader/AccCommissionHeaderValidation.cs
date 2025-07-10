//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionHeaderValidation
//
//    This class should be used for overriding validation in AutoAccCommissionHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionHeaderValidation : AutoAccCommissionHeaderValidation
	{
		public AccCommissionHeaderValidation(AutoAccCommissionHeader parent) : base(parent)
		{
		}
	}
}
