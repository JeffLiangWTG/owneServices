//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionLineValidation
//
//    This class should be used for overriding validation in AutoAccCommissionLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionLineValidation : AutoAccCommissionLineValidation
	{
		public AccCommissionLineValidation(AutoAccCommissionLine parent) : base(parent)
		{
		}
	}
}
