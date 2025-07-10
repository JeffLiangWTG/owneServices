namespace Enterprise.Customs.CN.GUI.CommercialInvoice
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
			InitializeComponentLostByDesignMode();
		}

		void InitializeComponentLostByDesignMode()
		{
			BindingSource.SetBindingMember(JZ_MarksAndNumbersLongTextBox, "Invoices.JZ_MarksAndNumbers");
		}
	}
}
