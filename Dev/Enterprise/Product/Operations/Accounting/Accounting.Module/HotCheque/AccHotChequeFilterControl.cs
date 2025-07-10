using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Filter control for AccHotCheque.
	/// </summary>
	public partial class AccHotChequeFilterControl : ZFilterStripControl
	{
		public AccHotChequeFilterControl()
		{
			//this.Enabled = false;
			InitializeComponent();
		}

		public AccHotChequeFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			//this.Enabled = false;
			InitializeComponent();
		}
	}
}
