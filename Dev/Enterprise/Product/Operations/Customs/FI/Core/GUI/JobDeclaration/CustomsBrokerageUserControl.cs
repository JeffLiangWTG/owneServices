using Enterprise.Customs.GUI;

namespace Enterprise.Customs.FI.GUI
{
	public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InvoiceGroupingTabPage.TabRelevant = false;
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			BaseInvoiceLineUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new ImportInvoiceLineUserControl();
			}
			else
			{
				result = new ExportInvoiceLineUserControl();
			}

			return result;
		}
	}
}
