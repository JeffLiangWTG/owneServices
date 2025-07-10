//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDashAPInvoiceClusterRefValidation
//
//    This class should be used for overriding validation in AutoDashAPInvoiceClusterRefValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceClusterRefValidation : AutoDashAPInvoiceClusterRefValidation
	{
		public DashAPInvoiceClusterRefValidation(AutoDashAPInvoiceClusterRef parent) : base(parent)
		{
		}
	}
}
