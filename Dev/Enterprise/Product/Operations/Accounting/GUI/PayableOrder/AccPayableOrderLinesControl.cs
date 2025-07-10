using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class AccPayableOrderLinesControl : ZUserControl
	{
		public AccPayableOrderLinesControl()
		{
			InitializeComponent();
			orderTotalsControl.AllowOutsideOfParent();
			OrderLinesGrid.AllowOverlap(orderTotalsControl);
		}
	}
}
