using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class NRCanEEFUserControl : ZUserControl
	{
		public NRCanEEFUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}
		protected void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			if (!isOnInvoiceLine)
			{
				detailsGroupBox.Controls.Remove(modelNameTextBox);
				detailsGroupBox.Controls.Remove(brandNameTextBox);
				detailsGroupBox.Controls.Remove(tradeNameTextBox);
			}
		}
	}
}
