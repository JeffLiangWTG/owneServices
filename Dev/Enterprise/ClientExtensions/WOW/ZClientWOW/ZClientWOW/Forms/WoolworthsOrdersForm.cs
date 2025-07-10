using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.Wow
{
	public partial class WoolworthsOrdersForm : OrdersForm
	{
		public WoolworthsOrdersForm()
		{
			InitializeComponent();
		}

		public WoolworthsOrdersForm(WoolworthsOrder order) : base(order)
		{
			InitializeComponent();
			order.InvoiceOrderMismatching += new CancelEventHandler(OnOrderInvoiceMismatching);
			order.IndentOrVendorOrderRequested += new CancelEventHandler(OnIndentOrVendorOrderRequested);
		}

		protected override OrdersUserControl NewOrdersUserControl()
		{
			return new WoolworthsOrdersUserControl();
		}

		#region Implementation

		protected void OnOrderInvoiceMismatching(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show("You cannot modify this field as it will break the link between the order line delivery and the invoice line.",
				"",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			//			if (Result == DialogResult.No)
			//			{
			e.Cancel = true;
			//			}
		}

		protected void OnIndentOrVendorOrderRequested(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show("Showing the indent or vendor order report will change the order status to 'Order Placed / finalised'. Is this OK?",
				"",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Error);
			if (result == DialogResult.No)
			{
				e.Cancel = true;
			}
		}

		#endregion
	}

	#region Implementation

	public class TestWoolworthsOrdersForm : WoolworthsOrdersForm
	{
		public TestWoolworthsOrdersForm(WoolworthsOrder bO) : base(bO)
		{
		}

		public OrdersUserControl GetOrdersUserControl()
		{
			return base.OrdersUserControl.Inner;
		}
	}

	#endregion
}
