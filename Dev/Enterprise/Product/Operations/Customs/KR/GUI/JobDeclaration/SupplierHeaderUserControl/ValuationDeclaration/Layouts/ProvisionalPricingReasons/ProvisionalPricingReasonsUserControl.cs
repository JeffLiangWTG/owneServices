using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ProvisionalPricingReasonsUserControl : ZUserControl
	{
		public ProvisionalPricingReasonsUserControl()
		{
			InitializeComponent();
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.ValuationDeclarationMessageSendingObjectParent);

			Controls.ChangeBindingPaths(BindingSource, ControlExtensionMethods.BindingPathForMessageSending);

			ProvisionalPricingReason101CheckBox.ReadOnly = true;
			ProvisionalPricingReason102CheckBox.ReadOnly = true;
			ProvisionalPricingReason103CheckBox.ReadOnly = true;
			ProvisionalPricingReason104CheckBox.ReadOnly = true;
			ProvisionalPricingReason105CheckBox.ReadOnly = true;
			ProvisionalPricingReason106CheckBox.ReadOnly = true;
			ProvisionalPricingReason107CheckBox.ReadOnly = true;
			ProvisionalPricingReason108CheckBox.ReadOnly = true;
			ProvisionalPricingReason109CheckBox.ReadOnly = true;
			ProvisionalPricingReason110CheckBox.ReadOnly = true;
			ProvisionalPricingReason111CheckBox.ReadOnly = true;
			ProvisionalPricingReason112CheckBox.ReadOnly = true;
			ProvisionalPricingReason113CheckBox.ReadOnly = true;
			ProvisionalPricingReason114CheckBox.ReadOnly = true;
			ProvisionalPricingReason115CheckBox.ReadOnly = true;
			ProvisionalPricingReason116CheckBox.ReadOnly = true;
			ProvisionalPricingReason117CheckBox.ReadOnly = true;
			ProvisionalPricingReason120CheckBox.ReadOnly = true;
			OtherReasonTextBox.ReadOnly = true;
		}
	}
}
