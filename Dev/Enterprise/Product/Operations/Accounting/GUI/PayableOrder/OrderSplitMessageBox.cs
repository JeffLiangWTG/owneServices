using CargoWise.Windows.UI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class OrderSplitMessageBox : KForm
	{
		public OrderSplitMessageBox()
		{
			InitializeComponent();
			this.Text = Res.GetString("74b46a35-a760-42ce-ab09-1bfd40240268", "Incomplete Order.");
			this.MessageLabel.Text = Res.GetString("1545d63a-5356-4345-b440-4ec5dd295c47", "This order has some incomplete order lines. Would you like to:") + " ";
		}

		public OrderSplitDialogResult OrderSplitDialogResult
		{
			get { return fOrderSplitDialogResult; }
		}

		#region Implementation

		OrderSplitDialogResult fOrderSplitDialogResult = OrderSplitDialogResult.No;

		void SplitButton_Click(object sender, System.EventArgs e)
		{
			fOrderSplitDialogResult = OrderSplitDialogResult.SplitOrder;
			this.Close();
		}

		void NoButton_Click(object sender, System.EventArgs e)
		{
			fOrderSplitDialogResult = OrderSplitDialogResult.No;
			this.Close();
		}

		void NewButton_Click(object sender, System.EventArgs e)
		{
			fOrderSplitDialogResult = OrderSplitDialogResult.CreateNewOrder;
			this.Close();
		}

		#endregion
	}

	public enum OrderSplitDialogResult
	{
		SplitOrder,
		CreateNewOrder,
		No
	}
}
