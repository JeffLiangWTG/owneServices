using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

		#region Create New User Controls for each tab

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

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new EntryMessageUserControl();

		#endregion
	}
}
