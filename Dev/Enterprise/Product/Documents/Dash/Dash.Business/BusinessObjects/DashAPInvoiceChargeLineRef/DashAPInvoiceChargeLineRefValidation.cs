//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceChargeLineRefValidation
//
//    This class should be used for overriding validation in AutoDashAPInvoiceChargeLineRefValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceChargeLineRefValidation : AutoDashAPInvoiceChargeLineRefValidation
	{
		public DashAPInvoiceChargeLineRefValidation(AutoDashAPInvoiceChargeLineRef parent) : base(parent)
		{
		}
	}
}
