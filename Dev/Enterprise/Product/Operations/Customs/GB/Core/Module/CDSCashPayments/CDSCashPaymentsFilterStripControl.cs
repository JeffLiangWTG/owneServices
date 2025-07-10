using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module
{
	public partial class CDSCashPaymentsFilterStripControl : ZFilterStripControl
	{
		public CDSCashPaymentsFilterStripControl()
		{
			InitializeComponent();
		}

		public CDSCashPaymentsFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
