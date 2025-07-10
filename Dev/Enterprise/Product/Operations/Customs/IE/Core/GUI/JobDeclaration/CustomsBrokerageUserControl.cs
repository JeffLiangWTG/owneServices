using Enterprise.Customs.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new EntryMessageUserControl();

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			BaseCustomsSupplierHeaderUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new ImportSupplierHeaderUserControl();
			}
			else
			{
				result = new ExportSupplierHeaderUserControl();
			}

			return result;
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
