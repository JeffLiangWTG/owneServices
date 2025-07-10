//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiUsageInvoiceValidation
//
//    This class should be used for overriding validation in AutoEdiUsageInvoiceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiUsageInvoiceValidation : AutoEdiUsageInvoiceValidation
	{
		public EdiUsageInvoiceValidation(AutoEdiUsageInvoice parent) : base(parent)
		{
		}
	}
}

