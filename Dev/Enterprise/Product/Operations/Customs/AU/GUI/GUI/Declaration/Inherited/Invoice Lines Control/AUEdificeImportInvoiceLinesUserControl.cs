namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUEdificeImportInvoiceLinesUserControl : AUImportInvoiceLineUserControl
	{
		public AUEdificeImportInvoiceLinesUserControl()
		{
			InitializeComponent();
			this.LineDetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(DeclarationType.EdificeImport);
		}

		#region Overrides

		protected override void SetDynamicControlStates()
		{
			base.SetDynamicControlStates();

			if (AdjustmentCurrencyLabel != null)
			{
				AdjustmentCurrencyLabel.Visible = currencyAmountVisible && currencyVisible;
			}
		}

		#endregion
	}
}
