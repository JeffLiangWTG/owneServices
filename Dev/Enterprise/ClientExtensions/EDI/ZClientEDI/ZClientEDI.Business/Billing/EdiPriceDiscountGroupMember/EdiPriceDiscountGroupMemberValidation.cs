//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceDiscountGroupMemberValidation
//
//    This class should be used for overriding validation in AutoEdiPriceDiscountGroupMemberValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceDiscountGroupMemberValidation : AutoEdiPriceDiscountGroupMemberValidation
	{
		public EdiPriceDiscountGroupMemberValidation(AutoEdiPriceDiscountGroupMember parent) : base(parent)
		{
		}
	}
}

