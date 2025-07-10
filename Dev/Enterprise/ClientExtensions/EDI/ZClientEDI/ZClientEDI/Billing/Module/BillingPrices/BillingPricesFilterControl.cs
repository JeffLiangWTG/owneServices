using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.Module.BillingPrices
{
	public partial class BillingPricesFilterControl : ZFilterStripControl
	{
		public BillingPricesFilterControl()
		{
			InitializeComponent();
		}

		public BillingPricesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
