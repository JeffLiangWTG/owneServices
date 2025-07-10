//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceChargeLineValidation
//
//    This class should be used for overriding validation in AutoDashAPInvoiceChargeLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceChargeLineValidation : AutoDashAPInvoiceChargeLineValidation
	{
		public DashAPInvoiceChargeLineValidation(AutoDashAPInvoiceChargeLine parent) : base(parent)
		{
		}
	}
}
