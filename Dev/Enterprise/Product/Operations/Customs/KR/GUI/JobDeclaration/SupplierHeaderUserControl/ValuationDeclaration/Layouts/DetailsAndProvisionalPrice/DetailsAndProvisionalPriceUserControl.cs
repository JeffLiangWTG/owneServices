using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class DetailsAndProvisionalPriceUserControl : ZUserControl
	{
		public DetailsAndProvisionalPriceUserControl()
		{
			InitializeComponent();
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.ValuationDeclarationMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, ControlExtensionMethods.BindingPathForMessageSending);
			InvoiceNoTextBox.ReadOnly = true;
			InvoiceDateEdit.ReadOnly = true;
			PurchaseOrderNoTextBox.ReadOnly = true;
			PurchaseOrderDateEdit.ReadOnly = true;
			ContractNoTextBox.ReadOnly = true;
			ContractDateEdit.ReadOnly = true;
			ProvisionalPricingDropEdit.ReadOnly = true;
			ProvisionalAdditionRateCalcEdit.ReadOnly = true;
			ProvisionalAdditionAmountCalcEdit.ReadOnly = true;
			EstimatedDateOfFinalPriceDateEdit.ReadOnly = true;
			ContractExpirationDateEdit.ReadOnly = true;
		}
	}
}
