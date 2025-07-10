namespace Enterprise.Customs.CA.GUI
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override string ColumnTitleForGSTApplies => CACustomsSupplierHeaderUserControl.IsCIFComponent;
	}
}
