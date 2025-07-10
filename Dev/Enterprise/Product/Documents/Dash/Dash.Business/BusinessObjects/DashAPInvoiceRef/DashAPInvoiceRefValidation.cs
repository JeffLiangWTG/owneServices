//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceRefValidation
//
//    This class should be used for overriding validation in AutoDashAPInvoiceRefValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceRefValidation : AutoDashAPInvoiceRefValidation
	{
		public DashAPInvoiceRefValidation(AutoDashAPInvoiceRef parent) : base(parent)
		{
		}
	}
}
