namespace Enterprise.Customs.IL.GUI
{
	public partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		protected override bool UseUniversalTariff => true;
	}
}
