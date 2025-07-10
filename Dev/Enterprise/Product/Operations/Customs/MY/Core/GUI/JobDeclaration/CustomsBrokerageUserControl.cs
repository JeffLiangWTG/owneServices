#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.MY.GUI
{
	public partial class CustomsBrokerageUserControl : Customs.GUI.BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		#region Create New User Controls for each tab

		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			Customs.GUI.BaseCustomsSupplierHeaderUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new MYImportSupplierHeaderUserControl();
			}
			else
			{
				result = new MYExportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new MYImportInvoiceLineUserControl();
			}
			else
			{
				result = new MYExportInvoiceLineUserControl();
			}

			return result;
		}

		protected override Customs.GUI.BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new MiscOptionsUserControl();
		}

		#endregion
	}
}
