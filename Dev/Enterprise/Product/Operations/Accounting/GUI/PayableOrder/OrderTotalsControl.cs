using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class OrderTotalsControl : ZUserControl
	{
		public OrderTotalsControl()
		{
			InitializeComponent();
			APH_Calc_TotalInvoicedPriceTextBox.AllowOverlap(APH_Calc_TotalLinePriceTextBox);
			APH_Calc_TotalQuantityReceivedTextBox.AllowOverlap(APH_Calc_TotalQuantityInvoicedTextBox);
		}
	}
}
