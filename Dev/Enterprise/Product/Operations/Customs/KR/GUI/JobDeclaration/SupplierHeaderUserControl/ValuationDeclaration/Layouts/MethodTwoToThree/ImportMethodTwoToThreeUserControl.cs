using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportMethodTwoToThreeUserControl : ZUserControl
	{
		public ImportMethodTwoToThreeUserControl()
		{
			InitializeComponent();
			this.BindingSource.DataSourceType = typeof(Business.JobComInvoiceHeader);
			Controls.ChangeBindingPaths(BindingSource, BindingPathForDeclaration);
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.ValuationDeclarationMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, ControlExtensionMethods.BindingPathForMessageSending);
		}
		const string BindingPathForDeclaration = "";
	}
}
