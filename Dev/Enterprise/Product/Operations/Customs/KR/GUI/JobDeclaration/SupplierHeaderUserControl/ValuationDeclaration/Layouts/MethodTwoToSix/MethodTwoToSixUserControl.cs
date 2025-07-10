using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MethodTwoToSixUserControl : ZUserControl
	{
		public MethodTwoToSixUserControl()
		{
			InitializeComponent();
			Controls.ChangeBindingPaths(BindingSource, BindingPathForDeclaration);
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.ValuationDeclarationMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, ControlExtensionMethods.BindingPathForMessageSending);
			this.SupportingDocument1TextBox.ReadOnly = true;
			this.SupportingDocument2TextBox.ReadOnly = true;
			this.SampleItemCheckBox.ReadOnly = true;
			this.GiftOrFreeDonationCheckBox.ReadOnly = true;
			this.AdvertisingUseCheckBox.ReadOnly = true;
			this.ForProductionAndManufactureCheckBox.ReadOnly = true;
			this.UseOfDefectiveRepairCheckBox.ReadOnly = true;
			this.ReplacementItemCheckBox.ReadOnly = true;
			this.PerformancePriceOfPaidTransactionCheckBox.ReadOnly = true;
			this.InvoiceCheckBox.ReadOnly = true;
			this.PriceListCheckBox.ReadOnly = true;
			this.ManufacturingCostCheckBox.ReadOnly = true;
			this.ItemUseCodeOtherReasonTextBox.ReadOnly = true;
			this.GoodsPricingBasisOtherReasonTextBox.ReadOnly = true;
		}
		const string BindingPathForDeclaration = "Invoices.";
	}
}
