using Enterprise.Customs.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			Customs.GUI.BaseCustomsSupplierHeaderUserControl result;

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

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;

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

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			return new EntryInstructionDetailsUserControl();
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			if (JobDeclaration.IsDeclarationIntegrated)
			{
				return new EntriesWithMessagesOnDeclarationUserControl(JobDeclaration);
			}

			return new ImportMessageUserControl(JobDeclaration);
		}
	}
}
