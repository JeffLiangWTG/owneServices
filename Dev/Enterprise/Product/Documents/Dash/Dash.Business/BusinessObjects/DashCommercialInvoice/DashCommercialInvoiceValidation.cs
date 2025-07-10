//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashCommercialInvoiceValidation
//
//    This class should be used for overriding validation in AutoDashCommercialInvoiceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashCommercialInvoiceValidation : AutoDashCommercialInvoiceValidation
	{
		public DashCommercialInvoiceValidation(AutoDashCommercialInvoice parent) : base(parent)
		{
		}
	}
}
