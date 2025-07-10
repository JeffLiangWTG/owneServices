using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class PriceUserControl : ZUserControl
	{
		public PriceUserControl()
		{
			InitializeComponent();
			PaymentAmountConvertToLocalCurrencyControl.SetReadOnly(true);
		}
		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.ValuationDeclarationMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, ControlExtensionMethods.BindingPathForMessageSending);
		}
	}
}
