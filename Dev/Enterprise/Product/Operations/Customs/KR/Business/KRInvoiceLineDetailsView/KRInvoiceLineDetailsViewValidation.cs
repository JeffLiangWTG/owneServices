//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKRInvoiceLineDetailsViewValidation
//
//    This class should be used for overriding validation in AutoKRInvoiceLineDetailsViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.KR.Business
{
	public class KRInvoiceLineDetailsViewValidation : AutoKRInvoiceLineDetailsViewValidation
	{
		public KRInvoiceLineDetailsViewValidation(AutoKRInvoiceLineDetailsView parent) : base(parent)
		{
		}
	}
}
