using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ABIProgramUserControl : ZUserControl
	{
		public ABIProgramUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();

			this.isOnInvoiceLine = isOnInvoiceLine;
			UpdateDataBinding();
		}

		readonly bool isOnInvoiceLine;

		void UpdateDataBinding()
		{
			if (!isOnInvoiceLine)
			{
				CommonNameTextBox.Dispose();
				TradeNameTextBox.Dispose();
				CountIntEdit.Dispose();
			}
		}
	}
}
