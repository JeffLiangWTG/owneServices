//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccAmbiguousCommissionValidation
//
//    This class should be used for overriding validation in AutoAccAmbiguousCommissionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class AccAmbiguousCommissionValidation : AutoAccAmbiguousCommissionValidation
	{
		public AccAmbiguousCommissionValidation(AutoAccAmbiguousCommission parent)
			: base(parent)
		{
		}
	}
}

