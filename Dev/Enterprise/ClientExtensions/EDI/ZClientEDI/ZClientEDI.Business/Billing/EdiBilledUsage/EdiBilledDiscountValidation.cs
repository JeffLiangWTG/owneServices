//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiBilledDiscountValidation
//
//    This class should be used for overriding validation in AutoEdiBilledDiscountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBilledDiscountValidation : AutoEdiBilledDiscountValidation
	{
		public EdiBilledDiscountValidation(AutoEdiBilledDiscount parent) : base(parent)
		{
		}
	}
}

