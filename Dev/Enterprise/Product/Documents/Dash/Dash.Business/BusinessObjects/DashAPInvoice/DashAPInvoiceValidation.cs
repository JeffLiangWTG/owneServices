//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceValidation
//
//    This class should be used for overriding validation in AutoDashAPInvoiceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceValidation : AutoDashAPInvoiceValidation
	{
		public DashAPInvoiceValidation(AutoDashAPInvoice parent) : base(parent)
		{
		}
	}
}
