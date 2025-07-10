using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class PostClearanceDetailsUserControl : ZUserControl
	{
		public PostClearanceDetailsUserControl()
		{
			InitializeComponent();

			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			Controls.ChangeBindingPaths(BindingSource, BindingPathForDeclaration);
		}
		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.JobDeclarationMiscMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, BindingPathForMessageSending);

			PostClearanceYNDropEdit.ReadOnly = true;
			ProductTypeDropEdit.ReadOnly = true;
			UseCodeDescriptionTextBox.ReadOnly = true;
			CustomsOfficeCodeFindBox.ReadOnly = true;
			SerialNumberTextBox.ReadOnly = true;
			GoodsLocationAddressControl.ReadOnly = true;
			GoodsLocationAddressControl.SetReadOnlyIncludingChildren(true);
		}

		const string BindingPathForMessageSending = "SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.";
		const string BindingPathForDeclaration = "FilteredInvoiceLines.";
	}
}
