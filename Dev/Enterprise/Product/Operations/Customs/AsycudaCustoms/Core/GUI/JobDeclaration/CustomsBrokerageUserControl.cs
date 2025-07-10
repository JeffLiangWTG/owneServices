using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
			InvoiceGroupingTabPage.TabRelevant = false;
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl() => JobDeclaration.IsExport ? new ExportInvoiceLineUserControl() : new BaseInvoiceLineUserControl();

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

		protected override void SetEntryInstructionsTabVisibility()
		{
			EntryInstructionDetailsTabPage.TabRelevant = JobDeclaration.AreMultipleEntryInstructionsAllowed && EntryInstructionsTabVisibleForCountry;
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;

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

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new CustomsEntryAndDiscardedMessagesUserControl();
		}
	}
}
