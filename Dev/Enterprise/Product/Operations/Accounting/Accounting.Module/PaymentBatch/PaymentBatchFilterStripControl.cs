using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class PaymentBatchFilterStripControl : ZFilterStripControl
	{
		public PaymentBatchFilterStripControl()
		{
			InitializeComponent();
		}

		public PaymentBatchFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
