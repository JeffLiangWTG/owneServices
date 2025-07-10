using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class AccPayableOrderLinesTotalByProductControl : ZUserControl
	{
		public AccPayableOrderLinesTotalByProductControl()
		{
			InitializeComponent();
			ProductSummaryLinesGrid.AllowOverlap(orderTotalsControl);
		}
	}
}
